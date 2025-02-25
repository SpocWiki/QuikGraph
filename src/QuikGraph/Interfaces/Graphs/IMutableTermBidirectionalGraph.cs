namespace QuikGraph
{
    /// <summary>
    /// A mutable directed graph with vertices of type <typeparamref name="TVertex"/>
    /// and terminal edges of type <typeparamref name="TEdge"/>, that is efficient
    /// to traverse both in and out edges.
    /// </summary>
    public interface IMutableTermBidirectionalGraph<TVertex, TEdge>
        : ITermBidirectionalGraph<TVertex, TEdge>
        , IMutableBidirectionalGraph<TVertex, TEdge>
        where TEdge : ITermEdge<TVertex>
    {
    }
}