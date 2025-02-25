using System.Collections.Generic;
using System.Linq;

namespace QuikGraph.Algorithms.RandomWalks
{
    /// <summary>
    /// Normalized Markov chain implementation.
    /// </summary>
    public sealed class NormalizedMarkovEdgeChain<TVertex, TEdge> : MarkovEdgeChainBase<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        /// <inheritdoc />
        public override bool TryGetSuccessor(IImplicitGraph<TVertex, TEdge> graph, TVertex vertex, out TEdge successor)
        {
            int outDegree = graph.OutDegree(vertex);
            if (outDegree > 0)
            {
                int index = Rand.Next(0, outDegree);
                successor = graph.OutEdge(vertex, index);
                return true;
            }

            successor = default(TEdge);
            return false;
        }

        /// <inheritdoc />
        public override bool TryGetSuccessor(IEnumerable<TEdge> edges, TVertex vertex, out TEdge successor)
        {
            TEdge[] edgeArray = edges.ToArray();
            if (edgeArray.Length > 0)
            {
                int index = Rand.Next(0, edgeArray.Length);
                successor = edgeArray[index];
                return true;
            }

            successor = default(TEdge);
            return false;
        }
    }
}