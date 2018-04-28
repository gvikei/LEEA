using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LEEA
{
    class CaliHouseDomain : IDomain
    {
        private List<Sample> sampleList;
        private List<Sample> allSampleList;
        private List<Sample> trainingSampleList;
        private List<Sample> testSampleList;
        private List<Sample> validationSampleList;

        public CaliHouseDomain()
        {
            allSampleList = new List<Sample>();
            var reader = new StreamReader(File.OpenRead(Environment.CurrentDirectory + "/../../../data/cal_housing.data"));
            
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] points = line.Split('-');

                double[] inputs = new double[8];
                double[] outputs = new double[1];

                
                // set the output for the digit in question
                for (int i = 0; i < points.Length; i++)
                {
                    double result;
                    string[] values = points[i].Split(',');
                    if (values.Length == 9)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            double.TryParse(values[j], out result);
                            inputs[j] = result;
                        }

                        double.TryParse(values[8], out result);
                        outputs[0] = result;

                        allSampleList.Add(new Sample(inputs, outputs));
                    }
                }
                         
            }

            // normalize the data
            double[] maxes = new double[9];
            double[] mins = new double[9];

            for (int i = 0; i < 9; i++)
            {
                mins[i] = double.MaxValue;
                maxes[i] = double.MinValue;
            }

            for (int i = 0; i < allSampleList.Count; i++)
            {
                for (int j = 0; j < allSampleList[i].Inputs.Length; j++)
                {
                    if (allSampleList[i].Inputs[j] > maxes[j])
                        maxes[j] = allSampleList[i].Inputs[j];
                    if (allSampleList[i].Inputs[j] < mins[j])
                        mins[j] = allSampleList[i].Inputs[j];
                }

                if (allSampleList[i].Outputs[0] > maxes[8])
                    maxes[8] = allSampleList[i].Outputs[0];
                if (allSampleList[i].Outputs[0] < mins[8])
                    mins[8] = allSampleList[i].Outputs[0];
            }

            for (int i = 0; i < allSampleList.Count; i++)
            {
                for (int j = 0; j < allSampleList[i].Inputs.Length; j++)
                {
                    double diff = maxes[j] - mins[j];
                    allSampleList[i].Inputs[j] = (allSampleList[i].Inputs[j] - mins[j]) / diff;
                }

                double difff = maxes[8] - mins[8];
                allSampleList[i].Outputs[0] = (allSampleList[i].Outputs[0] - mins[8]) / difff;
            }

            // generate training/test lists
            trainingSampleList = new List<Sample>();
            testSampleList = new List<Sample>();
            validationSampleList = new List<Sample>();

            for (int i = 0; i < allSampleList.Count && i < 20000; i++)
            {
                if (i % 2 == 0)
                    trainingSampleList.Add(allSampleList[i]);
                else
                    if (i % 4 == 1)
                        validationSampleList.Add(allSampleList[i]);
                    else
                        testSampleList.Add(allSampleList[i]);
            }
        }

        public int getInputs()
        {
            return 8;
        }

        public int getOutputs()
        {
            return 1;
        }

        public int getHiddenLayers()
        {
            return 2;
        }

        public int getHiddenLayerNeurons(int layer)
        {
            if (layer > 1)
                return 20;

            return 50;
        }

        public IDomainEvaluator createEvaluator()
        {
            return new CaliHouseEvaluator();
        }

        public List<Sample> getSamples()
        {
            return sampleList;            
        }

        public List<Sample> getAllSamples()
        {
            return allSampleList;            
        }

        public List<Sample> getTrainingSamples()
        {
            return trainingSampleList;
        }

        public List<Sample> getTestSamples()
        {
            return testSampleList;
        }

        public List<Sample> getValidationSamples()
        {
            return validationSampleList;
        }        

        public void generateSampleList()
        {
            Random r = new Random();
            switch (LEEAParams.EVALTYPE)
            {
                case LEEAParams.EvalType.FullSampleList:
                case LEEAParams.EvalType.Backprop:
                    sampleList = trainingSampleList;
                    break;
                case LEEAParams.EvalType.SingleSample:
                    sampleList = new List<Sample>();
                    sampleList.Add(trainingSampleList[r.Next(trainingSampleList.Count)]);
                    break;
                case LEEAParams.EvalType.FixedNumber:
                    if (LEEAParams.SAMPLESDIVERSE)
                    {
                        int index;
                        sampleList = new List<Sample>();
                        double[] caps = new double[LEEAParams.SAMPLECOUNT];
                        for (int i = 0; i < caps.Length; i++)
                            caps[i] = ((double)(i + 1)) / LEEAParams.SAMPLECOUNT;

                        int[] filled = new int[LEEAParams.SAMPLECOUNT];

                        while (filled.Sum() != LEEAParams.SAMPLECOUNT)
                        {
                            index = r.Next(trainingSampleList.Count);                            
                            double output = trainingSampleList[index].Outputs[0];
                            int location = 0;
                            while (output > caps[location])
                                location++;

                            if (filled[location] == 0)
                            {
                                sampleList.Add(trainingSampleList[index]);
                                filled[location] = 1;
                            }
                        }
                            
                    }
                    else
                    {
                        List<int> indicesAdded = new List<int>();
                        sampleList = new List<Sample>();
                        while (sampleList.Count < LEEAParams.SAMPLECOUNT)
                        {
                            int index = r.Next(trainingSampleList.Count);
                            if (!indicesAdded.Contains(index))
                            {
                                indicesAdded.Add(index);
                                sampleList.Add(trainingSampleList[index]);
                            }
                        }
                    }
                    break;
            }
        }
    }
}
