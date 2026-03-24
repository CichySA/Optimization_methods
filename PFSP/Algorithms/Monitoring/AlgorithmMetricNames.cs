namespace PFSP.Algorithms.Monitoring
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
        public const string MedianByGeneration = nameof(MedianByGeneration);
        public const string DeviationByGeneration = nameof(DeviationByGeneration);
        public const string BestInPopulationByGeneration = nameof(BestInPopulationByGeneration);
        public const string ElapsedOnFinished = nameof(ElapsedOnFinished);
        public const string Warnings = nameof(Warnings);
    }
}
