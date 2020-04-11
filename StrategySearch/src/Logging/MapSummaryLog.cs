using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using StrategySearch.Mapping;
using StrategySearch.Search;

namespace StrategySearch.Logging
{
   class MapSummaryLog
   {
		private string _logPath;
		private FeatureMap _map;

      public MapSummaryLog(string logPath, FeatureMap map)
      {
         _logPath = logPath;
         _map = map;
         
         // Create a log for individuals
         using (FileStream ow = File.Open(logPath,
					 FileMode.Create, FileAccess.Write, FileShare.None))
         {
            string[] dataLabels = {
                  "NumEvaluated",
                  "QD-Score",
                  "MeanNormFitness",
                  "MedianNormFitness",
                  "CellsOccupied",
                  "PercentOccupied",
                  "MaxNormFitness"
               };

            WriteText(ow, string.Join(",", dataLabels));
            ow.Close();
         }
      }

      private static void WriteText(Stream fs, string s)
      {
         s += "\n";
         byte[] info = new UTF8Encoding(true).GetBytes(s);
         fs.Write(info, 0, info.Length);
      }

      // Call this whenever you want the log to update with the latest
      // feature map data.
      public void UpdateLog(int count)
      {
         using (StreamWriter sw = File.AppendText(_logPath))
         {

            IEnumerable<int> dimensions =
               Enumerable.Repeat(_map.NumGroups, _map.NumFeatures);
            int maxCells = 1;
            foreach (int dim in dimensions)
            {     
               maxCells *= dim;
            }

            double QD_Score = 0.0;
            double maxNormFitness = 0.0;
            List<double> norms = new List<double>();
            foreach (string index in _map.EliteMap.Keys)
            {
               Individual cur = _map.EliteMap[index];

               QD_Score += cur.NormFitness;
               maxNormFitness = Math.Max(maxNormFitness, cur.NormFitness);
               norms.Add(cur.NormFitness);

            }
            double cellsOccupied = _map.EliteMap.Count;
            double percentOccupied = 100 * cellsOccupied / maxCells;
            double meanNormFitness = QD_Score / cellsOccupied;
            norms.Sort();
            double medianNormFitness = norms[norms.Count / 2];
            
            var rowData = new List<string>();
            rowData.Add(count.ToString());
            rowData.Add(QD_Score.ToString());
            rowData.Add(meanNormFitness.ToString());
            rowData.Add(medianNormFitness.ToString());
            rowData.Add(cellsOccupied.ToString());
            rowData.Add(percentOccupied.ToString());
            rowData.Add(maxNormFitness.ToString());
            sw.WriteLine(string.Join(",", rowData));
         }
      }
   }
}
