﻿using System;
using NUnit.Framework;

namespace QuikGraph.Tests.Structures
{
    /// <summary>
    /// Tests for <see cref="DelegateImplicitGraph{TVertex,TEdge}"/>.
    /// </summary>
    [TestFixture]
    internal sealed class DelegateImplicitGraphTests : DelegateGraphTestsBase
    {
        [Test]
        public void Construction()
        {
            var graph = new DelegateImplicitGraph<int, IEdge<int>>(
                GetEmptyGetter<int, IEdge<int>>());
            AssertGraphProperties(graph);

            graph = new DelegateImplicitGraph<int, IEdge<int>>(
                GetEmptyGetter<int, IEdge<int>>(),
                false);
            AssertGraphProperties(graph, false);

            #region Local function

            void AssertGraphProperties<TVertex, TEdge>(
                DelegateImplicitGraph<TVertex, TEdge> g,
                bool parallelEdges = true)
                where TEdge : class, IEdge<TVertex>
            {
                Assert.IsTrue(g.IsDirected);
                Assert.AreEqual(parallelEdges, g.AllowParallelEdges);
            }

            #endregion
        }

        [Test]
        public void Construction_Throws()
        {
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DelegateImplicitGraph<int, IEdge<int>>(null));
        }

        #region Contains Vertex

        [Test]
        public void ContainsVertex()
        {
            var data = new GraphData<int, IEdge<int>>();
            var graph = new DelegateImplicitGraph<int, IEdge<int>>(data.TryGetEdges);
            ContainsVertex_Test(data, graph);
        }

        [Test]
        public void ContainsVertex_Throws()
        {
            var graph = new DelegateImplicitGraph<TestVertex, IEdge<TestVertex>>(
                GetEmptyGetter<TestVertex, IEdge<TestVertex>>());
            ContainsVertex_Throws_Test(graph);
        }

        #endregion

        #region Out Edges

        [Test]
        public void OutEdge()
        {
            var data = new GraphData<int, IEdge<int>>();
            var graph = new DelegateImplicitGraph<int, IEdge<int>>(data.TryGetEdges);
            OutEdge_Test(data, graph);
        }

        [Test]
        public void OutEdge_Throws()
        {
            var graph1 = new DelegateImplicitGraph<TestVertex, IEdge<TestVertex>>(
                GetEmptyGetter<TestVertex, IEdge<TestVertex>>());
            OutEdge_NullThrows_Test(graph1);

            var data = new GraphData<int, IEdge<int>>();
            var graph2 = new DelegateImplicitGraph<int, IEdge<int>>(data.TryGetEdges);
            OutEdge_Throws_Test(data, graph2);
        }

        [Test]
        public void OutEdges()
        {
            var data = new GraphData<int, IEdge<int>>();
            var graph = new DelegateImplicitGraph<int, IEdge<int>>(data.TryGetEdges);
            OutEdges_Test(data, graph);
        }

        [Test]
        public void OutEdges_Throws()
        {
            var data = new GraphData<int, IEdge<int>>();
            var graph1 = new DelegateImplicitGraph<int, IEdge<int>>(data.TryGetEdges);
            OutEdges_Throws_Test(data, graph1);

            var graph2 = new DelegateImplicitGraph<EquatableTestVertex, Edge<EquatableTestVertex>>(
                GetEmptyGetter<EquatableTestVertex, Edge<EquatableTestVertex>>());
            OutEdges_NullThrows_Test(graph2);
        }

        #endregion

        #region Try Get Edges

        [Test]
        public void TryGetOutEdges()
        {
            var data = new GraphData<int, IEdge<int>>();
            var graph = new DelegateImplicitGraph<int, IEdge<int>>(data.TryGetEdges);
            GetOutEdges_Test(data, graph);
        }

        [Test]
        public void TryGetOutEdges_Throws()
        {
            var graph = new DelegateImplicitGraph<TestVertex, IEdge<TestVertex>>(
                GetEmptyGetter<TestVertex, IEdge<TestVertex>>());
            GetOutEdges_Throws_Test(graph);
        }

        #endregion
    }
}