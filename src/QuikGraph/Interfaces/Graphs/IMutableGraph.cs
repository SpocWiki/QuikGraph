namespace QuikGraph
{
    /// <summary>
    /// A mutable graph with vertices of type <typeparamref name="TVertex"/>
    /// and edges of type <typeparamref name="TEdge"/>.
    /// </summary>
    public interface IMutableGraph<TVertex, TEdge> : IGraph<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        /// <summary>
        /// Clears the vertex and edges.
        /// </summary>
        void Clear();
    }
}