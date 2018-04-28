using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;
using SharpNeat.Network;

namespace LEEA
{
    class QuickBlackBox : IBlackBox
    {
        private QEAGenome genome;
        public QuickBlackBox(QEAGenome genome)
        {
            this.genome = genome;
            inputs = new double[InputCount];
            outputs = new double[OutputCount];
            _inputSignalArray = new SignalArray(inputs, 0, inputs.Length);
            _outputSignalArray = new SignalArray(outputs, 0, outputs.Length);
        }

        private double[] inputs;
        private double[] outputs;
        private SignalArray _inputSignalArray;
        private SignalArray _outputSignalArray;

        public double[][] nodeActivation;

        #region IBlackBox members
        /// <summary>
        /// Gets the number of inputs to the blackbox. This is assumed to be fixed for the lifetime of the IBlackBox.
        /// </summary>
        public int InputCount { get { return genome.weights[0][0].Length - 1; } }

        /// <summary>
        /// Gets the number of outputs from the blackbox. This is assumed to be fixed for the lifetime of the IBlackBox.
        /// </summary>
        public int OutputCount { get { return genome.weights[genome.weights.Length - 1].Length; } }

        /// <summary>
        /// Gets an array of input values that feed into the black box. 
        /// </summary>
        public ISignalArray InputSignalArray { get { return _inputSignalArray; } }

        /// <summary>
        /// Gets an array of output values that feed out from the black box. 
        /// </summary>
        public ISignalArray OutputSignalArray { get { return _outputSignalArray; } }

        /// <summary>
        /// Gets a value indicating whether the black box's internal state is valid. It may become invalid if e.g. we ask a recurrent
        /// neural network to relax and it is unable to do so.
        /// </summary>
        public bool IsStateValid { get { return true; } }

        /// <summary>
        /// Activate the black box. This is a request for the box to accept its inputs and produce output signals
        /// ready for reading from OutputSignalArray.
        /// </summary>
        public void Activate()
        {            
            nodeActivation = new double[genome.weights.Length + 1][];

            // copy input signal array to first layer of nodeActivation array
            nodeActivation[0] = new double[InputCount + 1]; // +1 for the bias
            _inputSignalArray.CopyTo(nodeActivation[0], 0);
            nodeActivation[0][nodeActivation[0].Length - 1] = 1; // bias
            
            // now build up node activation arrays, one layer at a time
            for (int i = 1; i < nodeActivation.Length; i++)
            {
                // calculate the sum for each target node
                nodeActivation[i] = new double[genome.weights[i - 1].Length];
                for (int j = 0; j < genome.weights[i - 1].Length; j++)
                {
                    for (int k = 0; k < nodeActivation[i - 1].Length; k++)
                        nodeActivation[i][j] += nodeActivation[i - 1][k] * genome.weights[i - 1][j][k];

                    // run the activation function - steepened sigmoid
                    //if (i < nodeActivation.Length - 1)
                    if(LEEAParams.EVALTYPE == LEEAParams.EvalType.Backprop)
                        nodeActivation[i][j] = PlainSigmoid.__DefaultInstance.Calculate(nodeActivation[i][j], null);
                    else
                        nodeActivation[i][j] = SteepenedSigmoid.__DefaultInstance.Calculate(nodeActivation[i][j], null);
                }                
            }

            // set the output array as the last layer of the nodeactivation array
            _outputSignalArray.CopyFrom(nodeActivation[nodeActivation.Length - 1], 0);
        }

        /// <summary>
        /// Reset any internal state.
        /// </summary>
        public void ResetState()
        {
            // not needed in this implementation as signals are completely overwritten
        }
        #endregion
    }
}
