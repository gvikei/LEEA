using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LEEA
{
    enum Function
    {
        xsquared,
        complexsine,
        complex2dsine,
        timeseries
    }

    class FuncAppDomain : IDomain
    {
        private List<Sample> sampleList;
        private List<Sample> allSampleList;
        private List<Sample> testSampleList;
        private List<Sample> validationSampleList;
        private List<Sample> trainingSampleList;

        private Function function;

        public FuncAppDomain(Function function = Function.timeseries)
        {
            double stepsize = 0.05;
            this.function = function;
            allSampleList = new List<Sample>();
            if (function == Function.timeseries)
            {            
                // generate the series values
                double[] seriesValues = new double[2424];
                for (int i = 0; i < seriesValues.Length; i++)
                {
                    int r = 2;

                    if (i < r)
                        seriesValues[i] = 1.1 + 0.1 * i;
                    else
                    {
                        double y = 1;
                        double b = 2;
                        double n = 9.65;
                        double xr = seriesValues[i - r];
                        double x = seriesValues[i - 1];

                        seriesValues[i] = x + (b * xr / (1 + Math.Pow(xr, n)) - y * x);
                    }
                }

                // normalize the values to 0...1
                double min = seriesValues.Min();
                double max = seriesValues.Max();
                double diff = max - min;

                for (int i = 0; i < seriesValues.Length; i++)
                {
                    seriesValues[i] = (seriesValues[i] - min) / diff;
                }

                // from the series values, generate the samples (x-24, x-18, x-12, x-6) => (x)
                for (int i = 24; i < seriesValues.Length; i++)
                {
                    allSampleList.Add(new Sample(new double[] { seriesValues[i - 24], seriesValues[i - 18], seriesValues[i - 12], seriesValues[i - 6] }, new double[] { seriesValues[i] }));
                }
            }
            else
            {
                for (int i = (int)Math.Round(-1 / stepsize); i < 1 / stepsize; i++)
                    if (function == Function.xsquared || function == Function.complexsine)
                        allSampleList.Add(new Sample(new double[] { i * stepsize }, new double[] { getFunctionOutput(new double[] { i * stepsize }) }));
                    else if (function != Function.timeseries)
                    {
                        for (int j = (int)Math.Round(-1 / stepsize); j < 1 / stepsize; j++)
                        {
                            allSampleList.Add(new Sample(new double[] { i * stepsize, j * stepsize }, new double[] { getFunctionOutput(new double[] { i * stepsize, j * stepsize }) }));
                        }
                    }
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

        #region Functions
        public double getFunctionOutput(double[] input)
        {
            switch (function)
            {
                case (Function.xsquared):
                    return functionXSquared(input[0]);
                case (Function.complexsine):
                    return functionComplexSine(input[0]);
                case (Function.complex2dsine):
                    return functionComplex2DSine(input[0], input[1]);
                default:
                    return 0;
            }
        }

        public double functionXSquared(double input)
        {
            return input * input;
        }

        public double functionComplexSine(double input)
        {
            return (Math.Sin(input * (3 * input + 1) * 5) + 1) / 2;
        }

        public double functionComplex2DSine(double x, double y)
        {
            return (Math.Sin(x * (3 * y + 1) * 5) + 1) / 2;
        }
        
        #endregion

        public int getInputs()
        {
            switch (function)
            {
                case (Function.xsquared):
                    return 1;
                case (Function.complexsine):
                    return 1;
                case (Function.complex2dsine):
                    return 2;
                case (Function.timeseries):
                    return 4;
                default:
                    return 0;
            }
        }

        public int getOutputs()
        {
            return 1;
        }

        public int getHiddenLayers()
        {
            switch (function)
            {
                case (Function.xsquared):
                    return 1;
                case (Function.complexsine):
                    return 1;
                case(Function.complex2dsine):
                    return 2;
                case (Function.timeseries):
                    return 2;
                default:
                    return 0;
            }
        }

        public int getHiddenLayerNeurons(int layer)
        {
            switch (function)
            {
                case (Function.xsquared):
                    if (layer > 1)
                        return 0;
                    else
                        return 4;
                case (Function.complexsine):
                    if (layer > 1)
                        return 0;
                    else
                        return 10;
                case (Function.complex2dsine):
                    if (layer > 1)
                        return 20;
                    else
                        return 50;
                case (Function.timeseries):
                    if (layer > 1)
                        return 20;
                    else
                        return 50;
                default:
                    return 0;
            }
            
        }

        public IDomainEvaluator createEvaluator()
        {
            return new FuncAppEvaluator();
        }

        public List<Sample> getSamples()
        {
            return sampleList;
        }

        public List<Sample> getAllSamples()
        {
            return allSampleList;
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
            switch(LEEAParams.EVALTYPE) 
            {
                case LEEAParams.EvalType.FullSampleList:
                case LEEAParams.EvalType.Backprop:
                    sampleList = trainingSampleList;                  
                    break;
                case LEEAParams.EvalType.SingleSample:
                    sampleList = new List<Sample>();
                    int index = r.Next(trainingSampleList.Count);
                    sampleList.Add(trainingSampleList[index]);
                    break;                    
                case LEEAParams.EvalType.FixedNumber:
                    if (LEEAParams.SAMPLESDIVERSE)
                    {
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
                        sampleList = new List<Sample>();
                        List<int> indicesSelected = new List<int>();
                        while (sampleList.Count < LEEAParams.SAMPLECOUNT)
                        {
                            index = r.Next(trainingSampleList.Count);                            

                            if (!indicesSelected.Contains(index))
                            {
                                indicesSelected.Add(index);
                                sampleList.Add(trainingSampleList[index]);
                            }
                        }
                    }
                    break;
            }            
        }
    }
}
