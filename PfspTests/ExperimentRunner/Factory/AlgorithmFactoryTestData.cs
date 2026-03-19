using System.Text.Json;
using PFSP.Evaluators;
using PFSP.Instances;

namespace PfspTests.ExperimentRunner.Factory
{
    internal static class AlgorithmFactoryTestData
    {
        public static JsonElement Elem(string json) =>
            JsonDocument.Parse(json).RootElement.Clone();

        public static Instance SmallInstance() =>
            Instance.Create(
                new double[,]
                {
                    { 30, 29, 19, 50 },
                    { 39, 69,  6, 73 },
                    {  3, 86, 58, 60 },
                    { 72, 74, 51, 15 },
                    { 24, 47, 22, 90 }
                },
                new TotalFlowTimeEvaluator());
    }
}
