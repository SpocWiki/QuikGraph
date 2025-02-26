using System;
using System.Collections.Generic;
using NUnit.Framework;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Search;

namespace QuikGraph.Tests.Algorithms.Observers
{
    /// <summary>
    /// Tests for <see cref="VertexPredecessorRecorderObserver{TVertex,TEdge}"/>.
    /// </summary>
    [TestFixture]
    internal sealed class VertexPredecessorRecorderObserverTests : ObserverTestsBase
    {
        readonly Dictionary<int, IEdge<int>> _nullPredecessors = null;
        [Test]
        public void Constructor()
        {
            var recorder = _nullPredecessors.CreateVertexPredecessorRecorderObserver();
            CollectionAssert.IsEmpty(recorder.VerticesPredecessors);

            var predecessors = new Dictionary<int, IEdge<int>>();
            recorder = predecessors.CreateVertexPredecessorRecorderObserver();
            Assert.AreSame(predecessors, recorder.VerticesPredecessors);

            predecessors = new Dictionary<int, IEdge<int>>
            {
                [1] = Edge.Create(2, 1)
            };
            recorder = predecessors.CreateVertexPredecessorRecorderObserver();
            Assert.AreSame(predecessors, recorder.VerticesPredecessors);
        }

        [Test]
        public void Constructor_Throws_Not()
        {
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            _ = _nullPredecessors.CreateVertexPredecessorRecorderObserver();
        }

        [Test]
        public void Attach()
        {
            // DFS is used for tests but result may change if using another search algorithm
            // or another starting point
            {
                var recorder = _nullPredecessors.CreateVertexPredecessorRecorderObserver();

                var graph = new AdjacencyGraph<int, IEdge<int>>();

                var dfs = graph.CreateDepthFirstSearchAlgorithm();
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    CollectionAssert.IsEmpty(recorder.VerticesPredecessors);
                }
            }

            {
                var recorder = _nullPredecessors.CreateVertexPredecessorRecorderObserver();

                var graph = new AdjacencyGraph<int, IEdge<int>>();
                graph.AddVertexRange([1, 2]);

                var dfs = graph.CreateDepthFirstSearchAlgorithm();
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    CollectionAssert.IsEmpty(recorder.VerticesPredecessors);
                }
            }

            {
                var recorder = _nullPredecessors.CreateVertexPredecessorRecorderObserver();

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

                var dfs = graph.CreateDepthFirstSearchAlgorithm();
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    CollectionAssert.AreEqual(
                        new Dictionary<int, IEdge<int>>
                        {
                            [2] = edge12,
                            [3] = edge13,
                            [4] = edge24
                        }, 
                        recorder.VerticesPredecessors);
                }
            }

            {
                var recorder = _nullPredecessors.CreateVertexPredecessorRecorderObserver();

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

                var dfs = graph.CreateDepthFirstSearchAlgorithm();
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    CollectionAssert.AreEqual(
                        new Dictionary<int, IEdge<int>>
                        {
                            [2] = edge12,
                            [3] = edge13,
                            [4] = edge24
                        },
                        recorder.VerticesPredecessors);
                }
            }
        }

        [Test]
        public void Attach_Throws()
        {
            Attach_Throws_Test(_nullPredecessors.CreateVertexPredecessorRecorderObserver());
        }

        [Test]
        public void TryGetPath()
        {
            {
                var recorder = _nullPredecessors.CreateVertexPredecessorRecorderObserver();

                var graph = new AdjacencyGraph<int, IEdge<int>>();

                var dfs = graph.CreateDepthFirstSearchAlgorithm();
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    // Vertex not in the graph
                    Assert.IsFalse(recorder.TryGetPath(2, out _));
                }
            }

            {
                var recorder = _nullPredecessors.CreateVertexPredecessorRecorderObserver();

                var graph = new AdjacencyGraph<int, IEdge<int>>();
                graph.AddVertexRange([1, 2]);

                var dfs = graph.CreateDepthFirstSearchAlgorithm();
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    Assert.IsFalse(recorder.TryGetPath(2, out _));
                }
            }

            {
                var recorder = _nullPredecessors.CreateVertexPredecessorRecorderObserver();

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

                var dfs = graph.CreateDepthFirstSearchAlgorithm();
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    Assert.IsTrue(recorder.TryGetPath(4, out IEnumerable<IEdge<int>> path));
                    CollectionAssert.AreEqual(new[] { edge12, edge24 }, path);
                }
            }

            {
                var recorder = _nullPredecessors.CreateVertexPredecessorRecorderObserver();

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

                var dfs = graph.CreateDepthFirstSearchAlgorithm();
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    Assert.IsTrue(recorder.TryGetPath(4, out IEnumerable<IEdge<int>> path));
                    CollectionAssert.AreEqual(new[] { edge12, edge24 }, path);
                }
            }
        }

        [Test]
        public void TryGetPath_Throws()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => VertexPredecessorRecorderObserverX.CreateVertexPredecessorRecorderObserver<TestVertex, Edge<TestVertex>>(null).TryGetPath(null, out _));
        }
    }
}