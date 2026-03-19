namespace ExperimentRunner
{
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

        public static readonly int[] DefaultSamples = { 1, 10, 100, 1000, 10000, 100000 };
        public const int DefaultSeed = 123;
        public const string DefaultOutDir = "experiment_results";

        public string[] Instances { get; init; } = DefaultInstances;
        public int[] Samples { get; init; } = DefaultSamples;
        public int Seed { get; init; } = DefaultSeed;
        public string OutDir { get; init; } = DefaultOutDir;

        public static ExperimentRunnerConfiguration Default => new();

        public static ExperimentRunnerConfiguration From(string[]? instances, int[]? samples, int? seed, string? outDir)
        {
            return new ExperimentRunnerConfiguration
            {
                Instances = instances ?? DefaultInstances,
                Samples = samples ?? DefaultSamples,
                Seed = seed ?? DefaultSeed,
                OutDir = outDir ?? DefaultOutDir
            };
        }
    }
}
