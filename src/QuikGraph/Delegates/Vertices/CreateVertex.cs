using JetBrains.Annotations;

namespace QuikGraph
{
    /// <summary>
    /// Delegate to create a vertex in a graph.
    /// </summary>
    /// <param name="graph">Graph in with adding the vertex.</param>
    /// <returns>The created vertex.</returns>
    [NotNull]
    public delegate TVertex CreateVertexDelegate<TVertex, TEdge>([NotNull] IVertexListGraph<TVertex, TEdge> graph)
        where TEdge : IEdge<TVertex>;
}