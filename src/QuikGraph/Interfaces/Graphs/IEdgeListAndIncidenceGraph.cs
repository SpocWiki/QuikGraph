namespace QuikGraph
{
    /// <summary>
    /// Represents an incidence graph whose edges can be enumerated.
    /// </summary>
    public interface IEdgeListAndIncidenceGraph<TVertex,TEdge> 
        : IEdgeListGraph<TVertex,TEdge>
        , IIncidenceGraph<TVertex,TEdge>
        where TEdge : IEdge<TVertex>
    {
    }
}