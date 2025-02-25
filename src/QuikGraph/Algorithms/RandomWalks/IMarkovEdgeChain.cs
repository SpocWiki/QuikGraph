using System;
using JetBrains.Annotations;

namespace QuikGraph.Algorithms.RandomWalks
{
    /// <summary>
    /// A Markov edges chain.
    /// </summary>
    public interface IMarkovEdgeChain<TVertex, TEdge> : IEdgeChain<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        /// <summary>
        /// Random number generator for a Markov process to do random walks.
        /// </summary>
        [NotNull]
        Random Rand { get; set; }
    }
}