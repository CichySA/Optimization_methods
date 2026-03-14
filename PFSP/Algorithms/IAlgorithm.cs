using System;
using System.Threading;
using PFSP.Instances;
using PFSP.Solutions;

namespace PFSP.Algorithms
{
    public interface IAlgorithm
    {
        AlgorithmResult Solve(Instance instance, CancellationToken cancellationToken = default);
    }
}
