using System;
using PFSP.Solutions;

namespace PFSP.Algorithms
{
    public sealed record AlgorithmResult(
        ISolution Best,
        int Evaluations,
        TimeSpan Elapsed);
}
