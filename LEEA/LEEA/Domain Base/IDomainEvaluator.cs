using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Core;
using SharpNeat.Phenomes;

namespace LEEA
{
    public interface IDomainEvaluator : IPhenomeEvaluator<IBlackBox>
    {
        FitnessInfo Test(IBlackBox box);
    }
}
