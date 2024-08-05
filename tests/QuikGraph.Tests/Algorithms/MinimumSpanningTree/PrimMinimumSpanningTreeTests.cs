using System;
using NUnit.Framework;
using QuikGraph.Algorithms.MinimumSpanningTree;
using static QuikGraph.Tests.Algorithms.AlgorithmTestHelpers;

namespace QuikGraph.Tests.Algorithms.MinimumSpanningTree
{
    /// <summary>
    /// Tests for <see cref="PrimMinimumSpanningTreeAlgorithm{TVertex,TEdge}"/>.
    /// </summary>
    [TestFixture]
    internal sealed class PrimMinimumSpanningTreeTests : MinimumSpanningTreeTestsBase
    {
        [Test]
        public void Constructor()
        {
            var graph = new UndirectedGraph<int, IEdge<int>>();
            var algorithm = new PrimMinimumSpanningTreeAlgorithm<int, IEdge<int>>(graph, _ => 1.0);
            algorithm.AssertAlgorithmState(graph);

            algorithm = new PrimMinimumSpanningTreeAlgorithm<int, IEdge<int>>(null, graph, _ => 1.0);
            algorithm.AssertAlgorithmState(graph);
        }

        [Test]
        public void Constructor_Throws()
        {
            var graph = new UndirectedGraph<int, IEdge<int>>();

            // ReSharper disable ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => new PrimMinimumSpanningTreeAlgorithm<int, IEdge<int>>(null, _ => 1.0));
            Assert.Throws<ArgumentNullException>(
                () => new PrimMinimumSpanningTreeAlgorithm<int, IEdge<int>>(graph, null));
            Assert.Throws<ArgumentNullException>(
                () => new PrimMinimumSpanningTreeAlgorithm<int, IEdge<int>>(null, null));

            Assert.Throws<ArgumentNullException>(
                () => new PrimMinimumSpanningTreeAlgorithm<int, IEdge<int>>(null, null, _ => 1.0));
            Assert.Throws<ArgumentNullException>(
                () => new PrimMinimumSpanningTreeAlgorithm<int, IEdge<int>>(null, graph, null));
            Assert.Throws<ArgumentNullException>(
                () => new PrimMinimumSpanningTreeAlgorithm<int, IEdge<int>>(null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ObjectCreationAsStatement
        }

        [Test]
        public void Prim()
        {
            // Prim 10, 50, 100, 200, 300, 400
            UndirectedGraph<string, TaggedEdge<string, double>> graph = GetUndirectedCompleteGraph(10);
            PrimSpanningTree(graph, x => x.Tag);

            graph = GetUndirectedCompleteGraph(50);
            PrimSpanningTree(graph, x => x.Tag);

            graph = GetUndirectedCompleteGraph(100);
            PrimSpanningTree(graph, x => x.Tag);

            graph = GetUndirectedCompleteGraph(200);
            PrimSpanningTree(graph, x => x.Tag);

            graph = GetUndirectedCompleteGraph(300);
            PrimSpanningTree(graph, x => x.Tag);

            graph = GetUndirectedCompleteGraph(400);
            PrimSpanningTree(graph, x => x.Tag);
        }

        [Test]
        public void PrimMinimumSpanningTree()
        {
            foreach (UndirectedGraph<string, Edge<string>> graph in TestGraphFactory.GetUndirectedGraphs_All())
                Prim(graph);
        }
    }
}