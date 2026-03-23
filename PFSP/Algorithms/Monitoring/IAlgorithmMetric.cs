namespace PFSP.Algorithms.Monitoring
{
    public interface IAlgorithmMetric
    {
        string Name { get; }

        IReadOnlyCollection<AlgorithmEventKind> EventKinds { get; }

        void Observe(AlgorithmEventKind eventKind, AlgorithmState state, AlgorithmMetricRecorder recorder);
    }
}
