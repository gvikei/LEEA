using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;
using SharpNeat.Core;

namespace LEEA
{
    class BackpropAlgorithm
    {
        private QEAGenome network;
        private QuickBlackBox box;
        private IDomainEvaluator evaluator;

        public BackpropAlgorithm()
        {
            LEEAParams.DOMAIN.generateSampleList();
            evaluator = LEEAParams.DOMAIN.createEvaluator();

            // initialize the network, use the QEAGenome class since it's already structured well for this task
            int layers = LEEAParams.DOMAIN.getHiddenLayers() + 1; // layers needs to be the number of layers of connections
            int[] layerNodes = new int[layers + 1];

            layerNodes[0] = LEEAParams.DOMAIN.getInputs() + 1; // +1 for the bias!
            for (int i = 0; i < layers - 1; i++)
                layerNodes[i + 1] = LEEAParams.DOMAIN.getHiddenLayerNeurons(i + 1);

            layerNodes[layers] = LEEAParams.DOMAIN.getOutputs();

            network = new QEAGenome(layers, layerNodes, LEEAParams.BPINITIALWEIGHTMAX);
            box = new QuickBlackBox(network);
        }

        public void run()
        {
            // evaluate the network once first
            FitnessInfo fit = evaluator.Evaluate(box);

            // print stats            
            System.IO.File.AppendAllText("backprop.txt", fit._auxFitnessArr[0]._value + Environment.NewLine);

            LEEAParams.DOMAIN.generateSampleList();
            List<Sample> fullSampleList = LEEAParams.DOMAIN.getSamples();           

            // for rms prop with connection cache method
            double[][][] connectionCache = new double[network.weights.Length][][]; 
           
            // for rms prop with connection queue method
            double[][][][] connectionQueue = new double[network.weights.Length][][][];
            int connectionQueueIndex = 0;
            double[][][] connectionQueueAverages = new double[network.weights.Length][][]; // for efficient way of maintaining averages

            if (LEEAParams.RMSPROP)
            {
                if (LEEAParams.RMSCONNECTIONQUEUE)
                {
                    for (int i = 0; i < network.weights.Length; i++)
                    {
                        connectionQueue[i] = new double[network.weights[i].Length][][];
                        connectionQueueAverages[i] = new double[network.weights[i].Length][];
                        for (int j = 0; j < network.weights[i].Length; j++)
                        {
                            connectionQueue[i][j] = new double[network.weights[i][j].Length][];
                            connectionQueueAverages[i][j] = new double[network.weights[i][j].Length];
                            for (int k = 0; k < network.weights[i][j].Length; k++)
                                connectionQueue[i][j][k] = new double[LEEAParams.RMSPROPK];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < network.weights.Length; i++)
                    {
                        connectionCache[i] = new double[network.weights[i].Length][];
                        for (int j = 0; j < network.weights[i].Length; j++)
                        {
                            connectionCache[i][j] = new double[network.weights[i][j].Length];
                        }
                    }
                }                
            }

            for (int epoch = 0; epoch < LEEAParams.BPMAXEPOCHS; epoch++)
            {
                // generate a random order to evaluate the samples for each epoch
                List<int> sampleOrder = new List<int>(fullSampleList.Count);
                for (int i = 0; i < fullSampleList.Count; i++)
                    sampleOrder.Add(i);
                Random rng = new Random();
                sampleOrder = sampleOrder.OrderBy(a => rng.Next()).ToList();

                // go through each sample
                for (int i = 0; i < fullSampleList.Count; i++)
                {
                    int index = sampleOrder[i];

                    // activate the network
                    ISignalArray inputArr = box.InputSignalArray;
                    ISignalArray outputArr = box.OutputSignalArray;
                    inputArr.CopyFrom(fullSampleList[index].Inputs, 0);
                    box.ResetState();
                    box.Activate();

                    // calculate error
                    double[][] errors = new double[box.nodeActivation.Length][];
                    for(int j = 0; j < errors.Length; j++)
                    {
                        errors[j] = new double[box.nodeActivation[j].Length];
                    }

                    // output nodes
                    for (int j = 0; j < outputArr.Length; j++)
                    {
                        // calculate error for this output node
                        double error = (fullSampleList[index].Outputs[j] - outputArr[j]) * outputArr[j] * (1 - outputArr[j]);
                        
                        errors[errors.Length - 1][j] = error;
                    }

                    // hidden nodes
                    for (int j = errors.Length - 2; j > 0; j--) // layer by layer, counting backwards, don't need j=0 since that is the input layer
                    {
                        for (int k = 0; k < errors[j].Length; k++) // each node in the layer
                        {
                            double error = 0;
                            for (int l = 0; l < errors[j + 1].Length; l++) // each node in the upper layer it is connected to
                            {
                                error += box.nodeActivation[j][k] * (1 - box.nodeActivation[j][k]) * errors[j + 1][l] * network.weights[j][l][k];
                            }

                            errors[j][k] = error;
                        }
                    }

                    // update weights
                    for (int j = 0; j < network.weights.Length; j++)
                    {
                        for (int k = 0; k < network.weights[j].Length; k++)
                        {
                            for (int l = 0; l < network.weights[j][k].Length; l++)
                            {
                                if (LEEAParams.RMSPROP && LEEAParams.PLAINR)
                                {
                                    int mult = 1;
                                    if (box.nodeActivation[j][l] * errors[j + 1][k] < 0)
                                        mult = -1;
                                    network.weights[j][k][l] += LEEAParams.BPLEARNINGRATE * mult;
                                }
                                else if (LEEAParams.RMSPROP && LEEAParams.RMSCONNECTIONQUEUE)
                                {
                                    // get the magnitude of recent gradient for this connection
                                    double rms = connectionQueueAverages[j][k][l];                                                                                                            

                                    // add the current gradient to the magnitudes queue and update the averages array                                    
                                    double prev = connectionQueue[j][k][l][connectionQueueIndex];
                                    
                                    double dx = box.nodeActivation[j][l] * errors[j + 1][k];                                    
                                    connectionQueue[j][k][l][connectionQueueIndex] = dx * dx;

                                    connectionQueueAverages[j][k][l] = ((connectionQueueAverages[j][k][l] * connectionQueue[j][k][l].Length) + (connectionQueue[j][k][l][connectionQueueIndex] - prev)) / connectionQueue[j][k][l].Length;

                                    // update the weight based on the average recent gradient
                                    double delta = dx * LEEAParams.BPLEARNINGRATE;
                                    if (rms != 0)
                                    {
                                        rms = Math.Sqrt(rms + .00000001);
                                        delta /= rms;
                                    }
                                    network.weights[j][k][l] += delta;
                                }
                                else if (LEEAParams.RMSPROP)
                                {
                                    double dx = box.nodeActivation[j][l] * errors[j + 1][k];
                                    connectionCache[j][k][l] = (1 - LEEAParams.RMSCONNECTIONDECAY) * connectionCache[j][k][l] + LEEAParams.RMSCONNECTIONDECAY * dx * dx;
                                    network.weights[j][k][l] += LEEAParams.BPLEARNINGRATE * dx / Math.Sqrt(connectionCache[j][k][l] + .00000001);
                                }
                                else
                                {
                                    float delta = (float)(box.nodeActivation[j][l] * errors[j + 1][k] * LEEAParams.BPLEARNINGRATE);
                                    network.weights[j][k][l] += delta;
                                }
                            }
                        }
                    }

                    connectionQueueIndex++;
                    connectionQueueIndex = connectionQueueIndex % LEEAParams.RMSPROPK;
                }

                // decay the learning rate
                LEEAParams.BPLEARNINGRATE *= (1 - LEEAParams.BPLEARNINGRATEDECAY);                

                // print stats
                if (epoch % SharedParams.SharedParams.TRACKFITNESSSTRIDE == 0)
                {
                    // evaluate the network
                    fit = evaluator.Evaluate(box);

                    System.IO.File.AppendAllText("epoch.txt", System.DateTime.Now + ":" + epoch + Environment.NewLine);
                    System.IO.File.AppendAllText("training.txt", fit._fitness + Environment.NewLine);
                    System.IO.File.AppendAllText("validation.txt", fit._auxFitnessArr[0]._value + Environment.NewLine);

                    fit = evaluator.Test(box);
                    System.IO.File.AppendAllText("test.txt", fit._fitness + Environment.NewLine);
                }
            }
        }
    }
}
