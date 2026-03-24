using System.Text.Json;

namespace ExperimentRunner
{
    public sealed record AlgorithmSpec
    {
        public string Type { get; init; } = "Random";
        public int Iterations { get; init; } = 1;
        public JsonElement Parameters { get; init; }
    }

    public sealed record ExperimentRunnerConfiguration
    {
        public static readonly string[] DefaultInstances =
        {
            "tai_20_5_0",
            "tai_20_10_0",
            "tai_20_20_0",
            "tai_100_10_0",
            "tai_100_20_0",
            "tai_500_20_0"
        };

        public static readonly AlgorithmSpec[] DefaultAlgorithms = [];
        public const string DefaultOutDir = "experiment_results";

        public string[] Instances { get; init; } = DefaultInstances;
        public AlgorithmSpec[] Algorithms { get; init; } = DefaultAlgorithms;
        public string OutDir { get; init; } = DefaultOutDir;
        public JsonElement Parameters { get; init; }

        public static ExperimentRunnerConfiguration Default => new();
    }
}
