using PFSP.Algorithms;

namespace PFSP.Monitoring
{
    public interface IAlgorithmMetric
    {
        string Name { get; }

        void Observe(AlgorithmEventKind eventKind, AlgorithmState state, AlgorithmMetricRecorder recorder);
    }
}
