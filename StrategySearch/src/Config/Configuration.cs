namespace StrategySearch.Config
{
   class Configuration
   {
      public int NumTrials { get; set; }
      public int NumParams { get; set; }
      public string FunctionType { get; set; }
      public SearchParams Search { get; set; }
   }

   class SearchParams
   {
      public string Type { get; set; }
      public string ConfigFilename { get; set; }
      public bool LogIndividuals { get; set; }
   }

}
