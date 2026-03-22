using PFSP.Algorithms.Evolutionary.Operators;

namespace PFSP.Algorithms.Evolutionary.Operators.SelectionOperators
{
    public sealed record TournamentSelectionParameters : ISelectionParameters
    {
        public int TournamentSize { get; init; } = 5;
    }
}
