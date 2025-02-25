namespace QuikGraph
{
    /// <summary>
    /// A directed graph with vertices of type <typeparamref name="TVertex"/>
    /// and edges of type <typeparamref name="TEdge"/> that can be enumerated efficiently.
    /// </summary>
    public interface IVertexAndEdgeListGraph<TVertex, TEdge> 
        : IVertexListGraph<TVertex, TEdge>
        , IEdgeListGraph<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
    }
}