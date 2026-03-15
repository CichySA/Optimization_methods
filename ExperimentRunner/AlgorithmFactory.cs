using PFSP.Algorithms;
using PFSP.Algorithms.Random;

namespace ExperimentRunner
{
    public static class AlgorithmFactory
    {
        public static RandomAlgorithm CreateRandomAlgorithm() => new RandomAlgorithm();
        public static RandomParameters RandomParametersForRuns(int samples, int seed) => RandomParameters.ForRuns(samples, seed);

        // Create multiple algorithms using provided parameter specifications (samples, seed).
        public static List<(string Name, IAlgorithm Algo, IParameters Params)> CreateRandomAlgorithms(IEnumerable<(int Samples, int Seed)> specs)
        {
            var list = new List<(string Name, IAlgorithm Algo, IParameters Params)>();
            foreach (var s in specs)
            {
                var pars = RandomParameters.ForRuns(s.Samples, s.Seed);
                var name = $"Random_{pars.Samples}_s{pars.Seed}";
                list.Add((name, new RandomAlgorithm(), pars));
            }
            return list;
        }
    }
}
