using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Condensation;

namespace QuikGraph.Tests.Algorithms.Condensation
{
    /// <summary>
    /// Base class for condensation algorithms.
    /// </summary>
    internal abstract class CondensationGraphAlgorithmTestsBase
    {
        /// <summary> Checks that the Sum of the vertex count of the condensed graph is equal to the original graph's vertex count. </summary>
        protected static void CheckVertexCount<TVertex, TEdge>(
            [NotNull] IVertexSet<TVertex> graph,
            [NotNull] IVertexSet<AdjacencyGraph<TVertex, TEdge>> condensedGraph)
            where TEdge : IEdge<TVertex>
        {
            int count = condensedGraph.Vertices.Sum(vertices => vertices.VertexCount);
            Assert.AreEqual(graph.VertexCount, count, $"{nameof(graph.VertexCount)} does not match.");
        }

        /// <summary> Checks that the Sum of the edge count of the condensed graph is equal to the original graph's edge count. </summary>
        protected static void CheckEdgeCount<TVertex, TEdge>(
            [NotNull] IEdgeSet<TVertex, TEdge> graph,
            [NotNull] IEdgeListGraph<AdjacencyGraph<TVertex, TEdge>, CondensedEdge<TVertex, TEdge, AdjacencyGraph<TVertex, TEdge>>> condensedGraph)
            where TEdge : IEdge<TVertex>
        {
            // Check edge count
            int count = condensedGraph.Edges.Sum(edges => edges.Edges.Count)
                      + condensedGraph.Vertices.Sum(vertices => vertices.EdgeCount);
            Assert.AreEqual(graph.EdgeCount, count, $"{nameof(graph.EdgeCount)} does not match.");
        }

        protected static void CheckDAG<TVertex, TEdge>(
            [NotNull] IVertexAndEdgeListGraph<AdjacencyGraph<TVertex, TEdge>, CondensedEdge<TVertex, TEdge, AdjacencyGraph<TVertex, TEdge>>> condensedGraph)
            where TEdge : IEdge<TVertex>
        {
            Assert.IsTrue(condensedGraph.IsDirectedAcyclicGraph());
        }

    }
}