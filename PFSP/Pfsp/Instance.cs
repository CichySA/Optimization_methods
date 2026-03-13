using System;

namespace PFSP
{
    // Instance is an instance of the permutation flowshop scheduling problem.
    public class Instance
    {
        public IEvaluator Evaluator { get; set; }
        public int Jobs { get; set; }
        public int Machines { get; set; }
        public int Seed { get; set; }
        public double UpperBound { get; set; }
        public double LowerBound { get; set; }
        public int InstanceId { get; set; }
        // Matrix[machine, job]
        public double[,] Matrix { get; set; }

        // Evaluate returns the fitness of the given permutation.
        public double Evaluate(int[] permutation)
        {
            if (Evaluator == null) throw new InvalidOperationException("Evaluator not set on instance");
            if (permutation == null) throw new ArgumentNullException(nameof(permutation));
            if (permutation.Length != Jobs) throw new ArgumentException($"invalid permutation length, expected {Jobs}, got {permutation.Length}");
            return Evaluator.Evaluate(this, permutation);
        }
    }
}
