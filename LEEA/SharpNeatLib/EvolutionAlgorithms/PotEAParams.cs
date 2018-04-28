using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpNeat.EvolutionAlgorithms
{
    public class PotEAParams
    {
        public static bool SOLVABLEDOMAIN;
        public static int MAXGENERATIONS;
        public static double CHECKPOINTFITNESS;
        public static int FINALGENERATION;
        public static double FINALMAXFITNESS;
        public static List<double> MAXFITNESSLIST;
        public static List<double> AVGFITNESSLIST;
    }
}
