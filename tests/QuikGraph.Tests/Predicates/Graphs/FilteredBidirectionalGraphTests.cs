﻿using System;
using NUnit.Framework;
using QuikGraph.Predicates;
using static QuikGraph.Tests.GraphTestHelpers;

namespace QuikGraph.Tests.Predicates
{
    /// <summary>
    /// Tests for <see cref="FilteredBidirectionalGraph{TVertex,TEdge,TGraph}"/>.
    /// </summary>
    [TestFixture]
    internal sealed class FilteredBidirectionalGraphTests : FilteredGraphTestsBase
    {
        [Test]
        public void Construction()
        {
            VertexPredicate<int> vertexPredicate = _ => true;
            EdgePredicate<int, IEdge<int>> edgePredicate = _ => true;

            var graph = new BidirectionalGraph<int, IEdge<int>>();
            var filteredGraph = new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                graph,
                vertexPredicate,
                edgePredicate);
            AssertGraphProperties(filteredGraph, graph);

            graph = new BidirectionalGraph<int, IEdge<int>>(false);
            filteredGraph = new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                graph,
                vertexPredicate,
                edgePredicate);
            AssertGraphProperties(filteredGraph, graph, false);

            #region Local function

            void AssertGraphProperties<TVertex, TEdge, TGraph>(
                FilteredBidirectionalGraph<TVertex, TEdge, TGraph> g,
                TGraph expectedGraph,
                bool parallelEdges = true)
                where TEdge : IEdge<TVertex>
                where TGraph : IBidirectionalGraph<TVertex, TEdge>
            {
                Assert.AreSame(expectedGraph, g.BaseGraph);
                Assert.IsTrue(g.IsDirected);
                Assert.AreEqual(parallelEdges, g.AllowParallelEdges);
                Assert.AreSame(vertexPredicate, g.VertexPredicate);
                Assert.AreSame(edgePredicate, g.EdgePredicate);
                AssertEmptyGraph(g);
            }

            #endregion
        }

        [Test]
        public void Construction_Throws()
        {
            // ReSharper disable ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                    new BidirectionalGraph<int, IEdge<int>>(),
                    _ => true,
                    null));

            Assert.Throws<ArgumentNullException>(
                () => new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                    new BidirectionalGraph<int, IEdge<int>>(),
                    null,
                    _ => true));

            Assert.Throws<ArgumentNullException>(
                () => new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                    null,
                    _ => true,
                    _ => true));

            Assert.Throws<ArgumentNullException>(
                () => new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                    new BidirectionalGraph<int, IEdge<int>>(),
                    null,
                    null));

            Assert.Throws<ArgumentNullException>(
                () => new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                    null,
                    _ => true,
                    null));

            Assert.Throws<ArgumentNullException>(
                () => new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                    null,
                    null,
                    _ => true));

            Assert.Throws<ArgumentNullException>(
                () => new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                    null,
                    null,
                    null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ObjectCreationAsStatement
        }

        #region Vertices & Edges

        [Test]
        public void Vertices()
        {
            var wrappedGraph = new BidirectionalGraph<int, IEdge<int>>();
            Vertices_Test(
                wrappedGraph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        wrappedGraph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void Edges()
        {
            var wrappedGraph = new BidirectionalGraph<int, IEdge<int>>();
            Edges_Test(
                wrappedGraph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        wrappedGraph,
                        vertexPredicate,
                        edgePredicate));
        }

        #endregion

        #region Contains Vertex

        [Test]
        public void ContainsVertex()
        {
            var graph = new BidirectionalGraph<int, IEdge<int>>();
            ContainsVertex_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void ContainsVertex_Throws()
        {
            var filteredGraph = new FilteredBidirectionalGraph<TestVertex, Edge<TestVertex>, BidirectionalGraph<TestVertex, Edge<TestVertex>>>(
                new BidirectionalGraph<TestVertex, Edge<TestVertex>>(),
                _ => true,
                _ => true);
            ContainsVertex_Throws_Test(filteredGraph);
        }

        #endregion

        #region Contains Edge

        [Test]
        public void ContainsEdge()
        {
            var graph = new BidirectionalGraph<int, IEdge<int>>();
            ContainsEdge_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void ContainsEdge_EquatableEdge()
        {
            var graph = new BidirectionalGraph<int, EquatableEdge<int>>();
            ContainsEdge_EquatableEdge_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, EquatableEdge<int>, BidirectionalGraph<int, EquatableEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void ContainsEdge_SourceTarget()
        {
            var graph = new BidirectionalGraph<int, IEdge<int>>();
            ContainsEdge_SourceTarget_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void ContainsEdge_Throws()
        {
            var filteredGraph = new FilteredBidirectionalGraph<TestVertex, Edge<TestVertex>, BidirectionalGraph<TestVertex, Edge<TestVertex>>>(
                new BidirectionalGraph<TestVertex, Edge<TestVertex>>(),
                _ => true,
                _ => true);
            ContainsEdge_NullThrows_Test(filteredGraph);
            ContainsEdge_SourceTarget_Throws_Test(filteredGraph);
        }

        #endregion

        #region Out Edges

        [Test]
        public void OutEdge()
        {
            var graph = new BidirectionalGraph<int, IEdge<int>>();
            OutEdge_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void OutEdge_Throws()
        {
            var graph1 = new BidirectionalGraph<int, IEdge<int>>();
            OutEdge_Throws_Test(
                graph1,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        graph1,
                        vertexPredicate,
                        edgePredicate));

            var graph2 = new BidirectionalGraph<TestVertex, Edge<TestVertex>>();
            var filteredGraph2 = new FilteredBidirectionalGraph<TestVertex, Edge<TestVertex>, BidirectionalGraph<TestVertex, Edge<TestVertex>>>(
                graph2,
                _ => true,
                _ => true);
            OutEdge_NullThrows_Test(filteredGraph2);
        }

        [Test]
        public void OutEdges()
        {
            var graph = new BidirectionalGraph<int, IEdge<int>>();
            OutEdges_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void OutEdges_Throws()
        {
            var graph1 = new BidirectionalGraph<EquatableTestVertex, Edge<EquatableTestVertex>>();
            var filteredGraph1 = new FilteredBidirectionalGraph
                <
                    EquatableTestVertex,
                    Edge<EquatableTestVertex>,
                    BidirectionalGraph<EquatableTestVertex, Edge<EquatableTestVertex>>
                >(
                graph1,
                _ => true,
                _ => true);
            OutEdges_NullThrows_Test(filteredGraph1);
            OutEdges_Throws_Test(filteredGraph1);

            var graph2 = new BidirectionalGraph<int, IEdge<int>>();
            var filteredGraph2 = new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                graph2,
                vertex => vertex < 4,
                _ => true);

            graph2.AddVertexRange([1, 2, 3, 4, 5]);
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            Assert.Throws<VertexNotFoundException>(() => filteredGraph2.OutEdges(4));
            Assert.Throws<VertexNotFoundException>(() => filteredGraph2.OutEdges(5));
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        #endregion

        #region In Edges

        [Test]
        public void InEdge()
        {
            var graph = new BidirectionalGraph<int, IEdge<int>>();
            InEdge_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void InEdge_Throws()
        {
            var graph1 = new BidirectionalGraph<int, IEdge<int>>();
            InEdge_Throws_Test(
                graph1,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        graph1,
                        vertexPredicate,
                        edgePredicate));

            var graph2 = new BidirectionalGraph<TestVertex, Edge<TestVertex>>();
            var filteredGraph2 = new FilteredBidirectionalGraph<TestVertex, Edge<TestVertex>, BidirectionalGraph<TestVertex, Edge<TestVertex>>>(
                graph2,
                _ => true,
                _ => true);
            InEdge_NullThrows_Test(filteredGraph2);
        }

        [Test]
        public void InEdges()
        {
            var graph = new BidirectionalGraph<int, IEdge<int>>();
            InEdges_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void InEdges_Throws()
        {
            var graph1 = new BidirectionalGraph<EquatableTestVertex, Edge<EquatableTestVertex>>();
            var filteredGraph1 = new FilteredBidirectionalGraph
            <
                EquatableTestVertex,
                Edge<EquatableTestVertex>,
                BidirectionalGraph<EquatableTestVertex, Edge<EquatableTestVertex>>
            >(
                graph1,
                _ => true,
                _ => true);
            InEdges_NullThrows_Test(filteredGraph1);
            InEdges_Throws_Test(filteredGraph1);

            var graph2 = new BidirectionalGraph<int, IEdge<int>>();
            var filteredGraph2 = new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                graph2,
                vertex => vertex < 4,
                _ => true);

            graph2.AddVertexRange([1, 2, 3, 4, 5]);
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            Assert.Throws<VertexNotFoundException>(() => filteredGraph2.InEdges(4));
            Assert.Throws<VertexNotFoundException>(() => filteredGraph2.InEdges(5));
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        #endregion

        #region Degree

        [Test]
        public void Degree()
        {
            var graph = new BidirectionalGraph<int, IEdge<int>>();
            Degree_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void Degree_Throws()
        {
            var graph = new BidirectionalGraph<EquatableTestVertex, Edge<EquatableTestVertex>>();
            var filteredGraph = new FilteredBidirectionalGraph
            <
                EquatableTestVertex,
                Edge<EquatableTestVertex>,
                BidirectionalGraph<EquatableTestVertex, Edge<EquatableTestVertex>>
            >(
                graph,
                _ => true,
                _ => true);
            Degree_Throws_Test(filteredGraph);
        }

        #endregion

        #region Try Get Edges

        [Test]
        public void TryGetEdge()
        {
            var graph = new BidirectionalGraph<int, IEdge<int>>();
            TryGetEdge_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void TryGetEdge_Throws()
        {
            var filteredGraph = new FilteredBidirectionalGraph<TestVertex, Edge<TestVertex>, BidirectionalGraph<TestVertex, Edge<TestVertex>>>(
                new BidirectionalGraph<TestVertex, Edge<TestVertex>>(),
                _ => true,
                _ => true);
            TryGetEdge_Throws_Test(filteredGraph);
        }

        [Test]
        public void TryGetEdges()
        {
            var graph = new BidirectionalGraph<int, IEdge<int>>();
            TryGetEdges_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void TryGetEdges_Throws()
        {
            var filteredGraph = new FilteredBidirectionalGraph<TestVertex, Edge<TestVertex>, BidirectionalGraph<TestVertex, Edge<TestVertex>>>(
                new BidirectionalGraph<TestVertex, Edge<TestVertex>>(),
                _ => true,
                _ => true);
            TryGetEdges_Throws_Test(filteredGraph);
        }

        [Test]
        public void TryGetOutEdges()
        {
            var graph = new BidirectionalGraph<int, IEdge<int>>();
            TryGetOutEdges_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void TryGetOutEdges_Throws()
        {
            TryGetOutEdges_Throws_Test(
                new FilteredBidirectionalGraph<TestVertex, Edge<TestVertex>, BidirectionalGraph<TestVertex, Edge<TestVertex>>>(
                    new BidirectionalGraph<TestVertex, Edge<TestVertex>>(),
                    _ => true,
                    _ => true));
        }

        [Test]
        public void TryGetInEdges()
        {
            var graph = new BidirectionalGraph<int, IEdge<int>>();
            TryGetInEdges_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredBidirectionalGraph<int, IEdge<int>, BidirectionalGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void TryGetInEdges_Throws()
        {
            TryGetInEdges_Throws_Test(
                new FilteredBidirectionalGraph<TestVertex, Edge<TestVertex>, BidirectionalGraph<TestVertex, Edge<TestVertex>>>(
                    new BidirectionalGraph<TestVertex, Edge<TestVertex>>(),
                    _ => true,
                    _ => true));
        }

        #endregion
    }
}