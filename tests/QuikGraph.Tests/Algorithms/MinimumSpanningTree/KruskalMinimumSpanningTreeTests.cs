using System;
using NUnit.Framework;
using QuikGraph.Algorithms.MinimumSpanningTree;


namespace QuikGraph.Tests.Algorithms.MinimumSpanningTree
{
    /// <summary>
    /// Tests for <see cref="KruskalMinimumSpanningTreeAlgorithm{TVertex,TEdge}"/>.
    /// </summary>
    [TestFixture]
    internal sealed class KruskalMinimumSpanningTreeTests : MinimumSpanningTreeTestsBase
    {
        [Test]
        public void Constructor()
        {
            var graph = new UndirectedGraph<int, IEdge<int>>();
            var algorithm = new KruskalMinimumSpanningTreeAlgorithm<int, IEdge<int>>(graph, _ => 1.0);
            algorithm.AssertAlgorithmState(graph);

            algorithm = new KruskalMinimumSpanningTreeAlgorithm<int, IEdge<int>>(null, graph, _ => 1.0);
            algorithm.AssertAlgorithmState(graph);
        }

        [Test]
        public void Constructor_Throws()
        {
            var graph = new UndirectedGraph<int, IEdge<int>>();

            // ReSharper disable ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => new KruskalMinimumSpanningTreeAlgorithm<int, IEdge<int>>(null, _ => 1.0));
            Assert.Throws<ArgumentNullException>(
                () => new KruskalMinimumSpanningTreeAlgorithm<int, IEdge<int>>(graph, null));
            Assert.Throws<ArgumentNullException>(
                () => new KruskalMinimumSpanningTreeAlgorithm<int, IEdge<int>>(null, null));

            Assert.Throws<ArgumentNullException>(
                () => new KruskalMinimumSpanningTreeAlgorithm<int, IEdge<int>>(null, null, _ => 1.0));
            Assert.Throws<ArgumentNullException>(
                () => new KruskalMinimumSpanningTreeAlgorithm<int, IEdge<int>>(null, graph, null));
            Assert.Throws<ArgumentNullException>(
                () => new KruskalMinimumSpanningTreeAlgorithm<int, IEdge<int>>(null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ObjectCreationAsStatement
        }

        [Test]
        public void Kruskal()
        {
            // Kruskal 10, 50, 100, 200, 300, 400
            UndirectedGraph<string, TaggedEdge<string, double>> graph = GetUndirectedCompleteGraph(10);
            KruskalSpanningTree(graph, x => x.Tag);

            graph = GetUndirectedCompleteGraph(50);
            KruskalSpanningTree(graph, x => x.Tag);

            graph = GetUndirectedCompleteGraph(100);
            KruskalSpanningTree(graph, x => x.Tag);

            graph = GetUndirectedCompleteGraph(200);
            KruskalSpanningTree(graph, x => x.Tag);

            graph = GetUndirectedCompleteGraph(300);
            KruskalSpanningTree(graph, x => x.Tag);

            graph = GetUndirectedCompleteGraph(400);
            KruskalSpanningTree(graph, x => x.Tag);
        }
    }
}