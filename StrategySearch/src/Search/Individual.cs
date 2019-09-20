using System;

namespace StrategySearch.Search
{
   class Individual
   {
      public int ID { get; set; }
      
      public double Fitness { get; set; }
      public double[] ParamVector { get; set; }

      public Individual(int numParams)
      {
         ParamVector = new double[numParams];
      }
   }
}
