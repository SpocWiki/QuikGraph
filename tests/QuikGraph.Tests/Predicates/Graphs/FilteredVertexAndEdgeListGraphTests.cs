﻿using System;
using NUnit.Framework;
using QuikGraph.Predicates;
using static QuikGraph.Tests.GraphTestHelpers;

namespace QuikGraph.Tests.Predicates
{
    /// <summary>
    /// Tests for <see cref="FilteredVertexAndEdgeListGraph{TVertex,TEdge,TGraph}"/>.
    /// </summary>
    [TestFixture]
    internal sealed class FilteredVertexAndEdgeListGraphTests : FilteredGraphTestsBase
    {
        [Test]
        public void Construction()
        {
            Func<int, bool> vertexPredicate = _ => true;
            Func<IEdge<int>, bool> edgePredicate = _ => true;

            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var filteredGraph = new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                graph,
                vertexPredicate,
                edgePredicate);
            AssertGraphProperties(filteredGraph, graph);

            graph = new AdjacencyGraph<int, IEdge<int>>(false);
            filteredGraph = new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                graph,
                vertexPredicate,
                edgePredicate);
            AssertGraphProperties(filteredGraph, graph, false);

            #region Local function

            void AssertGraphProperties<TVertex, TEdge, TGraph>(
                FilteredVertexAndEdgeListGraph<TVertex, TEdge, TGraph> g,
                TGraph expectedGraph,
                bool parallelEdges = true)
                where TEdge : class, IEdge<TVertex>
                where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
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
                () => new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                    new AdjacencyGraph<int, IEdge<int>>(),
                    _ => true,
                    null));

            Assert.Throws<ArgumentNullException>(
                () => new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                    new AdjacencyGraph<int, IEdge<int>>(),
                    null,
                    _ => true));

            Assert.Throws<ArgumentNullException>(
                () => new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                    null,
                    _ => true,
                    _ => true));

            Assert.Throws<ArgumentNullException>(
                () => new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                    new AdjacencyGraph<int, IEdge<int>>(),
                    null,
                    null));

            Assert.Throws<ArgumentNullException>(
                () => new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                    null,
                    _ => true,
                    null));

            Assert.Throws<ArgumentNullException>(
                () => new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                    null,
                    null,
                    _ => true));

            Assert.Throws<ArgumentNullException>(
                () => new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
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
            var wrappedGraph = new AdjacencyGraph<int, IEdge<int>>();
            Vertices_Test(
                wrappedGraph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                        wrappedGraph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void Edges()
        {
            var wrappedGraph = new AdjacencyGraph<int, IEdge<int>>();
            Edges_Test(
                wrappedGraph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                        wrappedGraph,
                        vertexPredicate,
                        edgePredicate));
        }

        #endregion

        #region Contains Vertex

        [Test]
        public void ContainsVertex()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            ContainsVertex_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void ContainsVertex_Throws()
        {
            var filteredGraph = new FilteredVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>, AdjacencyGraph<TestVertex, Edge<TestVertex>>>(
                new AdjacencyGraph<TestVertex, Edge<TestVertex>>(),
                _ => true,
                _ => true);
            ContainsVertex_Throws_Test(filteredGraph);
        }

        #endregion

        #region Contains Edge

        [Test]
        public void ContainsEdge()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            ContainsEdge_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void ContainsEdge_EquatableEdge()
        {
            var graph = new AdjacencyGraph<int, EquatableEdge<int>>();
            ContainsEdge_EquatableEdge_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredVertexAndEdgeListGraph<int, EquatableEdge<int>, AdjacencyGraph<int, EquatableEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void ContainsEdge_SourceTarget()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            ContainsEdge_SourceTarget_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void ContainsEdge_Throws()
        {
            var filteredGraph = new FilteredVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>, AdjacencyGraph<TestVertex, Edge<TestVertex>>>(
                new AdjacencyGraph<TestVertex, Edge<TestVertex>>(),
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
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            OutEdge_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void OutEdge_Throws()
        {
            var graph1 = new AdjacencyGraph<int, IEdge<int>>();
            OutEdge_Throws_Test(
                graph1,
                (vertexPredicate, edgePredicate) =>
                    new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                        graph1,
                        vertexPredicate,
                        edgePredicate));

            var graph2 = new AdjacencyGraph<TestVertex, Edge<TestVertex>>();
            var filteredGraph2 = new FilteredVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>, AdjacencyGraph<TestVertex, Edge<TestVertex>>>(
                graph2,
                _ => true,
                _ => true);
            OutEdge_NullThrows_Test(filteredGraph2);
        }

        [Test]
        public void OutEdges()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            OutEdges_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void OutEdges_Throws()
        {
            var graph1 = new AdjacencyGraph<EquatableTestVertex, Edge<EquatableTestVertex>>();
            var filteredGraph1 = new FilteredVertexAndEdgeListGraph
                <
                    EquatableTestVertex,
                    Edge<EquatableTestVertex>,
                    AdjacencyGraph<EquatableTestVertex, Edge<EquatableTestVertex>>
                >(
                graph1,
                _ => true,
                _ => true);
            OutEdges_NullThrows_Test(filteredGraph1);
            OutEdges_Throws_Test(filteredGraph1);

            var graph2 = new AdjacencyGraph<int, IEdge<int>>();
            var filteredGraph2 = new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                graph2,
                vertex => vertex < 4,
                _ => true);

            graph2.AddVertexRange( 1, 2, 3, 4, 5 );
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            Assert.IsNull(filteredGraph2.OutEdges(4));
            Assert.IsNull(filteredGraph2.OutEdges(5));
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        #endregion

        #region Try Get Edges

        [Test]
        public void TryGetEdge()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            TryGetEdge_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void TryGetEdge_Throws()
        {
            var filteredGraph = new FilteredVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>, AdjacencyGraph<TestVertex, Edge<TestVertex>>>(
                new AdjacencyGraph<TestVertex, Edge<TestVertex>>(),
                _ => true,
                _ => true);
            TryGetEdge_Throws_Test(filteredGraph);
        }

        [Test]
        public void TryGetEdges()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            TryGetEdges_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void TryGetEdges_Throws()
        {
            var filteredGraph = new FilteredVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>, AdjacencyGraph<TestVertex, Edge<TestVertex>>>(
                new AdjacencyGraph<TestVertex, Edge<TestVertex>>(),
                _ => true,
                _ => true);
            TryGetEdges_Throws_Test(filteredGraph);
        }

        [Test]
        public void TryGetOutEdges()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            TryGetOutEdges_Test(
                graph,
                (vertexPredicate, edgePredicate) =>
                    new FilteredVertexAndEdgeListGraph<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(
                        graph,
                        vertexPredicate,
                        edgePredicate));
        }

        [Test]
        public void TryGetOutEdges_Throws()
        {
            TryGetOutEdges_Throws_Test(
                new FilteredVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>, AdjacencyGraph<TestVertex, Edge<TestVertex>>>(
                    new AdjacencyGraph<TestVertex, Edge<TestVertex>>(),
                    _ => true,
                    _ => true));
        }

        #endregion
    }
}