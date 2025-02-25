namespace QuikGraph.Algorithms.MinimumSpanningTree
{
    /// <summary>
    /// Represents a minimum spanning tree algorithm.
    /// </summary>
    public interface IMinimumSpanningTreeAlgorithm<TVertex, TEdge>
        : IAlgorithm<IUndirectedGraph<TVertex, TEdge>>
        , ITreeBuilderAlgorithm<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
    }
}