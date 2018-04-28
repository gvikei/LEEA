using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LEEA
{
    public class Sample
    {
        public double[] Inputs { get; set; }
        public double[] Outputs { get; set; }

        public Sample(double[] inputs, double[] outputs)
        {
            Inputs = inputs;
            Outputs = outputs;
        }
    }
}
