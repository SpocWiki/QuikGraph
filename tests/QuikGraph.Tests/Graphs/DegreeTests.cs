using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;

namespace QuikGraph.Tests.Graphs
{
    /// <summary> Test the degree of graphs. </summary>
    [TestFixture]
    internal sealed class DegreeTests
    {
        [TestCaseSource(typeof(TestGraphFactory), nameof(TestGraphFactory.GetBidirectionalGraphs_All))]
        public static void AssertDegreeSumEqualsTwiceEdgeCount<TVertex, TEdge>([NotNull] IBidirectionalGraph<TVertex, TEdge> graph) where TEdge : IEdge<TVertex>
        {
            int totalDegree = graph.Vertices.Sum(graph.Degree);

            Assert.AreEqual(graph.EdgeCount * 2, totalDegree);
        }

        [TestCaseSource(typeof(TestGraphFactory), nameof(TestGraphFactory.GetBidirectionalGraphs_All))]
        public static void AssertInDegreeSumEqualsEdgeCount<TVertex, TEdge>(
            [NotNull] IBidirectionalGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            int totalInDegree = graph.Vertices.Sum(graph.InDegree);

            Assert.AreEqual(graph.EdgeCount, totalInDegree);
        }

        [TestCaseSource(typeof(TestGraphFactory), nameof(TestGraphFactory.GetAdjacencyGraphs_All))]
        [TestCaseSource(typeof(TestGraphFactory), nameof(TestGraphFactory.GetBidirectionalGraphs_All))]
        public static void OutDegreeSumEqualsEdgeCount<TVertex, TEdge>([NotNull] IVertexAndEdgeListGraph<TVertex, TEdge> graph) where TEdge : IEdge<TVertex>
        {
            int totalOutDegree = graph.Vertices.Sum(graph.OutDegree);

            Assert.AreEqual(graph.EdgeCount, totalOutDegree);
        }

        [TestCaseSource(typeof(TestGraphFactory), nameof(TestGraphFactory.GetUndirectedGraphs_All))]
        public static void AssertAdjacentDegreeSumEqualsTwiceEdgeCount<TVertex, TEdge>([NotNull] IUndirectedGraph<TVertex, TEdge> graph) where TEdge : IEdge<TVertex>
        {
            int totalAdjacentDegree = graph.Vertices.Sum(graph.AdjacentDegree);

            Assert.AreEqual(graph.EdgeCount * 2, totalAdjacentDegree);
        }

    }
}