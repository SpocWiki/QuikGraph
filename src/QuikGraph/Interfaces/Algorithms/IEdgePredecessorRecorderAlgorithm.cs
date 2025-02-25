namespace QuikGraph.Algorithms
{
    /// <summary>
    /// An algorithm that exposes events to compute edges predecessors.
    /// </summary>
    public interface IEdgePredecessorRecorderAlgorithm<TVertex, out TEdge>
        where TEdge : IEdge<TVertex>
    {
        /// <summary>
        /// Fired when an edge is discovered.
        /// </summary>
        event EdgeEdgeAction<TVertex, TEdge> DiscoverTreeEdge;

        /// <summary>
        /// Fired when an edge is fully treated.
        /// </summary>
        event EdgeAction<TVertex, TEdge> FinishEdge;
    }
}