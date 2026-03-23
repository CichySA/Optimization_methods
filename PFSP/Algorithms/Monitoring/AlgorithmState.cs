using PFSP.Instances;
using PFSP.Solutions;
using System.Diagnostics;

namespace PFSP.Algorithms.Monitoring
{
    public abstract class AlgorithmState
    {
        protected AlgorithmState(Instance instance, IParameters parameters, Stopwatch stopwatch)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Stopwatch = stopwatch ?? throw new ArgumentNullException(nameof(stopwatch));
        }

        public Instance Instance { get; }

        public IParameters Parameters { get; }

        public TimeSpan Elapsed => Stopwatch.Elapsed;

        public long Evaluations { get; set; }

        public long BestFoundAtEvaluation { get; set; } = -1;

        public ISolution? Best { get; set; }

        protected Stopwatch Stopwatch { get; }
    }
}
