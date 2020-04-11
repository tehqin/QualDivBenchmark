namespace StrategySearch.Config
{
   class MapParams
   {
      public bool SeparateLoggingFolder { get; set; }
      public int MapLoggingFrequency { get; set; }
      public int SummaryLoggingFrequency { get; set; }

      public string Type { get; set; } 
      public int RemapFrequency { get; set; }
      public int StartSize { get; set; }
      public int EndSize { get; set; }

      public FeatureParams[] Features { get; set; }
   }

   class FeatureParams
   {
      public string Name { get; set; }
      public double MinValue { get; set; }
      public double MaxValue { get; set; }
   }
}
