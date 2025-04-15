using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Condensation;
using QuikGraph.Algorithms.ConnectedComponents;
using QuikGraph.Algorithms.MaximumFlow;
using QuikGraph.Algorithms.MinimumSpanningTree;
using QuikGraph.Algorithms.RandomWalks;
using QuikGraph.Algorithms.RankedShortestPath;
using QuikGraph.Algorithms.ShortestPath;
using QuikGraph.Algorithms.TopologicalSort;
using QuikGraph.Collections;
using QuikGraph.Helpers;
using QuikGraph.Tests.Structures;
using static QuikGraph.Tests.GraphTestHelpers;

namespace QuikGraph.Tests.Extensions
{
    /// <summary>
    /// Tests related to <see cref="AlgorithmExtensions"/>.
    /// </summary>
    internal sealed class AlgorithmExtensionsTests : GraphTestsBase
    {
        [Test]
        public void GetIndexer()
        {
            var dictionary1 = new Dictionary<int, double>();
            Func<int, double> indexer1 = AlgorithmExtensions.GetIndexer(dictionary1);

            Assert.Throws<KeyNotFoundException>(() => indexer1(12));

            dictionary1[12] = 42.0;
            Assert.AreEqual(42.0, indexer1(12));

            var dictionary2 = new Dictionary<TestVertex, TestVertex>();
            Func<TestVertex, TestVertex> indexer2 = AlgorithmExtensions.GetIndexer(dictionary2);

            var key = new TestVertex("1");
            var keyBis = new TestVertex("1");
            Assert.Throws<KeyNotFoundException>(() => indexer2(key));

            var value = new TestVertex("2");
            dictionary2[key] = value;
            Assert.AreSame(value, indexer2(key));

            Assert.Throws<KeyNotFoundException>(() => indexer2(keyBis));
        }

        [Test]
        public void GetIndexer_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => AlgorithmExtensions.GetIndexer<int, double>(null));
        }

        [Test]
        public void GetVertexIdentity()
        {
            var graph1 = new AdjacencyGraph<int, IEdge<int>>();
            VertexIdentity<int> vertexIdentity1 = graph1.GetVertexIdentity();

            Assert.AreEqual("12", vertexIdentity1(12));
            Assert.AreEqual("42", vertexIdentity1(42));
            // Check identity didn't change
            Assert.AreEqual("12", vertexIdentity1(12));
            Assert.AreEqual("42", vertexIdentity1(42));

            var graph2 = new AdjacencyGraph<TestVertex, Edge<TestVertex>>();
            VertexIdentity<TestVertex> vertexIdentity2 = graph2.GetVertexIdentity();

            var vertex1 = new TestVertex("12");
            var vertex2 = new TestVertex("42");
            Assert.AreEqual("0", vertexIdentity2(vertex1));
            Assert.AreEqual("1", vertexIdentity2(vertex2));
            // Check identity didn't change
            Assert.AreEqual("0", vertexIdentity2(vertex1));
            Assert.AreEqual("1", vertexIdentity2(vertex2));
        }

        [Test]
        public void GetVertexIdentity_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => AlgorithmExtensions.GetVertexIdentity<int>(null));
        }

        [Test]
        public void GetEdgeIdentity()
        {
            var graph1 = new AdjacencyGraph<int, IEdge<int>>();
            EdgeIdentity<int, IEdge<int>> edgeIdentity1 = graph1.GetEdgeIdentity();

            var edge1 = Edge.Create(1, 2);
            var edge2 = Edge.Create(2, 3);
            var edge3 = Edge.Create(1, 2);
            Assert.AreEqual("0", edgeIdentity1(edge1));
            Assert.AreEqual("1", edgeIdentity1(edge2));
            Assert.AreEqual("2", edgeIdentity1(edge3));
            // Check identity didn't change
            Assert.AreEqual("0", edgeIdentity1(edge1));
            Assert.AreEqual("1", edgeIdentity1(edge2));
            Assert.AreEqual("2", edgeIdentity1(edge3));

            var graph2 = new AdjacencyGraph<TestVertex, Edge<TestVertex>>();
            EdgeIdentity<TestVertex, Edge<TestVertex>> edgeIdentity2 = graph2.GetEdgeIdentity();

            var vertex1 = new TestVertex("1");
            var vertex2 = new TestVertex("2");
            var vertex3 = new TestVertex("3");
            var edge4 = new Edge<TestVertex>(vertex1, vertex2);
            var edge5 = new Edge<TestVertex>(vertex2, vertex3);
            var edge6 = new Edge<TestVertex>(vertex1, vertex2);
            Assert.AreEqual("0", edgeIdentity2(edge4));
            Assert.AreEqual("1", edgeIdentity2(edge5));
            Assert.AreEqual("2", edgeIdentity2(edge6));
            // Check identity didn't change
            Assert.AreEqual("0", edgeIdentity2(edge4));
            Assert.AreEqual("1", edgeIdentity2(edge5));
            Assert.AreEqual("2", edgeIdentity2(edge6));
        }

        [Test]
        public void GetEdgeIdentity_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => AlgorithmExtensions.GetEdgeIdentity<int, IEdge<int>>(null));
            Assert.Throws<ArgumentNullException>(() => AlgorithmExtensions.GetEdgeIdentity<TestVertex, Edge<TestVertex>>(null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void TreeBreadthFirstSearch()
        {
            var edge12 = Edge.Create(1, 2);
            var edge13 = Edge.Create(1, 3);
            var edge23 = Edge.Create(2, 3);
            var edge24 = Edge.Create(2, 4);
            var edge35 = Edge.Create(3, 5);
            var edge36 = Edge.Create(3, 6);

            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                edge12, edge13, edge23, edge24, edge35, edge36
            ]);
            graph.AddVertex(7);

            TryFunc<int, IEnumerable<IEdge<int>>> pathAccessor = graph.TreeBreadthFirstSearch(1);

            Assert.IsFalse(pathAccessor(7, out _));

            Assert.IsTrue(pathAccessor(5, out IEnumerable<IEdge<int>> path));
            CollectionAssert.AreEqual((IEdge<int>[]) [edge13, edge35], path);
        }

        [Test]
        public void TreeBreadthFirstSearch_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, Edge<TestVertex>>();

            var vertex = new TestVertex("1");
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).TreeBreadthFirstSearch(vertex));
            Assert.Throws<ArgumentNullException>(() => graph.TreeBreadthFirstSearch(null));
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).TreeBreadthFirstSearch(null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void TreeDepthFirstSearch()
        {
            var edge12 = Edge.Create(1, 2);
            var edge13 = Edge.Create(1, 3);
            var edge23 = Edge.Create(2, 3);
            var edge24 = Edge.Create(2, 4);
            var edge35 = Edge.Create(3, 5);
            var edge36 = Edge.Create(3, 6);

            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                edge12, edge13, edge23, edge24, edge35, edge36
            ]);
            graph.AddVertex(7);

            TryFunc<int, IEnumerable<IEdge<int>>> pathAccessor = graph.TreeDepthFirstSearch(1);

            Assert.IsFalse(pathAccessor(7, out _));

            Assert.IsTrue(pathAccessor(5, out IEnumerable<IEdge<int>> path));
            CollectionAssert.AreEqual((IEdge<int>[]) [edge12, edge23, edge35], path);
        }

        [Test]
        public void TreeDepthFirstSearch_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, Edge<TestVertex>>();

            var vertex = new TestVertex("1");
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).TreeDepthFirstSearch(vertex));
            Assert.Throws<ArgumentNullException>(() => graph.TreeDepthFirstSearch(null));
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).TreeDepthFirstSearch(null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void TreeCyclePoppingRandom()
        {
            var edge1 = Edge.Create(1, 2);
            var edge2 = Edge.Create(1, 3);
            var edge3 = Edge.Create(2, 1);
            var edge4 = Edge.Create(2, 3);
            var edge5 = Edge.Create(2, 4);
            var edge6 = Edge.Create(3, 2);
            var edge7 = Edge.Create(3, 5);
            var edge8 = Edge.Create(3, 6);
            var edge9 = Edge.Create(4, 1);
            var edge10 = Edge.Create(4, 2);
            var edge11 = Edge.Create(4, 5);
            var edge12 = Edge.Create(4, 6);
            var edge13 = Edge.Create(5, 6);
            var edge14 = Edge.Create(6, 2);
            var edge15 = Edge.Create(6, 3);

            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                edge1, edge2, edge3, edge4, edge5, edge6,
                edge7, edge8, edge9, edge10, edge11,
                edge12, edge13, edge14, edge15
            ]);
            graph.AddVertex(7);

            TryFunc<int, IEnumerable<IEdge<int>>> pathAccessor = graph.TreeCyclePoppingRandom(2);

            Assert.IsFalse(pathAccessor(7, out _));

            // Would require more tests...
        }

        [Test]
        public void TreeCyclePoppingRandom_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, Edge<TestVertex>>();

            var vertex = new TestVertex("1");
            var chain = new NormalizedMarkovEdgeChain<TestVertex, Edge<TestVertex>>();
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).TreeCyclePoppingRandom(vertex));
            Assert.Throws<ArgumentNullException>(() => graph.TreeCyclePoppingRandom(null));
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).TreeCyclePoppingRandom(null));

            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).TreeCyclePoppingRandom(vertex, chain));
            Assert.Throws<ArgumentNullException>(() => graph.TreeCyclePoppingRandom(null, chain));
            Assert.Throws<ArgumentException>(() => graph.TreeCyclePoppingRandom(vertex, null));
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).TreeCyclePoppingRandom(null, chain));
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).TreeCyclePoppingRandom(vertex, null));
            Assert.Throws<ArgumentNullException>(() => graph.TreeCyclePoppingRandom(null, null));
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).TreeCyclePoppingRandom(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        #region Shortest paths

        [Test]
        public void ShortestPaths_Dijkstra_AStar_BellmanFord_Dag()
        {
            var edge12 = Edge.Create(1, 2);
            var edge13 = Edge.Create(1, 3);
            var edge18 = Edge.Create(1, 8);
            var edge24 = Edge.Create(2, 4);
            var edge25 = Edge.Create(2, 5);
            var edge26 = Edge.Create(2, 6);
            var edge34 = Edge.Create(3, 4);
            var edge45 = Edge.Create(4, 5);
            var edge46 = Edge.Create(4, 6);
            var edge56 = Edge.Create(5, 6);
            var edge67 = Edge.Create(6, 7);
            var edge810 = Edge.Create(8, 10);
            var edge95 = Edge.Create(9, 5);
            var edge109 = Edge.Create(10, 9);

            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                edge12, edge13, edge18, edge24, edge25,
                edge26, edge34, edge45, edge46, edge56,
                edge67, edge810, edge95, edge109
            ]);

            TryFunc<int, IEnumerable<IEdge<int>>>[] algorithmResults = 
            [
                graph.ShortestPathsDijkstra(_ => 1.0, 2),
                graph.ShortestPathsAStar(_ => 1.0, _ => 1.0, 2),
                graph.ShortestPathsBellmanFord(_ => 1.0, 2, out _),
                graph.ShortestPathsDag(_ => 1.0, 2)
            ];

            foreach (TryFunc<int, IEnumerable<IEdge<int>>> result in algorithmResults)
            {
                CheckResult(result);
            }

            #region Local function

            void CheckResult(TryFunc<int, IEnumerable<IEdge<int>>> pathAccessor)
            {
                Assert.IsNotNull(pathAccessor);

                Assert.IsFalse(pathAccessor(1, out _));

                Assert.IsTrue(pathAccessor(7, out IEnumerable<IEdge<int>> path));
                CollectionAssert.AreEqual((IEdge<int>[]) [edge26, edge67], path);

                Assert.IsTrue(pathAccessor(4, out path));
                CollectionAssert.AreEqual((IEdge<int>[]) [edge24], path);
            }

            #endregion
        }

        [Test]
        public void ShortestPaths_BellmanFord_NegativeCycle()
        {
            var edge12 = Edge.Create(1, 2);
            var edge24 = Edge.Create(2, 4);
            var edge41 = Edge.Create(4, 1);
            
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                edge12, edge24, edge41
            ]);

            TryFunc<int, IEnumerable<IEdge<int>>> pathAccessor = graph.ShortestPathsBellmanFord(
                edge =>
                {
                    if (edge == edge12)
                        return 12.0;
                    if (edge == edge24)
                        return -42.0;
                    if (edge == edge41)
                        return 22.0;
                    return 1.0;
                },
                1,
                out bool foundNegativeCycle);
            Assert.IsNotNull(pathAccessor);
            Assert.IsTrue(foundNegativeCycle);

            // Path accessors is usable but will generate a stack overflow
            // if accessing path using edge in the negative cycle.
        }

        [Test]
        public void ShortestPathsDijkstra_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, Edge<TestVertex>>();

            var vertex = new TestVertex("1");
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsDijkstra(_ => 1.0, vertex));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsDijkstra(null, vertex));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsDijkstra(_ => 1.0, null));
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsDijkstra(null, vertex));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsDijkstra(null, null));
            Assert.Throws<ArgumentNullException>(() =>
                ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsDijkstra(_ => 1.0, null));
            Assert.Throws<ArgumentNullException>(() =>
                ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsDijkstra(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ShortestPathsAStar_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, Edge<TestVertex>>();

            var vertex = new TestVertex("1");
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsAStar(_ => 1.0, _ => 1.0, vertex));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsAStar(null, _ => 1.0, vertex));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsAStar(_ => 1.0, null, vertex));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsAStar(_ => 1.0, _ => 1.0, null));
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsAStar(null, _ => 1.0, vertex));
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsAStar(_ => 1.0, null, vertex));
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsAStar(_ => 1.0, _ => 1.0, null));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsAStar(null, null, vertex));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsAStar(null, _ => 1.0, null));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsAStar(_ => 1.0, null, null));
            Assert.Throws<ArgumentNullException>(() =>
                ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsAStar(null, null, vertex));
            Assert.Throws<ArgumentNullException>(() =>
                ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsAStar(null, _ => 1.0, null));
            Assert.Throws<ArgumentNullException>(() =>
                ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsAStar(_ => 1.0, null, null));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsAStar(null, null, null));
            Assert.Throws<ArgumentNullException>(() =>
                ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsAStar(null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ShortestPathsBellmanFord_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, Edge<TestVertex>>();

            var vertex = new TestVertex("1");
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsBellmanFord(_ => 1.0, vertex, out _));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsBellmanFord(null, vertex, out _));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsBellmanFord(_ => 1.0, null, out _));
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsBellmanFord(null, vertex, out _));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsBellmanFord(null, null, out _));
            Assert.Throws<ArgumentNullException>(() =>
                ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsBellmanFord(_ => 1.0, null, out _));
            Assert.Throws<ArgumentNullException>(() =>
                ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsBellmanFord(null, null, out _));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ShortestPathsDag_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, Edge<TestVertex>>();

            var vertex = new TestVertex("1");
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsDag(_ => 1.0, vertex));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsDag(null, vertex));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsDag(_ => 1.0, null));
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsDag(null, vertex));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsDag(null, null));
            Assert.Throws<ArgumentNullException>(() => 
                ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsDag(_ => 1.0, null));
            Assert.Throws<ArgumentNullException>(() =>
                ((IVertexAndEdgeListGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsDag(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ShortestPaths_UndirectedDijkstra()
        {
            var edge12 = Edge.Create(1, 2);
            var edge13 = Edge.Create(1, 3);
            var edge18 = Edge.Create(1, 8);
            var edge45 = Edge.Create(4, 5);
            var edge46 = Edge.Create(4, 6);
            var edge56 = Edge.Create(5, 6);
            var edge67 = Edge.Create(6, 7);
            var edge810 = Edge.Create(8, 10);

            var graph = new UndirectedGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                edge12, edge13, edge18, edge45,
                edge46, edge56, edge67, edge810
            ]);
            graph.AddVertex(9);

            TryFunc<int, IEnumerable<IEdge<int>>> pathAccessor = graph.ShortestPathsDijkstra(_ => 1.0, 2);
            Assert.IsNotNull(pathAccessor);

            Assert.IsFalse(pathAccessor(9, out _));

            Assert.IsTrue(pathAccessor(8, out IEnumerable<IEdge<int>> path));
            CollectionAssert.AreEqual((IEdge<int>[]) [edge12, edge18], path);

            Assert.IsTrue(pathAccessor(1, out path));
            CollectionAssert.AreEqual((IEdge<int>[]) [edge12], path);
        }

        [Test]
        public void ShortestPathsUndirectedDijkstra_Throws()
        {
            var graph = new UndirectedGraph<TestVertex, Edge<TestVertex>>();

            var vertex = new TestVertex("1");
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IUndirectedGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsDijkstra(_ => 1.0, vertex));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsDijkstra(null, vertex));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsDijkstra(_ => 1.0, null));
            Assert.Throws<ArgumentNullException>(
                () => ((IUndirectedGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsDijkstra(null, vertex));
            Assert.Throws<ArgumentNullException>(() => graph.ShortestPathsDijkstra(null, null));
            Assert.Throws<ArgumentNullException>(() =>
                ((IUndirectedGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsDijkstra(_ => 1.0, null));
            Assert.Throws<ArgumentNullException>(() =>
                ((IUndirectedGraph<TestVertex, Edge<TestVertex>>)null).ShortestPathsDijkstra(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        #endregion

        #region K-Shortest path

        [Test]
        public void RankedShortestPathHoffmanPavley()
        {
            var edge12 = Edge.Create(1, 2);
            var edge13 = Edge.Create(1, 3);
            var edge18 = Edge.Create(1, 8);
            var edge21 = Edge.Create(2, 1);
            var edge24 = Edge.Create(2, 4);
            var edge25 = Edge.Create(2, 5);
            var edge26 = Edge.Create(2, 6);
            var edge33 = Edge.Create(3, 3);
            var edge34 = Edge.Create(3, 4);
            var edge45 = Edge.Create(4, 5);
            var edge46 = Edge.Create(4, 6);
            var edge56 = Edge.Create(5, 6);
            var edge67 = Edge.Create(6, 7);
            var edge810 = Edge.Create(8, 10);
            var edge95 = Edge.Create(9, 5);
            var edge109 = Edge.Create(10, 9);

            var graph = new BidirectionalGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                edge12, edge13, edge18, edge21, edge24,
                edge25, edge26, edge33, edge34, edge45,
                edge46, edge56, edge67, edge810, edge95,
                edge109
            ]);

            IEnumerable<IEnumerable<IEdge<int>>> paths = graph.RankedShortestPathHoffmanPavley(_ => 1.0, 1, 5, 5);
            CollectionAssert.AreEqual(
                (IEdge<int>[][])
                [
                    [edge12, edge25],
                    [edge13, edge34, edge45],
                    [edge12, edge24, edge45],
                    [edge18, edge810, edge109, edge95]
                ],
                paths);

            paths = graph.RankedShortestPathHoffmanPavley(_ => 1.0, 1, 5);
            CollectionAssert.AreEqual(
                (IEdge<int>[][])
                [
                    [edge12, edge25],
                    [edge13, edge34, edge45],
                    [edge12, edge24, edge45]
                ],
                paths);
        }

        [Test]
        public void RankedShortestPathHoffmanPavley_Throws()
        {
            var graph = new BidirectionalGraph<TestVertex, Edge<TestVertex>>();

            var vertex = new TestVertex("1");
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IBidirectionalGraph<TestVertex, Edge<TestVertex>>)null).RankedShortestPathHoffmanPavley(_ => 1.0, vertex, vertex, int.MaxValue));
            Assert.Throws<ArgumentNullException>(
                () => graph.RankedShortestPathHoffmanPavley(null, vertex, vertex, int.MaxValue));
            Assert.Throws<ArgumentNullException>(
                () => graph.RankedShortestPathHoffmanPavley(_ => 1.0, null, vertex, int.MaxValue));
            Assert.Throws<ArgumentNullException>(
                () => graph.RankedShortestPathHoffmanPavley(_ => 1.0, vertex, null, int.MaxValue));
            Assert.Throws<ArgumentNullException>(
                () => ((IBidirectionalGraph<TestVertex, Edge<TestVertex>>)null).RankedShortestPathHoffmanPavley(null, vertex, vertex, int.MaxValue));
            Assert.Throws<ArgumentNullException>(
                () => ((IBidirectionalGraph<TestVertex, Edge<TestVertex>>)null).RankedShortestPathHoffmanPavley(_ => 1.0, null, vertex, int.MaxValue));
            Assert.Throws<ArgumentNullException>(
                () => ((IBidirectionalGraph<TestVertex, Edge<TestVertex>>)null).RankedShortestPathHoffmanPavley(_ => 1.0, vertex, null, int.MaxValue));
            Assert.Throws<ArgumentNullException>(
                () => graph.RankedShortestPathHoffmanPavley(null, null, vertex, int.MaxValue));
            Assert.Throws<ArgumentNullException>(
                () => graph.RankedShortestPathHoffmanPavley(null, vertex, null, int.MaxValue));
            Assert.Throws<ArgumentNullException>(
                () => graph.RankedShortestPathHoffmanPavley(_ => 1.0, null, null, int.MaxValue));
            Assert.Throws<ArgumentNullException>(
                () => ((IBidirectionalGraph<TestVertex, Edge<TestVertex>>)null).RankedShortestPathHoffmanPavley(null, null, vertex, int.MaxValue));
            Assert.Throws<ArgumentNullException>(
                () => ((IBidirectionalGraph<TestVertex, Edge<TestVertex>>)null).RankedShortestPathHoffmanPavley(null, vertex, null, int.MaxValue));
            Assert.Throws<ArgumentNullException>(
                () => graph.RankedShortestPathHoffmanPavley(null, null, null, int.MaxValue));
            Assert.Throws<ArgumentNullException>(
                () => ((IBidirectionalGraph<TestVertex, Edge<TestVertex>>)null).RankedShortestPathHoffmanPavley(null, null, null, int.MaxValue));
            // ReSharper restore AssignNullToNotNullAttribute

            Assert.Throws<ArgumentOutOfRangeException>(
                () => graph.RankedShortestPathHoffmanPavley(_ => 1.0, vertex, vertex, 0));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => graph.RankedShortestPathHoffmanPavley(_ => 1.0, vertex, vertex, -1));
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        #endregion

        [Pure]
        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> CreateSinksTestCases(
            [NotNull, InstantHandle] Func<IMutableVertexAndEdgeSet<int, IEdge<int>>> createGraph)
        {
            yield return new TestCaseData(
                createGraph(),
                Enumerable.Empty<int>());

            var edge12 = Edge.Create(1, 2);
            var edge14 = Edge.Create(1, 4);
            var edge22 = Edge.Create(2, 2);
            var edge23 = Edge.Create(2, 3);
            var edge24 = Edge.Create(2, 4);
            var edge25 = Edge.Create(2, 5);
            var edge35 = Edge.Create(3, 5);
            var edge41 = Edge.Create(4, 1);
            var edge45 = Edge.Create(4, 5);
            var edge46 = Edge.Create(4, 6);

            IMutableVertexAndEdgeSet<int, IEdge<int>> cycleGraph = createGraph();
            cycleGraph.AddVerticesAndEdgeRange(
            [
                edge12, edge24, edge41
            ]);
            yield return new TestCaseData(
                cycleGraph,
                Enumerable.Empty<int>());

            IMutableVertexAndEdgeSet<int, IEdge<int>> cycleGraph2 = createGraph();
            cycleGraph2.AddVerticesAndEdgeRange(
            [
                edge12, edge24, edge25, edge35, edge41, edge22
            ]);
            yield return new TestCaseData(
                cycleGraph2,
                (int[]) [5]);

            IMutableVertexAndEdgeSet<int, IEdge<int>> graph1 = createGraph();
            graph1.AddVerticesAndEdgeRange(
            [
                edge22
            ]);
            yield return new TestCaseData(
                graph1,
                Enumerable.Empty<int>());

            IMutableVertexAndEdgeSet<int, IEdge<int>> graph2 = createGraph();
            graph2.AddVerticesAndEdgeRange(
            [
                edge12, edge14, edge23, edge24, edge35, edge45
            ]);
            yield return new TestCaseData(
                graph2,
                (int[]) [5]);

            IMutableVertexAndEdgeSet<int, IEdge<int>> graph3 = createGraph();
            graph3.AddVerticesAndEdgeRange(
            [
                edge12, edge14, edge24, edge35, edge45, edge46
            ]);
            yield return new TestCaseData(
                graph3,
                (int[]) [5, 6]);
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> SinksTestCases
        {
            [UsedImplicitly]
            get
            {
                IEnumerable<TestCaseData> testCases = CreateSinksTestCases(() => new AdjacencyGraph<int, IEdge<int>>())
                    .Concat(CreateSinksTestCases(() => new BidirectionalGraph<int, IEdge<int>>()));
                foreach (TestCaseData testCase in testCases)
                {
                    yield return testCase;
                }
            }
        }

        [TestCaseSource(nameof(SinksTestCases))]
        public void Sinks(
            [NotNull] IVertexListGraph<int, IEdge<int>> graph,
            [NotNull] IEnumerable<int> expectedSinks)
        {
            CollectionAssert.AreEquivalent(expectedSinks, graph.Sinks());
        }

        [Test]
        public void Sinks_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexListGraph<int, IEdge<int>>)null).Sinks().ToArray());
        }

        /// <summary> Returns Test Cases of Graphs and its expected Root Nodes </summary>
        [Pure]
        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> CreateRootsTestCases(
            [NotNull, InstantHandle] Func<IMutableVertexAndEdgeSet<int, IEdge<int>>> createGraph)
        {
            yield return new TestCaseData(createGraph(), Enumerable.Empty<int>());

            var edge12 = Edge.Create(1, 2);
            var edge14 = Edge.Create(1, 4);
            var edge22 = Edge.Create(2, 2);
            var edge23 = Edge.Create(2, 3);
            var edge24 = Edge.Create(2, 4);
            var edge25 = Edge.Create(2, 5);
            var edge35 = Edge.Create(3, 5);
            var edge41 = Edge.Create(4, 1);
            var edge45 = Edge.Create(4, 5);
            var edge46 = Edge.Create(4, 6);

            IMutableVertexAndEdgeSet<int, IEdge<int>> cycleGraph = createGraph();
            cycleGraph.AddVerticesAndEdgeRange([edge12, edge24, edge41]);
            yield return new TestCaseData(cycleGraph, Enumerable.Empty<int>());

            IMutableVertexAndEdgeSet<int, IEdge<int>> cycleGraph2 = createGraph();
            cycleGraph2.AddVerticesAndEdgeRange([edge12, edge24, edge25, edge35, edge41, edge22]);
            yield return new TestCaseData(cycleGraph2, (int[]) [3]);

            IMutableVertexAndEdgeSet<int, IEdge<int>> graph1 = createGraph();
            graph1.AddVerticesAndEdgeRange([edge22]);
            yield return new TestCaseData(graph1, Enumerable.Empty<int>());

            IMutableVertexAndEdgeSet<int, IEdge<int>> graph2 = createGraph();
            graph2.AddVerticesAndEdgeRange([edge12, edge14, edge23, edge24, edge35, edge45]);
            yield return new TestCaseData(graph2, (int[]) [1]);

            IMutableVertexAndEdgeSet<int, IEdge<int>> graph3 = createGraph();
            graph3.AddVerticesAndEdgeRange([edge12, edge14, edge24, edge35, edge45, edge46]);
            yield return new TestCaseData(graph3, (int[]) [1, 3]);
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> RootsTestCases => CreateRootsTestCases(()
            => new AdjacencyGraph<int, IEdge<int>>());

        [TestCaseSource(nameof(RootsTestCases))]
        public void Roots_NotBidirectional(
            [NotNull] IVertexListGraph<int, IEdge<int>> graph,
            [NotNull] IEnumerable<int> expectedRoots)
        {
            CollectionAssert.AreEquivalent(expectedRoots, graph.Roots());
        }



        [TestCaseSource(typeof(TestGraphFactory), nameof(TestGraphFactory.GetNamedAdjacencyGraphs_All))]
        public void AdjacencyGraphRoots<T>(KeyValuePair<string, AdjacencyGraph<T, Edge<T>>> namedGraph)
        {
            var roots = new HashSet<T>(namedGraph.Value.Roots());
            if (GraphRoots.TryGetValue(namedGraph.Key, out var expected))
            {
                CollectionAssert.AreEqual(roots, expected);
            }
            else
            {
                Writer.Write("{ \"" + namedGraph.Key + "\", new List<string> ");
                roots.WriteList(Writer);
                Writer.WriteLine(" },");
                Writer.Flush();
            }
            foreach (Edge<T> edge in namedGraph.Value.Edges)
                Assert.IsFalse(roots.Contains(edge.Target));
        }

        private static TextWriter Writer = new StreamWriter(@"C:\_tmp\AdjacencyGraphRoots.cs");

        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> BidirectionalRootsTestCases
        {
            [UsedImplicitly]
            get
            {
                return CreateRootsTestCases(() => new BidirectionalGraph<int, IEdge<int>>());
            }
        }

        [TestCaseSource(nameof(BidirectionalRootsTestCases))]
        public void Roots_Bidirectional(
            [NotNull] IBidirectionalGraph<int, IEdge<int>> graph,
            [NotNull] IEnumerable<int> expectedRoots)
        {
            CollectionAssert.AreEquivalent(expectedRoots, graph.Roots());
        }

        [Test]
        public void Roots_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexListGraph<int, IEdge<int>>)null).Roots().ToArray());
            Assert.Throws<ArgumentNullException>(
                () => ((IBidirectionalGraph<int, IEdge<int>>)null).Roots().ToArray());
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> IsolatedVerticesTestCases
        {
            [UsedImplicitly]
            get
            {
                yield return new TestCaseData(
                    new BidirectionalGraph<int, IEdge<int>>(),
                    Enumerable.Empty<int>());

                var edge12 = Edge.Create(1, 2);
                var edge14 = Edge.Create(1, 4);
                var edge22 = Edge.Create(2, 2);
                var edge23 = Edge.Create(2, 3);
                var edge24 = Edge.Create(2, 4);
                var edge26 = Edge.Create(2, 6);
                var edge35 = Edge.Create(3, 5);
                var edge36 = Edge.Create(3, 6);
                var edge41 = Edge.Create(4, 1);
                var edge45 = Edge.Create(4, 5);
                var edge46 = Edge.Create(4, 6);

                var cycleGraph = new BidirectionalGraph<int, IEdge<int>>();
                cycleGraph.AddVerticesAndEdgeRange(
                [
                    edge12, edge24, edge41
                ]);
                yield return new TestCaseData(
                    cycleGraph,
                    Enumerable.Empty<int>());

                var cycleGraph2 = new BidirectionalGraph<int, IEdge<int>>();
                cycleGraph2.AddVerticesAndEdgeRange(
                [
                    edge12, edge24, edge41, edge22
                ]);
                yield return new TestCaseData(
                    cycleGraph2,
                    Enumerable.Empty<int>());

                var cycleGraph3 = new BidirectionalGraph<int, IEdge<int>>();
                cycleGraph3.AddVerticesAndEdgeRange(
                [
                    edge22
                ]);
                yield return new TestCaseData(
                    cycleGraph3,
                    Enumerable.Empty<int>());

                var cycleGraph4 = new BidirectionalGraph<int, IEdge<int>>();
                cycleGraph4.AddVerticesAndEdgeRange(
                [
                    edge12, edge22, edge24, edge41
                ]);
                cycleGraph4.AddVertex(5);
                yield return new TestCaseData(
                    cycleGraph4,
                    (int[]) [5]);

                var graph1 = new BidirectionalGraph<int, IEdge<int>>();
                graph1.AddVertexRange([4, 5]);
                graph1.AddVerticesAndEdgeRange(
                [
                    edge12, edge23, edge26, edge36
                ]);
                yield return new TestCaseData(
                    graph1,
                    (int[]) [4, 5]);

                var graph2 = new BidirectionalGraph<int, IEdge<int>>();
                graph2.AddVerticesAndEdgeRange(
                [
                    edge12, edge14, edge23, edge24, edge26, edge35, edge45, edge46
                ]);
                yield return new TestCaseData(
                    graph2,
                    Enumerable.Empty<int>());
            }
        }

        [TestCaseSource(nameof(IsolatedVerticesTestCases))]
        public void IsolatedVertices(
            [NotNull] IBidirectionalGraph<int, IEdge<int>> graph,
            [NotNull] IEnumerable<int> expectedIsolatedVertices)
        {
            CollectionAssert.AreEquivalent(expectedIsolatedVertices, graph.IsolatedVertices());
        }

        [Test]
        public void IsolatedVertices_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((BidirectionalGraph<int, IEdge<int>>)null).IsolatedVertices());
        }

        #region Topological sort

        [Test]
        public void TopologicalSort()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                Edge.Create(1, 2),
                Edge.Create(2, 4),
                Edge.Create(3, 1),
                Edge.Create(3, 5),
                Edge.Create(5, 7),
                Edge.Create(6, 3),
                Edge.Create(6, 7)
            ]);

            CollectionAssert.AreEqual(
                (int[]) [6, 3, 5, 7, 1, 2, 4],
                graph.TopologicalSort());
        }

        [Test]
        public void TopologicalSort_Undirected()
        {
            var graph = new UndirectedGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                Edge.Create(1, 2),
                Edge.Create(2, 4),
                Edge.Create(3, 1),
                Edge.Create(3, 5),
                Edge.Create(5, 7),
                Edge.Create(6, 7)
            ]);

            CollectionAssert.AreEqual(
                (int[]) [1, 3, 5, 7, 6, 2, 4],
                graph.TopologicalSort());
        }

        [Test]
        public void TopologicalSort_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexListGraph<int, IEdge<int>>) null).TopologicalSort());

            Assert.Throws<ArgumentNullException>(
                () => ((IUndirectedGraph<int, IEdge<int>>)null).TopologicalSort());
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void SourceFirstTopologicalSort()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                Edge.Create(1, 2),
                Edge.Create(2, 4),
                Edge.Create(3, 1),
                Edge.Create(3, 5),
                Edge.Create(5, 7),
                Edge.Create(6, 3),
                Edge.Create(6, 7)
            ]);

            CollectionAssert.AreEqual(
                (int[]) [6, 3, 1, 5, 2, 7, 4],
                graph.SourceFirstTopologicalSort());
        }

        [Test]
        public void SourceFirstTopologicalSort_Undirected()
        {
            var graph = new UndirectedGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                Edge.Create(1, 2),
                Edge.Create(2, 4),
                Edge.Create(3, 1),
                Edge.Create(3, 5),
                Edge.Create(5, 7),
                Edge.Create(6, 7)
            ]);

            CollectionAssert.AreEqual(
                (int[]) [4, 6, 2, 7, 1, 5, 3],
                graph.SourceFirstTopologicalSort());
        }

        [Test]
        public void SourceFirstTopologicalSort_Throws()
        {
            IVertexAndEdgeListGraph<int, IEdge<int>> nullEdgeListGraph = null;
            IUndirectedGraph<int, IEdge<int>> undirectedGraph = null;
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => nullEdgeListGraph.SourceFirstTopologicalSort());
            Assert.Throws<ArgumentNullException>(() => undirectedGraph.SourceFirstTopologicalSort());
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void SourceFirstBidirectionalTopologicalSort()
        {
            var graph = new BidirectionalGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                Edge.Create(1, 2),
                Edge.Create(2, 4),
                Edge.Create(3, 1),
                Edge.Create(3, 5),
                Edge.Create(5, 7),
                Edge.Create(6, 3),
                Edge.Create(6, 7)
            ]);

            CollectionAssert.AreEqual(
                (int[]) [6, 3, 1, 5, 2, 7, 4],
                graph.SourceFirstBidirectionalTopologicalSort());

            CollectionAssert.AreEqual(
                (int[]) [6, 3, 1, 5, 2, 7, 4],
                graph.SourceFirstBidirectionalTopologicalSort());

            CollectionAssert.AreEqual(
                (int[]) [4, 7, 2, 5, 1, 3, 6],
                graph.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward));
        }

        [Test]
        public void SourceFirstBidirectionalTopologicalSort_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IBidirectionalGraph<int, IEdge<int>>)null).SourceFirstBidirectionalTopologicalSort());

            Assert.Throws<ArgumentNullException>(
                () => ((IBidirectionalGraph<int, IEdge<int>>)null).SourceFirstBidirectionalTopologicalSort());
            Assert.Throws<ArgumentNullException>(
                () => ((IBidirectionalGraph<int, IEdge<int>>)null).SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        #endregion

        #region Connected components

        [Test]
        public void ConnectedComponents()
        {
            var graph = new UndirectedGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                Edge.Create(1, 2),
                Edge.Create(1, 4),
                Edge.Create(2, 3),
                Edge.Create(3, 1),
                Edge.Create(4, 5),
                Edge.Create(5, 6),
                Edge.Create(6, 7),
                Edge.Create(7, 5),

                Edge.Create(8, 9)
            ]);

            var components = new Dictionary<int, int>();

            Assert.AreEqual(2, graph.ConnectedComponents(components));
            CollectionAssert.AreEquivalent(
                new Dictionary<int, int>
                {
                    [1] = 0,
                    [2] = 0,
                    [3] = 0,
                    [4] = 0,
                    [5] = 0,
                    [6] = 0,
                    [7] = 0,
                    [8] = 1,
                    [9] = 1
                },
                components);
        }

        [Test]
        public void ConnectedComponents_Throws()
        {
            IUndirectedGraph<int, IEdge<int>> nullGraph = null;
            var undirectedGraph = new UndirectedGraph<int, IEdge<int>>();
            var components = new Dictionary<int, int>();

            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            _ = undirectedGraph.ConnectedComponents(null);

            Assert.Throws<ArgumentNullException>(
                () => nullGraph.ConnectedComponents(components));
            Assert.Throws<ArgumentNullException>(
                () => nullGraph.ConnectedComponents(null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void IncrementalConnectedComponent()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVertexRange([0, 1, 2, 3]);
            using (graph.IncrementalConnectedComponents(
                out Func<KeyValuePair<int, IDictionary<int, int>>> getComponents))
            {
                KeyValuePair<int, IDictionary<int, int>> current = getComponents();
                Assert.AreEqual(4, current.Key);

                graph.AddEdge(Edge.Create(0, 1));
                current = getComponents();
                Assert.AreEqual(3, current.Key);

                graph.AddEdge(Edge.Create(2, 3));
                current = getComponents();
                Assert.AreEqual(2, current.Key);

                graph.AddEdge(Edge.Create(1, 3));
                current = getComponents();
                Assert.AreEqual(1, current.Key);

                graph.AddVertex(4);
                current = getComponents();
                Assert.AreEqual(2, current.Key);
            }
        }

        [Test]
        public void IncrementalConnectedComponent_Throws()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.IncrementalConnectedComponents<int, IEdge<int>>(null, out _));
        }

        [Test]
        public void StronglyConnectedComponents()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                Edge.Create(1, 2),
                Edge.Create(1, 4),
                Edge.Create(2, 3),
                Edge.Create(3, 1),
                Edge.Create(4, 5),
                Edge.Create(5, 6),
                Edge.Create(6, 7),
                Edge.Create(7, 5)
            ]);

            var components = new Dictionary<int, int>();

            Assert.AreEqual(3, graph.StronglyConnectedComponents(components));
            CollectionAssert.AreEquivalent(
                new Dictionary<int, int>
                {
                    [1] = 2,
                    [2] = 2,
                    [3] = 2,
                    [4] = 1,
                    [5] = 0,
                    [6] = 0,
                    [7] = 0
                },
                components);
        }

        [Test]
        public void StronglyConnectedComponents_Throws()
        {
            var adjacencyGraph = new AdjacencyGraph<int, IEdge<int>>();
            var components = new Dictionary<int, int>();

            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            _ = adjacencyGraph.StronglyConnectedComponents();
            IVertexListGraph<int, IEdge<int>> nullGraph = null;
            Assert.Throws<ArgumentNullException>(
                () => nullGraph.StronglyConnectedComponents(components));
            Assert.Throws<ArgumentNullException>(
                () => nullGraph.StronglyConnectedComponents());
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void WeaklyConnectedComponents()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
            [
                Edge.Create(1, 2),
                Edge.Create(1, 4),
                Edge.Create(2, 3),
                Edge.Create(3, 1),
                Edge.Create(4, 5),
                Edge.Create(5, 6),
                Edge.Create(6, 7),
                Edge.Create(7, 5),
                
                Edge.Create(8, 9)
            ]);

            var components = new Dictionary<int, int>();

            Assert.AreEqual(2, graph.WeaklyConnectedComponents(components));
            CollectionAssert.AreEquivalent(
                new Dictionary<int, int>
                {
                    [1] = 0,
                    [2] = 0,
                    [3] = 0,
                    [4] = 0,
                    [5] = 0,
                    [6] = 0,
                    [7] = 0,
                    [8] = 1,
                    [9] = 1
                },
                components);
        }

        [Test]
        public void WeaklyConnectedComponents_Throws()
        {
            var adjacencyGraph = new AdjacencyGraph<int, IEdge<int>>();
            var components = new Dictionary<int, int>();
            AdjacencyGraph<int, IEdge<int>> nullGraph = null;

            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            _ = adjacencyGraph.WeaklyConnectedComponents();
            Assert.Throws<ArgumentNullException>(
                () => nullGraph.WeaklyConnectedComponents(components));
            Assert.Throws<ArgumentNullException>(
                () => nullGraph.WeaklyConnectedComponents());
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void StronglyCondensedGraph_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.CondensateStronglyConnected<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(null));
        }

        [Test]
        public void WeaklyCondensedGraph_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.CondensateWeaklyConnected<int, IEdge<int>, AdjacencyGraph<int, IEdge<int>>>(null));
        }

        [Test]
        public void EdgesCondensedGraph_Throws()
        {
            var graph = new BidirectionalGraph<int, IEdge<int>>();

            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IBidirectionalGraph<int, IEdge<int>>)null).CondensateEdges(_ => true));
            Assert.Throws<ArgumentNullException>(
                () => graph.CondensateEdges(null));
            Assert.Throws<ArgumentNullException>(
                () => ((IBidirectionalGraph<int, IEdge<int>>)null).CondensateEdges(null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        #endregion

        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> OddVerticesTestCases
        {
            [UsedImplicitly]
            get
            {
                yield return new TestCaseData(
                    new AdjacencyGraph<int, IEdge<int>>(),
                    Enumerable.Empty<int>());

                var edge12 = Edge.Create(1, 2);
                var edge14 = Edge.Create(1, 4);
                var edge22 = Edge.Create(2, 2);
                var edge23 = Edge.Create(2, 3);
                var edge24 = Edge.Create(2, 4);
                var edge25 = Edge.Create(2, 5);
                var edge26 = Edge.Create(2, 6);
                var edge35 = Edge.Create(3, 5);
                var edge41 = Edge.Create(4, 1);
                var edge45 = Edge.Create(4, 5);
                var edge46 = Edge.Create(4, 6);

                var cycleGraph = new AdjacencyGraph<int, IEdge<int>>();
                cycleGraph.AddVerticesAndEdgeRange(
                [
                    edge12, edge24, edge41
                ]);
                yield return new TestCaseData(
                    cycleGraph,
                    Enumerable.Empty<int>());

                var cycleGraph2 = new AdjacencyGraph<int, IEdge<int>>();
                cycleGraph2.AddVerticesAndEdgeRange(
                [
                    edge12, edge24, edge41, edge22
                ]);
                yield return new TestCaseData(
                    cycleGraph2,
                    Enumerable.Empty<int>());

                var cycleGraph3 = new AdjacencyGraph<int, IEdge<int>>();
                cycleGraph3.AddVerticesAndEdgeRange(
                [
                    edge12, edge24, edge25, edge35, edge41, edge22
                ]);
                yield return new TestCaseData(
                    cycleGraph3,
                    (int[]) [2, 3]);

                var cycleGraph4 = new AdjacencyGraph<int, IEdge<int>>();
                cycleGraph4.AddVerticesAndEdgeRange(
                [
                    edge12, edge22, edge24, edge25, edge35, edge41, edge45
                ]);
                yield return new TestCaseData(
                    cycleGraph4,
                    (int[]) [2, 3, 4, 5]);

                var graph1 = new AdjacencyGraph<int, IEdge<int>>();
                graph1.AddVerticesAndEdgeRange(
                [
                    edge12, edge14, edge23, edge24, edge35, edge45
                ]);
                yield return new TestCaseData(
                    graph1,
                    (int[]) [2, 4]);

                var graph2 = new AdjacencyGraph<int, IEdge<int>>();
                graph2.AddVerticesAndEdgeRange(
                [
                    edge12, edge14, edge23, edge24, edge26, edge35, edge45, edge46
                ]);
                yield return new TestCaseData(
                    graph2,
                    Enumerable.Empty<int>());
            }
        }

        [TestCaseSource(nameof(OddVerticesTestCases))]
        public void OddVertices(
            [NotNull] IVertexAndEdgeListGraph<int, IEdge<int>> graph,
            [NotNull] IEnumerable<int> expectedOddVertices)
        {
            CollectionAssert.AreEquivalent(expectedOddVertices, graph.OddVertices());
        }

        [Test]
        public void OddVertices_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((AdjacencyGraph<int, IEdge<int>>)null).OddVertices());
        }

        [Pure]
        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> CreateIsDirectedAcyclicGraphTestCases(
            [NotNull, InstantHandle] Func<IMutableVertexAndEdgeSet<int, IEdge<int>>> createGraph)
        {
            // Empty graph
            yield return new TestCaseData(createGraph())
            {
                ExpectedResult = true
            };

            var edge12 = Edge.Create(1, 2);
            var edge13 = Edge.Create(1, 3);
            var edge14 = Edge.Create(1, 4);
            var edge16 = Edge.Create(1, 6);
            var edge22 = Edge.Create(2, 2);
            var edge23 = Edge.Create(2, 3);
            var edge24 = Edge.Create(2, 4);
            var edge25 = Edge.Create(2, 5);
            var edge31 = Edge.Create(3, 1);
            var edge34 = Edge.Create(3, 4);
            var edge35 = Edge.Create(3, 5);
            var edge41 = Edge.Create(4, 1);
            var edge44 = Edge.Create(4, 4);
            var edge52 = Edge.Create(5, 2);
            var edge56 = Edge.Create(5, 6);

            // Not empty acyclic
            var adjacencyGraph1 = createGraph();
            adjacencyGraph1.AddVertexRange([1, 2, 3]);
            yield return new TestCaseData(adjacencyGraph1)
            {
                ExpectedResult = true
            };

            var adjacencyGraph2 = createGraph();
            adjacencyGraph2.AddVerticesAndEdgeRange(
            [
                edge12, edge14, edge23, edge24
            ]);
            yield return new TestCaseData(adjacencyGraph2)
            {
                ExpectedResult = true
            };

            var adjacencyGraph3 = createGraph();
            adjacencyGraph3.AddVertex(0);
            adjacencyGraph3.AddVerticesAndEdgeRange(
            [
                edge12, edge14, edge23, edge56
            ]);
            yield return new TestCaseData(adjacencyGraph3)
            {
                ExpectedResult = true
            };

            // Not acyclic
            var cyclicGraph1 = createGraph();
            cyclicGraph1.AddVerticesAndEdge(edge22);
            yield return new TestCaseData(cyclicGraph1)
            {
                ExpectedResult = false
            };

            var cyclicGraph2 = createGraph();
            cyclicGraph2.AddVerticesAndEdgeRange(
            [
                edge12, edge14, edge22, edge23, edge24
            ]);
            yield return new TestCaseData(cyclicGraph2)
            {
                ExpectedResult = false
            };

            var cyclicGraph3 = createGraph();
            cyclicGraph3.AddVerticesAndEdgeRange(
            [
                edge12, edge14, edge23, edge24, edge41
            ]);
            yield return new TestCaseData(cyclicGraph3)
            {
                ExpectedResult = false
            };

            var cyclicGraph4 = createGraph();
            cyclicGraph4.AddVerticesAndEdgeRange(
            [
                edge12, edge13, edge23, edge31, edge34, edge44
            ]);
            yield return new TestCaseData(cyclicGraph4)
            {
                ExpectedResult = false
            };

            var cyclicGraph5 = createGraph();
            cyclicGraph5.AddVertex(0);
            cyclicGraph5.AddVerticesAndEdgeRange(
            [
                edge16, edge23, edge25, edge34, edge35, edge52
            ]);
            yield return new TestCaseData(cyclicGraph5)
            {
                ExpectedResult = false
            };
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> IsDirectedAcyclicGraphTestCases
        {
            [UsedImplicitly]
            get
            {
                IEnumerable<TestCaseData> testCases = CreateIsDirectedAcyclicGraphTestCases(() => new AdjacencyGraph<int, IEdge<int>>())
                    .Concat(CreateIsDirectedAcyclicGraphTestCases(() => new BidirectionalGraph<int, IEdge<int>>()));
                foreach (TestCaseData testCase in testCases)
                {
                    yield return testCase;
                }
            }
        }

        [TestCaseSource(nameof(IsDirectedAcyclicGraphTestCases))]
        public bool IsDirectedAcyclicGraph([NotNull] IVertexAndEdgeListGraph<int, IEdge<int>> graph)
        {
            return graph.IsDirectedAcyclicGraph();
        }

        [TestCaseSource(nameof(IsDirectedAcyclicGraphTestCases))]
        public bool IsDirectedAcyclicGraph_FromEdges([NotNull] IVertexAndEdgeListGraph<int, IEdge<int>> graph)
        {
            return graph.Edges.IsDirectedAcyclicGraph<int, IEdge<int>>();
        }

        [Test]
        public void IsDirectedAcyclicGraph_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((AdjacencyGraph<int, IEdge<int>>)null).IsDirectedAcyclicGraph());

            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<IEdge<int>>)null).IsDirectedAcyclicGraph<int, IEdge<int>>());
            IEdge<int>[] edges = [Edge.Create(1, 2), null, Edge.Create(1, 3)];
            Assert.Throws<ArgumentNullException>(
                () => edges.IsDirectedAcyclicGraph<int, IEdge<int>>());
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> IsUndirectedAcyclicGraphTestCases
        {
            [UsedImplicitly]
            get
            {
                // Empty graph
                yield return new TestCaseData(new UndirectedGraph<int, IEdge<int>>())
                {
                    ExpectedResult = true
                };

                var edge12 = Edge.Create(1, 2);
                var edge14 = Edge.Create(1, 4);
                var edge16 = Edge.Create(1, 6);
                var edge22 = Edge.Create(2, 2);
                var edge23 = Edge.Create(2, 3);
                var edge24 = Edge.Create(2, 4);
                var edge25 = Edge.Create(2, 5);
                var edge35 = Edge.Create(3, 5);
                var edge56 = Edge.Create(5, 6);

                // Not empty acyclic
                var undirectedGraph1 = new UndirectedGraph<int, IEdge<int>>();
                undirectedGraph1.AddVertexRange([1, 2, 3]);
                yield return new TestCaseData(undirectedGraph1)
                {
                    ExpectedResult = true
                };

                var undirectedGraph2 = new UndirectedGraph<int, IEdge<int>>();
                undirectedGraph2.AddVerticesAndEdgeRange(
                [
                    edge12, edge23, edge24
                ]);
                yield return new TestCaseData(undirectedGraph2)
                {
                    ExpectedResult = true
                };

                var undirectedGraph3 = new UndirectedGraph<int, IEdge<int>>();
                undirectedGraph3.AddVertex(0);
                undirectedGraph3.AddVerticesAndEdgeRange(
                [
                    edge12, edge14, edge23, edge56
                ]);
                yield return new TestCaseData(undirectedGraph3)
                {
                    ExpectedResult = true
                };

                // Not acyclic
                var cyclicGraph1 = new UndirectedGraph<int, IEdge<int>>();
                cyclicGraph1.AddVerticesAndEdge(edge22);
                yield return new TestCaseData(cyclicGraph1)
                {
                    ExpectedResult = false
                };

                var cyclicGraph2 = new UndirectedGraph<int, IEdge<int>>();
                cyclicGraph2.AddVerticesAndEdgeRange(
                [
                    edge12, edge14, edge22, edge23, edge24
                ]);
                yield return new TestCaseData(cyclicGraph2)
                {
                    ExpectedResult = false
                };

                var cyclicGraph3 = new UndirectedGraph<int, IEdge<int>>();
                cyclicGraph3.AddVerticesAndEdgeRange(
                [
                    edge12, edge14, edge23, edge24
                ]);
                yield return new TestCaseData(cyclicGraph3)
                {
                    ExpectedResult = false
                };

                var cyclicGraph4 = new UndirectedGraph<int, IEdge<int>>();
                cyclicGraph4.AddVertex(0);
                cyclicGraph4.AddVerticesAndEdgeRange(
                [
                    edge16, edge23, edge25, edge35
                ]);
                yield return new TestCaseData(cyclicGraph4)
                {
                    ExpectedResult = false
                };
            }
        }

        [TestCaseSource(nameof(IsUndirectedAcyclicGraphTestCases))]
        public bool IsUndirectedAcyclicGraph([NotNull] IUndirectedGraph<int, IEdge<int>> graph)
        {
            return graph.IsUndirectedAcyclicGraph();
        }

        [TestCaseSource(nameof(IsUndirectedAcyclicGraphTestCases))]
        public bool IsUndirectedAcyclicGraph_FromEdges([NotNull] IUndirectedGraph<int, IEdge<int>> graph)
        {
            return graph.Edges.IsUndirectedAcyclicGraph<int, IEdge<int>>();
        }

        [Test]
        public void IsUndirectedAcyclicGraph_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((UndirectedGraph<int, IEdge<int>>)null).IsUndirectedAcyclicGraph());

            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<IEdge<int>>)null).IsUndirectedAcyclicGraph<int, IEdge<int>>());
            IEdge<int>[] edges = [Edge.Create(1, 2), null, Edge.Create(1, 3)];
            Assert.Throws<ArgumentNullException>(
                () => edges.IsUndirectedAcyclicGraph<int, IEdge<int>>());
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ComputePredecessorCost()
        {
            var predecessors = new Dictionary<int, IEdge<int>>();
            var edgeCosts = new Dictionary<IEdge<int>, double>();

            Assert.AreEqual(0, AlgorithmExtensions.ComputePredecessorCost(predecessors, edgeCosts, 1));

            var edge12 = Edge.Create(1, 2);
            predecessors[2] = edge12;
            edgeCosts[edge12] = 12;
            Assert.AreEqual(0, AlgorithmExtensions.ComputePredecessorCost(predecessors, edgeCosts, 1));
            Assert.AreEqual(12, AlgorithmExtensions.ComputePredecessorCost(predecessors, edgeCosts, 2));

            var edge31 = Edge.Create(3, 1);
            predecessors[1] = edge31;
            edgeCosts[edge31] = -5;
            var edge34 = Edge.Create(3, 4);
            predecessors[4] = edge34;
            edgeCosts[edge34] = 42;

            Assert.AreEqual(-5, AlgorithmExtensions.ComputePredecessorCost(predecessors, edgeCosts, 1));
            Assert.AreEqual(7, AlgorithmExtensions.ComputePredecessorCost(predecessors, edgeCosts, 2));
            Assert.AreEqual(0, AlgorithmExtensions.ComputePredecessorCost(predecessors, edgeCosts, 3));
            Assert.AreEqual(42, AlgorithmExtensions.ComputePredecessorCost(predecessors, edgeCosts, 4));
        }

        [Test]
        public void ComputePredecessorCost_Throws()
        {
            var predecessors = new Dictionary<TestVertex, Edge<TestVertex>>();
            var edgeCosts = new Dictionary<Edge<TestVertex>, double>();
            var vertex1 = new TestVertex("1");
            var vertex2 = new TestVertex("2");

            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.ComputePredecessorCost(null, edgeCosts, vertex1));
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.ComputePredecessorCost(predecessors, null, vertex1));
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.ComputePredecessorCost(predecessors, edgeCosts, null));
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.ComputePredecessorCost<TestVertex, Edge<TestVertex>>(null, null, vertex1));
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.ComputePredecessorCost(predecessors, null, null));
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.ComputePredecessorCost<TestVertex, Edge<TestVertex>>(null, null, null));

            // Wrong usage
            predecessors[vertex2] = new Edge<TestVertex>(vertex1, vertex2);
            Assert.Throws<KeyNotFoundException>(
                () => AlgorithmExtensions.ComputePredecessorCost(predecessors, edgeCosts, vertex2));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ComputeDisjointSet()
        {
            var emptyGraph = new UndirectedGraph<int, IEdge<int>>();
            IDisjointSet<int> disjointSet = emptyGraph.ComputeDisjointSet();
            Assert.AreEqual(0, disjointSet.ElementCount);
            Assert.AreEqual(0, disjointSet.SetCount);

            var graph = new UndirectedGraph<int, IEdge<int>>();
            graph.AddVertexRange([1, 2, 3, 4]);
            disjointSet = graph.ComputeDisjointSet();
            Assert.AreEqual(4, disjointSet.ElementCount);
            Assert.AreEqual(4, disjointSet.SetCount);

            graph.AddEdgeRange(
            [
                Edge.Create(1, 2),
                Edge.Create(2, 3),
                Edge.Create(1, 4)
            ]);
            graph.AddVertex(5);
            disjointSet = graph.ComputeDisjointSet();
            Assert.AreEqual(5, disjointSet.ElementCount);
            Assert.AreEqual(2, disjointSet.SetCount);
            Assert.IsTrue(disjointSet.AreInSameSet(1, 2));
            Assert.IsTrue(disjointSet.AreInSameSet(1, 3));
            Assert.IsTrue(disjointSet.AreInSameSet(1, 4));
            Assert.IsFalse(disjointSet.AreInSameSet(1, 5));
        }

        [Test]
        public void ComputeDisjointSet_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((UndirectedGraph<int, IEdge<int>>)null).ComputeDisjointSet());
        }

        [Test]
        public void MinimumSpanningTreePrim_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => new UndirectedGraph<int, IEdge<int>>().MinimumSpanningTreePrim(null));
            Assert.Throws<ArgumentNullException>(
                () => ((UndirectedGraph<int, IEdge<int>>)null).MinimumSpanningTreePrim(_ => 1.0));
            Assert.Throws<ArgumentNullException>(
                () => ((UndirectedGraph<int, IEdge<int>>)null).MinimumSpanningTreePrim(null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void MinimumSpanningTreeKruskal_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => new UndirectedGraph<int, IEdge<int>>().MinimumSpanningTreeKruskal(null));
            Assert.Throws<ArgumentNullException>(
                () => ((UndirectedGraph<int, IEdge<int>>)null).MinimumSpanningTreeKruskal(_ => 1.0));
            Assert.Throws<ArgumentNullException>(
                () => ((UndirectedGraph<int, IEdge<int>>)null).MinimumSpanningTreeKruskal(null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void OfflineLeastCommonAncestor_Throws()
        {
            var vertex1 = new TestVertex("1");
            var vertex2 = new TestVertex("2");
            var graph1 = new AdjacencyGraph<TestVertex, Edge<TestVertex>>();
            graph1.AddVertexRange([vertex1, vertex2]);
            SEquatableEdge<TestVertex>[] pairs1 = [new SEquatableEdge<TestVertex>(vertex1, vertex2)];
            
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexListGraph<TestVertex, Edge<TestVertex>>)null).OfflineLeastCommonAncestor(vertex1, pairs1));
            Assert.Throws<ArgumentNullException>(
                () => graph1.OfflineLeastCommonAncestor(null, pairs1));
            Assert.Throws<ArgumentNullException>(
                () => graph1.OfflineLeastCommonAncestor(vertex1, null));
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexListGraph<TestVertex, Edge<TestVertex>>)null).OfflineLeastCommonAncestor(null, pairs1));
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexListGraph<TestVertex, Edge<TestVertex>>)null).OfflineLeastCommonAncestor(vertex1, null));
            Assert.Throws<ArgumentNullException>(
                () => graph1.OfflineLeastCommonAncestor(null, null));
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexListGraph<TestVertex, Edge<TestVertex>>)null).OfflineLeastCommonAncestor(null, null));

            SEquatableEdge<int>[] pairs2 = [new SEquatableEdge<int>(1, 2)];
            var graph2 = new AdjacencyGraph<int, IEdge<int>>();
            Assert.Throws<ArgumentException>(
                () => graph2.OfflineLeastCommonAncestor(1, pairs2));

            var graph3 = new AdjacencyGraph<int, IEdge<int>>();
            graph3.AddVertex(1);
            Assert.Throws<ArgumentException>(
                () => graph3.OfflineLeastCommonAncestor(1, pairs2));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void MaximumFlow_Throws()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVertexRange([1, 2]);
            Func<IEdge<int>, double> capacities = _ => 1.0;
            EdgeFactory<int, IEdge<int>> edgeFactory = Edge.Create;
            var reverseEdgesAlgorithm = graph.CreateReversedEdgeAugmentorAlgorithm(edgeFactory);

            Assert.Throws<ArgumentException>(
                () => graph.MaximumFlow(capacities, 1, 1, out _, edgeFactory, reverseEdgesAlgorithm));

            Assert.Throws<InvalidOperationException>(
                () => graph.MaximumFlow(capacities, 1, 2, out _, edgeFactory, reverseEdgesAlgorithm));
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> CloneTestCases
        {
            [UsedImplicitly]
            get
            {
                yield return new TestCaseData(new AdjacencyGraph<int, EquatableEdge<int>>());
                yield return new TestCaseData(new BidirectionalGraph<int, EquatableEdge<int>>());
            }
        }

        [TestCaseSource(nameof(CloneTestCases))]
        public void Clone([NotNull] IMutableVertexAndEdgeSet<int, EquatableEdge<int>> cloned)
        {
            var emptyGraph1 = new AdjacencyGraph<int, EquatableEdge<int>>();
            emptyGraph1.Clone(v => v, (_, v1, v2) => new EquatableEdge<int>(v1, v2), cloned);
            AssertEmptyGraph(cloned);

            cloned.Clear();
            var notEmptyGraph = new AdjacencyGraph<int, EquatableEdge<int>>();
            notEmptyGraph.AddVerticesAndEdgeRange(
            [
                new EquatableEdge<int>(1, 2),
                new EquatableEdge<int>(2, 2),
                new EquatableEdge<int>(2, 3),
                new EquatableEdge<int>(3, 1)
            ]);
            notEmptyGraph.Clone(v => v, (_, v1, v2) => new EquatableEdge<int>(v1, v2), cloned);
            AssertHasVertices(cloned, [1, 2, 3]);
            AssertHasEdges(
                cloned,
                [
                    new EquatableEdge<int>(1, 2),
                    new EquatableEdge<int>(2, 2),
                    new EquatableEdge<int>(2, 3),
                    new EquatableEdge<int>(3, 1)
                ]);

            // Clone is not empty
            cloned.Clear();
            cloned.AddVerticesAndEdge(new EquatableEdge<int>(1, 4));
            notEmptyGraph.Clone(v => v, (_, v1, v2) => new EquatableEdge<int>(v1, v2), cloned);
            // Clone has been cleaned and then re-filled
            AssertHasVertices(cloned, [1, 2, 3]);
            AssertHasEdges(
                cloned,
                [
                    new EquatableEdge<int>(1, 2),
                    new EquatableEdge<int>(2, 2),
                    new EquatableEdge<int>(2, 3),
                    new EquatableEdge<int>(3, 1)
                ]);
        }

        [Test]
        public void Clone_Throws()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var clone = new AdjacencyGraph<int, IEdge<int>>();

            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.Clone(null, v => v, (e, _, _) => e, clone));
            Assert.Throws<ArgumentNullException>(
                () => graph.Clone(null, (e, _, _) => e, clone));
            Assert.Throws<ArgumentNullException>(
                () => graph.Clone(v => v, null, clone));
            Assert.Throws<ArgumentNullException>(
                () => graph.Clone(v => v, (e, _, _) => e, null));
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.Clone(null, null, (e, _, _) => e, clone));
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.Clone(null, v => v, null, clone));
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.Clone<int, IEdge<int>>(null, v => v, (e, _, _) => e, null));
            Assert.Throws<ArgumentNullException>(
                () => graph.Clone(null, null, clone));
            Assert.Throws<ArgumentNullException>(
                () => graph.Clone(null, (e, _, _) => e, null));
            Assert.Throws<ArgumentNullException>(
                () => graph.Clone(v => v, null, null));
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.Clone(null, null, null, clone));
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.Clone<int, IEdge<int>>(null, null, (e, _, _) => e, null));
            Assert.Throws<ArgumentNullException>(
                () => graph.Clone(null, null, null));
            Assert.Throws<ArgumentNullException>(
                () => AlgorithmExtensions.Clone<int, IEdge<int>>(null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        static readonly Dictionary<string, string[]> GraphRoots = new ()
        {
{ "Empty", [] },
{ "g.10.0", ["n8"] },
{ "g.10.1", ["n0","n3","n5","n8","n9"] },
{ "g.10.11", ["n3"] },
{ "g.10.12", ["n4","n7"] },
{ "g.10.13", ["n4","n7"] },
{ "g.10.14", ["n2","n5","n7"] },
{ "g.10.15", ["n4","n6"] },
{ "g.10.16", ["n1","n7"] },
{ "g.10.17", ["n0"] },
{ "g.10.19", ["n7"] },
{ "g.10.2", ["n4","n5","n6","n8","n9"] },
{ "g.10.20", ["n7"] },
{ "g.10.22", ["n7"] },
{ "g.10.24", ["n7"] },
{ "g.10.25", ["n0"] },
{ "g.10.27", ["n0"] },
{ "g.10.28", ["n6"] },
{ "g.10.29", ["n6"] },
{ "g.10.3", ["n0"] },
{ "g.10.30", ["n6"] },
{ "g.10.31", ["n6"] },
{ "g.10.34", ["n8"] },
{ "g.10.37", ["n3","n5"] },
{ "g.10.38", ["n0"] },
{ "g.10.39", ["n0"] },
{ "g.10.4", ["n0"] },
{ "g.10.40", ["n0"] },
{ "g.10.41", ["n0"] },
{ "g.10.42", ["n0","n2","n3","n8","n9"] },
{ "g.10.45", ["n6"] },
{ "g.10.46", ["n6"] },
{ "g.10.5", ["n0"] },
{ "g.10.50", ["n0"] },
{ "g.10.56", ["n0"] },
{ "g.10.57", ["n6"] },
{ "g.10.58", ["n6"] },
{ "g.10.6", ["n0"] },
{ "g.10.60", ["n0"] },
{ "g.10.61", ["n2"] },
{ "g.10.62", ["n2"] },
{ "g.10.68", ["n0","n9"] },
{ "g.10.69", ["n0","n5","n8","n9"] },
{ "g.10.7", ["n0","n9"] },
{ "g.10.70", ["n0"] },
{ "g.10.71", ["n2"] },
{ "g.10.72", ["n1"] },
{ "g.10.74", ["n0"] },
{ "g.10.75", ["n0","n3","n4"] },
{ "g.10.78", ["n0","n2","n6","n8","n9"] },
{ "g.10.79", ["n7"] },
{ "g.10.8", ["n0"] },
{ "g.10.80", ["n0"] },
{ "g.10.82", ["n7"] },
{ "g.10.83", ["n7"] },
{ "g.10.85", ["n8"] },
{ "g.10.86", ["n0","n7","n9"] },
{ "g.10.88", ["n9"] },
{ "g.10.89", ["n0"] },
{ "g.10.9", ["n0"] },
{ "g.10.90", ["n0"] },
{ "g.10.91", ["n0"] },
{ "g.10.92", ["n0","n3","n4","n5","n7","n8","n9"] },
{ "g.10.93", ["n1"] },
{ "g.10.94", ["n0"] },
{ "g.100.0", ["n0","n1","n4","n11","n15","n23","n49","n51","n68","n71","n76","n83","n84","n89","n91","n92","n93","n94","n95","n96","n97","n98","n99"] },
{ "g.100.1", ["n0","n1","n4","n11","n15","n23","n49","n51","n68","n71","n76","n83","n84","n89","n91","n92","n93","n94","n95","n96","n97","n98","n99"] },
{ "g.100.3", ["n4","n9","n10","n13","n19","n20","n21","n25","n26","n27","n28","n29","n34","n35","n41","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n64","n65","n70","n71","n72","n73","n80","n90","n91","n93","n96","n97"] },
{ "g.11.0", ["n1","n3","n9"] },
{ "g.11.10", ["n4","n9"] },
{ "g.11.16", ["n0","n9","n10"] },
{ "g.11.17", ["n0","n9","n10"] },
{ "g.11.19", ["n9"] },
{ "g.11.2", ["n2","n6"] },
{ "g.11.20", ["n0","n1","n7","n10"] },
{ "g.11.21", ["n0"] },
{ "g.11.22", ["n0","n6","n10"] },
{ "g.11.23", ["n9"] },
{ "g.11.24", ["n9"] },
{ "g.11.25", ["n0","n10"] },
{ "g.11.28", ["n0"] },
{ "g.11.29", ["n8"] },
{ "g.11.3", ["n6","n9"] },
{ "g.11.30", ["n0"] },
{ "g.11.35", ["n0"] },
{ "g.11.36", ["n0"] },
{ "g.11.37", ["n8"] },
{ "g.11.38", ["n0"] },
{ "g.11.45", ["n0"] },
{ "g.11.46", ["n0"] },
{ "g.11.49", ["n0","n2","n4","n7","n8","n9"] },
{ "g.11.5", ["n0","n6","n8"] },
{ "g.11.50", ["n0"] },
{ "g.11.51", ["n0","n4"] },
{ "g.11.55", ["n0"] },
{ "g.11.57", ["n0"] },
{ "g.11.6", ["n0"] },
{ "g.11.60", ["n0","n3"] },
{ "g.11.61", ["n0"] },
{ "g.11.62", ["n0"] },
{ "g.11.63", ["n0"] },
{ "g.11.64", ["n0","n6"] },
{ "g.11.65", ["n4"] },
{ "g.11.66", ["n5"] },
{ "g.11.68", ["n7"] },
{ "g.11.69", ["n0","n8"] },
{ "g.11.7", ["n0","n5","n7"] },
{ "g.11.71", ["n0"] },
{ "g.11.72", ["n4"] },
{ "g.11.73", ["n0"] },
{ "g.11.75", ["n0","n7"] },
{ "g.11.76", ["n0"] },
{ "g.11.9", ["n1","n5"] },
{ "g.12.0", ["n5"] },
{ "g.12.101", ["n3","n7"] },
{ "g.12.102", ["n0"] },
{ "g.12.104", ["n0","n2","n5","n8","n9","n10"] },
{ "g.12.105", ["n0","n3","n5","n8","n9","n10"] },
{ "g.12.106", ["n0"] },
{ "g.12.107", ["n0","n11"] },
{ "g.12.108", ["n0"] },
{ "g.12.109", ["n0","n4","n8","n10","n11"] },
{ "g.12.110", ["n0"] },
{ "g.12.111", ["n0"] },
{ "g.12.112", ["n0"] },
{ "g.12.113", ["n0","n3","n5","n6"] },
{ "g.12.114", ["n2","n3"] },
{ "g.12.117", ["n0","n3","n4","n6","n8","n9"] },
{ "g.12.118", ["n0"] },
{ "g.12.121", ["n2","n9"] },
{ "g.12.122", ["n0"] },
{ "g.12.123", ["n0"] },
{ "g.12.124", ["n0"] },
{ "g.12.126", ["n11"] },
{ "g.12.128", ["n0","n7"] },
{ "g.12.133", ["n0","n10","n11"] },
{ "g.12.134", ["n1"] },
{ "g.12.135", ["n3","n4","n5","n7","n8","n9","n10","n11"] },
{ "g.12.138", ["n0"] },
{ "g.12.140", ["n0"] },
{ "g.12.141", ["n0","n9","n11"] },
{ "g.12.145", ["n9"] },
{ "g.12.146", ["n9"] },
{ "g.12.22", ["n8"] },
{ "g.12.31", ["n6"] },
{ "g.12.34", ["n0"] },
{ "g.12.35", ["n0"] },
{ "g.12.40", ["n5"] },
{ "g.12.43", ["n1","n3","n9"] },
{ "g.12.45", ["n9","n10","n11"] },
{ "g.12.48", ["n1","n3"] },
{ "g.12.49", ["n4","n7"] },
{ "g.12.50", ["n4","n9"] },
{ "g.12.51", ["n4","n8"] },
{ "g.12.52", ["n6"] },
{ "g.12.55", ["n9"] },
{ "g.12.60", ["n6"] },
{ "g.12.62", ["n6"] },
{ "g.12.63", ["n6"] },
{ "g.12.64", ["n8"] },
{ "g.12.66", ["n10"] },
{ "g.12.7", ["n6"] },
{ "g.12.75", ["n6"] },
{ "g.12.76", ["n9"] },
{ "g.12.77", ["n6"] },
{ "g.12.78", ["n6"] },
{ "g.12.79", ["n6"] },
{ "g.12.8", ["n6"] },
{ "g.12.88", ["n6"] },
{ "g.12.89", ["n0"] },
{ "g.12.9", ["n0"] },
{ "g.12.90", ["n0"] },
{ "g.12.91", ["n6"] },
{ "g.12.92", ["n0"] },
{ "g.12.93", ["n0"] },
{ "g.12.94", ["n0"] },
{ "g.12.98", ["n0","n9"] },
{ "g.12.99", ["n0"] },
{ "g.13.102", ["n1","n5"] },
{ "g.13.105", ["n9"] },
{ "g.13.111", ["n0"] },
{ "g.13.114", ["n0"] },
{ "g.13.116", ["n0"] },
{ "g.13.117", ["n11","n12"] },
{ "g.13.12", ["n0","n4","n9"] },
{ "g.13.120", ["n6","n9"] },
{ "g.13.17", ["n0"] },
{ "g.13.21", ["n11","n12"] },
{ "g.13.25", ["n0"] },
{ "g.13.26", ["n0","n6","n9","n10","n11","n12"] },
{ "g.13.29", ["n3","n10"] },
{ "g.13.30", ["n2","n7","n9"] },
{ "g.13.31", ["n0"] },
{ "g.13.34", ["n12"] },
{ "g.13.35", ["n9"] },
{ "g.13.36", ["n6"] },
{ "g.13.37", ["n4","n7"] },
{ "g.13.38", ["n5","n10"] },
{ "g.13.41", ["n10"] },
{ "g.13.42", ["n9"] },
{ "g.13.44", ["n0"] },
{ "g.13.45", ["n0"] },
{ "g.13.46", ["n10"] },
{ "g.13.47", ["n7"] },
{ "g.13.48", ["n0"] },
{ "g.13.49", ["n0"] },
{ "g.13.51", ["n0"] },
{ "g.13.52", ["n0","n7"] },
{ "g.13.53", ["n8"] },
{ "g.13.54", ["n8"] },
{ "g.13.55", ["n0"] },
{ "g.13.57", ["n0","n9","n11"] },
{ "g.13.6", ["n0"] },
{ "g.13.60", ["n0"] },
{ "g.13.61", ["n0"] },
{ "g.13.62", ["n5"] },
{ "g.13.63", ["n8"] },
{ "g.13.67", ["n0"] },
{ "g.13.68", ["n0"] },
{ "g.13.69", ["n0"] },
{ "g.13.7", ["n0"] },
{ "g.13.76", ["n0"] },
{ "g.13.77", ["n0"] },
{ "g.13.78", ["n0"] },
{ "g.13.79", ["n0"] },
{ "g.13.80", ["n0"] },
{ "g.13.81", ["n0","n3"] },
{ "g.13.83", ["n0"] },
{ "g.13.84", ["n0"] },
{ "g.13.85", ["n0"] },
{ "g.13.86", ["n0"] },
{ "g.13.87", ["n0"] },
{ "g.13.88", ["n0"] },
{ "g.13.9", ["n0"] },
{ "g.13.91", ["n0","n8"] },
{ "g.13.92", ["n0","n8"] },
{ "g.13.93", ["n4"] },
{ "g.13.94", ["n0"] },
{ "g.13.95", ["n7"] },
{ "g.13.96", ["n5"] },
{ "g.13.97", ["n5"] },
{ "g.13.99", ["n8"] },
{ "g.14.0", ["n2","n7","n13"] },
{ "g.14.1", ["n3","n11"] },
{ "g.14.12", ["n7"] },
{ "g.14.13", ["n4"] },
{ "g.14.14", ["n12"] },
{ "g.14.15", ["n0"] },
{ "g.14.16", ["n5","n9","n10"] },
{ "g.14.17", ["n0","n10"] },
{ "g.14.19", ["n0","n12"] },
{ "g.14.2", ["n0"] },
{ "g.14.20", ["n0","n13"] },
{ "g.14.21", ["n0"] },
{ "g.14.22", ["n0"] },
{ "g.14.23", ["n0"] },
{ "g.14.24", ["n0"] },
{ "g.14.25", ["n0"] },
{ "g.14.26", ["n0"] },
{ "g.14.27", ["n0"] },
{ "g.14.28", ["n0"] },
{ "g.14.31", ["n0"] },
{ "g.14.33", ["n0"] },
{ "g.14.34", ["n0"] },
{ "g.14.35", ["n2","n3","n4","n7","n8","n9","n10","n13"] },
{ "g.14.36", ["n0","n11"] },
{ "g.14.37", ["n0"] },
{ "g.14.38", ["n0"] },
{ "g.14.39", ["n0"] },
{ "g.14.40", ["n0"] },
{ "g.14.43", ["n7","n11","n13"] },
{ "g.14.47", ["n0"] },
{ "g.14.55", ["n0"] },
{ "g.14.57", ["n0"] },
{ "g.14.58", ["n11"] },
{ "g.14.59", ["n0"] },
{ "g.14.6", ["n7"] },
{ "g.14.60", ["n0","n1","n6","n9"] },
{ "g.14.61", ["n4"] },
{ "g.14.62", ["n0"] },
{ "g.14.65", ["n0","n11"] },
{ "g.14.66", ["n0","n11"] },
{ "g.14.67", ["n8"] },
{ "g.14.68", ["n11","n12","n13"] },
{ "g.14.7", ["n0"] },
{ "g.14.9", ["n0"] },
{ "g.15.10", ["n0","n8","n11"] },
{ "g.15.11", ["n11","n13"] },
{ "g.15.12", ["n10","n13"] },
{ "g.15.13", ["n10","n12"] },
{ "g.15.14", ["n7","n11"] },
{ "g.15.17", ["n0"] },
{ "g.15.2", ["n7","n12"] },
{ "g.15.20", ["n0","n11","n12","n13","n14"] },
{ "g.15.21", ["n0","n11"] },
{ "g.15.22", ["n0","n14"] },
{ "g.15.23", ["n0","n14"] },
{ "g.15.24", ["n1"] },
{ "g.15.26", ["n0"] },
{ "g.15.28", ["n0"] },
{ "g.15.29", ["n0"] },
{ "g.15.3", ["n0"] },
{ "g.15.31", ["n2"] },
{ "g.15.32", ["n0"] },
{ "g.15.33", ["n0"] },
{ "g.15.34", ["n0"] },
{ "g.15.35", ["n0"] },
{ "g.15.38", ["n3","n4","n5","n6","n7","n8","n9","n10","n11","n12","n13","n14"] },
{ "g.15.39", ["n0"] },
{ "g.15.40", ["n2","n10","n11","n12","n14"] },
{ "g.15.41", ["n2","n10","n11","n12","n14"] },
{ "g.15.43", ["n0"] },
{ "g.15.47", ["n1","n3"] },
{ "g.15.48", ["n2","n4"] },
{ "g.15.49", ["n0"] },
{ "g.15.50", ["n0","n9"] },
{ "g.15.52", ["n0","n9"] },
{ "g.15.56", ["n0"] },
{ "g.15.59", ["n0","n12","n14"] },
{ "g.15.6", ["n8","n13"] },
{ "g.15.7", ["n4","n8","n12"] },
{ "g.15.8", ["n5"] },
{ "g.15.9", ["n2","n4","n6","n8","n12"] },
{ "g.16.16", ["n1","n13"] },
{ "g.16.17", ["n6","n12"] },
{ "g.16.18", ["n7","n9","n12"] },
{ "g.16.21", ["n5","n10"] },
{ "g.16.23", ["n0"] },
{ "g.16.25", ["n0"] },
{ "g.16.27", ["n0"] },
{ "g.16.28", ["n0","n11","n15"] },
{ "g.16.31", ["n0","n4","n13","n14"] },
{ "g.16.34", ["n0","n3","n4","n5","n6","n7","n8","n10","n11","n12","n13","n14","n15"] },
{ "g.16.36", ["n0"] },
{ "g.16.5", ["n9","n13","n15"] },
{ "g.16.50", ["n15"] },
{ "g.16.52", ["n0"] },
{ "g.16.55", ["n0"] },
{ "g.16.57", ["n0"] },
{ "g.16.58", ["n9","n10","n11","n12","n13","n14"] },
{ "g.16.59", ["n0"] },
{ "g.16.60", ["n0"] },
{ "g.16.61", ["n0"] },
{ "g.16.62", ["n0"] },
{ "g.16.63", ["n2","n10","n11","n12","n14","n15"] },
{ "g.16.64", ["n0"] },
{ "g.16.65", ["n1"] },
{ "g.16.66", ["n0"] },
{ "g.16.70", ["n0"] },
{ "g.16.76", ["n0"] },
{ "g.16.79", ["n0","n10"] },
{ "g.16.8", ["n10","n14","n15"] },
{ "g.16.80", ["n0","n9"] },
{ "g.16.81", ["n0"] },
{ "g.16.82", ["n0","n13","n15"] },
{ "g.16.9", ["n0","n14","n15"] },
{ "g.16.91", ["n4","n14"] },
{ "g.16.95", ["n0","n13","n15"] },
{ "g.17.10", ["n6"] },
{ "g.17.11", ["n16"] },
{ "g.17.12", ["n0"] },
{ "g.17.13", ["n0"] },
{ "g.17.17", ["n0","n9","n11"] },
{ "g.17.18", ["n0"] },
{ "g.17.2", ["n7"] },
{ "g.17.20", ["n0","n14","n15","n16"] },
{ "g.17.21", ["n0","n12"] },
{ "g.17.23", ["n0"] },
{ "g.17.24", ["n0"] },
{ "g.17.26", ["n0"] },
{ "g.17.28", ["n0"] },
{ "g.17.3", ["n5","n13"] },
{ "g.17.30", ["n15","n16"] },
{ "g.17.31", ["n0"] },
{ "g.17.32", ["n15"] },
{ "g.17.33", ["n0"] },
{ "g.17.34", ["n0"] },
{ "g.17.36", ["n0"] },
{ "g.17.37", ["n0"] },
{ "g.17.39", ["n0","n1"] },
{ "g.17.4", ["n1","n5","n7","n13"] },
{ "g.17.40", ["n4","n12"] },
{ "g.17.41", ["n5"] },
{ "g.17.42", ["n3","n4","n5","n8","n9","n10","n11","n14","n15","n16"] },
{ "g.17.45", ["n4","n15"] },
{ "g.17.6", ["n3","n10"] },
{ "g.17.7", ["n9","n14"] },
{ "g.17.8", ["n13"] },
{ "g.17.9", ["n0"] },
{ "g.18.0", ["n3","n16"] },
{ "g.18.1", ["n0"] },
{ "g.18.10", ["n2"] },
{ "g.18.11", ["n3","n6"] },
{ "g.18.12", ["n0","n8"] },
{ "g.18.13", ["n0"] },
{ "g.18.14", ["n0"] },
{ "g.18.15", ["n0"] },
{ "g.18.16", ["n0"] },
{ "g.18.17", ["n10"] },
{ "g.18.18", ["n11"] },
{ "g.18.19", ["n1","n10","n13"] },
{ "g.18.2", ["n0"] },
{ "g.18.20", ["n0"] },
{ "g.18.21", ["n3","n7"] },
{ "g.18.22", ["n11"] },
{ "g.18.24", ["n12"] },
{ "g.18.26", ["n0"] },
{ "g.18.28", ["n0"] },
{ "g.18.29", ["n0","n8","n11","n13","n16"] },
{ "g.18.30", ["n0"] },
{ "g.18.31", ["n0"] },
{ "g.18.34", ["n12"] },
{ "g.18.35", ["n0"] },
{ "g.18.36", ["n0"] },
{ "g.18.37", ["n10","n11","n12","n13","n14","n15","n16","n17"] },
{ "g.18.38", ["n14","n16","n17"] },
{ "g.18.4", ["n9","n10","n11","n12","n13","n14","n15","n16","n17"] },
{ "g.18.41", ["n0","n1"] },
{ "g.18.5", ["n0","n3","n7","n14","n17"] },
{ "g.18.6", ["n2","n6","n11","n15"] },
{ "g.18.7", ["n13","n16"] },
{ "g.18.8", ["n1","n8"] },
{ "g.18.9", ["n5","n7","n14"] },
{ "g.19.0", ["n0","n14","n15"] },
{ "g.19.11", ["n1","n4","n15"] },
{ "g.19.12", ["n0"] },
{ "g.19.18", ["n0"] },
{ "g.19.19", ["n0"] },
{ "g.19.20", ["n7","n9","n10","n11","n12","n15","n16","n17","n18"] },
{ "g.19.21", ["n0","n16"] },
{ "g.19.22", ["n0"] },
{ "g.19.23", ["n0","n13","n14"] },
{ "g.19.24", ["n0"] },
{ "g.19.25", ["n9"] },
{ "g.19.26", ["n9"] },
{ "g.19.27", ["n16"] },
{ "g.19.28", ["n15"] },
{ "g.19.29", ["n6","n15"] },
{ "g.19.31", ["n0"] },
{ "g.19.32", ["n16"] },
{ "g.19.34", ["n0"] },
{ "g.19.35", ["n1","n11","n14"] },
{ "g.19.36", ["n0","n17"] },
{ "g.19.38", ["n0","n4","n5","n6","n7","n8","n9","n10","n11","n15","n17","n18"] },
{ "g.19.39", ["n0"] },
{ "g.19.4", ["n8","n16"] },
{ "g.19.40", ["n0"] },
{ "g.19.41", ["n12"] },
{ "g.19.43", ["n0","n3","n5","n15","n17","n18"] },
{ "g.19.44", ["n0"] },
{ "g.19.45", ["n0","n8","n16","n17","n18"] },
{ "g.19.6", ["n0","n3"] },
{ "g.19.7", ["n4","n8"] },
{ "g.19.8", ["n7","n12","n17"] },
{ "g.19.9", ["n16"] },
{ "g.20.1", ["n0","n13","n16"] },
{ "g.20.13", ["n0"] },
{ "g.20.14", ["n0","n9"] },
{ "g.20.2", ["n0"] },
{ "g.20.22", ["n0"] },
{ "g.20.26", ["n0"] },
{ "g.20.28", ["n0"] },
{ "g.20.29", ["n1"] },
{ "g.20.3", ["n2","n6","n12","n16"] },
{ "g.20.30", ["n1"] },
{ "g.20.31", ["n0"] },
{ "g.20.34", ["n0"] },
{ "g.20.36", ["n4"] },
{ "g.20.37", ["n0"] },
{ "g.20.39", ["n1"] },
{ "g.20.4", ["n7","n17"] },
{ "g.20.40", ["n1"] },
{ "g.20.41", ["n1"] },
{ "g.20.42", ["n2"] },
{ "g.20.43", ["n0","n9","n13","n18"] },
{ "g.20.45", ["n0"] },
{ "g.20.46", ["n0"] },
{ "g.20.47", ["n0","n14","n19"] },
{ "g.20.48", ["n0","n9","n14","n19"] },
{ "g.20.49", ["n0"] },
{ "g.20.5", ["n8","n13","n15"] },
{ "g.20.50", ["n0","n10","n16"] },
{ "g.20.51", ["n19"] },
{ "g.20.52", ["n0","n1"] },
{ "g.20.53", ["n0","n2","n18","n19"] },
{ "g.20.6", ["n10","n18"] },
{ "g.20.7", ["n0","n16"] },
{ "g.20.8", ["n0","n7","n10","n11"] },
{ "g.20.9", ["n1"] },
{ "g.21.0", ["n0","n2","n3","n4","n5","n6","n7","n8","n9","n10","n11","n13","n14","n15","n16","n17","n18","n19","n20"] },
{ "g.21.1", ["n0"] },
{ "g.21.10", ["n0","n14","n15","n16"] },
{ "g.21.11", ["n0","n3"] },
{ "g.21.12", ["n0"] },
{ "g.21.13", ["n0","n18"] },
{ "g.21.14", ["n0"] },
{ "g.21.15", ["n0"] },
{ "g.21.17", ["n0","n3","n5","n7","n9","n11","n13"] },
{ "g.21.2", ["n4","n7","n12","n20"] },
{ "g.21.22", ["n11"] },
{ "g.21.23", ["n14"] },
{ "g.21.24", ["n5"] },
{ "g.21.27", ["n5"] },
{ "g.21.29", ["n0","n5","n9","n12","n16"] },
{ "g.21.3", ["n11","n13","n16"] },
{ "g.21.30", ["n0","n3","n5","n7","n11","n14","n17","n19","n20"] },
{ "g.21.31", ["n20"] },
{ "g.21.33", ["n0","n4","n5","n7","n9"] },
{ "g.21.34", ["n0","n3","n4","n7","n10","n11","n14","n17","n20"] },
{ "g.21.35", ["n1"] },
{ "g.21.37", ["n1"] },
{ "g.21.4", ["n9","n13","n17"] },
{ "g.21.40", ["n0"] },
{ "g.21.41", ["n0","n19","n20"] },
{ "g.21.45", ["n0","n2","n19","n20"] },
{ "g.21.5", ["n0"] },
{ "g.21.6", ["n0"] },
{ "g.21.8", ["n0"] },
{ "g.21.9", ["n0"] },
{ "g.22.0", ["n2","n18","n20"] },
{ "g.22.12", ["n0","n4","n7","n8","n12","n16","n20"] },
{ "g.22.14", ["n9","n19"] },
{ "g.22.15", ["n5","n13","n15","n17","n20"] },
{ "g.22.17", ["n6","n10","n18"] },
{ "g.22.18", ["n5"] },
{ "g.22.19", ["n7","n11","n14","n18"] },
{ "g.22.20", ["n12","n20"] },
{ "g.22.21", ["n8","n13","n16","n18"] },
{ "g.22.23", ["n0"] },
{ "g.22.24", ["n0","n5"] },
{ "g.22.25", ["n0"] },
{ "g.22.26", ["n0"] },
{ "g.22.27", ["n0","n5","n6","n7","n12","n13","n14","n15","n20"] },
{ "g.22.30", ["n2","n3","n4","n11","n12","n13","n14","n19"] },
{ "g.22.31", ["n0","n2"] },
{ "g.22.35", ["n0"] },
{ "g.22.36", ["n0","n16"] },
{ "g.22.37", ["n0","n12","n14","n15"] },
{ "g.22.39", ["n0","n3","n4","n8","n9","n10","n11","n15","n20"] },
{ "g.22.4", ["n0","n9","n15","n19","n20","n21"] },
{ "g.22.41", ["n19"] },
{ "g.22.42", ["n0"] },
{ "g.22.44", ["n0"] },
{ "g.22.49", ["n0"] },
{ "g.22.50", ["n0"] },
{ "g.22.51", ["n0"] },
{ "g.22.52", ["n0","n17"] },
{ "g.22.54", ["n5"] },
{ "g.22.60", ["n0","n4","n9","n14","n17","n20"] },
{ "g.22.62", ["n0"] },
{ "g.22.66", ["n0","n6","n10","n15","n17","n20"] },
{ "g.22.67", ["n0","n6","n10","n15","n17","n20"] },
{ "g.22.68", ["n0","n4","n7","n16","n18","n20"] },
{ "g.22.69", ["n0","n11","n14","n15","n17"] },
{ "g.22.7", ["n0"] },
{ "g.22.70", ["n19","n20","n21"] },
{ "g.22.76", ["n16","n17","n18"] },
{ "g.22.8", ["n0"] },
{ "g.22.9", ["n0"] },
{ "g.23.1", ["n2","n19","n20"] },
{ "g.23.100", ["n0"] },
{ "g.23.102", ["n0"] },
{ "g.23.103", ["n0"] },
{ "g.23.106", ["n0"] },
{ "g.23.107", ["n0"] },
{ "g.23.108", ["n0"] },
{ "g.23.11", ["n6","n11"] },
{ "g.23.112", ["n0","n9","n11","n14","n17","n19","n22"] },
{ "g.23.115", ["n0","n19"] },
{ "g.23.117", ["n0"] },
{ "g.23.118", ["n5"] },
{ "g.23.119", ["n0"] },
{ "g.23.12", ["n5","n19"] },
{ "g.23.121", ["n0"] },
{ "g.23.13", ["n4","n9"] },
{ "g.23.14", ["n10","n13","n16"] },
{ "g.23.15", ["n12","n15","n18","n21"] },
{ "g.23.16", ["n10"] },
{ "g.23.17", ["n11","n17","n19","n21"] },
{ "g.23.18", ["n0"] },
{ "g.23.19", ["n7","n9","n12","n17","n21"] },
{ "g.23.2", ["n0"] },
{ "g.23.20", ["n11","n15","n22"] },
{ "g.23.21", ["n7"] },
{ "g.23.22", ["n17","n21"] },
{ "g.23.37", ["n17","n21"] },
{ "g.23.39", ["n17","n21"] },
{ "g.23.4", ["n17","n18","n19"] },
{ "g.23.40", ["n8","n12","n14","n18","n19","n20","n22"] },
{ "g.23.41", ["n0"] },
{ "g.23.43", ["n0"] },
{ "g.23.44", ["n0"] },
{ "g.23.45", ["n0","n1"] },
{ "g.23.48", ["n0","n7"] },
{ "g.23.5", ["n20","n21","n22"] },
{ "g.23.67", ["n0"] },
{ "g.23.69", ["n0"] },
{ "g.23.7", ["n13","n19"] },
{ "g.23.71", ["n4"] },
{ "g.23.72", ["n0","n17"] },
{ "g.23.74", ["n0"] },
{ "g.23.75", ["n0","n1","n13","n15","n16"] },
{ "g.23.76", ["n0","n3"] },
{ "g.23.77", ["n2","n3"] },
{ "g.23.8", ["n11","n14","n17"] },
{ "g.23.81", ["n19"] },
{ "g.23.83", ["n19"] },
{ "g.23.85", ["n5"] },
{ "g.23.86", ["n5"] },
{ "g.23.88", ["n0"] },
{ "g.23.9", ["n12","n20"] },
{ "g.23.90", ["n5"] },
{ "g.23.92", ["n5"] },
{ "g.23.96", ["n5"] },
{ "g.23.98", ["n0","n3","n5","n6","n7","n9","n10","n11","n12","n19"] },
{ "g.23.99", ["n0","n8","n9","n10","n12","n13","n18","n21","n22"] },
{ "g.24.0", ["n2","n19","n20"] },
{ "g.24.1", ["n16","n19","n23"] },
{ "g.24.15", ["n3","n5","n7","n10","n13","n16","n19","n21"] },
{ "g.24.16", ["n16"] },
{ "g.24.17", ["n11","n13"] },
{ "g.24.18", ["n6","n9"] },
{ "g.24.19", ["n5","n13","n21"] },
{ "g.24.2", ["n4","n8","n12"] },
{ "g.24.26", ["n22"] },
{ "g.24.29", ["n22"] },
{ "g.24.3", ["n10","n15","n17"] },
{ "g.24.30", ["n22"] },
{ "g.24.33", ["n0"] },
{ "g.24.34", ["n2","n5","n6","n16","n23"] },
{ "g.24.35", ["n2","n10","n18","n23"] },
{ "g.24.36", ["n0"] },
{ "g.24.37", ["n0"] },
{ "g.24.38", ["n0","n3"] },
{ "g.24.39", ["n6"] },
{ "g.24.41", ["n14"] },
{ "g.24.43", ["n6"] },
{ "g.24.44", ["n12","n23"] },
{ "g.24.45", ["n12","n23"] },
{ "g.24.46", ["n0","n16","n18","n23"] },
{ "g.24.47", ["n14","n16","n18","n23"] },
{ "g.24.48", ["n20","n21","n22","n23"] },
{ "g.24.6", ["n8","n12"] },
{ "g.24.7", ["n3","n9","n20"] },
{ "g.24.8", ["n5","n11"] },
{ "g.24.9", ["n15"] },
{ "g.25.0", ["n2","n19","n20"] },
{ "g.25.1", ["n0"] },
{ "g.25.12", ["n19","n21"] },
{ "g.25.13", ["n9","n15","n17","n20","n22"] },
{ "g.25.16", ["n22","n23"] },
{ "g.25.18", ["n8","n12","n14","n16"] },
{ "g.25.21", ["n0","n2","n24"] },
{ "g.25.22", ["n0","n3","n24"] },
{ "g.25.26", ["n0","n5","n8","n11","n14","n18"] },
{ "g.25.28", ["n19","n20","n21","n22","n23","n24"] },
{ "g.25.32", ["n0","n12","n15","n16","n17","n18"] },
{ "g.25.33", ["n19","n20","n21","n22","n23","n24"] },
{ "g.25.34", ["n0","n10","n13","n23","n24"] },
{ "g.25.36", ["n0"] },
{ "g.25.37", ["n0"] },
{ "g.25.38", ["n0"] },
{ "g.25.4", ["n12","n15","n18"] },
{ "g.25.40", ["n0","n2","n6","n9","n12","n17"] },
{ "g.25.43", ["n0","n5","n6","n7","n8","n17"] },
{ "g.25.45", ["n0","n5","n6","n7","n8","n10"] },
{ "g.25.46", ["n0","n4","n10","n12"] },
{ "g.25.47", ["n0","n4","n5","n7","n9","n11","n13","n16","n18","n21","n23","n24"] },
{ "g.25.48", ["n0","n3","n5","n7","n9","n11","n13","n16","n18","n21","n23","n24"] },
{ "g.25.49", ["n0","n4","n5","n7","n9","n12","n15","n17","n19","n22","n23"] },
{ "g.25.5", ["n10","n14"] },
{ "g.25.51", ["n0","n4","n6","n8","n10","n13","n16","n18","n21","n23","n24"] },
{ "g.25.52", ["n0","n3","n12","n13","n17","n18"] },
{ "g.25.53", ["n0","n4","n8"] },
{ "g.25.55", ["n0"] },
{ "g.25.6", ["n13","n21"] },
{ "g.25.7", ["n13","n17"] },
{ "g.25.8", ["n20","n23"] },
{ "g.25.9", ["n12","n17","n19","n23"] },
{ "g.26.0", ["n2","n18","n19"] },
{ "g.26.1", ["n2","n19","n20"] },
{ "g.26.10", ["n0"] },
{ "g.26.11", ["n18","n19","n24"] },
{ "g.26.12", ["n0"] },
{ "g.26.13", ["n0","n5","n6","n7","n8","n17"] },
{ "g.26.17", ["n0","n4","n5","n7","n11","n15","n17","n20","n21"] },
{ "g.26.19", ["n0","n2","n3","n5","n9","n11","n16","n19","n21","n22"] },
{ "g.26.2", ["n14","n17","n21"] },
{ "g.26.20", ["n0","n2","n3","n5","n9","n11","n16","n19","n21","n22"] },
{ "g.26.22", ["n0"] },
{ "g.26.23", ["n1","n3","n5","n7"] },
{ "g.26.24", ["n0","n4","n8","n22"] },
{ "g.26.26", ["n0","n2","n4","n6"] },
{ "g.26.27", ["n0","n4","n8","n22"] },
{ "g.26.29", ["n0","n7","n8","n9","n11","n12","n13","n14","n15","n18","n19","n20","n21","n22","n23","n24","n25"] },
{ "g.26.3", ["n6","n14","n20","n22"] },
{ "g.26.30", ["n0","n10","n17","n18","n20","n21","n25"] },
{ "g.26.31", ["n25"] },
{ "g.26.32", ["n0","n2","n19","n20"] },
{ "g.26.4", ["n6","n8","n11","n14","n17","n19","n21"] },
{ "g.26.5", ["n11","n15"] },
{ "g.26.6", ["n22"] },
{ "g.26.8", ["n13","n20"] },
{ "g.26.9", ["n15","n19","n21","n23"] },
{ "g.27.0", ["n8","n10","n12","n21"] },
{ "g.27.1", ["n9","n14","n19","n24"] },
{ "g.27.11", ["n0"] },
{ "g.27.12", ["n0"] },
{ "g.27.14", ["n0"] },
{ "g.27.15", ["n0","n2","n4","n6"] },
{ "g.27.16", ["n0"] },
{ "g.27.3", ["n17","n21"] },
{ "g.27.4", ["n0"] },
{ "g.27.7", ["n0","n5"] },
{ "g.27.9", ["n0","n2","n7","n9","n13","n18"] },
{ "g.28.1", ["n2","n22","n23"] },
{ "g.28.12", ["n0"] },
{ "g.28.14", ["n0"] },
{ "g.28.16", ["n0"] },
{ "g.28.2", ["n20","n24"] },
{ "g.28.20", ["n0"] },
{ "g.28.21", ["n0"] },
{ "g.28.22", ["n0","n1"] },
{ "g.28.23", ["n0","n8","n10","n21","n24","n26"] },
{ "g.28.24", ["n0","n3","n8","n10","n12","n14","n16","n19","n21","n24","n26","n27"] },
{ "g.28.25", ["n26","n27"] },
{ "g.28.26", ["n0","n1","n19"] },
{ "g.28.29", ["n0","n1","n17","n23","n27"] },
{ "g.28.3", ["n3","n24"] },
{ "g.28.30", ["n0"] },
{ "g.28.4", ["n7","n11","n14","n19","n24"] },
{ "g.28.5", ["n5","n8","n11","n14","n17","n20","n23"] },
{ "g.28.6", ["n12","n15","n23"] },
{ "g.28.7", ["n0","n13","n15","n17","n19","n22"] },
{ "g.28.8", ["n0","n8","n10","n13","n17","n20","n23","n24","n25","n26","n27"] },
{ "g.28.9", ["n0","n5"] },
{ "g.29.1", ["n2","n21","n22"] },
{ "g.29.12", ["n0","n5","n6","n8","n10","n12","n14","n17","n18","n20","n23","n24","n26","n27","n28"] },
{ "g.29.13", ["n0","n7","n8","n9","n10","n11","n12"] },
{ "g.29.14", ["n0","n27"] },
{ "g.29.15", ["n0"] },
{ "g.29.16", ["n1","n3"] },
{ "g.29.2", ["n5","n8","n15","n20"] },
{ "g.29.3", ["n3"] },
{ "g.29.4", ["n9","n12","n16","n22"] },
{ "g.29.5", ["n13","n24"] },
{ "g.29.6", ["n19","n27"] },
{ "g.29.7", ["n0","n1","n8","n24","n25","n28"] },
{ "g.29.9", ["n0","n13","n24","n26","n27"] },
{ "g.30.0", ["n2","n19","n20"] },
{ "g.30.1", ["n2","n21","n22"] },
{ "g.30.11", ["n1","n8","n17"] },
{ "g.30.12", ["n0","n21","n26","n28"] },
{ "g.30.15", ["n23"] },
{ "g.30.17", ["n0"] },
{ "g.30.19", ["n3","n4","n5","n8","n9","n10","n11","n15","n16","n17","n18","n20","n21","n23","n24","n27","n28","n29"] },
{ "g.30.2", ["n11","n13","n29"] },
{ "g.30.20", ["n28"] },
{ "g.30.23", ["n28"] },
{ "g.30.24", ["n0"] },
{ "g.30.25", ["n0"] },
{ "g.30.26", ["n0","n2","n4","n6","n8","n13"] },
{ "g.30.28", ["n0","n12","n14","n16","n18","n20","n22"] },
{ "g.30.3", ["n11","n13","n29"] },
{ "g.30.31", ["n0"] },
{ "g.30.4", ["n3","n5","n8","n16","n21"] },
{ "g.30.5", ["n28","n29"] },
{ "g.30.6", ["n6","n10","n17","n21","n28"] },
{ "g.30.8", ["n6","n7","n8","n12","n14","n18","n19","n23","n26"] },
{ "g.30.9", ["n1","n3","n19","n24","n26"] },
{ "g.31.0", ["n6","n7","n8","n12","n14","n18","n19","n23","n26"] },
{ "g.31.1", ["n0","n22","n23","n28"] },
{ "g.31.2", ["n0","n7","n23","n28"] },
{ "g.31.28", ["n23","n29"] },
{ "g.31.29", ["n0"] },
{ "g.31.3", ["n21","n23","n25","n27","n29"] },
{ "g.31.31", ["n12","n25"] },
{ "g.31.32", ["n0","n8","n12","n15","n18","n20"] },
{ "g.31.34", ["n29"] },
{ "g.31.36", ["n0","n2","n4","n6","n8","n13"] },
{ "g.31.37", ["n0"] },
{ "g.31.4", ["n7","n11","n26"] },
{ "g.31.6", ["n26"] },
{ "g.31.8", ["n0"] },
{ "g.31.9", ["n0"] },
{ "g.32.1", ["n5","n8","n16","n21"] },
{ "g.32.10", ["n4","n9","n14","n19"] },
{ "g.32.11", ["n13"] },
{ "g.32.12", ["n20","n22","n24","n26","n28","n30"] },
{ "g.32.13", ["n10","n13","n16","n19","n23","n26","n29"] },
{ "g.32.14", ["n13","n24"] },
{ "g.32.15", ["n10","n16","n19"] },
{ "g.32.17", ["n0"] },
{ "g.32.18", ["n0"] },
{ "g.32.19", ["n14"] },
{ "g.32.2", ["n3","n8","n10","n17","n30"] },
{ "g.32.20", ["n0","n1"] },
{ "g.32.21", ["n0","n1"] },
{ "g.32.22", ["n0","n7"] },
{ "g.32.23", ["n23","n24","n30"] },
{ "g.32.25", ["n0","n17","n19","n20","n21","n22","n23","n26","n27","n28","n29","n30","n31"] },
{ "g.32.27", ["n0"] },
{ "g.32.28", ["n0","n18","n20","n23","n26","n28","n30"] },
{ "g.32.5", ["n0"] },
{ "g.32.6", ["n0"] },
{ "g.32.8", ["n0"] },
{ "g.32.9", ["n27","n28"] },
{ "g.33.0", ["n11","n13","n15","n32"] },
{ "g.33.1", ["n0","n7","n22","n24","n32"] },
{ "g.33.10", ["n1","n32"] },
{ "g.33.12", ["n0","n2","n13","n14","n21","n32"] },
{ "g.33.16", ["n32"] },
{ "g.33.2", ["n18","n29"] },
{ "g.33.3", ["n10","n13","n15","n18","n21","n24","n27","n30"] },
{ "g.33.4", ["n0","n4","n8","n10","n14","n16","n18","n19","n21","n23","n26","n27","n28","n29","n31","n32"] },
{ "g.33.5", ["n24","n25","n26","n27","n28","n29","n30","n31","n32"] },
{ "g.33.6", ["n0","n7","n10","n16","n21","n24"] },
{ "g.33.7", ["n0","n2","n6","n9","n13","n18","n19","n29"] },
{ "g.33.8", ["n0","n18","n20","n21","n22","n23","n24","n27","n28","n29","n30","n31","n32"] },
{ "g.34.1", ["n6","n9","n17","n23","n31"] },
{ "g.34.12", ["n0","n2","n8","n15","n20","n21","n25","n26","n27","n30","n31","n33"] },
{ "g.34.13", ["n0","n4","n13","n16","n17","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32"] },
{ "g.34.17", ["n10"] },
{ "g.34.18", ["n0"] },
{ "g.34.2", ["n6","n9","n17","n23","n31"] },
{ "g.34.3", ["n6","n13","n20","n25","n31"] },
{ "g.34.4", ["n16","n20"] },
{ "g.34.7", ["n0"] },
{ "g.34.8", ["n0"] },
{ "g.34.9", ["n0","n8","n11","n13","n18","n25","n30","n31","n32","n33"] },
{ "g.35.0", ["n5","n10","n14","n20","n27","n33"] },
{ "g.35.1", ["n0","n5","n10","n21","n28","n34"] },
{ "g.35.10", ["n0"] },
{ "g.35.12", ["n0"] },
{ "g.35.15", ["n3","n13","n14","n29","n34"] },
{ "g.35.2", ["n1","n15","n22","n32"] },
{ "g.35.20", ["n0","n10","n15","n17","n30","n34"] },
{ "g.35.23", ["n32"] },
{ "g.35.24", ["n0","n3","n14","n17","n18","n23"] },
{ "g.35.25", ["n0","n3","n14","n17","n18","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33"] },
{ "g.35.26", ["n32","n33","n34"] },
{ "g.35.27", ["n0"] },
{ "g.35.3", ["n3","n9","n17","n23"] },
{ "g.35.32", ["n1"] },
{ "g.35.4", ["n7","n10","n13","n16","n19","n23","n26","n29"] },
{ "g.35.7", ["n0","n3","n5","n7","n10","n13","n16","n18","n26","n30"] },
{ "g.35.9", ["n0","n26","n30","n32"] },
{ "g.36.0", ["n0","n5","n11","n22","n29","n32","n35"] },
{ "g.36.1", ["n16","n26"] },
{ "g.36.15", ["n0","n2"] },
{ "g.36.16", ["n10","n27"] },
{ "g.36.17", ["n0","n2","n4","n5","n7","n8","n9","n10","n12","n13","n14","n15","n16","n17","n18","n19","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35"] },
{ "g.36.18", ["n0","n2","n3","n4","n5","n9"] },
{ "g.36.19", ["n0","n13","n17","n20","n23","n28"] },
{ "g.36.22", ["n0"] },
{ "g.36.23", ["n4","n9","n16","n24","n30","n35"] },
{ "g.36.29", ["n4","n9","n16","n24","n30","n35"] },
{ "g.36.3", ["n9","n12","n15","n18","n21","n25","n28","n31"] },
{ "g.36.4", ["n18","n29"] },
{ "g.36.5", ["n30","n33"] },
{ "g.36.7", ["n0","n3","n5","n7","n10","n13","n16","n18","n26","n30"] },
{ "g.36.8", ["n14"] },
{ "g.36.9", ["n1","n14","n26"] },
{ "g.37.1", ["n10","n17","n20","n26"] },
{ "g.37.12", ["n0","n4"] },
{ "g.37.14", ["n33"] },
{ "g.37.15", ["n0","n15","n18","n36"] },
{ "g.37.17", ["n0"] },
{ "g.37.18", ["n0"] },
{ "g.37.19", ["n0","n4","n9","n10","n19","n35","n36"] },
{ "g.37.2", ["n11","n16","n26","n30","n33"] },
{ "g.37.20", ["n0","n8","n10","n11","n27","n35","n36"] },
{ "g.37.3", ["n16"] },
{ "g.37.4", ["n28","n30","n32"] },
{ "g.37.5", ["n0"] },
{ "g.37.6", ["n0"] },
{ "g.37.8", ["n0"] },
{ "g.37.9", ["n0","n26"] },
{ "g.38.12", ["n0"] },
{ "g.38.15", ["n0","n24","n25","n31"] },
{ "g.38.16", ["n33","n34","n35","n36","n37"] },
{ "g.38.17", ["n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37"] },
{ "g.38.19", ["n1","n34"] },
{ "g.38.2", ["n36","n37"] },
{ "g.38.20", ["n0","n19"] },
{ "g.38.21", ["n0","n19"] },
{ "g.38.3", ["n21","n28"] },
{ "g.38.4", ["n15","n18","n20","n22","n26"] },
{ "g.38.5", ["n26"] },
{ "g.38.6", ["n0"] },
{ "g.38.7", ["n0"] },
{ "g.38.8", ["n0","n11","n23","n25","n28","n32","n35"] },
{ "g.38.9", ["n0"] },
{ "g.39.0", ["n37","n38"] },
{ "g.39.1", ["n29"] },
{ "g.39.23", ["n0"] },
{ "g.39.26", ["n0"] },
{ "g.39.28", ["n0"] },
{ "g.39.29", ["n0"] },
{ "g.39.30", ["n0"] },
{ "g.39.4", ["n16","n23"] },
{ "g.39.44", ["n0"] },
{ "g.39.45", ["n21","n29"] },
{ "g.39.46", ["n22","n30"] },
{ "g.39.47", ["n22","n30"] },
{ "g.39.53", ["n0"] },
{ "g.39.56", ["n0"] },
{ "g.39.57", ["n35"] },
{ "g.39.7", ["n18","n21","n23","n24","n32","n33","n34","n35"] },
{ "g.39.8", ["n14","n18","n22","n23","n28","n29","n34","n35"] },
{ "g.39.9", ["n0","n8","n9","n19","n26"] },
{ "g.40.0", ["n0","n19"] },
{ "g.40.12", ["n0"] },
{ "g.40.13", ["n0"] },
{ "g.40.16", ["n0"] },
{ "g.40.17", ["n0"] },
{ "g.40.18", ["n0"] },
{ "g.40.19", ["n0","n1"] },
{ "g.40.20", ["n0"] },
{ "g.40.21", ["n0"] },
{ "g.40.22", ["n35","n36"] },
{ "g.40.27", ["n34"] },
{ "g.40.3", ["n33","n38","n39"] },
{ "g.40.5", ["n10","n28"] },
{ "g.40.7", ["n18","n28"] },
{ "g.40.8", ["n29","n36","n37","n38","n39"] },
{ "g.40.9", ["n31"] },
{ "g.41.0", ["n37","n38","n39","n40"] },
{ "g.41.1", ["n27","n34"] },
{ "g.41.12", ["n0"] },
{ "g.41.13", ["n29"] },
{ "g.41.2", ["n5","n9","n19"] },
{ "g.41.25", ["n37","n38","n40"] },
{ "g.41.26", ["n0","n40"] },
{ "g.41.3", ["n9","n13","n18","n20","n29","n31","n33"] },
{ "g.41.36", ["n1","n2","n8","n33"] },
{ "g.41.4", ["n0"] },
{ "g.41.7", ["n0","n37"] },
{ "g.41.9", ["n13","n28","n29"] },
{ "g.42.0", ["n0","n3","n15","n28"] },
{ "g.42.1", ["n0","n3","n15","n28"] },
{ "g.42.15", ["n0","n35","n36","n37","n40","n41"] },
{ "g.42.2", ["n0","n3","n9","n12","n15","n28"] },
{ "g.42.20", ["n0","n32","n39","n40"] },
{ "g.42.21", ["n0"] },
{ "g.42.23", ["n0","n14","n29","n30"] },
{ "g.42.25", ["n0"] },
{ "g.42.26", ["n0"] },
{ "g.42.27", ["n0"] },
{ "g.42.3", ["n0","n3","n9","n15","n28","n41"] },
{ "g.42.32", ["n0","n5","n16","n23"] },
{ "g.42.34", ["n15","n26","n37","n38"] },
{ "g.42.4", ["n25","n28"] },
{ "g.42.5", ["n0","n23","n27","n28","n29","n30","n31","n32","n34","n35","n36","n37","n38","n39","n40","n41"] },
{ "g.42.7", ["n0"] },
{ "g.42.8", ["n0","n28","n41"] },
{ "g.42.9", ["n0","n13","n23","n39","n41"] },
{ "g.43.0", ["n0"] },
{ "g.43.1", ["n0"] },
{ "g.43.12", ["n0","n5","n33","n35"] },
{ "g.43.14", ["n0"] },
{ "g.43.15", ["n0"] },
{ "g.43.17", ["n0"] },
{ "g.43.18", ["n0"] },
{ "g.43.19", ["n2","n8","n12","n19","n28","n34","n38","n40","n41","n42"] },
{ "g.43.20", ["n28","n42"] },
{ "g.43.23", ["n42"] },
{ "g.43.24", ["n0","n10","n18","n19","n20","n25","n28","n31","n33","n35","n37","n38","n39","n40","n41"] },
{ "g.43.25", ["n0","n10","n18","n19","n20","n26","n27","n28","n31","n33","n35","n37","n38","n39","n40","n41"] },
{ "g.43.26", ["n0","n11","n19","n20","n21","n27","n28","n29","n32","n34","n36","n37","n38","n39","n40","n41"] },
{ "g.43.3", ["n17"] },
{ "g.43.5", ["n0"] },
{ "g.43.6", ["n0","n41"] },
{ "g.43.7", ["n0","n1"] },
{ "g.43.8", ["n0"] },
{ "g.43.9", ["n0"] },
{ "g.44.0", ["n30"] },
{ "g.44.1", ["n29","n34"] },
{ "g.44.10", ["n0","n40"] },
{ "g.44.11", ["n0","n1"] },
{ "g.44.14", ["n0"] },
{ "g.44.15", ["n0"] },
{ "g.44.16", ["n0"] },
{ "g.44.19", ["n10","n17","n21","n31","n32","n34","n38","n43"] },
{ "g.44.20", ["n1","n16","n18","n21","n25","n37","n39"] },
{ "g.44.21", ["n0","n3","n13","n20","n23","n24","n28","n29","n33","n37","n40"] },
{ "g.44.22", ["n0"] },
{ "g.44.3", ["n23","n41"] },
{ "g.44.4", ["n0"] },
{ "g.44.5", ["n0","n8","n10","n36"] },
{ "g.44.7", ["n0"] },
{ "g.44.9", ["n0"] },
{ "g.45.0", ["n15","n21","n30","n34","n37","n40"] },
{ "g.45.1", ["n0"] },
{ "g.45.4", ["n0"] },
{ "g.45.5", ["n0","n37","n44"] },
{ "g.45.7", ["n0"] },
{ "g.45.8", ["n0","n4","n18","n40","n44"] },
{ "g.45.9", ["n0","n6","n11","n17","n25","n33","n43"] },
{ "g.46.11", ["n0"] },
{ "g.46.12", ["n0"] },
{ "g.46.16", ["n0"] },
{ "g.46.17", ["n0"] },
{ "g.46.19", ["n0"] },
{ "g.46.3", ["n0"] },
{ "g.46.4", ["n16"] },
{ "g.46.5", ["n21","n25","n31","n36","n42"] },
{ "g.46.6", ["n0","n12"] },
{ "g.46.9", ["n0"] },
{ "g.47.1", ["n18","n38"] },
{ "g.47.15", ["n0"] },
{ "g.47.16", ["n0","n25","n35"] },
{ "g.47.17", ["n0","n3","n9","n17","n33","n46"] },
{ "g.47.2", ["n0"] },
{ "g.47.23", ["n0","n3","n9","n17","n33"] },
{ "g.47.25", ["n0"] },
{ "g.47.26", ["n0"] },
{ "g.47.27", ["n0","n1","n9","n12","n15","n16","n17","n18","n20","n21","n25","n26","n28","n30","n31","n32","n33","n36","n37","n38","n40","n43","n44","n46"] },
{ "g.47.3", ["n0","n1"] },
{ "g.47.30", ["n0"] },
{ "g.47.32", ["n0","n26","n30","n43"] },
{ "g.47.5", ["n0","n1"] },
{ "g.47.6", ["n0"] },
{ "g.47.7", ["n0"] },
{ "g.47.8", ["n0"] },
{ "g.47.9", ["n0"] },
{ "g.48.0", ["n2","n12","n16","n27","n35","n45"] },
{ "g.48.10", ["n0","n2","n5","n8","n11","n22","n23","n24","n26","n27","n28","n31","n33","n34","n37","n39","n40","n41"] },
{ "g.48.16", ["n0","n32","n39","n42","n44","n46"] },
{ "g.48.18", ["n0","n3","n8","n14","n22","n35"] },
{ "g.48.19", ["n0","n46"] },
{ "g.48.23", ["n0","n3","n44"] },
{ "g.48.24", ["n0","n26","n30","n43"] },
{ "g.48.29", ["n0","n26","n30","n43"] },
{ "g.48.3", ["n23","n28","n32","n45"] },
{ "g.48.31", ["n0","n25","n28","n43"] },
{ "g.48.35", ["n0"] },
{ "g.48.4", ["n0"] },
{ "g.48.6", ["n0","n44"] },
{ "g.48.8", ["n0"] },
{ "g.48.9", ["n21","n39","n45"] },
{ "g.49.11", ["n0","n48"] },
{ "g.49.12", ["n0","n48"] },
{ "g.49.16", ["n0"] },
{ "g.49.17", ["n0","n5","n6","n11","n13","n16","n20","n21","n24","n25","n26","n27","n30","n31","n33","n35","n36","n40","n41","n43","n47"] },
{ "g.49.19", ["n0"] },
{ "g.49.5", ["n16","n28","n37"] },
{ "g.49.6", ["n25","n44","n47"] },
{ "g.49.8", ["n0"] },
{ "g.49.9", ["n0"] },
{ "g.50.7", ["n3","n4","n10","n11","n15","n22","n23","n24","n25","n28","n31","n32","n35","n39","n40","n43"] },
{ "g.50.8", ["n0"] },
{ "g.50.9", ["n0"] },
{ "g.51.0", ["n0","n11","n18","n20","n24","n27","n36","n39","n42","n45","n47","n49"] },
{ "g.51.2", ["n44"] },
{ "g.51.4", ["n0","n1"] },
{ "g.51.5", ["n1","n29"] },
{ "g.52.0", ["n0","n13","n19","n25","n48","n50","n51"] },
{ "g.52.1", ["n0"] },
{ "g.52.2", ["n0"] },
{ "g.52.3", ["n0","n5","n8","n11","n12","n13","n14","n15","n16","n17","n18","n19","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n36","n38","n39","n40","n41","n42","n43","n44","n47","n48","n49","n50"] },
{ "g.52.4", ["n0","n3","n31","n34","n35","n40"] },
{ "g.53.13", ["n45","n46","n47","n49","n50","n51","n52"] },
{ "g.53.3", ["n0","n28","n29","n31","n33","n42","n48","n49"] },
{ "g.53.5", ["n19","n44","n46","n47","n52"] },
{ "g.53.6", ["n0"] },
{ "g.53.9", ["n0"] },
{ "g.54.0", ["n3","n7","n9","n11","n13","n18","n27","n36","n49"] },
{ "g.54.1", ["n0","n1"] },
{ "g.54.2", ["n0"] },
{ "g.54.3", ["n0","n7"] },
{ "g.54.6", ["n0","n5","n8","n11","n12","n13","n14","n15","n16","n17","n18","n19","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n36","n38","n39","n40","n41","n42","n43","n44","n47","n48","n49","n50","n51","n52"] },
{ "g.54.7", ["n0","n9","n18","n25","n35","n51","n53"] },
{ "g.54.8", ["n0"] },
{ "g.55.0", ["n15","n21","n23","n26","n29","n32","n34","n36","n39","n42","n46"] },
{ "g.55.13", ["n0"] },
{ "g.55.23", ["n0"] },
{ "g.55.24", ["n0"] },
{ "g.55.26", ["n0","n49","n50","n51","n52","n53","n54"] },
{ "g.55.30", ["n0","n42","n43","n44","n45","n47","n52"] },
{ "g.55.32", ["n0"] },
{ "g.55.33", ["n0","n8"] },
{ "g.55.6", ["n0"] },
{ "g.55.8", ["n0"] },
{ "g.55.9", ["n0"] },
{ "g.56.0", ["n0"] },
{ "g.56.1", ["n0"] },
{ "g.56.10", ["n0","n2","n12","n25","n26","n27","n33","n43","n45"] },
{ "g.56.11", ["n0"] },
{ "g.56.2", ["n0","n1"] },
{ "g.56.3", ["n0","n1"] },
{ "g.56.4", ["n10","n17","n21","n33","n34","n41","n46","n48","n51","n53"] },
{ "g.56.5", ["n0","n14","n49","n50","n51","n52","n53","n55"] },
{ "g.56.7", ["n0","n32"] },
{ "g.56.9", ["n0","n9"] },
{ "g.57.0", ["n44","n47"] },
{ "g.57.14", ["n0"] },
{ "g.57.15", ["n0"] },
{ "g.57.18", ["n56"] },
{ "g.57.19", ["n0","n10","n12","n13","n14","n15","n16","n17","n18","n19","n20","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n33","n35","n36","n37","n38","n39","n40","n41","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n56"] },
{ "g.57.21", ["n0","n10","n12","n13","n14","n15","n16","n17","n19","n20","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n33","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n56"] },
{ "g.57.22", ["n15","n40","n48","n51","n52","n53","n54","n55","n56"] },
{ "g.57.23", ["n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56"] },
{ "g.57.26", ["n22"] },
{ "g.57.27", ["n0"] },
{ "g.57.28", ["n0","n2","n14","n27","n28","n29","n35"] },
{ "g.57.29", ["n0","n2","n14","n28","n29","n30","n36"] },
{ "g.57.5", ["n0","n25","n33","n47"] },
{ "g.57.6", ["n0"] },
{ "g.57.8", ["n0"] },
{ "g.57.9", ["n0"] },
{ "g.58.0", ["n37","n41","n44"] },
{ "g.58.10", ["n0","n2","n14","n28","n29","n30","n36"] },
{ "g.58.3", ["n0","n1"] },
{ "g.58.4", ["n0","n3","n13","n18","n25","n28","n33","n42","n48","n50","n53","n55"] },
{ "g.58.6", ["n0"] },
{ "g.58.7", ["n0","n11","n12","n13","n14","n15","n16","n17","n18","n19","n20","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n35","n37","n38","n39","n41","n42","n43","n46","n47","n48","n49","n50","n51","n53","n54","n55","n56"]},
{ "g.58.9", ["n15","n40","n48","n51","n52","n53","n54","n55","n56","n57"] },
{ "g.59.0", ["n33","n35","n54"] },
{ "g.59.12", ["n0"] },
{ "g.59.14", ["n58"] },
{ "g.59.2", ["n15","n20","n48"] },
{ "g.59.9", ["n0"] },
{ "g.60.0", ["n22","n23","n40","n48","n57"] },
{ "g.60.1", ["n0","n1"] },
{ "g.60.16", ["n0"] },
{ "g.60.9", ["n0"] },
{ "g.61.11", ["n0"] },
{ "g.61.15", ["n0"] },
{ "g.61.2", ["n0","n6","n8","n9","n10","n45","n46","n49","n50","n57","n58","n59","n60"] },
{ "g.61.25", ["n0"] },
{ "g.61.3", ["n0","n32","n39","n40","n42","n58"] },
{ "g.61.31", ["n0"] },
{ "g.61.32", ["n0"] },
{ "g.61.35", ["n0","n10","n12","n13","n14","n15","n16","n17","n19","n20","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n33","n35","n36","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n60"] },
{ "g.61.39", ["n0","n11","n12","n13","n14","n17","n18","n19","n20","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n54","n55","n56","n57","n58","n59","n60"] },
{ "g.61.4", ["n0","n1"] },
{ "g.61.40", ["n0","n11","n12","n13","n14","n15","n18","n19","n20","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n36","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n54","n55","n56","n57","n58","n59","n60"] },
{ "g.61.54", ["n0","n13","n14","n15","n16","n17","n27","n28","n30","n33","n37","n41","n42","n43","n47","n51","n52","n59"] },
{ "g.61.55", ["n0","n13","n14","n15","n16","n17","n27","n28","n30","n33","n37","n41","n42","n43","n47","n51","n52","n59"] },
{ "g.61.7", ["n0"] },
{ "g.61.8", ["n0"] },
{ "g.61.9", ["n0"] },
{ "g.62.0", ["n0","n5","n32","n41","n44","n45","n48","n49","n50","n51","n55","n56","n60","n61"] },
{ "g.62.1", ["n0"] },
{ "g.62.2", ["n0","n1"] },
{ "g.62.3", ["n0","n1"] },
{ "g.62.4", ["n0","n1"] },
{ "g.62.5", ["n0","n3","n7","n17","n18","n28","n32","n35","n38","n39"] },
{ "g.62.6", ["n0","n15","n16","n17","n18","n19","n20","n21","n22","n24","n25","n26","n27","n28","n29","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61"] },
{ "g.62.7", ["n0","n7","n9","n10","n13","n14","n15","n16","n17","n18","n19","n22","n23","n24","n25","n26","n27","n28","n29","n30","n32","n34","n36","n37","n38","n39","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61"] },
{ "g.63.0", ["n0"] },
{ "g.63.1", ["n0"] },
{ "g.63.18", ["n0"] },
{ "g.63.22", ["n0"] },
{ "g.63.8", ["n0"] },
{ "g.63.9", ["n0"] },
{ "g.64.14", ["n0","n12","n28","n33","n40","n42","n44","n46","n49","n51","n53","n61","n63"] },
{ "g.64.16", ["n0","n12","n28","n33","n40","n42","n44","n46","n49","n51","n53","n61","n63"] },
{ "g.64.17", ["n52","n63"] },
{ "g.64.18", ["n0"] },
{ "g.64.7", ["n41"] },
{ "g.64.8", ["n0","n1"] },
{ "g.64.9", ["n0","n6","n23","n30","n36","n39","n41","n43","n45","n49","n52","n61","n63"] },
{ "g.65.0", ["n0","n42"] },
{ "g.65.1", ["n0","n9","n11","n12","n13","n14","n15","n16","n17","n18","n19","n20","n21","n22","n23","n24","n26","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n43","n44","n45","n46","n47","n48"] },
{ "g.66.3", ["n0"] },
{ "g.66.6", ["n51","n55","n60","n61","n62","n63","n64","n65"] },
{ "g.66.7", ["n25","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65"] },
{ "g.66.8", ["n25","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65"] },
{ "g.67.0", ["n0"] },
{ "g.67.1", ["n0"] },
{ "g.68.1", ["n0"] },
{ "g.68.4", ["n0","n10","n11","n12","n13","n14","n17","n18","n19","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n37","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n65","n66","n67"] },
{ "g.69.0", ["n0"] },
{ "g.69.1", ["n0"] },
{ "g.69.2", ["n0","n8","n15","n16","n17","n18","n19","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n34","n35","n36","n37","n38","n39","n40","n41","n42","n51","n55","n56","n57","n58","n59","n60","n62","n63","n64","n65","n66","n67","n68"] },
{ "g.69.3", ["n0","n14","n15","n16","n17","n18","n19","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68"] },
{ "g.69.4", ["n0","n13","n14","n15","n16","n17","n18","n19","n20","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n67"] },
{ "g.69.5", ["n0","n14","n15","n16","n17","n18","n19","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68"] },
{ "g.69.7", ["n0","n43","n63","n64","n65"] },
{ "g.70.1", ["n0"] },
{ "g.71.2", ["n0","n15","n16","n17","n18","n19","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70"] },
{ "g.71.5", ["n1","n34","n61"] },
{ "g.72.0", ["n0"] },
{ "g.73.4", ["n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72"] },
{ "g.73.7", ["n0"] },
{ "g.73.8", ["n0","n53","n62","n65","n66","n67","n68","n69","n70","n72"] },
{ "g.73.9", ["n60"] },
{ "g.74.1", ["n0"] },
{ "g.74.2", ["n3","n8","n11","n13","n18","n29","n34","n47","n49","n57"] },
{ "g.74.4", ["n1"] },
{ "g.74.5", ["n0","n23","n41","n46","n51","n63","n64","n65","n66","n69","n71","n73"] },
{ "g.75.0", ["n0"] },
{ "g.75.1", ["n0"] },
{ "g.75.2", ["n0","n14","n15","n16","n17","n18","n19","n20","n21","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74"] },
{ "g.75.3", ["n0","n24","n42","n47","n52","n64","n65","n66","n67","n70","n72","n74"] },
{ "g.75.4", ["n7","n9","n10","n11","n21","n23","n24","n25","n26","n27","n29","n31","n34","n37","n39","n40","n45","n47","n48","n49","n50","n51","n55","n57","n63","n64","n65","n68","n69","n71","n73","n74"] },
{ "g.75.5", ["n0","n18","n32","n41","n44","n56"] },
{ "g.75.6", ["n0","n10","n15","n19","n34","n43","n46","n58","n74"] },
{ "g.75.7", ["n0","n10","n15","n19","n34","n43","n46","n58","n74"] },
{ "g.76.1", ["n66"] },
{ "g.76.2", ["n60"] },
{ "g.76.4", ["n17","n20","n65"] },
{ "g.77.0", ["n0","n16","n17","n18","n19","n20","n21","n22","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n69","n70","n71","n72","n73","n74","n75","n76"] },
{ "g.77.1", ["n0","n14","n15","n16","n17","n18","n19","n20","n21","n22","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76"] },
{ "g.77.2", ["n0","n14","n15","n16","n17","n18","n19","n20","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76"] },
{ "g.77.5", ["n0","n3","n6","n9","n12","n13","n14","n15","n16","n19","n22","n24","n27","n30","n32","n34","n37","n39","n41"] },
{ "g.77.6", ["n0","n10","n11","n13","n18","n21","n22","n23","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n42","n44","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76"] },
{ "g.78.0", ["n0","n17","n18","n19","n20","n21","n22","n23","n24","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76","n77"] },
{ "g.78.3", ["n1"] },
{ "g.79.0", ["n0"] },
{ "g.79.1", ["n0","n15","n18"] },
{ "g.79.3", ["n0","n4","n15","n18","n29","n36","n41","n44","n47","n66"] },
{ "g.79.5", ["n17","n20","n67"] },
{ "g.79.6", ["n0","n10","n17","n18","n20","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n34","n36","n38","n42","n43","n44","n45","n46","n47","n48","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76","n77","n78"] },
{ "g.79.7", ["n9","n10","n11","n12","n16","n19","n21","n23","n25","n27","n29","n30","n31","n32","n44","n45","n46","n53","n54","n55","n56","n57","n58","n59","n60","n65","n66","n67","n68","n70","n73","n74","n75","n76"] },
{ "g.80.1", ["n0"] },
{ "g.80.4", ["n0"] },
{ "g.80.5", ["n0"] },
{ "g.80.6", ["n16","n21","n25","n69","n74","n76","n77"] },
{ "g.81.0", ["n0"] },
{ "g.81.15", ["n0","n19","n30","n33","n34","n39","n40","n47","n55","n56","n60","n68","n70","n72","n77","n80"] },
{ "g.81.16", ["n0","n15","n16","n17","n18","n19","n20","n21","n22","n23","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76","n77","n78"] },
{ "g.81.5", ["n0","n73"] },
{ "g.81.9", ["n19","n38","n39","n46","n51","n67","n68","n69","n70","n71","n72","n77","n80"] },
{ "g.82.1", ["n1"] },
{ "g.82.4", ["n0","n20","n39","n40","n47","n52","n68","n69","n70","n71","n72","n73","n78","n81"] },
{ "g.82.5", ["n20","n39","n40","n47","n52","n68","n69","n70","n71","n72","n73","n78","n81"] },
{ "g.83.3", ["n1"] },
{ "g.84.0", ["n0"] },
{ "g.84.1", ["n0"] },
{ "g.84.2", ["n0","n10","n11","n12","n14","n19","n22","n23","n24","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n43","n45","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n66","n68","n69","n70","n71","n72","n73","n74","n75","n76","n77","n78","n79","n81","n82","n83"] },
{ "g.85.1", ["n0","n68"] },
{ "g.85.2", ["n0"] },
{ "g.85.3", ["n0","n11","n17","n28","n37","n47","n49","n50","n52","n53","n55","n56","n57","n73","n77","n81"] },
{ "g.85.8", ["n0","n8","n18","n24","n37","n47","n49","n50","n52","n53","n55","n56","n57","n73","n77","n81"] },
{ "g.86.0", ["n0"] },
{ "g.86.1", ["n0","n12","n14","n15","n16","n17","n18","n20","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n36","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76","n77","n78","n79","n80","n81","n82","n83","n84","n85"] },
{ "g.86.2", ["n0"] },
{ "g.86.3", ["n17","n30","n33","n34","n67","n79","n80","n81","n84"] },
{ "g.86.4", ["n17","n30","n33","n34","n67","n79","n80","n81","n84"] },
{ "g.86.5", ["n0","n46"] },
{ "g.87.0", ["n0","n2"] },
{ "g.87.4", ["n18","n31","n34","n35","n68","n80","n81","n82","n85"] },
{ "g.87.5", ["n18","n31","n34","n35","n68","n80","n81","n82","n85"] },
{ "g.87.6", ["n0","n2","n8","n15","n20","n21","n25","n26","n27","n30","n31","n34","n35","n42","n43","n62","n63","n64","n65","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76","n77","n78","n79","n80","n81","n82","n85","n86"] },
{ "g.88.0", ["n0","n13","n14","n15","n16","n17","n18","n19","n20","n21","n22","n23","n24","n25","n26","n27","n28","n29","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n54","n55","n56","n57","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76","n77","n78","n79","n80","n81","n82","n83","n84","n85","n86","n87"] },
{ "g.88.2", ["n0","n14","n15","n17","n18","n19","n20","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n34","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76","n77","n78","n79","n80","n81","n82","n83","n84","n85","n86","n87"] },
{ "g.89.0", ["n0","n12","n13","n14","n15","n17","n18","n19","n20","n21","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n35","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n58","n59","n60","n61","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76","n77","n78","n79","n80","n81","n82","n83","n84","n85","n86","n87","n88"] },
{ "g.89.1", ["n0","n10","n11","n13","n14","n19","n20","n22","n23","n24","n25","n26","n27","n28","n29","n30","n31","n32","n33","n34","n35","n37","n38","n40","n42","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76","n77","n78","n79","n82","n83","n84","n85","n86","n87","n88"] },
{ "g.90.0", ["n0","n10"] },
{ "g.90.1", ["n0"] },
{ "g.90.2", ["n0"] },
{ "g.90.3", ["n0","n16","n17","n18","n19","n20","n21","n22","n23","n24","n25","n26","n27","n28","n30","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76","n77","n78","n79","n80","n81","n82","n83","n84","n85","n86","n87","n88","n89"] },
{ "g.91.0", ["n0","n17","n18","n19","n20","n21","n22","n23","n24","n25","n27","n29","n30","n31","n33","n36","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n73","n74","n75","n76","n77","n78","n79","n80","n81","n83","n84","n86","n87","n88","n89","n90"] },
{ "g.91.1", ["n3","n4","n5","n8","n9","n10","n11","n15","n16","n17","n18","n20","n21","n23","n24","n27","n28","n29","n33","n34","n35","n36","n38","n40","n41","n42","n44","n46","n47","n48","n51","n52","n53","n54","n57","n58","n59","n60","n64","n65","n66","n67","n71","n72","n73","n74","n76","n78","n79","n81","n82","n83","n86","n87","n88","n89","n90"] },
{ "g.92.0", ["n0"] },
{ "g.92.1", ["n57","n60"] },
{ "g.92.3", ["n0"] },
{ "g.92.4", ["n0","n38"] },
{ "g.93.0", ["n0"] },
{ "g.94.0", ["n0","n19","n20","n21","n22","n23","n24","n25","n26","n28","n30","n31","n32","n34","n37","n38","n39","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n74","n75","n76","n77","n78","n79","n80","n81","n82","n84","n85","n87","n88","n89","n90","n91","n92","n93"] },
{ "g.94.2", ["n0","n10","n20","n22"] },
{ "g.94.3", ["n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76","n77","n78","n79","n80","n81","n82","n83","n84","n85","n86","n87","n88","n89","n90","n91","n92","n93"] },
{ "g.95.0", ["n0"] },
{ "g.95.1", ["n0"] },
{ "g.95.2", ["n0"] },
{ "g.95.3", ["n0","n18","n29","n51","n65","n67","n73"] },
{ "g.96.2", ["n0","n16","n20","n24","n27","n33","n39","n44","n47","n50","n53","n60","n68","n69","n71","n74","n81","n86","n92","n95"] },
{ "g.96.3", ["n0","n17","n18","n19","n20","n21","n22","n23","n24","n25","n26","n27","n29","n30","n31","n32","n33","n34","n35","n38","n39","n40","n41","n42","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76","n77","n78","n79","n80","n81","n82","n83","n84","n85","n86","n87","n88","n89","n90","n91","n92","n93","n94","n95"] },
{ "g.96.4", ["n0","n17","n18","n19","n20","n21","n22","n23","n24","n25","n26","n27","n28","n29","n31","n32","n33","n34","n35","n36","n37","n38","n39","n40","n41","n42","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76","n77","n78","n79","n80","n81","n82","n83","n84","n85","n86","n87","n88","n89","n90","n93","n94","n95"] },
{ "g.96.5", ["n0","n17","n18","n19","n20","n21","n22","n23","n24","n25","n26","n27","n29","n30","n31","n32","n33","n34","n35","n38","n39","n40","n41","n42","n44","n45","n46","n47","n48","n49","n50","n51","n52","n53","n54","n55","n56","n57","n58","n59","n60","n61","n62","n63","n64","n65","n66","n67","n68","n69","n70","n71","n72","n73","n74","n75","n76","n77","n78","n79","n80","n81","n82","n83","n84","n85","n86","n87","n88","n89","n90","n91","n92","n93","n94","n95"] },
{ "g.96.9", ["n6","n11","n12","n13","n14","n15","n16","n18","n19","n20","n21","n22","n23","n24","n25","n27","n29","n30","n33","n34","n35","n36","n38","n40","n41","n42","n43","n44","n45","n46","n47","n48","n49","n50","n51","n57","n58","n59","n60","n62","n63","n66","n69","n73","n76","n77","n80","n84","n85","n94","n95"]  },
{ "g.97.1", ["n0"] },
{ "g.99.0", ["n0","n54"] },
{ "g.99.1", ["n83","n84","n85","n86","n87","n88","n89","n90","n91","n92","n93","n94","n95","n96","n97","n98"] },
{ "g.99.2", ["n83","n84","n85","n86","n87","n88","n89","n90","n91","n92","n93","n94","n95","n96","n97","n98"] },
            };
    }
}