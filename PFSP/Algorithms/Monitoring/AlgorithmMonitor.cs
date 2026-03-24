using System;
using System.Collections.Generic;
using System.Linq;

namespace PFSP.Algorithms.Monitoring
{
    public sealed class AlgorithmMonitor
    {
        private readonly IAlgorithmMetric[] _metrics;
        private readonly AlgorithmMetricRecorder _recorder;

        public AlgorithmMonitor(
            AlgorithmResult result,
            AlgorithmMonitoringOptions? options = null,
            IEnumerable<IAlgorithmMetric>? additionalMetrics = null)
        {
            options ??= new AlgorithmMonitoringOptions();

            var availableMetrics = StandardAlgorithmMetrics.Create().ToList();
            if (additionalMetrics is not null)
                availableMetrics.AddRange(additionalMetrics);

            _metrics = options.Enabled
                ? [.. availableMetrics.Where(metric => options.IsMetricEnabled(metric.Name))]
                : [];
            _recorder = new AlgorithmMetricRecorder(result);
        }

        public void Emit(AlgorithmEventKind eventKind, AlgorithmState state)
        {
            ArgumentNullException.ThrowIfNull(state);

            switch (eventKind)
            {
                case AlgorithmEventKind.Started:
                    state.StartTimer();
                    break;
                case AlgorithmEventKind.CandidateEvaluated:
                    state.Evaluations++;
                    break;
                case AlgorithmEventKind.Finished:
                    state.StopTimer();
                    if (state.EvaluationBudget > 0 && state.Evaluations > state.EvaluationBudget)
                        _recorder.RecordWarning("Algorithm went over NFE budget");
                    break;
            }

            foreach (var metric in _metrics)
                metric.Observe(eventKind, state, _recorder);
        }
    }
}
