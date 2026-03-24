using PFSP.Instances;
using Xunit;

namespace PfspTests.Instances
{
    public class InstanceReaderTests
    {
        private const int Jobs = 20;
        private const int Machines = 5;
        private const string BaseName = "tai20_5_0";

        // Expected matrix from PFSP/go-taillard/pfsp/instances/tai20_5_0.fsp
        private static readonly int[,] ExpectedMatrix = new int[Machines, Jobs]
        {
            { 54, 83, 15, 71, 77, 36, 53, 38, 27, 87, 76, 91, 14, 29, 12, 77, 32, 87, 68, 94 },
            { 79,  3, 11, 99, 56, 70, 99, 60,  5, 56,  3, 61, 73, 75, 47, 14, 21, 86,  5, 77 },
            { 16, 89, 49, 15, 89, 45, 60, 23, 57, 64,  7,  1, 63, 41, 63, 47, 26, 75, 77, 40 },
            { 66, 58, 31, 68, 78, 91, 13, 59, 49, 85, 85,  9, 39, 41, 56, 40, 54, 77, 51, 31 },
            { 58, 56, 20, 85, 53, 35, 53, 41, 69, 13, 86, 72,  8, 49, 47, 87, 58, 18, 68, 28 }
        };

        [Fact]
        public void Read_ByNumbers_LoadsTai20_5_0()
        {
            var inst = InstanceReader.Read(Jobs, Machines, 0);
            Assert.NotNull(inst);
            Assert.Equal(Jobs, inst.Jobs);
            Assert.Equal(Machines, inst.Machines);
            Assert.NotNull(inst.Matrix);
            Assert.Equal(inst.Machines, inst.Matrix.GetLength(0));
            Assert.Equal(inst.Jobs, inst.Matrix.GetLength(1));

            // Check full matrix content
            for (int m = 0; m < Machines; m++)
            for (int j = 0; j < Jobs; j++)
            {
                Assert.Equal(ExpectedMatrix[m, j], (int)inst.Matrix[m, j]);
            }
        }

        [Fact]
        public void Read_ByBaseName_LoadsTai20_5_0()
        {
            var inst = InstanceReader.Read(BaseName);
            Assert.NotNull(inst);
            Assert.Equal(Jobs, inst.Jobs);
            Assert.Equal(Machines, inst.Machines);
            Assert.NotNull(inst.Matrix);
            Assert.Equal(inst.Machines, inst.Matrix.GetLength(0));
            Assert.Equal(inst.Jobs, inst.Matrix.GetLength(1));

            // Check full matrix content
            for (int m = 0; m < Machines; m++)
            for (int j = 0; j < Jobs; j++)
            {
                Assert.Equal(ExpectedMatrix[m, j], (int)inst.Matrix[m, j]);
            }
        }

        [Fact]
        public void Read_NullBaseName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => InstanceReader.Read(null!));
        }
    }
}
