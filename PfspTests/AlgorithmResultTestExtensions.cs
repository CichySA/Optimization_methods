using PFSP.Algorithms;

namespace PfspTests
{
    internal static class AlgorithmResultTestExtensions
    {
        public static double[] GetDenseMetric(this AlgorithmResult result, string name)
            => Assert.IsType<double[]>(result.ExperimentalData[name]);

        public static double GetSingleDenseMetric(this AlgorithmResult result, string name)
            => Assert.Single(result.GetDenseMetric(name));

        public static AlgorithmMetricPoint[] GetIndexedMetric(this AlgorithmResult result, string name)
            => Assert.IsType<AlgorithmMetricPoint[]>(result.ExperimentalData[name]);
    }
}
