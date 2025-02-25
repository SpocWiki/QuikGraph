using JetBrains.Annotations;

namespace QuikGraph.Algorithms
{
    /// <summary>
    /// Represents an algorithm to run on a graph.
    /// </summary>
    public interface IAlgorithm<out TGraph> : IComputation
    {
        /// <summary>
        /// Gets the graph to visit with this algorithm.
        /// </summary>
        [NotNull]
        TGraph VisitedGraph { get; }
    }
}