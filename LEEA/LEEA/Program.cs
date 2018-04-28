using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LEEA
{
    class Program
    {
        static void Main(string[] args)
        {
            // check for params file
            if (File.Exists("params.txt"))
            {
                string line;
                StreamReader sr = new StreamReader("params.txt");
                while ((line = sr.ReadLine()) != null)
                {
                    string[] words = line.Split(' ');
                    int num;
                    double doub;
                    switch (words[0])
                    {     
                        case "DOMAIN":
                            if (words[1] == "FuncApp")                            
                                LEEAParams.DOMAIN = new FuncAppDomain(Function.complex2dsine);
                            if (words[1] == "TimeSeries")
                                LEEAParams.DOMAIN = new FuncAppDomain(Function.timeseries);
                            if (words[1] == "CaliHouse")
                                LEEAParams.DOMAIN = new CaliHouseDomain();
                            break;
                        case "MAXGENERATIONS":
                            if(Int32.TryParse(words[1], out num))
                                LEEAParams.MAXGENERATIONS = num;
                            break;
                        case "POPSIZE":                            
                            if (Int32.TryParse(words[1], out num))
                                LEEAParams.POPSIZE = num;
                            break;
                        case "EVALTYPE":
                            if (words[1] == "LEEA")
                            {
                                LEEAParams.EVALTYPE = LEEAParams.EvalType.FixedNumber;
                                LEEAParams.SAMPLESDIVERSE = true;
                            }
                            if (words[1] == "SS")
                                LEEAParams.EVALTYPE = LEEAParams.EvalType.SingleSample;
                            if (words[1] == "FULL")
                                LEEAParams.EVALTYPE = LEEAParams.EvalType.FullSampleList;
                            if (words[1] == "SGD")
                            {
                                LEEAParams.EVALTYPE = LEEAParams.EvalType.Backprop;
                                LEEAParams.RMSPROP = false;
                            }
                            if (words[1] == "RMS")
                            {
                                LEEAParams.EVALTYPE = LEEAParams.EvalType.Backprop;
                                LEEAParams.RMSPROP = true;
                            }
                            break;
                        case "SAMPLECOUNT":                            
                            if (Int32.TryParse(words[1], out num))
                                LEEAParams.SAMPLECOUNT = num;
                            break;
                        case "MUTATIONPOWER":
                            if (Double.TryParse(words[1], out doub))
                                LEEAParams.MUTATIONPOWER = doub;
                            break;
                        case "MUTATIONPOWERDECAY":
                            if (Double.TryParse(words[1], out doub))
                                LEEAParams.MUTATIONPOWERDECAY = doub;
                            break;
                        case "MUTATIONRATE":
                            if (Double.TryParse(words[1], out doub))
                                LEEAParams.MUTATIONRATE = doub;
                            break;
                        case "MUTATIONRATEDECAY":
                            if (Double.TryParse(words[1], out doub))
                                LEEAParams.MUTATIONRATEDECAY = doub;
                            break;
                        case "SEXPROPORTION":
                            if (Double.TryParse(words[1], out doub))
                                LEEAParams.SEXPROPORTION = doub;
                            break;
                        case "SELECTIONPROPORTION":
                            if (Double.TryParse(words[1], out doub))
                                LEEAParams.SELECTIONPROPORTION = doub;
                            break;
                        case "SPECIESCOUNT":
                            if (Int32.TryParse(words[1], out num))
                                LEEAParams.SPECIESCOUNT = num;
                            break;
                        case "TRACKFITNESSSTRIDE":
                            if (Int32.TryParse(words[1], out num))
                                SharedParams.SharedParams.TRACKFITNESSSTRIDE = num;
                            break;
                        case "USEFITNESSBANK":
                            if (words[1] == "TRUE")
                                SharedParams.SharedParams.USEFITNESSBANK = true;
                            else
                                SharedParams.SharedParams.USEFITNESSBANK = false;
                            break;
                        case "FITNESSBANKDECAYRATE":
                            if (Double.TryParse(words[1], out doub))
                                SharedParams.SharedParams.FITNESSBANKDECAYRATE = doub;
                            break;
                        case "BPLEARNINGRATE":
                            if (Double.TryParse(words[1], out doub))
                                LEEAParams.BPLEARNINGRATE = doub;
                            break;
                        case "BPLEARNINGRATEDECAY":
                            if (Double.TryParse(words[1], out doub))
                                LEEAParams.BPLEARNINGRATEDECAY = doub;
                            break;
                        case "RMSCONNECTIONQUEUE":
                            if (words[1] == "TRUE")
                                LEEAParams.RMSCONNECTIONQUEUE = true;
                            else
                                LEEAParams.RMSCONNECTIONQUEUE = false;
                            break;                            
                        case "RMSPROPK":
                            if (Int32.TryParse(words[1], out num))
                                LEEAParams.RMSPROPK = num;
                            break;
                        case "RMSCONNECTIONDECAY":
                            if (Double.TryParse(words[1], out doub))
                                LEEAParams.RMSCONNECTIONDECAY = doub;
                            break;
                        case "BPMAXEPOCHS":
                            if (Int32.TryParse(words[1], out num))
                                LEEAParams.BPMAXEPOCHS = num;
                            break;
                        case "THREADS":
                            if (Int32.TryParse(words[1], out num))
                                LEEAParams.THREADS = num;
                            break;
                    }
                }
            }

            for (int i = 0; i < LEEAParams.RUNS; i++)
            {
                LEEAExperiment pe = new LEEAExperiment();
                pe.runExperiment();
            }            
        }        
    }
}
