using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedParams
{
    public class SharedParams
    {
        public static bool TRACKFITNESS = true;
        public static int TRACKFITNESSSTRIDE = 5; // 1 will output fitness for every generation
        public static bool USEFITNESSBANK = true;
        public static double FITNESSBANKDECAYRATE = .2; // .1 = 10% decay per evaluation, 1 = effectively disabled

        public static bool PERFORMFULLEVAL = true; // perform a full evaluation and store that in alt fitness, this param will be overriden on generations where fitness tracking is not performed        
    }
}
