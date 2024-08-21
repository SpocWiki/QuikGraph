using System;
using NUnit.Framework;
using QuikGraph.Algorithms.VertexCover;


namespace QuikGraph.Tests.Algorithms.VertexCover
{
    /// <summary>
    /// Tests for <see cref="MinimumVertexCoverApproximationAlgorithm{TVertex,TEdge}"/>.
    /// </summary>
    [TestFixture]
    internal sealed class MinimumVertexCoverApproximationAlgorithmTests
    {
        [Test]
        public void Constructor()
        {
            var graph = new UndirectedGraph<int, IEdge<int>>();
            var algorithm = new MinimumVertexCoverApproximationAlgorithm<int, IEdge<int>>(graph);
            algorithm.AssertAlgorithmState(graph);
            Assert.IsNull(algorithm.CoverSet);

            algorithm = new MinimumVertexCoverApproximationAlgorithm<int, IEdge<int>>(graph, new Random(123));
            algorithm.AssertAlgorithmState(graph);
            Assert.IsNull(algorithm.CoverSet);
        }

        [Test]
        public void Constructor_Throws()
        {
            // ReSharper disable ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => new MinimumVertexCoverApproximationAlgorithm<int, IEdge<int>>(null));
            Assert.Throws<ArgumentNullException>(
                () => new MinimumVertexCoverApproximationAlgorithm<int, IEdge<int>>(null, new Random(123)));
            Assert.Throws<ArgumentNullException>(
                () => new MinimumVertexCoverApproximationAlgorithm<int, IEdge<int>>(new UndirectedGraph<int, IEdge<int>>(), null));
            Assert.Throws<ArgumentNullException>(
                () => new MinimumVertexCoverApproximationAlgorithm<int, IEdge<int>>(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ObjectCreationAsStatement
        }

        [Test]
        public void Cover()
        {
            var graph = new UndirectedGraph<int, IEdge<int>>();
            var algorithm = new MinimumVertexCoverApproximationAlgorithm<int, IEdge<int>>(graph);
            algorithm.Compute();
            CollectionAssert.IsEmpty(algorithm.CoverSet);

            graph.AddVertexRange(new[] { 1, 2, 3 });
            algorithm = new MinimumVertexCoverApproximationAlgorithm<int, IEdge<int>>(graph);
            algorithm.Compute();
            CollectionAssert.IsEmpty(algorithm.CoverSet);

            graph.AddVerticesAndEdgeRange(new[]
            {
                Edge.Create(1, 2),
                Edge.Create(2, 2),
                Edge.Create(3, 1)
            });
            algorithm = new MinimumVertexCoverApproximationAlgorithm<int, IEdge<int>>(graph, new Random(123456));
            algorithm.Compute();
            CollectionAssert.AreEquivalent(
                new[] { 1, 2 },
                algorithm.CoverSet);

            graph.AddVertex(4);
            algorithm = new MinimumVertexCoverApproximationAlgorithm<int, IEdge<int>>(graph, new Random(123456));
            algorithm.Compute();
            CollectionAssert.AreEquivalent(
                new[] { 1, 2 },
                algorithm.CoverSet);

            graph.AddVerticesAndEdgeRange(new[]
            {
                Edge.Create(5, 2)
            });
            algorithm = new MinimumVertexCoverApproximationAlgorithm<int, IEdge<int>>(graph, new Random(123456));
            algorithm.Compute();
            CollectionAssert.AreEquivalent(
                new[] { 1, 2 },
                algorithm.CoverSet);

            graph.AddVerticesAndEdgeRange(new[]
            {
                Edge.Create(6, 7),
                Edge.Create(7, 8),
                Edge.Create(9, 8)
            });
            algorithm = new MinimumVertexCoverApproximationAlgorithm<int, IEdge<int>>(graph, new Random(123456));
            algorithm.Compute();
            CollectionAssert.AreEquivalent(
                new[] { 2, 3, 7, 9 },
                algorithm.CoverSet);

            // Other seed give other results
            algorithm = new MinimumVertexCoverApproximationAlgorithm<int, IEdge<int>>(graph, new Random(456789));
            algorithm.Compute();
            CollectionAssert.AreEquivalent(
                new[] { 1, 2, 7, 8 },
                algorithm.CoverSet);
        }
    }
}