using PFSP.Evaluators;
using System.Reflection;

namespace PfspTests
{
    public class ValidateInputsTests
    {
        private static MethodInfo GetValidateMethod()
        {
            var mi = typeof(TotalFlowTimeEvaluator).GetMethod("ValidateInputs", BindingFlags.NonPublic | BindingFlags.Static);
            return mi ?? throw new InvalidOperationException("ValidateInputs method not found");
        }

        [Fact]
        public void NullInstance_ThrowsArgumentNullException()
        {
            var mi = GetValidateMethod();
            var ex = Assert.Throws<TargetInvocationException>(() => mi.Invoke(null, [null, new int[] { 0 }]));
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        [Fact]
        public void NullPermutation_ThrowsArgumentNullException()
        {
            var mi = GetValidateMethod();
            var matrix = new double[1, 2] { { 1, 1 } };
            var instance = PFSP.Instances.Instance.Create(matrix, new TotalFlowTimeEvaluator());
            var ex = Assert.Throws<TargetInvocationException>(() => mi.Invoke(null, [instance, null]));
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        [Fact]
        public void EmptyPermutation_ThrowsArgumentException()
        {
            var mi = GetValidateMethod();
            var matrix = new double[1, 2] { { 1, 1 } };
            var instance = PFSP.Instances.Instance.Create(matrix, new TotalFlowTimeEvaluator());
            var ex = Assert.Throws<TargetInvocationException>(() => mi.Invoke(null, [instance, new int[0]]));
            Assert.IsType<ArgumentException>(ex.InnerException);
        }

        [Fact]
        public void PermutationLongerThanJobs_ThrowsArgumentException()
        {
            var mi = GetValidateMethod();
            var matrix = new double[1, 2] { { 1, 1 } };
            var instance = PFSP.Instances.Instance.Create(matrix, new TotalFlowTimeEvaluator());
            var perm = new int[] { 0, 1, 2 };
            var ex = Assert.Throws<TargetInvocationException>(() => mi.Invoke(null, [instance, perm]));
            Assert.IsType<ArgumentException>(ex.InnerException);
        }

        [Fact]
        public void PermutationContainsOutOfRange_ThrowsArgumentException()
        {
            var mi = GetValidateMethod();
            var matrix = new double[1, 3] { { 1, 1, 1 } };
            var instance = PFSP.Instances.Instance.Create(matrix, new TotalFlowTimeEvaluator());
            var perm = new int[] { 0, 3 };
            var ex = Assert.Throws<TargetInvocationException>(() => mi.Invoke(null, [instance, perm]));
            Assert.IsType<ArgumentException>(ex.InnerException);
        }
    }
}
