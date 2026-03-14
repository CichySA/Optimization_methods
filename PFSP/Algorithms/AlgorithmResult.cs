using System;
using PFSP.Solutions;

namespace PFSP.Algorithms
{
    /// <summary>
    /// Result of an algorithm run.
    /// </summary>
    public sealed record AlgorithmResult(ISolution Best, long Evaluations, TimeSpan Elapsed)
    {
        /// <summary>
        /// The evaluation index at which the best solution was discovered, or -1
        /// if unknown.
        /// </summary>
        public long BestFoundAtEvaluation { get; init; } = -1;
    }
}
