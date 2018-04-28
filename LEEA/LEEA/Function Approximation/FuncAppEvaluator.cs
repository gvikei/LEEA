using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes.NeuralNets;
using SharpNeat.Network;
using SharpNeat.Core;
using SharpNeat.Phenomes;

namespace LEEA
{
    class FuncAppEvaluator : IDomainEvaluator
    {
        ulong _evalCount;
        bool _stopConditionSatisfied;

        #region IPhenomeEvaluator<IBlackBox> Members

        /// <summary>
        /// Gets the total number of evaluations that have been performed.
        /// </summary>
        public ulong EvaluationCount
        {
            get { return _evalCount; }
        }

        /// <summary>
        /// Gets a value indicating whether some goal fitness has been achieved and that
        /// the the evolutionary algorithm/search should stop. This property's value can remain false
        /// to allow the algorithm to run indefinitely.
        /// </summary>
        public bool StopConditionSatisfied
        {
            get { return _stopConditionSatisfied; }
        }

        /// <summary>
        /// Evaluate the provided IBlackBox against the MNIST problem domain and return its fitness/novlety score.
        /// </summary>
        public FitnessInfo Evaluate(IBlackBox box)
        {
            //just keep track of evals
            //_evalCount++;

            double fitness, altFitness;

            //these are our inputs and outputs
            ISignalArray inputArr = box.InputSignalArray;
            ISignalArray outputArr = box.OutputSignalArray;

            List<Sample> sampleList = LEEAParams.DOMAIN.getSamples();
            int sampleCount = sampleList.Count;
            fitness = sampleList.Count;
            
            foreach (Sample sample in sampleList)
            {
                inputArr.CopyFrom(sample.Inputs, 0);
                box.ResetState();
                box.Activate();

                fitness -= Math.Abs(outputArr[0] - sample.Outputs[0]) * Math.Abs(outputArr[0] - sample.Outputs[0]);               
            }
            
            // set alternate fitness to fitness measured against all samples
            if (SharedParams.SharedParams.PERFORMFULLEVAL)
            {
                sampleList = LEEAParams.DOMAIN.getValidationSamples();
                altFitness = sampleList.Count;

                foreach (Sample sample in sampleList)
                {
                    inputArr.CopyFrom(sample.Inputs, 0);
                    box.ResetState();
                    box.Activate();

                    altFitness -= Math.Abs(outputArr[0] - sample.Outputs[0]) * Math.Abs(outputArr[0] - sample.Outputs[0]);
                }
            }
            else
                altFitness = 0;

            fitness = Math.Max(fitness, LEEAParams.MINFITNESS) + LEEAParams.FITNESSBASE;
            return new FitnessInfo(fitness, altFitness);
        }

        public FitnessInfo Test(IBlackBox box)
        {
            double fitness, altFitness;

            //these are our inputs and outputs
            ISignalArray inputArr = box.InputSignalArray;
            ISignalArray outputArr = box.OutputSignalArray;

            List<Sample> sampleList = LEEAParams.DOMAIN.getTestSamples();
            int sampleCount = sampleList.Count;
            fitness = sampleList.Count;

            foreach (Sample sample in sampleList)
            {
                inputArr.CopyFrom(sample.Inputs, 0);
                box.ResetState();
                box.Activate();

                fitness -= Math.Abs(outputArr[0] - sample.Outputs[0]) * Math.Abs(outputArr[0] - sample.Outputs[0]);
            }

            fitness = Math.Max(fitness, LEEAParams.MINFITNESS) + LEEAParams.FITNESSBASE;
            return new FitnessInfo(fitness, 0);
        }

        /// <summary>
        /// Reset the internal state of the evaluation scheme if any exists.
        /// Note. The XOR problem domain has no internal state. This method does nothing.
        /// </summary>
        public void Reset()
        {

        }

        public void NewGeneration()
        {
            LEEAParams.DOMAIN.generateSampleList();
        }

        #endregion
    }
}
