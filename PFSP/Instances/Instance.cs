using PFSP.Evaluators;
using PFSP.Solutions;

namespace PFSP.Instances
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

        // Evaluate returns the fitness of the given solution.
        public double Evaluate(ISolution solution)
        {
            if (Evaluator == null) throw new InvalidOperationException("Evaluator not set on instance");
            return Evaluator.Evaluate(this, solution);
        }
    }
}
