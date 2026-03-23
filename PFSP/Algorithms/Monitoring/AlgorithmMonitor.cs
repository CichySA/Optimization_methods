using System;
using System.Collections.Generic;
using System.Linq;

namespace PFSP.Algorithms.Monitoring
{
    public sealed class AlgorithmMonitor
    {
        private readonly HashSet<AlgorithmEventKind> _enabledEventKinds;
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
            _enabledEventKinds = [.. _metrics.SelectMany(metric => metric.EventKinds)];
            _recorder = new AlgorithmMetricRecorder(result);
        }

        public bool Handles(AlgorithmEventKind eventKind) => _enabledEventKinds.Contains(eventKind);

        public void Emit(AlgorithmEventKind eventKind, AlgorithmState state)
        {
            ArgumentNullException.ThrowIfNull(state);

            if (!Handles(eventKind))
                return;

            foreach (var metric in _metrics)
                metric.Observe(eventKind, state, _recorder);
        }
    }
}
