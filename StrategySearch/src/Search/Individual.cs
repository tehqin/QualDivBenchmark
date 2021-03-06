using System;

namespace StrategySearch.Search
{
   class Individual
   {
      public int ID { get; set; }
      public int EmitterID { get; set; }
      public int Generation { get; set; }
      
      public double Fitness { get; set; }
      public double NormFitness { get; set; }
      public double Delta { get; set; }
      public bool IsNovel { get; set; }
      public double[] ParamVector { get; set; }
      public double[] Features { get; set; }

      public Individual(int numParams)
      {
         ParamVector = new double[numParams];
         IsNovel = false;
      }

      public double Reduce(double v)
      {
         if (Math.Abs(v) > 5.12)
            return 5.12 / v;
         return v;
      }

      public double GetStatByName(string featureName)
      {
         if (featureName == "Sum1")
         {
            double sum = 0;
            int half = ParamVector.Length/2;
            for (int i=0; i<half; i++)
               sum += Reduce(ParamVector[i]);
            return sum;
         }
         
         if (featureName == "Sum2")
         {
            double sum = 0;
            int half = ParamVector.Length/2;
            for (int i=half; i<ParamVector.Length; i++)
               sum += Reduce(ParamVector[i]);
            return sum;
         }

         Console.WriteLine("Unspecified feature name.");
         return 0.0;
      }
   }
}
