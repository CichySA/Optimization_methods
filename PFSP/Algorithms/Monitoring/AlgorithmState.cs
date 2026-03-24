using PFSP.Instances;
using PFSP.Solutions;
using System.Diagnostics;

namespace PFSP.Algorithms.Monitoring
{
    public abstract class AlgorithmState
    {
        private readonly Stopwatch _stopwatch = new();

        protected AlgorithmState(Instance instance, IParameters parameters)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public Instance Instance { get; }

        public IParameters Parameters { get; }

        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public long Evaluations { get; set; }

        public long BestFoundAtEvaluation { get; set; } = -1;

        public ISolution? Best { get; set; }

        public long EvaluationBudget { get; set; } = 0;

        internal void StartTimer() => _stopwatch.Start();

        internal void StopTimer() => _stopwatch.Stop();
    }
}
