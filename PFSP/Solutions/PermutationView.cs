namespace PFSP.Solutions
{
    /// <summary>
    /// Lightweight, reusable <see cref="ISolution"/> wrapper that exposes an existing
    /// buffer as a solution without copying it.  Intended solely as a transient argument
    /// to <c>Instance.Evaluate</c>; the caller must not store or share this instance
    /// after the buffer is mutated or returned.
    /// </summary>
    internal sealed class PermutationView(int[] buffer) : ISolution
    {
        public int[] Permutation { get; } = buffer;
        public double Cost => 0.0;
    }
}
