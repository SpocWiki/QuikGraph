using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using NUnit.Framework;
using QuikGraph.Algorithms.ConnectedComponents;
using QuikGraph.Helpers;

namespace QuikGraph.Tests.Algorithms.ConnectedComponents
{
    /// <summary>
    /// Tests for <see cref="WeaklyConnectedComponentsAlgorithm{TVertex,TEdge}"/>.
    /// </summary>
    [TestFixture]
    internal sealed partial class WeaklyConnectedComponentsAlgorithmTests
    {
        private static readonly TextWriter Writer = new StreamWriter(@"C:\_tmp\ConnComp.txt");

        [TestCaseSource(typeof(TestGraphFactory), nameof(TestGraphFactory.GetNamedAdjacencyGraphs_All))]
        public void RunWeaklyConnectedComponentsAndCheck(KeyValuePair<string, AdjacencyGraph<string, Edge<string>>> namedGraph)
        {
            var graph = namedGraph.Value;
            var algorithm = graph.CreateWeaklyConnectedComponentsAlgorithm();
            algorithm.Compute();

            if (GraphRoots.TryGetValue(namedGraph.Key, out var expected))
            {
                Assert.IsTrue(algorithm.ComponentNo.IsEqualTo(expected));
            }
            else
            {
                Writer.Write("{ \"" + namedGraph.Key + "\", new Dictionary<string, int> { ");
                algorithm.ComponentNo.WriteDict(Writer);
                Writer.WriteLine(" } },");
                Writer.Flush();
            }

            Assert.AreEqual(graph.VertexCount, algorithm.ComponentNo.Count);
            if (graph.VertexCount == 0)
            {
                Assert.IsTrue(algorithm.ComponentCount == 0);
                return;
            }

            Assert.Positive(algorithm.ComponentCount);
            Assert.LessOrEqual(algorithm.ComponentCount, graph.VertexCount);
            foreach (var pair in algorithm.ComponentNo)
            {
                Assert.GreaterOrEqual(pair.Value, 0);
                Assert.IsTrue(pair.Value < algorithm.ComponentCount, $"{pair.Value} < {algorithm.ComponentCount}");
            }

            foreach (string vertex in graph.Vertices)
            {
                foreach (var edge in graph.OutEdges(vertex))
                {
                    Assert.AreEqual(algorithm.ComponentNo[edge.Source], algorithm.ComponentNo[edge.Target]);
                }
            }
        }

        [Test]
        public void Constructor()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var components = new Dictionary<int, int>();
            var algorithm = graph.CreateWeaklyConnectedComponentsAlgorithm();
            AssertAlgorithmProperties(algorithm, graph);

            algorithm = graph.CreateWeaklyConnectedComponentsAlgorithm(components);
            AssertAlgorithmProperties(algorithm, graph);

            algorithm = graph.CreateWeaklyConnectedComponentsAlgorithm(components);
            AssertAlgorithmProperties(algorithm, graph);

            #region Local function

            void AssertAlgorithmProperties<TVertex, TEdge>(
                WeaklyConnectedComponentsAlgorithm<TVertex, TEdge> algo,
                IVertexListGraph<TVertex, TEdge> g)
                where TEdge : IEdge<TVertex>
            {
                algo.AssertAlgorithmState(g);
                Assert.AreEqual(0, algo.ComponentCount);
                CollectionAssert.IsEmpty(algo.ComponentNo);
                CollectionAssert.IsEmpty(algo.Graphs);
            }

            #endregion
        }

        [Test]
        public void Constructor_Throws()
        {
            var adjacencyGraph = new AdjacencyGraph<int, IEdge<int>>();
            var components = new Dictionary<int, int>();
            IVertexListGraph<int, IEdge<int>> nullGraph = null;

            // ReSharper disable ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => nullGraph.CreateWeaklyConnectedComponentsAlgorithm());

            _ = adjacencyGraph.CreateWeaklyConnectedComponentsAlgorithm();
            Assert.Throws<ArgumentNullException>(
                () => nullGraph.CreateWeaklyConnectedComponentsAlgorithm(components));
            Assert.Throws<ArgumentNullException>(
                () => nullGraph.CreateWeaklyConnectedComponentsAlgorithm());

            _ = adjacencyGraph.CreateWeaklyConnectedComponentsAlgorithm();
            Assert.Throws<ArgumentNullException>(
                () => nullGraph.CreateWeaklyConnectedComponentsAlgorithm(components));
            Assert.Throws<ArgumentNullException>(
                () => nullGraph.CreateWeaklyConnectedComponentsAlgorithm());
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ObjectCreationAsStatement
        }

        [Test]
        public void OneComponent()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                Edge.Create(1, 2),
                Edge.Create(1, 3),
                Edge.Create(2, 3),
                Edge.Create(4, 2),
                Edge.Create(4, 3)
            ]);

            var algorithm = graph.CreateWeaklyConnectedComponentsAlgorithm();
            algorithm.Compute();

            Assert.AreEqual(1, algorithm.ComponentCount);
            CollectionAssert.AreEquivalent(
                new Dictionary<int, int>
                {
                    [1] = 0,
                    [2] = 0,
                    [3] = 0,
                    [4] = 0
                },
                algorithm.ComponentNo);
            Assert.AreEqual(1, algorithm.Graphs.Length);
            CollectionAssert.AreEquivalent(
                new[] { 1, 2, 3, 4 },
                algorithm.Graphs[0].Vertices);
        }

        [Test]
        public void TwoComponents()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                Edge.Create(1, 2),
                Edge.Create(1, 3),
                Edge.Create(2, 3),
                Edge.Create(4, 2),
                Edge.Create(4, 3),

                Edge.Create(5, 6),
                Edge.Create(5, 7),
                Edge.Create(7, 6)
            ]);

            var algorithm = graph.CreateWeaklyConnectedComponentsAlgorithm();
            algorithm.Compute();

            Assert.AreEqual(2, algorithm.ComponentCount);
            CollectionAssert.AreEquivalent(
                new Dictionary<int, int>
                {
                    [1] = 0,
                    [2] = 0,
                    [3] = 0,
                    [4] = 0,
                    [5] = 1,
                    [6] = 1,
                    [7] = 1
                },
                algorithm.ComponentNo);
            Assert.AreEqual(2, algorithm.Graphs.Length);
            CollectionAssert.AreEquivalent(
                new[] { 1, 2, 3, 4  },
                algorithm.Graphs[0].Vertices);
            CollectionAssert.AreEquivalent(
                new[] { 5, 6, 7 },
                algorithm.Graphs[1].Vertices);
        }

        [Test]
        public void MultipleComponents()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                Edge.Create(1, 2),
                Edge.Create(1, 3),
                Edge.Create(2, 3),
                Edge.Create(4, 2),
                Edge.Create(4, 3),

                Edge.Create(5, 6),
                Edge.Create(5, 7),
                Edge.Create(7, 6),

                Edge.Create(8, 9)
            ]);
            graph.AddVertex(10);

            var algorithm = graph.CreateWeaklyConnectedComponentsAlgorithm();
            algorithm.Compute();

            Assert.AreEqual(4, algorithm.ComponentCount);
            CollectionAssert.AreEquivalent(
                new Dictionary<int, int>
                {
                    [1] = 0,
                    [2] = 0,
                    [3] = 0,
                    [4] = 0,
                    [5] = 1,
                    [6] = 1,
                    [7] = 1,
                    [8] = 2,
                    [9] = 2,
                    [10] = 3
                },
                algorithm.ComponentNo);
            Assert.AreEqual(4, algorithm.Graphs.Length);
            CollectionAssert.AreEquivalent(
                new[] { 1, 2, 3, 4 },
                algorithm.Graphs[0].Vertices);
            CollectionAssert.AreEquivalent(
                new[] { 5, 6, 7 },
                algorithm.Graphs[1].Vertices);
            CollectionAssert.AreEquivalent(
                new[] { 8, 9 },
                algorithm.Graphs[2].Vertices);
            CollectionAssert.AreEquivalent(
                new[] { 10 },
                algorithm.Graphs[3].Vertices);
        }
    }
}