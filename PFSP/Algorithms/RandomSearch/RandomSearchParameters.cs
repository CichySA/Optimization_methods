using PFSP.Algorithms.Monitoring;

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
        public int Samples { get; init; }
        public TimeSpan? TimeLimit { get; init; }
        public AlgorithmMonitoringOptions Monitoring { get; init; } = new();
        public bool UseTimeLimit => TimeLimit.HasValue;

        private RandomSearchParameters() { }

        private RandomSearchParameters(int seed, int samples, TimeSpan? timeLimit)
        {
            Seed = seed;
            Samples = samples;
            TimeLimit = timeLimit;
        }

        private static void Validate(int seed, int samples, TimeSpan? timeLimit)
        {
            if (seed < 0)
                throw new ArgumentException("Seed must be non-negative (0 means random)", nameof(seed));

            if (timeLimit.HasValue && samples > 0)
                throw new ArgumentException("Both Samples and TimeLimit cannot be specified at the same time.");

            if (!timeLimit.HasValue && samples <= 0)
                throw new ArgumentException("Samples must be greater than zero when TimeLimit is not provided.");

            if (timeLimit.HasValue && timeLimit.Value <= TimeSpan.Zero)
                throw new ArgumentException("TimeLimit must be greater than zero when specified.", nameof(timeLimit));
        }

        // Simple, explicit factories are easy to understand for callers.
        public static RandomSearchParameters ForRuns(int samples, int seed = 0)
        {
            Validate(seed, samples, null);
            return new RandomSearchParameters(seed, samples, null);
        }

        public static RandomSearchParameters ForTimeLimit(TimeSpan timeLimit, int seed = 0)
        {
            Validate(seed, 0, timeLimit);
            return new RandomSearchParameters(seed, 0, timeLimit);
        }
    }
}
