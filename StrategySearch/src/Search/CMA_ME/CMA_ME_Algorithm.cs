using System;
using System.Collections.Generic;

using StrategySearch.Config;
using StrategySearch.Logging;
using StrategySearch.Emitters;
using StrategySearch.Mapping;
using StrategySearch.Mapping.Sizers;

namespace StrategySearch.Search.CMA_ME
{
   class CMA_ME_Algorithm : SearchAlgorithm
   {
      public CMA_ME_Algorithm(int trialID, CMA_ME_Params searchParams, int numParams)
      {
         _params = searchParams;
      
         _individualsEvaluated = 0;
         _maxFitness = Double.MinValue;

         _trialID = trialID;
         initMap();
         
         // Create the population of emitters.
         _emitters = new List<Emitter>();
         foreach (EmitterParams ep in searchParams.Emitters)
         {
            // Make this many emitters of the given type.
            for (int i=0; i<ep.Count; i++)
            {
               if (ep.Type.Equals("Improvement"))
                  _emitters.Add(new ImprovementEmitter(ep, _featureMap, numParams));
               else if (ep.Type.Equals("Optimizing"))
                  _emitters.Add(new OptimizingEmitter(ep, _featureMap, numParams));
               else if (ep.Type.Equals("RandomDirection"))
                  _emitters.Add(new RandomDirectionEmitter(ep, _featureMap, numParams));
               else
                  Console.WriteLine("Emitter Not Found: "+ep.Type);
            }
         }
      }

      private CMA_ME_Params _params;

      private double _maxFitness;
      private List<Emitter> _emitters;
      private int _individualsEvaluated;
      private FeatureMap _featureMap;

      private int _trialID;
      private FrequentMapLog _map_log;
      private MapSummaryLog _summary_log;

      private void initMap()
      {
         var mapSizer = new LinearMapSizer(_params.Map.StartSize,
                                           _params.Map.EndSize);

         Console.WriteLine("Map Type: " + _params.Map.Type);
         if (_params.Map.Type.Equals("FixedFeature"))
            _featureMap = new FixedFeatureMap(_params.Search.NumToEvaluate,
                                              _params.Map, mapSizer);
         else if (_params.Map.Type.Equals("SlidingFeature"))
            _featureMap = new SlidingFeatureMap(_params.Search.NumToEvaluate,
                                                _params.Map, mapSizer);
         else
            Console.WriteLine("ERROR: No feature map specified in config file.");
      
         string emitterLabel = "none";
         if (_params.Emitters[0].Type.Equals("Improvement"))
            emitterLabel = "imp";
         else if (_params.Emitters[0].Type.Equals("Optimizing"))
            emitterLabel = "opt";
         else if (_params.Emitters[0].Type.Equals("RandomDirection"))
            emitterLabel = "rd";

         string prefix = "logs/";
         if (_params.Map.SeparateLoggingFolder)
            prefix = string.Format("logs/cma_me_{0}", emitterLabel);

         string mapName = string.Format("{0}/map_{1}.csv", prefix, _trialID);
         string summaryName = string.Format("{0}/summary_{1}.csv", prefix, _trialID);
         _map_log = new FrequentMapLog(mapName, _featureMap);
         _summary_log = new MapSummaryLog(summaryName, _featureMap);
      }

      public bool IsRunning() => _individualsEvaluated < _params.Search.NumToEvaluate;
      public bool IsBlocking()
      {
         foreach (Emitter em in _emitters)
            if (!em.IsBlocking())
               return false;
         return true;
      }

      public Individual GenerateIndividual()
      {
			int pos = 0;
			Emitter em = null;
         for (int i=0; i<_emitters.Count; i++)
         {
            if (!_emitters[i].IsBlocking())
            {
               if (em == null || em.NumReleased > _emitters[i].NumReleased)
               {
                  em = _emitters[i];
                  pos = i;
               }
            }
         }

         if (em == null)
            return null;

         Individual ind = em.GenerateIndividual();
         ind.EmitterID = pos;
         return ind;
      }

      public void ReturnEvaluatedIndividual(Individual ind)
      {
         ind.ID = _individualsEvaluated;
         _individualsEvaluated++;

         ind.Features = new double[_params.Map.Features.Length];
         for (int i=0; i<_params.Map.Features.Length; i++)
            ind.Features[i] = ind.GetStatByName(_params.Map.Features[i].Name);

			_emitters[ind.EmitterID].ReturnEvaluatedIndividual(ind);
         _maxFitness = Math.Max(_maxFitness, ind.Fitness);
         
         if (_individualsEvaluated % _params.Map.MapLoggingFrequency == 0)
            _map_log.UpdateLog();

         if (_individualsEvaluated % _params.Map.SummaryLoggingFrequency == 0)
            _summary_log.UpdateLog(_individualsEvaluated);
      }
   }
}
