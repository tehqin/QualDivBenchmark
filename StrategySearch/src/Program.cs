using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

      static MapElitesLineAlgorithm generate_map_elites_line(int trialID, SearchParams config, int numParams)
      {
         var searchConfig =
            Toml.ReadFile<MapElitesParams>(config.ConfigFilename);
         foreach (var feature in searchConfig.Map.Features)
         {
            feature.MinValue = -(numParams * boundaryValue) / 2.0;
            feature.MaxValue = (numParams * boundaryValue) / 2.0;
         }
         return new MapElitesLineAlgorithm(trialID, searchConfig, numParams);
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
         if (config.Type == "MAP-Elites-Line")
            return generate_map_elites_line(trialID, config, numParams);
         return null;
      }

      static void run_search(Configuration config, int trialID, int fid)
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

         Console.WriteLine("Ending search "+trialID);
      }

      static void run_search(Configuration config)
      {
         int fid = config.FunctionType == "Rastrigin" ? 1 : 0;

/*
         Parallel.For(0, config.NumTrials,
                   trialID => { run_search(config, trialID, fid); } 
               );
*/

         for (int trialID=0; trialID<config.NumTrials; trialID++)
         {
            run_search(config, trialID, fid);
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
