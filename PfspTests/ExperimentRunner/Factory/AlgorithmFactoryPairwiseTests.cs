using System.Text.Json;
using Xunit;
using System.Collections.Generic;

namespace PfspTests.ExperimentRunner.Factory
{
    [Trait("Area", "ExperimentRunner")]
    [Trait("Component", "AlgorithmFactory")]
    [Trait("Kind", "Unit")]
    public class AlgorithmFactoryPairwiseTests
    {
        [Fact]
        public void ExpandParameterSets_PairwiseProduct_YieldsIndexAlignedPairs()
        {
            var json = JsonDocument.Parse(@"{ ""Product"": ""Pairwise"", ""ParameterGrid"": { ""A"": [1, 2, 3], ""B"": [10, 20, 30] } }")
                .RootElement.Clone();

            // Invoke the private ExpandParameterSets method via reflection to observe the raw JsonElement outputs.
            var mi = typeof(global::ExperimentRunner.AlgorithmFactory).GetMethod("ExpandParameterSets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
            var enumerable = (System.Collections.IEnumerable)mi.Invoke(null, new object[] { json })!;

            var list = new List<JsonElement>();
            foreach (var item in enumerable)
                list.Add((JsonElement)item!);

            Assert.Equal(3, list.Count);

            Assert.Equal(1, list[0].GetProperty("A").GetInt32());
            Assert.Equal(10, list[0].GetProperty("B").GetInt32());

            Assert.Equal(2, list[1].GetProperty("A").GetInt32());
            Assert.Equal(20, list[1].GetProperty("B").GetInt32());

            Assert.Equal(3, list[2].GetProperty("A").GetInt32());
            Assert.Equal(30, list[2].GetProperty("B").GetInt32());
        }
    }
}
