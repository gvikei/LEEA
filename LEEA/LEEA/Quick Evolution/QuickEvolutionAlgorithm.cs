using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SharpNeat.Core;
using SharpNeat.Utility;

namespace LEEA
{
    public class QuickEvolutionAlgorithm
    {
        public static double MUTATIONPOWER;
        public static double MUTATIONRATE;

        private double decayRate, rateDecayRate;
        private List<QEAGenome> genomeList;
        private FastRandom r;
        private IDomainEvaluator evaluator;
        private int currentGeneration;
        private ParallelOptions po;

        private EvaluationWorker[] workers = new EvaluationWorker[LEEAParams.THREADS - 1];
        private Thread[] threads = new Thread[LEEAParams.THREADS - 1];
        int start = 0;

        public QuickEvolutionAlgorithm(IDomainEvaluator evaluator)
        {
            this.evaluator = evaluator;
            r = new FastRandom();
            po = new ParallelOptions();
            po.MaxDegreeOfParallelism = LEEAParams.THREADS;
            currentGeneration = 0;            

            createPopulation();
            evaluatePopulation();

            MUTATIONPOWER = LEEAParams.MUTATIONPOWER;
            MUTATIONRATE = LEEAParams.MUTATIONRATE;
            // calculate decay rate so that final mutation power is decayed properly
            decayRate = Math.Pow((1 - LEEAParams.MUTATIONPOWERDECAY), 1.0f / LEEAParams.MAXGENERATIONS);
            rateDecayRate = Math.Pow((1 - LEEAParams.MUTATIONRATEDECAY), 1.0f / LEEAParams.MAXGENERATIONS);
        }

        private void createPopulation()
        {
            genomeList = new List<QEAGenome>();

            int layers = LEEAParams.DOMAIN.getHiddenLayers() + 1; // layers needs to be the number of layers of connections
            int[] layerNodes = new int[layers + 1];

            layerNodes[0] = LEEAParams.DOMAIN.getInputs() + 1; // +1 for the bias!
            for(int i = 0; i < layers - 1; i++)
                layerNodes[i + 1] = LEEAParams.DOMAIN.getHiddenLayerNeurons(i + 1);

            layerNodes[layers] = LEEAParams.DOMAIN.getOutputs();

            Parallel.For(0, LEEAParams.POPSIZE, po, i =>            
            {
                QEAGenome genome = new QEAGenome(layers, layerNodes, 1);
                lock(genomeList)
                {
                    genomeList.Add(genome);
                }
            });

            // set up our worker threads
            int end;

            for (int i = 0; i < workers.Length; i++)
            {
                end = start + genomeList.Count / LEEAParams.THREADS;
                workers[i] = new EvaluationWorker(start, end, genomeList, evaluator);
                start = end;
            }

            // speciate the newly created folks
            speciatePopulation();
        }

        public void run()
        {
            while (currentGeneration <= LEEAParams.MAXGENERATIONS)
            {
                currentGeneration++;
                if (currentGeneration % SharedParams.SharedParams.TRACKFITNESSSTRIDE == 0)
                    SharedParams.SharedParams.PERFORMFULLEVAL = true;
                else
                    SharedParams.SharedParams.PERFORMFULLEVAL = false;

                performOneGeneration();                                
            }
        }

        private void performOneGeneration()
        {        
            // produce offspring
            produceOffspring();

            // speciate the new kids
            speciatePopulation();

            // evaluate the new population
            evaluatePopulation();
          
            // update evolution parameters
            MUTATIONPOWER *= decayRate;
            MUTATIONRATE *= rateDecayRate;
        }

        private void evaluatePopulation()
        {
            evaluator.NewGeneration();

            for (int i = 0; i < workers.Length; i++)
            {
                workers[i].setGenomeList(genomeList);
                threads[i] = new Thread(new ThreadStart(workers[i].DoWork));
                threads[i].Start();
            }            

            EvaluationWorker mainWorker = new EvaluationWorker(start, genomeList.Count, genomeList, evaluator);
            mainWorker.DoWork();

            for (int i = 0; i < threads.Length; i++)
                threads[i].Join();

            updateStats();
        }

        private void speciatePopulation()
        {
            if (LEEAParams.SPECIESCOUNT == 1)
                return;

            double[][] centroids = new double[LEEAParams.SPECIESCOUNT][];
            double[][] flatWeights = new double[genomeList.Count][];

            int connectionCount = genomeList[0].weights.SelectMany(s => s).SelectMany(s => s).ToArray().Length;

            // flatten the weight arrays to them more efficient to work with
            Parallel.For(0, genomeList.Count, i =>
            //for (int i = 0; i < genomeList.Count; i++)
            {
                flatWeights[i] = new double[connectionCount];
                Array.Copy(genomeList[i].weights.SelectMany(s => s).SelectMany(s => s).ToArray(), flatWeights[i], connectionCount);
            });           

            // set the centroid to x random individuals in the population.  This ensures each cluster has at least one individual.
            for (int i = 0; i < LEEAParams.SPECIESCOUNT; i++)
            {
                centroids[i] = new double[connectionCount];
                Array.Copy(flatWeights[i], centroids[i], connectionCount);
            }

            int kMeansCount = 0;
            bool changed = true;

            // so long as individuals are being reassigned, continue the kmeans loop.. max iterations of 5
            while (changed && kMeansCount < 5)
            {
                changed = kMeansLoop(flatWeights, centroids);
                kMeansCount++;
            }
                
            // if any empty species exist, we need to assign them
            bool[] nonEmpty = new bool[LEEAParams.SPECIESCOUNT];
            Parallel.For(0, genomeList.Count, po, i =>
            //for (int i = 0; i < genomeList.Count; i++)
            {
                nonEmpty[genomeList[i].Species] = true;
            });
            
            for (int i = 0; i < nonEmpty.Length; i++)
                if (!nonEmpty[i])
                {
                    // find the genome that is furthest from its centroid    
                    double furthest = 0;
                    int furthestIndex = 0;
                    Object lockObj = new object();

                    Parallel.For(0, genomeList.Count, po, j =>
                    //for (int j = 0; j < genomeList.Count; j++)
                    {
                        double distance = calculateVectorDistance(flatWeights[j], centroids[genomeList[j].Species]);

                        if (distance > furthest)
                        {
                            lock (lockObj)
                            {
                                // now that we have lock, check again in case furthest was updated by another thread
                                if (distance > furthest)
                                {
                                    furthest = distance;
                                    furthestIndex = j;
                                }
                            }
                        }
                    });

                    // set this genome's weights as the new centroid
                    Array.Copy(flatWeights[furthestIndex], centroids[i], flatWeights[furthestIndex].Length);
                    genomeList[furthestIndex].Species = i;

                    // now respeciate the population with this new centroid in place
                    kMeansLoop(flatWeights, centroids, true);
                }            
        }

        private bool kMeansLoop(double[][] flatWeights, double[][] centroids, bool skipCentroidCalculation = false)
        {
            bool changed = false;

            Parallel.For(0, genomeList.Count, i =>
            //for (int i = 0; i < genomeList.Count; i++)
            {
                // find the nearest centroid for this individual
                int nearest = 0;
                double nearestDistance = double.MaxValue;

                for (int j = 0; j < centroids.Length; j++)
                {
                    double distance = calculateVectorDistance(centroids[j], flatWeights[i]);                   

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = j;
                    }
                }

                if (genomeList[i].Species != nearest)
                {
                    changed = true;
                    genomeList[i].Species = nearest;
                }
            });

            if (changed && !skipCentroidCalculation)
            {
                // recalculate the centroids
                Parallel.For(0, centroids.Length, i =>
                //for (int i = 0; i < centroids.Length; i++)
                {
                    int genomes = 0;
                    for (int j = 0; j < centroids[i].Length; j++)
                        centroids[i][j] = 0;

                    for (int j = 0; j < genomeList.Count; j++)
                    {
                        if (genomeList[j].Species == i)
                        {
                            genomes++;
                            for (int k = 0; k < flatWeights[j].Length; k++)
                                centroids[i][k] += flatWeights[j][k];
                        }
                    }

                    if (genomes != 0)
                    {
                        for (int j = 0; j < centroids[i].Length; j++)
                            centroids[i][j] /= genomes;
                    }
                });
            }

            return changed;
        }

        // assumes a and b are the same length
        private double calculateVectorDistance(double[] a, double[] b)
        {
            double distance = 0;

            for (int i = 0; i < a.Length; i++)
            {
                distance += (a[i] - b[i]) * (a[i] - b[i]);
            }

            return distance;
        }

        private void produceOffspring()
        {
            double[] offspringCount = new double[LEEAParams.SPECIESCOUNT];

            // generate species-specific genome lists
            List<QEAGenome>[] speciesGenomes = new List<QEAGenome>[LEEAParams.SPECIESCOUNT];
            for (int i = 0; i < LEEAParams.SPECIESCOUNT; i++)
                speciesGenomes[i] = new List<QEAGenome>();

            for (int i = 0; i < genomeList.Count; i++)
                speciesGenomes[genomeList[i].Species].Add(genomeList[i]);

            // determine offspring count for each species
            if (LEEAParams.SPECIESCOUNT == 1)
            {
                offspringCount[0] = LEEAParams.POPSIZE;                
            }
            else
            {
                double[] specieFitness = new double[LEEAParams.SPECIESCOUNT];
                // calculate species stats to determine how many offspring each species is granted
                
                for (int i = 0; i < LEEAParams.SPECIESCOUNT; i++)
                {                    
                    for (int j = 0; j < speciesGenomes[i].Count; j++)
                    {
                        specieFitness[i] += speciesGenomes[i][j].Fitness;                                                    
                    }

                    if (speciesGenomes[i].Count != 0)
                        specieFitness[i] /= speciesGenomes[i].Count;
                }

                
                for (int i = 0; i < LEEAParams.SPECIESCOUNT; i++)
                    offspringCount[i] = Math.Round(LEEAParams.POPSIZE * specieFitness[i] / specieFitness.Sum());

                // rounding error could leave us a few short or long of the pop size, trim or fill to reach population size            
                RouletteWheelLayout rwl = new RouletteWheelLayout(offspringCount);
                while (offspringCount.Sum() > LEEAParams.POPSIZE)
                {
                    int index = RouletteWheel.SingleThrow(rwl, r);
                    if (offspringCount[index] > 1)
                        offspringCount[index]--;
                }

                while (offspringCount.Sum() < LEEAParams.POPSIZE)
                {
                    int index = RouletteWheel.SingleThrow(rwl, r);
                    offspringCount[index]++;
                }
            }
            
            List<QEAGenome> newGeneration = new List<QEAGenome>();

            // generate offspring for each species
            // parallelism doesn't work if speciescount = 1 here!
           
            for (int i = 0; i < LEEAParams.SPECIESCOUNT; i++)
            {
                if (offspringCount[i] > 0)
                {
                    // sort the genome list by fitness
                    Comparison<QEAGenome> comparison = (x, y) => y.Fitness.CompareTo(x.Fitness);
                    speciesGenomes[i].Sort(comparison);

                    // determine the top X individuals that we will select from
                    int selectionNumber = (int)(speciesGenomes[i].Count * LEEAParams.SELECTIONPROPORTION);
                    if (selectionNumber == 0)
                        selectionNumber = 1;

                    // build list of probabilities based on fitness
                    double[] probabilities = new double[selectionNumber];

                    for (int j = 0; j < probabilities.Length; j++)
                        probabilities[j] = speciesGenomes[i][j].Fitness;

                    RouletteWheelLayout rw = new RouletteWheelLayout(probabilities);

                    // build a list of matings to be performed.  This must be done outside of the parallelized section.
                    int[][] matings = new int[(int)offspringCount[i]][];
                    for (int j = 0; j < matings.Length; j++)
                    {
                        matings[j] = new int[2];

                        // select main parent
                        int index = RouletteWheel.SingleThrow(rw, r);

                        if (r.NextDouble() < LEEAParams.SEXPROPORTION && probabilities.Length > 1) // can't have sexual reproduction if this species only has a single member
                        {                            
                            matings[j][0] = index;
                            
                            int parent2 = index;
                            while(parent2 == index)
                                parent2 = RouletteWheel.SingleThrow(rw, r);
                            matings[j][1] = parent2;
                        }
                        else
                        {
                            matings[j][0] = index;
                            matings[j][1] = int.MinValue;
                        }
                    }

                    Parallel.For(0, matings.Length, po, j =>
                    //for (int j = 0; j < matings.Length; j++)
                    {
                        // mutate
                        QEAGenome child;
                        if (matings[j][1] > int.MinValue)
                        {
                            // sexual reproduction
                            child = speciesGenomes[i][matings[j][0]].createOffspring(speciesGenomes[i][matings[j][1]]);
                            child.Fitness = (speciesGenomes[i][matings[j][0]].Fitness + speciesGenomes[i][matings[j][1]].Fitness) / 2;
                        }
                        else
                        {
                            child = speciesGenomes[i][matings[j][0]].createOffspring();
                            child.Fitness = speciesGenomes[i][matings[j][0]].Fitness;
                        }

                        lock (newGeneration)
                        {
                            newGeneration.Add(child);
                        }
                    });
                }
            }

            // encourage the garbage collector to free up some memory
            foreach (QEAGenome g in genomeList)
            {
                g.weights = null;                
            }
            genomeList = null;
            

            genomeList = newGeneration;
        }                

        private void updateStats()
        {
            if (currentGeneration % 1000 == 0)
                System.IO.File.AppendAllText("gen.txt", System.DateTime.Now + ":" + currentGeneration + Environment.NewLine);            

            if (SharedParams.SharedParams.TRACKFITNESS)
            {
                if (currentGeneration % SharedParams.SharedParams.TRACKFITNESSSTRIDE == 0)
                {
                    // determine who is best at the validation set
                    double maxFit = 0;
                    double maxAlt = 0;
                    int maxAltIndex = 0;

                    for (int j = 0; j < genomeList.Count; j++)
                    {
                        if (genomeList[j].Fitness > maxFit)
                            maxFit = genomeList[j].Fitness;

                        if (genomeList[j].AltFitness > maxAlt)
                        {
                            maxAlt = genomeList[j].AltFitness;
                            maxAltIndex = j;
                        }
                    }

                    FitnessInfo fit = evaluator.Test(new QuickBlackBox(genomeList[maxAltIndex]));

                    System.IO.File.AppendAllText("training.txt", maxFit + Environment.NewLine);
                    System.IO.File.AppendAllText("validation.txt", maxAlt + Environment.NewLine);
                    System.IO.File.AppendAllText("test.txt", fit._fitness + Environment.NewLine);
                }
            }            
        }
    }

    class EvaluationWorker
    {
        int start, end;
        List<QEAGenome> genomeList;
        IDomainEvaluator evaluator;

        public EvaluationWorker(int start, int end, List<QEAGenome> genomeList, IDomainEvaluator evaluator)
        {
            this.start = start;
            this.end = end;
            this.genomeList = genomeList;
            this.evaluator = evaluator;
        }

        public void setGenomeList(List<QEAGenome> genomeList)
        {
            this.genomeList = genomeList;
        }

        public void DoWork()
        {
            for (int i = start; i < end; i++)
            {
                FitnessInfo f = evaluator.Evaluate(new QuickBlackBox(genomeList[i]));
                if (SharedParams.SharedParams.USEFITNESSBANK)
                {
                    genomeList[i].Fitness *= 1 - SharedParams.SharedParams.FITNESSBANKDECAYRATE;  // decay the previous fitness
                    genomeList[i].Fitness += f._fitness; // add the new value
                }
                else
                    genomeList[i].Fitness = f._fitness;
                genomeList[i].AltFitness = f._auxFitnessArr[0]._value;
            }
        }
    }
}
