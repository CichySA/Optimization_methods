namespace PFSP.Algorithms
{
    /// <summary>
    /// Marker interface for algorithm parameter objects.
    /// Concrete algorithms should define parameter classes implementing this interface.
    /// </summary>
    public interface IParameters
    {
        /// <summary>
        /// Returns a serialization-friendly representation of these parameters,
        /// with operator instances replaced by their canonical string names.
        /// </summary>
        object ToSerializableObject() => this;
    }
}
