using PFSP.Algorithms;
using PFSP.Solutions;

namespace ExperimentRunner
{
    public static class ExperimentRunRecordFactory
    {
        public static ExperimentRunRecord Create(
            string instanceName,
            string algorithmName,
            IParameters parameters,
            int? seed,
            AlgorithmResult result,
            DateTimeOffset timestamp)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(instanceName);
            ArgumentException.ThrowIfNullOrWhiteSpace(algorithmName);
            ArgumentNullException.ThrowIfNull(parameters);
            ArgumentNullException.ThrowIfNull(result);

            return new ExperimentRunRecord
            {
                Instance = instanceName,
                Algorithm = algorithmName,
                Parameters = parameters,
                Seed = seed,
                Best = new ExperimentSolutionRecord
                {
                    Permutation = [.. result.Best.Permutation],
                    Cost = result.Best.Cost
                },
                Metrics = result.ExperimentalData.ToDictionary(pair => pair.Key, pair => pair.Value),
                Timestamp = timestamp
            };
        }
    }

    public sealed record ExperimentRunRecord
    {
        public required string Instance { get; init; }
        public required string Algorithm { get; init; }
        public required object Parameters { get; init; }
        public int? Seed { get; init; }
        public required ExperimentSolutionRecord Best { get; init; }
        public required IReadOnlyDictionary<string, object> Metrics { get; init; }
        public DateTimeOffset Timestamp { get; init; }
    }

    public sealed record ExperimentSolutionRecord
    {
        public required int[] Permutation { get; init; }
        public double Cost { get; init; }
    }
}
