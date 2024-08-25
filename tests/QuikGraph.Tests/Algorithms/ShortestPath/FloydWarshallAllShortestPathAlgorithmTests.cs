using System;
using System.Collections.Generic;
using NUnit.Framework;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.ShortestPath;


namespace QuikGraph.Tests.Algorithms.ShortestPath
{
    /// <summary>
    /// Tests for <see cref="FloydWarshallAllShortestPathAlgorithm{TVertex,TEdge}"/>.
    /// </summary>
    [TestFixture]
    internal sealed class FloydWarshallAllShortestPathAlgorithmTests : FloydWarshallTestsBase
    {
        [Test]
        public void Constructor()
        {
            Func<IEdge<int>, double> Weights = _ => 1.0;

            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var algorithm = new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(graph, Weights);
            algorithm.AssertAlgorithmState(graph);

            algorithm = new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(graph, Weights, DistanceRelaxers.CriticalDistance);
            algorithm.AssertAlgorithmState(graph);

            algorithm = new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(null, graph, Weights, DistanceRelaxers.CriticalDistance);
            algorithm.AssertAlgorithmState(graph);
        }

        [Test]
        public void Constructor_Throws()
        {
            // ReSharper disable ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            var graph = new AdjacencyGraph<int, IEdge<int>>();

            Func<IEdge<int>, double> Weights = _ => 1.0;

            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(null, Weights));
            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(graph, null));
            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(null, null));

            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(null, Weights, DistanceRelaxers.CriticalDistance));
            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(graph, null, DistanceRelaxers.CriticalDistance));
            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(graph, Weights, null));
            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(null, null, DistanceRelaxers.CriticalDistance));
            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(null, Weights, null));
            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(graph, null, null));
            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(null, null, null));

            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(null, null, Weights, DistanceRelaxers.CriticalDistance));
            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(null, graph, null, DistanceRelaxers.CriticalDistance));
            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(null, graph, Weights, null));
            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(null, null, null, DistanceRelaxers.CriticalDistance));
            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(null, null, Weights, null));
            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(null, graph, null, null));
            Assert.Throws<ArgumentNullException>(
                () => new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ObjectCreationAsStatement
        }

        [Test]
        public void TryGetDistance()
        {
            const int vertex1 = 1;
            const int vertex2 = 2;
            const int vertex3 = 3;

            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdge(Edge.Create(vertex1, vertex2));
            graph.AddVertex(vertex3);

            var algorithm = new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(graph, _ => 1.0);

            Assert.IsFalse(algorithm.TryGetDistance(vertex1, vertex2, out _));
            Assert.IsFalse(algorithm.TryGetDistance(vertex1, vertex3, out _));

            algorithm.Compute();

            Assert.IsTrue(algorithm.TryGetDistance(vertex1, vertex2, out double distance));
            Assert.AreEqual(1, distance);

            Assert.IsFalse(algorithm.TryGetDistance(vertex1, vertex3, out _));
        }

        [Test]
        public void TryGetDistance_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, IEdge<TestVertex>>();
            var algorithm = new FloydWarshallAllShortestPathAlgorithm<TestVertex, IEdge<TestVertex>>(graph, _ => 1.0);

            var vertex = new TestVertex();
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => algorithm.TryGetDistance(vertex, null, out _));
            Assert.Throws<ArgumentNullException>(() => algorithm.TryGetDistance(null, vertex, out _));
            Assert.Throws<ArgumentNullException>(() => algorithm.TryGetDistance(null, null, out _));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void TryGetPath()
        {
            const int vertex1 = 1;
            const int vertex2 = 2;
            const int vertex3 = 3;
            const int vertex4 = 4;

            var edge12 = Edge.Create(vertex1, vertex2);
            var edge24 = Edge.Create(vertex2, vertex4);

            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdge(edge12);
            graph.AddVerticesAndEdge(edge24);
            graph.AddVertex(vertex3);

            var algorithm = new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(graph, _ => 1.0);

            Assert.IsNull(algorithm.GetPath(vertex1, vertex1));
            Assert.IsNull(algorithm.GetPath(vertex1, vertex2));
            Assert.IsNull(algorithm.GetPath(vertex1, vertex4));
            Assert.IsNull(algorithm.GetPath(vertex1, vertex3));

            algorithm.Compute();

            Assert.IsNull(algorithm.GetPath(vertex1, vertex1));

            var path = algorithm.GetPath(vertex1, vertex2);
            CollectionAssert.AreEqual(
                new[] { edge12 },
                path);

            path = algorithm.GetPath(vertex1, vertex4);
            CollectionAssert.AreEqual(
                new[] { edge12, edge24 },
                path);

            Assert.IsNull(algorithm.GetPath(vertex1, vertex3));
        }

        [Test]
        public void TryGetPath_Throws()
        {
            var graph1 = new AdjacencyGraph<TestVertex, IEdge<TestVertex>>();
            var algorithm1 = new FloydWarshallAllShortestPathAlgorithm<TestVertex, IEdge<TestVertex>>(graph1, _ => 1.0);

            var vertex = new TestVertex();
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => algorithm1.GetPath(vertex, null));
            Assert.Throws<ArgumentNullException>(() => algorithm1.GetPath(null, vertex));
            Assert.Throws<ArgumentNullException>(() => algorithm1.GetPath(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void FloydWarshallSimpleGraph()
        {
            var distances = new Dictionary<IEdge<char>, double>();
            AdjacencyGraph<char, IEdge<char>> graph = CreateGraph(distances);
            var algorithm = new FloydWarshallAllShortestPathAlgorithm<char, IEdge<char>>(graph, e => distances[e]);
            algorithm.Compute();

            Assert.IsTrue(algorithm.TryGetDistance('A', 'A', out double distance));
            Assert.AreEqual(0, distance);

            Assert.IsTrue(algorithm.TryGetDistance('A', 'B', out distance));
            Assert.AreEqual(6, distance);

            Assert.IsTrue(algorithm.TryGetDistance('A', 'C', out distance));
            Assert.AreEqual(1, distance);

            Assert.IsTrue(algorithm.TryGetDistance('A', 'D', out distance));
            Assert.AreEqual(4, distance);

            Assert.IsTrue(algorithm.TryGetDistance('A', 'E', out distance));
            Assert.AreEqual(5, distance);
        }

        [Test]
        public void FloydWarshall_Throws()
        {
            // Without negative cycle
            var edge12 = Edge.Create(1, 2);
            var edge23 = Edge.Create(2, 3);
            var edge34 = Edge.Create(3, 4);

            var negativeWeightGraph = new AdjacencyGraph<int, IEdge<int>>();
            negativeWeightGraph.AddVerticesAndEdgeRange(edge12, edge23, edge34);

            var algorithm = new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(
                negativeWeightGraph,
                e =>
                {
                    if (e == edge12)
                        return 12.0;
                    if (e == edge23)
                        return -23.0;
                    if (e == edge34)
                        return -34.0;
                    return 1.0;
                });
            Assert.DoesNotThrow(() => algorithm.Compute());

            // With negative cycle
            var edge41 = Edge.Create(4, 1);

            var negativeCycleGraph = new AdjacencyGraph<int, IEdge<int>>();
            negativeCycleGraph.AddVerticesAndEdgeRange(edge12, edge23, edge34, edge41);

            algorithm = new FloydWarshallAllShortestPathAlgorithm<int, IEdge<int>>(
                negativeCycleGraph,
                e =>
                {
                    if (e == edge12)
                        return 12.0;
                    if (e == edge23)
                        return -23.0;
                    if (e == edge34)
                        return -34.0;
                    if (e == edge41)
                        return 41.0;
                    return 1.0;
                });
            Assert.Throws<NegativeCycleGraphException>(() => algorithm.Compute());
        }
    }
}