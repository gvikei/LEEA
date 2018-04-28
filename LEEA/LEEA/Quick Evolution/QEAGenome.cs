using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Utility;

namespace LEEA
{
    class QEAGenome
    {
        public double[][][] weights; // first index represents layer, second is target node index, third is source node index
        private FastRandom r;
        public double Fitness { get; set; }
        public double AltFitness { get; set; }
        public int Species { get; set; }

        // produces a new random individual
        // layers is number of layers of connections, layer nodes includes input (don't forget to add the bias node) and output nodes, and so should be (layers+1) in size
        // delta is maximize initial weight size
        public QEAGenome(int layers, int[] layerNodes, double delta) 
        {
            r = new FastRandom();

            weights = new double[layers][][];
            
            for (int i = 0; i < layers; i++)
            {
                weights[i] = new double[layerNodes[i + 1]][];
                for (int j = 0; j < layerNodes[i + 1]; j++)
                    weights[i][j] = new double[layerNodes[i]];
            }

            initialize(delta);
        }

        private void initialize(double delta) // randomly initialize the weights for this individual, with a maximum perterbance of delta
        {
            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        weights[i][j][k] = (float)(r.NextDouble() * delta * 2 - delta);
                    }
                }
            }
        }

        public QEAGenome(QEAGenome copy)
        {
            r = new FastRandom();
            this.weights = copyArray(copy.weights);
        }

        private double[][][] copyArray(double[][][] source)
        {
            double[][][] dest = new double[source.Length][][];

            for (int i = 0; i < dest.Length; i++)
            {
                dest[i] = new double[source[i].Length][];
                for (int j = 0; j < dest[i].Length; j++)
                {
                    dest[i][j] = new double[source[i][j].Length];
                    Array.Copy(source[i][j], dest[i][j], source[i][j].Length);
                }
            }

            return dest;
        }

        public QEAGenome createOffspring() // asexual reproduction
        {
            QEAGenome child = new QEAGenome(this);
            
            for (int i = 0; i < child.weights.Length; i++)
            {
                for (int j = 0; j < child.weights[i].Length; j++)
                {
                    for (int k = 0; k < child.weights[i][j].Length; k++)
                    {
                        if(r.NextDouble() < LEEAParams.MUTATIONRATE)
                            child.weights[i][j][k] += (float)(r.NextDouble() * QuickEvolutionAlgorithm.MUTATIONPOWER * 2 - QuickEvolutionAlgorithm.MUTATIONPOWER);
                    }
                }
            }
            return child;
        }

        public QEAGenome createOffspring(QEAGenome parent2) // sexual reproduction
        {
            QEAGenome child = new QEAGenome(this);
            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        if (r.NextDouble() < 0.5)
                            weights[i][j][k] = parent2.weights[i][j][k];
                    }
                }
            }
            return child;
        }
    }
}
