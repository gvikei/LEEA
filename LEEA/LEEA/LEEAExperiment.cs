using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LEEA
{
    class LEEAExperiment
    {
        private IDomain domain;

        public LEEAExperiment()
        {
            this.domain = LEEAParams.DOMAIN;
        }

        public void runExperiment()
        {
            if (LEEAParams.EVALTYPE == LEEAParams.EvalType.Backprop)
            {
                BackpropAlgorithm bpa = new BackpropAlgorithm();
                bpa.run();
            }
            else
            {                
                QuickEvolutionAlgorithm qea = new QuickEvolutionAlgorithm(LEEAParams.DOMAIN.createEvaluator());
                qea.run();                
            }
        }        
    }
}
