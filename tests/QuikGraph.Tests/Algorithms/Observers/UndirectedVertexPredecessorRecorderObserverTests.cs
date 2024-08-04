using System;
using System.Collections.Generic;
using NUnit.Framework;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Search;

namespace QuikGraph.Tests.Algorithms.Observers
{
    /// <summary>
    /// Tests for <see cref="UndirectedVertexPredecessorRecorderObserver{TVertex,TEdge}"/>.
    /// </summary>
    [TestFixture]
    internal sealed class UndirectedVertexPredecessorRecorderObserverTests : ObserverTestsBase
    {
        [Test]
        public void Constructor()
        {
            var recorder = new UndirectedVertexPredecessorRecorderObserver<int, Edge<int>>();
            CollectionAssert.IsEmpty(recorder.VerticesPredecessors);

            var predecessors = new Dictionary<int, Edge<int>>();
            recorder = new UndirectedVertexPredecessorRecorderObserver<int, Edge<int>>(predecessors);
            Assert.AreSame(predecessors, recorder.VerticesPredecessors);

            predecessors = new Dictionary<int, Edge<int>>
            {
                [1] = Edge.Create(2, 1)
            };
            recorder = new UndirectedVertexPredecessorRecorderObserver<int, Edge<int>>(predecessors);
            Assert.AreSame(predecessors, recorder.VerticesPredecessors);
        }

        [Test]
        public void Constructor_Throws()
        {
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new UndirectedVertexPredecessorRecorderObserver<int, Edge<int>>(null));
        }

        [Test]
        public void Attach()
        {
            // Undirected DFS is used for tests but result may change if using another search algorithm
            // or another starting point
            {
                var recorder = new UndirectedVertexPredecessorRecorderObserver<int, Edge<int>>();

                var graph = new UndirectedGraph<int, Edge<int>>();

                var dfs = new UndirectedDepthFirstSearchAlgorithm<int, Edge<int>>(graph);
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    CollectionAssert.IsEmpty(recorder.VerticesPredecessors);
                }
            }

            {
                var recorder = new UndirectedVertexPredecessorRecorderObserver<int, Edge<int>>();

                var graph = new UndirectedGraph<int, Edge<int>>();
                graph.AddVertexRange(new[] { 1, 2 });

                var dfs = new UndirectedDepthFirstSearchAlgorithm<int, Edge<int>>(graph);
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    CollectionAssert.IsEmpty(recorder.VerticesPredecessors);
                }
            }

            {
                var recorder = new UndirectedVertexPredecessorRecorderObserver<int, Edge<int>>();

                var edge12 = Edge.Create(1, 2);
                var edge14 = Edge.Create(1, 4);
                var edge31 = Edge.Create(3, 1);
                var edge33 = Edge.Create(3, 3);
                var edge34 = Edge.Create(3, 4);
                var edge42 = Edge.Create(4, 2);
                var graph = new UndirectedGraph<int, Edge<int>>();
                graph.AddVerticesAndEdgeRange(new[]
                {
                    edge12, edge14, edge31, edge33, edge34, edge42
                });

                var dfs = new UndirectedDepthFirstSearchAlgorithm<int, Edge<int>>(graph);
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    CollectionAssert.AreEqual(
                        new Dictionary<int, Edge<int>>
                        {
                            [2] = edge12,
                            [3] = edge34,
                            [4] = edge42
                        },
                        recorder.VerticesPredecessors);
                }
            }
        }

        [Test]
        public void Attach_Throws()
        {
            Attach_Throws_Test(new UndirectedVertexPredecessorRecorderObserver<int, Edge<int>>());
        }

        [Test]
        public void TryGetPath()
        {
            {
                var recorder = new UndirectedVertexPredecessorRecorderObserver<int, Edge<int>>();

                var graph = new UndirectedGraph<int, Edge<int>>();

                var dfs = new UndirectedDepthFirstSearchAlgorithm<int, Edge<int>>(graph);
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    // Vertex not in the graph
                    Assert.IsFalse(recorder.TryGetPath(2, out _));
                }
            }

            {
                var recorder = new UndirectedVertexPredecessorRecorderObserver<int, Edge<int>>();

                var graph = new UndirectedGraph<int, Edge<int>>();
                graph.AddVertexRange(new[] { 1, 2 });

                var dfs = new UndirectedDepthFirstSearchAlgorithm<int, Edge<int>>(graph);
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    Assert.IsFalse(recorder.TryGetPath(2, out _));
                }
            }

            {
                var recorder = new UndirectedVertexPredecessorRecorderObserver<int, Edge<int>>();

                var edge12 = Edge.Create(1, 2);
                var edge14 = Edge.Create(1, 4);
                var edge31 = Edge.Create(3, 1);
                var edge33 = Edge.Create(3, 3);
                var edge34 = Edge.Create(3, 4);
                var edge42 = Edge.Create(4, 2);
                var graph = new UndirectedGraph<int, Edge<int>>();
                graph.AddVerticesAndEdgeRange(new[]
                {
                    edge12, edge14, edge31, edge33, edge34, edge42
                });

                var dfs = new UndirectedDepthFirstSearchAlgorithm<int, Edge<int>>(graph);
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    Assert.IsTrue(recorder.TryGetPath(4, out IEnumerable<Edge<int>> path));
                    CollectionAssert.AreEqual(new[] { edge12, edge42 }, path);
                }
            }
        }

        [Test]
        public void TryGetPath_Throws()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => new UndirectedVertexPredecessorRecorderObserver<TestVertex, Edge<TestVertex>>().TryGetPath(null, out _));
        }
    }
}