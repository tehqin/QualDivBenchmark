using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;

using StrategySearch.Config;
using StrategySearch.NeuralNet;
using StrategySearch.Search;
using StrategySearch.Search.CMA_ES;
using StrategySearch.Search.CMA_ME;
using StrategySearch.Search.MapElites;

namespace StrategySearch
{
   class Program
   {
      static double boundaryValue = 5.12;

      static double eval_sphere(double[] vs)
      {
         double sum = 0;
         foreach (double x in vs)
         {
            double v = x - boundaryValue * 0.4;
            sum += v * v;
         }
         return -sum;
      }

      static double eval_rastrigin(double[] vs)
      {
         double A = 10;
         double left = A * vs.Length;
         double right = 0;
         foreach (double x in vs)
         {
            double v = x - boundaryValue * 0.4;
            right += v * v - A * Math.Cos(2 * Math.PI * v);
         }
         return -(left+right);
      }

      static double evaluate(double[] vs)
      {
         return eval_sphere(vs);
      }

      static double evaluate_nn(double[] vs, int[] layers)
      {
         int numInputs = layers[0]-1;
         var network = new FullyConnectedNetwork(layers);
         network.SetWeights(vs);

         int count = 0;
         double result = 0;
         for (int mask=0; mask<(1<<numInputs); mask++)
         {
            var inputVector = new double[numInputs+1];
            int sigNum = 1;
            for (int i=0; i<numInputs; i++)
               sigNum *= (mask&(1<<i)) > 0 ? 1 : -1;
            for (int i=0; i<numInputs; i++)
               inputVector[i] = (mask&(1<<i)) > 0 ? 1 : -1;
            inputVector[numInputs] = 1;

            double cur = network.Evaluate(inputVector)[0]; 
            result += cur;
            count++;
         }
         return result / count;
      }

      static void run_cma_es(int numGenerations, int numParents, int populationSize, double mutationRate)
      {
         var _params = new CMA_ES_Params();
         _params.PopulationSize = populationSize;
         _params.NumToEvaluate = populationSize * numGenerations;
         _params.NumParents = numParents;
         _params.MutationPower = mutationRate;
         
         int numParams = 100;

         /*
         int[] layers = new int[]{10, 5, 4, 1};
         int numParams = 0;
         for (int i=0; i<layers.Length-1; i++)
            numParams += layers[i]*layers[i+1];
         */
         
         Console.WriteLine(numParams);
        
         
         SearchAlgorithm search = new CMA_ES_Algorithm(_params, numParams);
         while (search.IsRunning())
         {
            Individual cur = search.GenerateIndividual();
            //cur.Fitness = evaluate_nn(cur.ParamVector, layers);
            cur.Fitness = evaluate(cur.ParamVector);

            search.ReturnEvaluatedIndividual(cur);
         }
      }

      static void run_me()
      {
         int numParams = 100;
         Console.WriteLine(numParams);
         
         var searchParams = new MapElitesSearchParams();
         searchParams.InitialPopulation = 100;
         searchParams.NumToEvaluate = 50000;
         searchParams.MutationPower = 0.8;

         var feature1 = new FeatureParams();
         feature1.Name = "Sum1";
         feature1.MinValue = -(numParams * boundaryValue) / 2.0;
         feature1.MaxValue = (numParams * boundaryValue) / 2.0;
         Console.WriteLine(feature1.MinValue);
         Console.WriteLine(feature1.MaxValue);
         
         var feature2 = new FeatureParams();
         feature2.Name = "Sum2";
         feature2.MinValue = -(numParams * boundaryValue) / 2.0;
         feature2.MaxValue = (numParams * boundaryValue) / 2.0;
 
         var mapParams = new MapParams();
         mapParams.Type = "FixedFeature";
         mapParams.StartSize = 80;
         mapParams.EndSize = 80;
         mapParams.Features = new FeatureParams[]{feature1, feature2};
        
         var meParams = new MapElitesParams();
         meParams.Search = searchParams;
         meParams.Map = mapParams;
        
         SearchAlgorithm search = new MapElitesAlgorithm(meParams, numParams);
         while (search.IsRunning())
         {
            Individual cur = search.GenerateIndividual();
            cur.Fitness = evaluate(cur.ParamVector);
            search.ReturnEvaluatedIndividual(cur);
         }
      }

      static void run_cma_me()
      {
         int numParams = 100;
         Console.WriteLine(numParams);
         
         var searchParams = new CMA_ME_SearchParams();
         searchParams.NumToEvaluate = 50000;

         var feature1 = new FeatureParams();
         feature1.Name = "Sum1";
         feature1.MinValue = -(numParams * boundaryValue) / 2.0;
         feature1.MaxValue = (numParams * boundaryValue) / 2.0;
         
         var feature2 = new FeatureParams();
         feature2.Name = "Sum2";
         feature2.MinValue = -(numParams * boundaryValue) / 2.0;
         feature2.MaxValue = (numParams * boundaryValue) / 2.0;
 
         var mapParams = new MapParams();
         mapParams.Type = "FixedFeature";
         mapParams.StartSize = 80;
         mapParams.EndSize = 80;
         mapParams.Features = new FeatureParams[]{feature1, feature2};
        
         var impEmitter = new EmitterParams();
         impEmitter.Type = "Improvement";
         impEmitter.Count = 1;
         impEmitter.PopulationSize = 37;
         impEmitter.MutationPower = 0.8;
         
         var meParams = new CMA_ME_Params();
         meParams.Search = searchParams;
         meParams.Map = mapParams;
         meParams.Emitters = new EmitterParams[]{ impEmitter };
        
         double best = Double.MinValue;
         SearchAlgorithm search = new CMA_ME_Algorithm(meParams, numParams);
         while (search.IsRunning())
         {
            Individual cur = search.GenerateIndividual();
            cur.Fitness = evaluate(cur.ParamVector);
            best = Math.Max(best, cur.Fitness);
            search.ReturnEvaluatedIndividual(cur);
         }
         Console.WriteLine(best);
      }


      static void Main(string[] args)
      {
         //run_cma_es(500, 50, 100, 0.8);
         //run_cma_me();
         run_me();
      }
   }
}
