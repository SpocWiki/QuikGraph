using JetBrains.Annotations;

namespace QuikGraph.Algorithms
{
    /// <summary> Represents an algorithm to run on the <see cref="VisitedGraph"/>. </summary>
    /// <typeparam name="TGraph">Graph type.</typeparam>
    public interface IAlgorithm<out TGraph> : IComputation
    {
        /// <summary> The graph to visit/transform with this algorithm. </summary>
        [NotNull]
        TGraph VisitedGraph { get; }
    }
}