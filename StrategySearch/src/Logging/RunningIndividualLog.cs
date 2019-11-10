using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using StrategySearch.Search;

namespace StrategySearch.Logging
{
   class RunningIndividualLog
   {
      private string _logPath;
      private bool _isInitiated;

      public RunningIndividualLog(string logPath)
      {
         _logPath = logPath;
         _isInitiated = false;
      }

      private static void writeText(Stream fs, string s)
      {
         s += "\n";
         byte[] info = new UTF8Encoding(true).GetBytes(s);
         fs.Write(info, 0, info.Length);
      }

      private void initLog(Individual cur)
      {
         _isInitiated = true;

         // Create a log for individuals
         using (FileStream ow = File.Open(_logPath,
                   FileMode.Create, FileAccess.Write, FileShare.None))
         {
            // The data to maintain for individuals evaluated.
            string[] individualLabels = {
                  "Individual",
                  "Emitter",
                  "Generation",
                  "Fitness",
               };
            var dataLabels = individualLabels.Concat(new string[0]);

            if (cur.Features != null)
            {
               var featureLabels = new string[cur.Features.Length];
               for (int i=0; i<cur.Features.Length; i++)
                  featureLabels[i] = string.Format("Feature:{0}", i);
               dataLabels = dataLabels.Concat(featureLabels);
            }

            var weightLabels = new string[cur.ParamVector.Length];
            for (int i=0; i<weightLabels.Length; i++)
               weightLabels[i] = string.Format("Weight:{0}", i);
            dataLabels = dataLabels.Concat(weightLabels);

            writeText(ow, string.Join(",", dataLabels));
            ow.Close();
         }
      }

    	public void LogIndividual(Individual cur)
    	{
         // Put the header on the log file if this is the first
         // individual in the experiment.
         if (!_isInitiated)
            initLog(cur);

         using (StreamWriter sw = File.AppendText(_logPath))
         {
            string[] individualData = {
                  cur.ID.ToString(),
                  cur.EmitterID.ToString(),
                  cur.Generation.ToString(),
                  cur.Fitness.ToString(),
               };

            var stratWeights = cur.ParamVector.Select(x => x.ToString());
            var data = individualData.Concat(stratWeights);

            sw.WriteLine(string.Join(",", data));
            sw.Close();
         }
      }
   }
}
