using PFSP.Solutions;

namespace PFSP.Algorithms
{
    public sealed class AlgorithmResult
    {
        private readonly Dictionary<string, object> _experimentalData = [];
        private ISolution? _best;

        public AlgorithmResult(IParameters parms)
        {
            Parameters = parms;
        }

        public AlgorithmResult(IParameters parms, ISolution best)
        {
            Parameters = parms;
            SetBest(best);
        }

        public ISolution Best => _best ?? throw new InvalidOperationException("Best solution has not been recorded yet.");

        public bool HasBest => _best is not null;

        public long EvaluationBudget { get; set; } = 0;

        public IParameters Parameters { get; set; }

        internal IDictionary<string, object> ExperimentalDataStorage => _experimentalData;

        public IReadOnlyDictionary<string, object> ExperimentalData =>
            _experimentalData.ToDictionary(
                pair => pair.Key,
                pair => pair.Value switch
                {
                    List<double> dense => (object)dense.ToArray(),
                    List<AlgorithmMetricPoint> indexed => indexed.ToArray(),
                    List<string> warnings => (object)warnings.ToArray(),
                    _ => pair.Value
                });

        public void SetBest(ISolution best)
        {
            ArgumentNullException.ThrowIfNull(best);
            _best = best;
        }

        public bool UpdateBest(ISolution candidate)
        {
            ArgumentNullException.ThrowIfNull(candidate);

            if (_best is not null && candidate.Cost >= _best.Cost)
                return false;

            _best = candidate;
            return true;
        }
    }

    public readonly record struct AlgorithmMetricPoint(long Index, double Value);
}
