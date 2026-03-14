namespace PFSP.Solutions
{
    /// <summary>
    /// A permutation solution for permutation-based problems.
    ///
    /// Important: the primary constructor stores the provided <see cref="Permutation"/>
    /// array by reference. Callers must be explicit about ownership to avoid
    /// accidental aliasing when reusing buffers for performance.
    ///
    /// Use <see cref="WrapBuffer(int[], double)"/> when you want to wrap an existing
    /// array without copying. Use <see cref="CreateCopy(int[], double)"/> to create
    /// a solution that owns an independent copy of the permutation array.
    /// </summary>
    public sealed record PermutationSolution(int[] Permutation, double Cost) : ISolution
    {
        /// <summary>
        /// Wraps an existing permutation buffer without copying. The returned
        /// solution holds a reference to the provided array; modifying the array
        /// after wrapping will affect the solution. Use this for low-allocation
        /// evaluation loops where the caller manages the buffer lifetime.
        /// </summary>
        public static PermutationSolution WrapBuffer(int[] buffer, double cost) => new(buffer, cost);

        /// <summary>
        /// Creates a solution that owns an independent copy of the provided
        /// permutation array. This is the safe default for callers that do not
        /// reuse the original array.
        /// </summary>
        public static PermutationSolution CreateCopy(int[] permutation, double cost)
        {
            if (permutation == null) throw new ArgumentNullException(nameof(permutation));
            var copy = new int[permutation.Length];
            Array.Copy(permutation, copy, permutation.Length);
            return new PermutationSolution(copy, cost);
        }
    }
}
