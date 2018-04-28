using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LEEA
{
    public interface IDomain
    {
        int getInputs();
        int getOutputs();
        int getHiddenLayers();
        int getHiddenLayerNeurons(int layer);
        IDomainEvaluator createEvaluator();
        List<Sample> getSamples(); // gets the training samples for the current epoch/generation
        List<Sample> getAllSamples();
        List<Sample> getValidationSamples();
        List<Sample> getTestSamples();        
        void generateSampleList();
    }
}
