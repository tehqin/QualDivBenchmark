namespace StrategySearch.NeuralNet
{
   interface Network
   {
      void SetWeights(double[] weightVector);
      double[] Evaluate(double[] input);
   }
}
