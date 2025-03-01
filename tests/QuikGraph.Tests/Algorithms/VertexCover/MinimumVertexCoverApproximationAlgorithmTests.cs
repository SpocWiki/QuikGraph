﻿using System;
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
            var algorithm = graph.CreateMinimumVertexCoverApproximationAlgorithm();
            algorithm.AssertAlgorithmState(graph);
            Assert.IsNull(algorithm.CoverSet);

            algorithm = graph.CreateMinimumVertexCoverApproximationAlgorithm(new Random(123));
            algorithm.AssertAlgorithmState(graph);
            Assert.IsNull(algorithm.CoverSet);
        }

        [Test]
        public void Constructor_Throws()
        {
            var graph = new UndirectedGraph<int, IEdge<int>>();
            // ReSharper disable ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            IUndirectedGraph<int, IEdge<int>> nullGraph = null;
            Assert.Throws<ArgumentNullException>(
                () => nullGraph.CreateMinimumVertexCoverApproximationAlgorithm());
            Assert.Throws<ArgumentNullException>(
                () => nullGraph.CreateMinimumVertexCoverApproximationAlgorithm(new Random(123)));
            _ = graph.CreateMinimumVertexCoverApproximationAlgorithm(null);
            Assert.Throws<ArgumentNullException>(
                () => nullGraph.CreateMinimumVertexCoverApproximationAlgorithm(null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ObjectCreationAsStatement
        }

        [Test]
        public void Cover()
        {
            var graph = new UndirectedGraph<int, IEdge<int>>();
            var algorithm = graph.CreateMinimumVertexCoverApproximationAlgorithm();
            algorithm.Compute();
            CollectionAssert.IsEmpty(algorithm.CoverSet);

            graph.AddVertexRange([1, 2, 3]);
            algorithm = graph.CreateMinimumVertexCoverApproximationAlgorithm();
            algorithm.Compute();
            CollectionAssert.IsEmpty(algorithm.CoverSet);

            graph.AddVerticesAndEdgeRange(
            [
                Edge.Create(1, 2),
                Edge.Create(2, 2),
                Edge.Create(3, 1)
            ]);
            algorithm = graph.CreateMinimumVertexCoverApproximationAlgorithm(new Random(123456));
            algorithm.Compute();
            CollectionAssert.AreEquivalent(
                new[] { 1, 2 },
                algorithm.CoverSet);

            graph.AddVertex(4);
            algorithm = graph.CreateMinimumVertexCoverApproximationAlgorithm(new Random(123456));
            algorithm.Compute();
            CollectionAssert.AreEquivalent(
                new[] { 1, 2 },
                algorithm.CoverSet);

            graph.AddVerticesAndEdgeRange(
            [
                Edge.Create(5, 2)
            ]);
            algorithm = graph.CreateMinimumVertexCoverApproximationAlgorithm(new Random(123456));
            algorithm.Compute();
            CollectionAssert.AreEquivalent(
                new[] { 1, 2 },
                algorithm.CoverSet);

            graph.AddVerticesAndEdgeRange(
            [
                Edge.Create(6, 7),
                Edge.Create(7, 8),
                Edge.Create(9, 8)
            ]);
            algorithm = graph.CreateMinimumVertexCoverApproximationAlgorithm(new Random(123456));
            algorithm.Compute();
            CollectionAssert.AreEquivalent(
                new[] { 2, 3, 7, 9 },
                algorithm.CoverSet);

            // Other seed give other results
            algorithm = graph.CreateMinimumVertexCoverApproximationAlgorithm(new Random(456789));
            algorithm.Compute();
            CollectionAssert.AreEquivalent(
                new[] { 1, 2, 7, 8 },
                algorithm.CoverSet);
        }
    }
}