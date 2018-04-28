using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LEEA
{
    class LEEAParams
    {
        public enum EvalType
        {
            SingleSample,            
            FullSampleList,
            FixedNumber,
            Backprop
        }

        public static int RUNS = 1;
        public static int MAXGENERATIONS = 2000000;

        public static IDomain DOMAIN = new FuncAppDomain();
        public static int THREADS = 4;
        public static int POPSIZE = 1000;
        public static EvalType EVALTYPE = EvalType.Backprop;
        public static int SAMPLECOUNT = 2; // only used for EvalType = FixedNumber
        public static bool SAMPLESDIVERSE = true; // enforce some sort of criteria that ensures the samples viewed are somewhat diverse.  for classification tasks, this could mean having a sample from each class
        public static bool STATICNETWORK = true;

        // evolution params
        public static bool MUTATIONGAUSSIAN = false; // false = uniform, true = gaussian
        public static double MUTATIONPOWER = 0.03; // maximum size of mutation for uniform, sigma for gaussian
        public static double MUTATIONPOWERDECAY = 0.99; // amount to decay mutation power throughout the run, set to 0 to effective disable power decay, set to 1 to end the run with 0 mutation power.. power will be decayed gradually every generation
        public static double MUTATIONRATE = 0.04; // proportion of weights to mutate
        public static double MUTATIONRATEDECAY = 0; // amount to decay mutation rate throughout the run
        public static double SEXPROPORTION = 0.5; // proportion of offspring produced by sexual reproduction
        public static double SELECTIONPROPORTION = 0.4; // proportion of best individuals to select from
        public static int SPECIESCOUNT = 1; // number of species, set to 1 to effectively disable speciation

        // fitness params
        public static double MINFITNESS = 0.00001; //0.00001
        public static double FITNESSBASE = 0;

        // backprop params
        public static bool RMSPROP = false; // use RMSprop rather than SGD
        public static bool PLAINR = false; // use plain RProp instead of RMSprop
        public static int RMSPROPK = 250; // number of recent magnitudes to keep track of        
        public static bool RMSCONNECTIONQUEUE = true; // use RMSprop with connection queue (RMSPROPK used for queue length) rather than decay
        public static double RMSCONNECTIONDECAY = 0.01; // decay rate for RMSprop with decayed connection cache
        public static double BPINITIALWEIGHTMAX = 5; // max value initial weights may have
        public static int BPMAXEPOCHS = 5000; // maximum number of epochs
        public static double BPLEARNINGRATE = 0.3; // initial learning rate
        public static double BPLEARNINGRATEDECAY = 0.0001; // learning rate decay per epoch
    }
}
