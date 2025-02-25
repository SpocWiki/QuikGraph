using System.Collections.Generic;
using JetBrains.Annotations;

namespace QuikGraph.Algorithms
{
    /// <summary>
    /// Represents a storage of edges colorization state.
    /// </summary>
    public interface IEdgeColorizerAlgorithm<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        /// <summary>
        /// Treated edges with their colors (colorized edges).
        /// </summary>
        [NotNull]
        IDictionary<TEdge, GraphColor> EdgesColors { get; }
    }
}