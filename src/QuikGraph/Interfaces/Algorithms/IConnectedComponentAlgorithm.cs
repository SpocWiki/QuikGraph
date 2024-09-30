using System.Collections.Generic;
using JetBrains.Annotations;

namespace QuikGraph.Algorithms
{
    /// <summary> Represents an algorithm dealing with graph connected components. </summary>
    public interface IConnectedComponentAlgorithm<TVertex, TEdge, out TGraph> : IAlgorithm<TGraph>
        where TGraph : IGraph<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        /// <summary> Number of components, i.e. distinct Values of <see cref="ComponentIndex"/>. </summary>
        int ComponentCount { get; }

        /// <summary> Graph components as a Dictionary from <typeparamref name="TVertex"/> to 0-based Component-Index. </summary>
        [NotNull]
        IDictionary<TVertex, int> ComponentIndex { get; }
    }
}