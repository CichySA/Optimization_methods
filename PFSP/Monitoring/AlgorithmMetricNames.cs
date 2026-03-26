namespace PFSP.Monitoring
{
    public static class AlgorithmMetricNames
    {
        public const string Evaluations = nameof(Evaluations);
        public const string ElapsedMs = nameof(ElapsedMs);
        public const string BestFoundAtEvaluation = nameof(BestFoundAtEvaluation);
        public const string BestCostByEvaluation = nameof(BestCostByEvaluation);
        public const string BestCostByIteration = nameof(BestCostByIteration);
        public const string CurrentCostByIteration = nameof(CurrentCostByIteration);
        public const string TemperatureByIteration = nameof(TemperatureByIteration);
        public const string BestByGeneration = nameof(BestByGeneration);
        public const string WorstByGeneration = nameof(WorstByGeneration);
        public const string MeanByGeneration = nameof(MeanByGeneration);
        public const string DeviationByGeneration = nameof(DeviationByGeneration);
        public const string BestInPopulationByGeneration = nameof(BestInPopulationByGeneration);
        public const string WorstInPopulationByGeneration = nameof(WorstInPopulationByGeneration);
        public const string HemmingDistanceByGeneration = nameof(HemmingDistanceByGeneration);
        public const string Warnings = nameof(Warnings);
    }
}
