using PFSP.Instances;

namespace PFSP.Solutions.PermutationSolutionGenerators
{
    /// <summary>
    /// Produces a uniformly random permutation solution.
    /// The RNG is seeded once at construction time; 0 means choose a random seed.
    /// 
    /// Use shuffling buffers via the <see cref="Shuffle(int[])"/> for performance-critical code.
    /// </summary>
    public sealed class RandomPermutationSolutionGenerator(int seed = 0) : IPermutationSolutionGenerator
    {
        private readonly Random _rnd = seed == 0 ? new Random() : new Random(seed);
        private int[] _identity = [];

        public PermutationSolution Create(Instance instance)
        {
            ArgumentNullException.ThrowIfNull(instance);

            int n = instance.Jobs;
            var perm = new int[n];
            Shuffle(perm);
            return PermutationSolution.CreateCopy(perm, 0.0);
        }

        /// <summary>
        /// Fills <paramref name="buffer"/> with a uniformly random permutation of
        /// [0, buffer.Length) without allocating. The caller owns the buffer.
        /// </summary>
        public void Shuffle(int[] buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            int n = buffer.Length;

            // Rebuild the cached identity template only when the job count changes.
            if (_identity.Length != n)
            {
                _identity = new int[n];
                for (int i = 0; i < n; i++) _identity[i] = i;
            }

            _identity.AsSpan().CopyTo(buffer);  // SIMD-accelerated copy

            // Fisher-Yates shuffle
            for (int i = n - 1; i > 0; i--)
            {
                int j = _rnd.Next(i + 1);
                (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
            }
        }
    }
}
