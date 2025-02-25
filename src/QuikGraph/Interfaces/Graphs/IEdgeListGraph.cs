namespace QuikGraph
{
    /// <summary>
    /// A graph with vertices of type <typeparamref name="TVertex"/>
    /// and edges of type <typeparamref name="TEdge"/> whose edges can be enumerated.
    /// </summary>
    public interface IEdgeListGraph<TVertex, TEdge> : IGraph<TVertex, TEdge>, IEdgeSet<TVertex, TEdge>, IVertexSet<TVertex>
        where TEdge : IEdge<TVertex>
    {
    }
}