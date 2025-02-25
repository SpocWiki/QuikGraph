using JetBrains.Annotations;

namespace QuikGraph
{
    /// <summary>
    /// Delegate to perform a check on the given <paramref name="vertex"/>.
    /// </summary>
    /// <param name="vertex">Vertex to check condition.</param>
    /// <returns>True if the <paramref name="vertex"/> matches the predicate, false otherwise.</returns>
    public delegate bool VertexPredicate<in TVertex>([NotNull] TVertex vertex);
}