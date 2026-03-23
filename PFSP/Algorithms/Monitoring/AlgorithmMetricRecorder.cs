namespace PFSP.Algorithms.Monitoring
{
    public sealed class AlgorithmMetricRecorder
    {
        private readonly AlgorithmResult _result;

        public AlgorithmMetricRecorder(AlgorithmResult result)
        {
            _result = result ?? throw new ArgumentNullException(nameof(result));
        }

        public void RecordDense(string name, double value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            var storage = _result.ExperimentalDataStorage;
            if (!storage.TryGetValue(name, out var series))
            {
                series = new List<double>();
                storage[name] = series;
            }

            ((List<double>)series).Add(value);
        }

        public void RecordIndexed(string name, long index, double value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            var storage = _result.ExperimentalDataStorage;
            if (!storage.TryGetValue(name, out var series))
            {
                series = new List<AlgorithmMetricPoint>();
                storage[name] = series;
            }

            ((List<AlgorithmMetricPoint>)series).Add(new AlgorithmMetricPoint(index, value));
        }
    }
}
