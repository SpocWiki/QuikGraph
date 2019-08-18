using System;
using NUnit.Framework;

namespace QuikGraph.Tests.Structures
{
    /// <summary>
    /// Tests for <see cref="UndirectedBidirectionalGraph{TVertex,TEdge}"/>.
    /// </summary>
    [TestFixture]
    internal class UndirectedBidirectionalGraphTests : GraphTestsBase
    {
        [Test]
        public void Construction()
        {
            var wrappedGraph = new BidirectionalGraph<int, Edge<int>>(true);
            var graph = new UndirectedBidirectionalGraph<int, Edge<int>>(wrappedGraph);
            AssertGraphProperties(graph);

            wrappedGraph = new BidirectionalGraph<int, Edge<int>>(false);
            graph = new UndirectedBidirectionalGraph<int, Edge<int>>(wrappedGraph);
            AssertGraphProperties(graph, false);

            #region Local function

            void AssertGraphProperties<TVertex, TEdge>(UndirectedBidirectionalGraph<TVertex, TEdge> g, bool parallelEdges = true)
                where TEdge : IEdge<TVertex>
            {
                Assert.IsFalse(g.IsDirected);
                Assert.AreEqual(parallelEdges, g.AllowParallelEdges);
                Assert.IsNotNull(g.VisitedGraph);
                AssertEmptyGraph(g);
            }

            #endregion
        }

        [Test]
        public void Construction_Throws()
        {
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new UndirectedBidirectionalGraph<int, Edge<int>>(null));
        }

        #region Contains Vertex

        [Test]
        public void ContainsVertex()
        {
            var wrappedGraph = new BidirectionalGraph<TestVertex, Edge<TestVertex>>();
            ContainsVertex_ImmutableGraph_Test(
                wrappedGraph,
                () => new UndirectedBidirectionalGraph<TestVertex, Edge<TestVertex>>(wrappedGraph));
        }

        [Test]
        public void ContainsVertex_EquatableVertex()
        {
            var wrappedGraph = new BidirectionalGraph<EquatableTestVertex, Edge<EquatableTestVertex>>();
            ContainsVertex_EquatableVertex_ImmutableGraph_Test(
                wrappedGraph,
                () => new UndirectedBidirectionalGraph<EquatableTestVertex, Edge<EquatableTestVertex>>(wrappedGraph));
        }

        [Test]
        public void ContainsVertex_Throws()
        {
            var wrappedGraph = new BidirectionalGraph<TestVertex, Edge<TestVertex>>();
            var graph = new UndirectedBidirectionalGraph<TestVertex, Edge<TestVertex>>(wrappedGraph);
            ContainsVertex_Throws_Test(graph);
        }

        #endregion

        #region Contains Edge

        [Test]
        public void ContainsEdge()
        {
            var wrappedGraph = new BidirectionalGraph<int, Edge<int>>();
            ContainsEdge_ImmutableGraph_Test(
                wrappedGraph,
                () => new UndirectedBidirectionalGraph<int, Edge<int>>(wrappedGraph));
        }

        [Test]
        public void ContainsEdge_EquatableEdge()
        {
            var wrappedGraph = new BidirectionalGraph<int, EquatableEdge<int>>();
            ContainsEdge_EquatableEdge_ImmutableGraph_Test(
                wrappedGraph,
                () => new UndirectedBidirectionalGraph<int, EquatableEdge<int>>(wrappedGraph));
        }

        [Test]
        public void ContainsEdge_SourceTarget()
        {
            var wrappedGraph = new BidirectionalGraph<int, Edge<int>>();
            ContainsEdge_SourceTarget_ImmutableGraph_UndirectedGraph_Test(
                wrappedGraph,
                () => new UndirectedBidirectionalGraph<int, Edge<int>>(wrappedGraph));
        }

        [Test]
        public void ContainsEdge_Throws()
        {
            var wrappedGraph = new BidirectionalGraph<TestVertex, Edge<TestVertex>>();
            var graph = new UndirectedBidirectionalGraph<TestVertex, Edge<TestVertex>>(wrappedGraph);
            ContainsEdge_Throws_Test(graph);
            ContainsEdge_SourceTarget_Throws_UndirectedGraph_Test(graph);
        }

        #endregion

        #region Adjacent Edges

        [Test]
        public void AdjacentEdge_Throws()
        {
            var wrappedGraph = new BidirectionalGraph<int, Edge<int>>();
            var graph = new UndirectedBidirectionalGraph<int, Edge<int>>(wrappedGraph);
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Assert.Throws<NotSupportedException>(() => graph.AdjacentEdge(1, 0));
        }

        [Test]
        public void AdjacentEdges()
        {
            var wrappedGraph = new BidirectionalGraph<int, Edge<int>>();
            AdjacentEdges_ImmutableGraph_Test(
                wrappedGraph,
                () => new UndirectedBidirectionalGraph<int, Edge<int>>(wrappedGraph));
        }

        [Test]
        public void AdjacentEdges_Throws()
        {
            var wrappedGraph = new BidirectionalGraph<TestVertex, Edge<TestVertex>>();
            var graph = new UndirectedBidirectionalGraph<TestVertex, Edge<TestVertex>>(wrappedGraph);
            AdjacentEdges_Throws_Test(graph);
        }

        #endregion

        #region Try Get Edges

        [Test]
        public void TryGetEdge()
        {
            var wrappedGraph = new BidirectionalGraph<int, Edge<int>>();
            TryGetEdge_ImmutableGraph_UndirectedGraph_Test(
                wrappedGraph,
                () => new UndirectedBidirectionalGraph<int, Edge<int>>(wrappedGraph));
        }

        [Test]
        public void TryGetEdge_Throws()
        {
            var wrappedGraph = new BidirectionalGraph<TestVertex, Edge<TestVertex>>();
            var graph = new UndirectedBidirectionalGraph<TestVertex, Edge<TestVertex>>(wrappedGraph);
            TryGetEdge_Throws_UndirectedGraph_Test(graph);
        }

        #endregion
    }
}
