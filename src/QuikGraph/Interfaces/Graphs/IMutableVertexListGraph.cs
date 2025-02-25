namespace QuikGraph
{
    /// <summary>
    /// A mutable vertex list graph with vertices of type <typeparamref name="TVertex"/>
    /// and edges of type <typeparamref name="TEdge"/>.
    /// </summary>
    public interface IMutableVertexListGraph<TVertex, TEdge>
        : IMutableIncidenceGraph<TVertex, TEdge>
        , IMutableVertexSet<TVertex>
         where TEdge : IEdge<TVertex>
    {
    }
}