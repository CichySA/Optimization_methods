using System;
using System.Collections.Generic;

namespace PFSP.Monitoring
{
    public sealed record AlgorithmMonitoringOptions
    {
        public bool Enabled { get; init; } = false;
        public List<string> EnabledMetrics { get; init; } = [];

        internal bool IsMetricEnabled(string metricName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(metricName);

            if (EnabledMetrics.Count == 0)
                return true;

            foreach (var enabledMetric in EnabledMetrics)
            {
                if (string.Equals(enabledMetric, metricName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
