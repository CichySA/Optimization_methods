using PFSP.Monitoring;

namespace PFSP.Algorithms.RandomSearch
{
    /// <summary>
    /// Parameters for the Random algorithm.
    /// Use the factory methods `ForRuns` or `ForTimeLimit` to create instances,
    /// or use `with` to make small modifications.
    /// </summary>
    public sealed record RandomSearchParameters : IParameters
    {
        public int Seed { get; init; }
        public int Iterations { get; init; }
        public TimeSpan? TimeLimit { get; init; }
        public long EvaluationBudget { get; init; }
        public AlgorithmMonitoringOptions Monitoring { get; init; } = new();
        public bool UseTimeLimit => TimeLimit.HasValue;

        private RandomSearchParameters() { }

        private RandomSearchParameters(int seed, int iterations, TimeSpan? timeLimit, long evaluationBudget)
        {
            Seed = seed;
            Iterations = iterations;
            TimeLimit = timeLimit;
            EvaluationBudget = evaluationBudget;
        }

        private static void Validate(int seed, int iterations, TimeSpan? timeLimit)
        {
            if (seed < 0)
                throw new ArgumentException(
                    $"Seed must be non-negative (0 means random). Actual: {seed}.",
                    nameof(seed));

            if (timeLimit.HasValue && iterations > 0)
                throw new ArgumentException(
                    $"Both Iterations and TimeLimit cannot be specified at the same time. Iterations: {iterations}, TimeLimit: {timeLimit}.",
                    nameof(iterations));

            if (!timeLimit.HasValue && iterations <= 0)
                throw new ArgumentException(
                    $"Iterations must be greater than zero when TimeLimit is not provided. Actual: {iterations}.",
                    nameof(iterations));

            if (timeLimit.HasValue && timeLimit.Value <= TimeSpan.Zero)
                throw new ArgumentException(
                    $"TimeLimit must be greater than zero when specified. Actual: {timeLimit}.",
                    nameof(timeLimit));
        }

        // Simple, explicit factories are easy to understand for callers.
        public static RandomSearchParameters ForRuns(int samples, int seed = 0, long evaluationBudget = 0)
        {
            Validate(seed, samples, null);
            return new RandomSearchParameters(seed, samples, null, evaluationBudget);
        }

        public static RandomSearchParameters ForTimeLimit(TimeSpan timeLimit, int seed = 0, long evaluationBudget = 0)
        {
            Validate(seed, 0, timeLimit);
            return new RandomSearchParameters(seed, 0, timeLimit, evaluationBudget);
        }
    }
}
