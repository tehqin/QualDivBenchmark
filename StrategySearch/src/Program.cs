using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

using Nett;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;

using StrategySearch.Config;
using StrategySearch.Logging;
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

      static double evaluate(int i, double[] vs)
      {
         return i == 0 ? eval_sphere(vs) : eval_rastrigin(vs);
      }

      static void run_me()
      {
         int numParams = 20;
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
        
         SearchAlgorithm search = new MapElitesAlgorithm(0, meParams, numParams);
         while (search.IsRunning())
         {
            Individual cur = search.GenerateIndividual();
            cur.Fitness = evaluate(0, cur.ParamVector);
            search.ReturnEvaluatedIndividual(cur);
         }
      }

      static void run_cma_me()
      {
         int numParams = 30;
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
         impEmitter.Count = 5;
         impEmitter.OverflowFactor = 1.10;
         impEmitter.PopulationSize = 37;
         impEmitter.MutationPower = 0.8;
         
         var optEmitter = new EmitterParams();
         optEmitter.Type = "Optimizing";
         optEmitter.Count = 5;
         optEmitter.OverflowFactor = 1.10;
         optEmitter.PopulationSize = 37;
         optEmitter.NumParents = impEmitter.PopulationSize / 2;
         optEmitter.MutationPower = 0.8;
         
         var rdEmitter = new EmitterParams();
         rdEmitter.Type = "RandomDirection";
         rdEmitter.Count = 5;
         rdEmitter.OverflowFactor = 1.10;
         rdEmitter.PopulationSize = 37;
         rdEmitter.MutationPower = 0.8;
 
         var meParams = new CMA_ME_Params();
         meParams.Search = searchParams;
         meParams.Map = mapParams;
         meParams.Emitters = new EmitterParams[]{ impEmitter, optEmitter };
        
         double best = Double.MinValue;
         SearchAlgorithm search = new CMA_ME_Algorithm(0, meParams, numParams);
         while (search.IsRunning())
         {
            Individual cur = search.GenerateIndividual();
            cur.Fitness = evaluate(0, cur.ParamVector);
            best = Math.Max(best, cur.Fitness);
            search.ReturnEvaluatedIndividual(cur);
         }
         Console.WriteLine(best);
      }

      static void run_me_tuning(int id, double sigma)
      {
         int numParams = 20;
         
         var searchParams = new MapElitesSearchParams();
         searchParams.InitialPopulation = 100;
         searchParams.NumToEvaluate = 50000;
         searchParams.MutationPower = sigma;

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
         mapParams.StartSize = 100;
         mapParams.EndSize = 100;
         mapParams.Features = new FeatureParams[]{feature1, feature2};
        
         var meParams = new MapElitesParams();
         meParams.Search = searchParams;
         meParams.Map = mapParams;
        
         double maxValue = Double.MinValue;
         SearchAlgorithm search = new MapElitesAlgorithm(0, meParams, numParams);
         while (search.IsRunning())
         {
            Individual cur = search.GenerateIndividual();
            cur.Fitness = evaluate(0, cur.ParamVector);
            maxValue = Math.Max(cur.Fitness, maxValue);
            search.ReturnEvaluatedIndividual(cur);
         }
      }

      static void run_tuning(double lowSigma, double highSigma)
      {
         int numIters = 101;
         for (int i=0; i<numIters; i++)
         {
            double portion = i / (numIters-1.0);
            double sigma = (highSigma-lowSigma) * portion + lowSigma;
            run_me_tuning(i, sigma);
         }
      }


      static CMA_ES_Algorithm generate_cma_es(int trialID, SearchParams config, int numParams)
      {
         var searchConfig =
            Toml.ReadFile<CMA_ES_Params>(config.ConfigFilename);
         return new CMA_ES_Algorithm(trialID, searchConfig, numParams);
      }

      static MapElitesAlgorithm generate_map_elites(int trialID, SearchParams config, int numParams)
      {
         var searchConfig =
            Toml.ReadFile<MapElitesParams>(config.ConfigFilename);
         foreach (var feature in searchConfig.Map.Features)
         {
            feature.MinValue = -(numParams * boundaryValue) / 2.0;
            feature.MaxValue = (numParams * boundaryValue) / 2.0;
         }
         return new MapElitesAlgorithm(trialID, searchConfig, numParams);
      }

      static CMA_ME_Algorithm generate_cma_me(int trialID, SearchParams config, int numParams)
      {
         var searchConfig =
            Toml.ReadFile<CMA_ME_Params>(config.ConfigFilename);
         foreach (var feature in searchConfig.Map.Features)
         {
            feature.MinValue = -(numParams * boundaryValue) / 2.0;
            feature.MaxValue = (numParams * boundaryValue) / 2.0;
         }
         return new CMA_ME_Algorithm(trialID, searchConfig, numParams);
      }

      static SearchAlgorithm generate_search(int trialID, SearchParams config, int numParams)
      {
         if (config.Type == "CMA-ES")
            return generate_cma_es(trialID, config, numParams);
         if (config.Type == "CMA-ME")
            return generate_cma_me(trialID, config, numParams);
         if (config.Type == "MAP-Elites")
            return generate_map_elites(trialID, config, numParams);
         return null;
      }


      static void run_search(Configuration config)
      {
         int fid = config.FunctionType == "Rastrigin" ? 1 : 0;

         for (int trialID=0; trialID<config.NumTrials; trialID++)
         {
            Console.WriteLine("Starting search "+trialID);

            int individualCount = 0;
            string logFilepath = string.Format("logs/individuals_{0}.csv", trialID);
            RunningIndividualLog individualLog = new RunningIndividualLog(logFilepath);
            SearchAlgorithm algo = generate_search(trialID, config.Search, config.NumParams);
            while (algo.IsRunning())
            {
               Individual cur = algo.GenerateIndividual();
               cur.ID = individualCount++;
               cur.Fitness = evaluate(fid, cur.ParamVector);
               individualLog.LogIndividual(cur);
               algo.ReturnEvaluatedIndividual(cur);
            }
         }
      }

      static void Main(string[] args)
      {
         //run_tuning(0.1, 1.1);
         
         var config = Toml.ReadFile<Configuration>(args[0]);
         run_search(config);
      }
   }
}
