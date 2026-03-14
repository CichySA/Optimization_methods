namespace PFSP.Moves
{
    public interface IMove
    {
        void Apply(Span<int> permutation);
    }
}
