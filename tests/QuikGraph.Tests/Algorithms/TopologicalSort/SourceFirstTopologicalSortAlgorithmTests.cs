﻿using System;
using JetBrains.Annotations;
using NUnit.Framework;
using QuikGraph.Algorithms.TopologicalSort;
using static QuikGraph.Tests.QuikGraphUnitTestsHelpers;

namespace QuikGraph.Tests.Algorithms
{
    /// <summary> Tests for <see cref="SourceFirstTopologicalSortAlgorithm{TVertex,TEdge}"/>. </summary>
    [TestFixture]
    internal sealed class SourceFirstTopologicalSortAlgorithmTests
    {

        [TestCaseSource(typeof(TestGraphFactory), nameof(TestGraphFactory.GetAdjacencyGraphs_All))]
        public static void RunSourceFirstTopologicalSortAndCheck<TVertex, TEdge>([NotNull] IVertexAndEdgeListGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            var algorithm = new SourceFirstTopologicalSortAlgorithm<TVertex, TEdge>(graph);
            algorithm.Compute();

            Assert.IsNotNull(algorithm.SortedVertices);
            Assert.AreEqual(graph.VertexCount, algorithm.SortedVertices.Length);
            Assert.IsNotNull(algorithm.InDegrees);
            Assert.AreEqual(graph.VertexCount, algorithm.InDegrees.Count);
        }

        [Test]
        public void Constructor()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var algorithm = new SourceFirstTopologicalSortAlgorithm<int, IEdge<int>>(graph);
            AssertAlgorithmProperties(algorithm, graph);

            algorithm = new SourceFirstTopologicalSortAlgorithm<int, IEdge<int>>(graph, -10);
            AssertAlgorithmProperties(algorithm, graph);

            algorithm = new SourceFirstTopologicalSortAlgorithm<int, IEdge<int>>(graph, 0);
            AssertAlgorithmProperties(algorithm, graph);

            algorithm = new SourceFirstTopologicalSortAlgorithm<int, IEdge<int>>(graph, 10);
            AssertAlgorithmProperties(algorithm, graph);

            #region Local function

            void AssertAlgorithmProperties<TVertex, TEdge>(
                SourceFirstTopologicalSortAlgorithm<TVertex, TEdge> algo,
                IVertexAndEdgeListGraph<TVertex, TEdge> g)
                where TEdge : IEdge<TVertex>
            {
                algo.AssertAlgorithmState(g);
                Assert.IsNull(algo.SortedVertices);
                CollectionAssert.IsEmpty(algo.InDegrees);
            }

            #endregion
        }

        [Test]
        public void Constructor_Throws()
        {
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => new SourceFirstTopologicalSortAlgorithm<int, IEdge<int>>(null));
        }

        [Test]
        public void SimpleGraph()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
                Edge.Create(1, 2),
                Edge.Create(2, 3),
                Edge.Create(2, 6),
                Edge.Create(2, 8),
                Edge.Create(4, 2),
                Edge.Create(4, 5),
                Edge.Create(5, 6),
                Edge.Create(7, 5),
                Edge.Create(7, 8)
            );

            var algorithm = new SourceFirstTopologicalSortAlgorithm<int, IEdge<int>>(graph);
            algorithm.Compute();

            CollectionAssert.AreEqual(
                new[] { 1, 7, 4, 2, 5, 8, 3, 6 },
                algorithm.SortedVertices);
        }

        [Test]
        public void SimpleGraphOneToAnother()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
                Edge.Create(0, 1),
                Edge.Create(1, 2),
                Edge.Create(1, 3),
                Edge.Create(2, 3),
                Edge.Create(3, 4)
            );

            var algorithm = new SourceFirstTopologicalSortAlgorithm<int, IEdge<int>>(graph);
            algorithm.Compute();

            CollectionAssert.AreEqual(
                new[] { 0, 1, 2, 3, 4 },
                algorithm.SortedVertices);
        }

        [Test]
        public void ForestGraph()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
                Edge.Create(0, 1),
                Edge.Create(1, 2),
                Edge.Create(1, 3),
                Edge.Create(2, 3),
                Edge.Create(3, 4),

                Edge.Create(5, 6)
            );

            var algorithm = new SourceFirstTopologicalSortAlgorithm<int, IEdge<int>>(graph);
            algorithm.Compute();

            CollectionAssert.AreEqual(
                new[] { 0, 5, 1, 6, 2, 3, 4 },
                algorithm.SortedVertices);
        }

        [Test]
        public void GraphWithSelfEdge_Throws()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
                Edge.Create(0, 1),
                Edge.Create(1, 2),
                Edge.Create(1, 3),
                Edge.Create(2, 3),
                Edge.Create(2, 2),
                Edge.Create(3, 4)
            );

            var algorithm = new SourceFirstTopologicalSortAlgorithm<int, IEdge<int>>(graph);
            Assert.Throws<CyclicGraphException>(() => algorithm.Compute());
        }

        [Test]
        public void SourceFirstTopologicalSort_DCT8()
        {
            var graph = TestGraphFactory.LoadGraph(GetGraphFilePath("DCT8.graphml"));
            RunSourceFirstTopologicalSortAndCheck(graph);
        }

        [Test]
        public void SourceFirstTopologicalSort_Throws()
        {
            var cyclicGraph = new AdjacencyGraph<int, IEdge<int>>();
            cyclicGraph.AddVerticesAndEdgeRange(
                Edge.Create(1, 2),
                Edge.Create(2, 3),
                Edge.Create(1, 4),
                Edge.Create(3, 1)
            );

            var algorithm = new SourceFirstTopologicalSortAlgorithm<int, IEdge<int>>(cyclicGraph);
            Assert.Throws<CyclicGraphException>(() => algorithm.Compute());
        }
    }
}