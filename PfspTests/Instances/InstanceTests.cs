using PFSP.Evaluators;
using PFSP.Instances;

namespace PfspTests.Instances
{
    public class InstanceTests
    {
        [Fact]
        public void Create_NullMatrix_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Instance.Create(null!, new TotalFlowTimeEvaluator()));
        }

        [Fact]
        public void Create_NullEvaluator_ThrowsArgumentNullException()
        {
            var matrix = new double[1, 2] { { 1, 2 } };
            Assert.Throws<ArgumentNullException>(() => Instance.Create(matrix, null!));
        }

        [Fact]
        public void Create_SetsPropertiesCorrectly()
        {
            var matrix = new double[3, 4];
            var evaluator = new TotalFlowTimeEvaluator();

            var inst = Instance.Create(matrix, evaluator, seed: 7, upperBound: 99.0, lowerBound: 10.0);

            Assert.Equal(3, inst.Machines);
            Assert.Equal(4, inst.Jobs);
            Assert.Equal(7, inst.Seed);
            Assert.Equal(99.0, inst.UpperBound);
            Assert.Equal(10.0, inst.LowerBound);
            Assert.Same(evaluator, inst.Evaluator);
            Assert.Same(matrix, inst.Matrix);
        }
    }
}
