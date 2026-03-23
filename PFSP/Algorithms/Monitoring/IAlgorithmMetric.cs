namespace PFSP.Algorithms.Monitoring
{
    public interface IAlgorithmMetric
    {
        string Name { get; }

        void Observe(AlgorithmEventKind eventKind, AlgorithmState state, AlgorithmMetricRecorder recorder);
    }
}
