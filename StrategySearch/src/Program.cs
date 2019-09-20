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

namespace StrategySearch
{
   class Program
   {
      static double eval_sphere(double[] vs)
      {
         double sum = 0;
         foreach (double x in vs)
         {
            double v = x - 5.12 * 0.4;
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
            double v = x - 5.12 * 0.4;
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

      static void run_cma_es(int numGenerations, int numElites, int populationSize, double mutationRate)
      {
         var _params = new CMA_ES_Params();
         _params.PopulationSize = populationSize;
         _params.NumToEvaluate = populationSize * numGenerations;
         _params.NumElites = numElites;
         _params.MutationScalar = mutationRate;

         /*
         int[] layers = new int[]{10, 5, 4, 1};
         int numParams = 0;
         for (int i=0; i<layers.Length-1; i++)
            numParams += layers[i]*layers[i+1];
         */
         
         int numParams = 100;
         Console.WriteLine(numParams);
         double maxFitness = -100000000000000000.0;
         
         SearchAlgorithm search = new CMA_ES_Algorithm(_params, numParams);
         while (search.IsRunning())
         {
            Individual cur = search.GenerateIndividual();
            //cur.Fitness = evaluate_nn(cur.ParamVector, layers);
            cur.Fitness = evaluate(cur.ParamVector);
            maxFitness = Math.Max(maxFitness, cur.Fitness);

            search.ReturnEvaluatedIndividual(cur);
         }
      }

      static void Main(string[] args)
      {
         run_cma_es(200, 50, 100, 0.8);
         //run_cma_es(1, 1, 1, 1.0);
      }
   }
}
