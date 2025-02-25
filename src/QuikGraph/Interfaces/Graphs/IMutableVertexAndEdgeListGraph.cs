namespace QuikGraph
{
    /// <summary>
    /// A mutable vertex and edge list graph with vertices of type
    /// <typeparamref name="TVertex"/> and edges of type <typeparamref name="TEdge"/>.
    /// </summary>
    public interface IMutableVertexAndEdgeListGraph<TVertex, TEdge>
        : IMutableVertexListGraph<TVertex, TEdge>
        , IMutableVertexAndEdgeSet<TVertex, TEdge>
        , IVertexAndEdgeListGraph<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
    }
}