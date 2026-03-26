using PFSP.Algorithms;

namespace PFSP.Monitoring
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

        public bool TryGetLastDense(string name, out double value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            var storage = _result.ExperimentalDataStorage;
            value = 0.0;
            if (!storage.TryGetValue(name, out var series) || series is not List<double> list || list.Count == 0)
                return false;

            value = list[^1];
            return true;
        }

        public void RecordWarning(string message)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            var storage = _result.ExperimentalDataStorage;
            if (!storage.TryGetValue(AlgorithmMetricNames.Warnings, out var existing) || existing is not List<string> list)
            {
                list = [];
                storage[AlgorithmMetricNames.Warnings] = list;
            }
            list.Add(message);
        }
    }
}
