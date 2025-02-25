namespace QuikGraph
{
    /// <summary>
    /// A directed graph data structure where out-edges can be traversed,
    /// i.e. a vertex set + implicit graph.
    /// </summary>
    public interface IVertexListGraph<TVertex, TEdge>
        : IIncidenceGraph<TVertex, TEdge>
        , IVertexSet<TVertex>
        where TEdge : IEdge<TVertex>
    {
    }
}