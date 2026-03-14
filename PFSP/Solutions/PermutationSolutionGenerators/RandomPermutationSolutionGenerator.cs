using PFSP.Instances;

namespace PFSP.Solutions.PermutationSolutionGenerators
{
    /// <summary>
    /// Produces a uniformly random permutation solution.
    /// The RNG is seeded once at construction time; 0 means choose a random seed.
    /// </summary>
    public sealed class RandomPermutationSolutionGenerator(int seed = 0) : IPermutationSolutionGenerator
    {
        private readonly Random _rnd = seed == 0 ? new Random() : new Random(seed);

        /// <summary>
        /// Fills <paramref name="buffer"/> with a uniformly random permutation of
        /// [0, n) using the inside-out Fisher-Yates algorithm.  No pre-copy of an
        /// identity array is required; each position is written exactly once.
        /// </summary>
        public void ShuffleInto(int[] buffer, Instance instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            int n = instance.Jobs;
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (buffer.Length < n) throw new ArgumentException("buffer too small", nameof(buffer));

            ShuffleInto(buffer, n);
        }

        /// <summary>
        /// Hot-path overload: fills <paramref name="buffer"/> with a uniformly random
        /// permutation of [0, <paramref name="n"/>) without performing argument validation.
        /// The caller is responsible for ensuring the buffer is non-null and large enough.
        /// </summary>
        internal void ShuffleInto(int[] buffer, int n)
        {
            // Inside-out Fisher-Yates: O(n) single forward pass, no identity copy.
            for (int i = 0; i < n; i++)
            {
                int j = _rnd.Next(i + 1);  // j in [0, i]
                buffer[i] = buffer[j];
                buffer[j] = i;
            }
        }

        public PermutationSolution Create(Instance instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            var result = new int[instance.Jobs];
            ShuffleInto(result, instance);
            return new PermutationSolution(result, 0.0);
        }
    }
}
