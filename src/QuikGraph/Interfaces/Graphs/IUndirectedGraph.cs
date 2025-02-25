namespace QuikGraph
{
    /// <summary>
    /// An undirected graph with vertices of type <typeparamref name="TVertex"/>
    /// and edges of type <typeparamref name="TEdge"/>.
    /// </summary>
    public interface IUndirectedGraph<TVertex, TEdge>
        : IImplicitUndirectedGraph<TVertex, TEdge>
        , IEdgeListGraph<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
    }
}