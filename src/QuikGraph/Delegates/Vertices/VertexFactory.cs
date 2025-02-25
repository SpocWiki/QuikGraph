using JetBrains.Annotations;

namespace QuikGraph
{
    /// <summary>
    /// Delegate to create a vertex.
    /// </summary>
    /// <returns>The created vertex.</returns>
    [NotNull]
    public delegate TVertex VertexFactory<out TVertex>();
}