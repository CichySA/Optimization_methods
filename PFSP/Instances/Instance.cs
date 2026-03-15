using System;
using PFSP.Evaluators;
using PFSP.Solutions;

namespace PFSP.Instances
{
    // Instance is an instance of the permutation flowshop scheduling problem.
    public class Instance
    {
        public IEvaluator Evaluator { get; private set; }
        public int Jobs { get; private set; }
        public int Machines { get; private set; }
        public int Seed { get; private set; }
        public double UpperBound { get; private set; }
        public double LowerBound { get; private set; }
        public int InstanceId { get; private set; }
        // Matrix[machine, job]
        public double[,] Matrix { get; private set; }

        // Prevent direct public construction
        private Instance()
        {
            Evaluator = null!;
            Matrix = null!;
        }

        // Create an instance from a processing-time matrix and an explicit evaluator.
        public static Instance Create(double[,] matrix, IEvaluator evaluator, int seed = 0, double upperBound = 0, double lowerBound = 0)
        {
            ArgumentNullException.ThrowIfNull(matrix);
            ArgumentNullException.ThrowIfNull(evaluator);
            int machines = matrix.GetLength(0);
            int jobs = matrix.GetLength(1);
            if (machines <= 0) throw new ArgumentException("Machines must be > 0", nameof(matrix));
            if (jobs <= 0) throw new ArgumentException("Jobs must be > 0", nameof(matrix));

            var inst = new Instance
            {
                Matrix = matrix,
                Machines = machines,
                Jobs = jobs,
                Evaluator = evaluator,
                Seed = seed,
                UpperBound = upperBound,
                LowerBound = lowerBound
            };
            return inst;
        }

        // Create an instance with the default TotalFlowTimeEvaluator.
        public static Instance CreateWithDefaultEvaluator(double[,] matrix, int seed = 0, double upperBound = 0, double lowerBound = 0)
        {
            return Create(matrix, new TotalFlowTimeEvaluator(), seed, upperBound, lowerBound);
        }

        // Evaluate returns the fitness of the given solution.
        public double Evaluate(ISolution solution)
        {
            return Evaluator.Evaluate(this, solution);
        }
    }
}
