using System;
using System.Collections.Generic;
using NUnit.Framework;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Search;

namespace QuikGraph.Tests.Algorithms.Observers
{
    /// <summary>
    /// Tests for <see cref="VertexDistanceRecorderObserver{TVertex,TEdge}"/>.
    /// </summary>
    [TestFixture]
    internal sealed class VertexDistanceRecorderObserverTests : ObserverTestsBase
    {
        readonly Func<IEdge<int>, double> _edgeWeights = _ => 1.0;
        readonly Func<IEdge<int>, double> _nullEdgeWeights = null;
        [Test]
        public void Constructor()
        {
            var recorder = _edgeWeights.CreateVertexDistanceRecorderObserver();
            Assert.AreSame(_edgeWeights, recorder.EdgeWeights);
            Assert.IsNotNull(recorder.DistanceRelaxer);
            Assert.IsNotNull(recorder.Distances);

            var distances = new Dictionary<int, double>();
            recorder = _edgeWeights.CreateVertexDistanceRecorderObserver(
                DistanceRelaxers.ShortestDistance,
                distances);
            Assert.AreSame(_edgeWeights, recorder.EdgeWeights);
            Assert.AreSame(DistanceRelaxers.ShortestDistance,recorder.DistanceRelaxer);
            Assert.AreSame(distances, recorder.Distances);
        }

        [Test]
        public void Constructor_Throws()
        {
            // ReSharper disable ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => _nullEdgeWeights.CreateVertexDistanceRecorderObserver());

            Assert.Throws<ArgumentNullException>(
                () => _nullEdgeWeights.CreateVertexDistanceRecorderObserver(DistanceRelaxers.ShortestDistance, new Dictionary<int, double>()));
            _ = _edgeWeights.CreateVertexDistanceRecorderObserver(null, new Dictionary<int, double>());
            _ = _edgeWeights.CreateVertexDistanceRecorderObserver(DistanceRelaxers.ShortestDistance);
            Assert.Throws<ArgumentNullException>(
                () => _nullEdgeWeights.CreateVertexDistanceRecorderObserver(null, new Dictionary<int, double>()));
            _ = _edgeWeights.CreateVertexDistanceRecorderObserver();
            Assert.Throws<ArgumentNullException>(
                () => _nullEdgeWeights.CreateVertexDistanceRecorderObserver(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ObjectCreationAsStatement
        }

        [Test]
        public void Attach()
        {
            // DFS is used for tests but result may change if using another search algorithm
            // or another starting point
            {
                var recorder = _edgeWeights.CreateVertexDistanceRecorderObserver();

                var graph = new AdjacencyGraph<int, IEdge<int>>();

                var dfs = new DepthFirstSearchAlgorithm<int, IEdge<int>>(graph);
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    CollectionAssert.IsEmpty(recorder.Distances);
                }
            }

            {
                var recorder = _edgeWeights.CreateVertexDistanceRecorderObserver();

                var graph = new AdjacencyGraph<int, IEdge<int>>();
                graph.AddVertexRange([1, 2]);

                var dfs = new DepthFirstSearchAlgorithm<int, IEdge<int>>(graph);
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    CollectionAssert.IsEmpty(recorder.Distances);
                }
            }

            {
                var recorder = _edgeWeights.CreateVertexDistanceRecorderObserver();

                // Graph without cycle
                var edge12 = Edge.Create(1, 2);
                var edge13 = Edge.Create(1, 3);
                var edge14 = Edge.Create(1, 4);
                var edge24 = Edge.Create(2, 4);
                var edge31 = Edge.Create(3, 1);
                var edge33 = Edge.Create(3, 3);
                var edge34 = Edge.Create(3, 4);
                var graph = new AdjacencyGraph<int, IEdge<int>>();
                graph.AddVerticesAndEdgeRange(
                [
                    edge12, edge13, edge14, edge24, edge31, edge33, edge34
                ]);

                var dfs = new DepthFirstSearchAlgorithm<int, IEdge<int>>(graph);
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    CollectionAssert.AreEqual(
                        new Dictionary<int, double>
                        {
                            [1] = 0,
                            [2] = 1,
                            [3] = 1,
                            [4] = 2
                        },
                        recorder.Distances);
                }
            }

            {
                var recorder = _edgeWeights.CreateVertexDistanceRecorderObserver();

                // Graph with cycle
                var edge12 = Edge.Create(1, 2);
                var edge13 = Edge.Create(1, 3);
                var edge14 = Edge.Create(1, 4);
                var edge24 = Edge.Create(2, 4);
                var edge31 = Edge.Create(3, 1);
                var edge33 = Edge.Create(3, 3);
                var edge34 = Edge.Create(3, 4);
                var edge41 = Edge.Create(4, 1);
                var graph = new AdjacencyGraph<int, IEdge<int>>();
                graph.AddVerticesAndEdgeRange(
                [
                    edge12, edge13, edge14, edge24, edge31, edge33, edge34, edge41
                ]);

                var dfs = new DepthFirstSearchAlgorithm<int, IEdge<int>>(graph);
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    CollectionAssert.AreEqual(
                        new Dictionary<int, double>
                        {
                            [1] = 0,
                            [2] = 1,
                            [3] = 1,
                            [4] = 2
                        },
                        recorder.Distances);
                }
            }
        }

        [Test]
        public void Attach_Throws()
        {
            Attach_Throws_Test(_edgeWeights.CreateVertexDistanceRecorderObserver());
        }
    }
}