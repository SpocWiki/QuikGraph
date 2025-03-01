﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Search;

namespace QuikGraph.Tests.Algorithms.Observers
{
    /// <summary>
    /// Tests for <see cref="UndirectedVertexDistanceRecorderObserver{TVertex,TEdge}"/>.
    /// </summary>
    [TestFixture]
    internal sealed class UndirectedVertexDistanceRecorderObserverTests : ObserverTestsBase
    {
        [Test]
        public void Constructor()
        {
            Func<IEdge<int>, double> edgeWeights = _ => 1.0;
            var recorder = new UndirectedVertexDistanceRecorderObserver<int, IEdge<int>>(edgeWeights);
            Assert.AreSame(edgeWeights, recorder.EdgeWeights);
            Assert.IsNotNull(recorder.DistanceRelaxer);
            Assert.IsNotNull(recorder.Distances);

            var distances = new Dictionary<int, double>();
            recorder = new UndirectedVertexDistanceRecorderObserver<int, IEdge<int>>(
                edgeWeights,
                DistanceRelaxers.ShortestDistance,
                distances);
            Assert.AreSame(edgeWeights, recorder.EdgeWeights);
            Assert.AreSame(DistanceRelaxers.ShortestDistance,recorder.DistanceRelaxer);
            Assert.AreSame(distances, recorder.Distances);
        }

        [Test]
        public void Constructor_Throws()
        {
            // ReSharper disable ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => new UndirectedVertexDistanceRecorderObserver<int, IEdge<int>>(null));

            Assert.Throws<ArgumentNullException>(
                () => new UndirectedVertexDistanceRecorderObserver<int, IEdge<int>>(null, DistanceRelaxers.ShortestDistance, new Dictionary<int, double>()));
            Assert.Throws<ArgumentNullException>(
                () => new UndirectedVertexDistanceRecorderObserver<int, IEdge<int>>(_ => 1.0, null, new Dictionary<int, double>()));
            Assert.Throws<ArgumentNullException>(
                () => new UndirectedVertexDistanceRecorderObserver<int, IEdge<int>>(_ => 1.0, DistanceRelaxers.ShortestDistance, null));
            Assert.Throws<ArgumentNullException>(
                () => new UndirectedVertexDistanceRecorderObserver<int, IEdge<int>>(null, null, new Dictionary<int, double>()));
            Assert.Throws<ArgumentNullException>(
                () => new UndirectedVertexDistanceRecorderObserver<int, IEdge<int>>(_ => 1.0, null, null));
            Assert.Throws<ArgumentNullException>(
                () => new UndirectedVertexDistanceRecorderObserver<int, IEdge<int>>(null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ObjectCreationAsStatement
        }

        [Test]
        public void Attach()
        {
            // Undirected DFS is used for tests but result may change if using another search algorithm
            // or another starting point
            {
                var recorder = new UndirectedVertexDistanceRecorderObserver<int, IEdge<int>>(_ => 1.0);

                var graph = new UndirectedGraph<int, IEdge<int>>();

                var dfs = graph.CreateUndirectedDepthFirstSearchAlgorithm();
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    CollectionAssert.IsEmpty(recorder.Distances);
                }
            }

            {
                var recorder = new UndirectedVertexDistanceRecorderObserver<int, IEdge<int>>(_ => 1.0);

                var graph = new UndirectedGraph<int, IEdge<int>>();
                graph.AddVertexRange([1, 2]);

                var dfs = graph.CreateUndirectedDepthFirstSearchAlgorithm();
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    CollectionAssert.IsEmpty(recorder.Distances);
                }
            }

            {
                var recorder = new UndirectedVertexDistanceRecorderObserver<int, IEdge<int>>(_ => 1.0);

                var edge12 = Edge.Create(1, 2);
                var edge14 = Edge.Create(1, 4);
                var edge31 = Edge.Create(3, 1);
                var edge33 = Edge.Create(3, 3);
                var edge34 = Edge.Create(3, 4);
                var edge42 = Edge.Create(4, 2);
                var graph = new UndirectedGraph<int, IEdge<int>>();
                graph.AddVerticesAndEdgeRange(
                [
                    edge12, edge14, edge31, edge33, edge34, edge42
                ]);

                var dfs = graph.CreateUndirectedDepthFirstSearchAlgorithm();
                using (recorder.Attach(dfs))
                {
                    dfs.Compute();

                    CollectionAssert.AreEqual(
                        new Dictionary<int, double>
                        {
                            [1] = 0,
                            [2] = 1,
                            [3] = 3,
                            [4] = 2
                        },
                        recorder.Distances);
                }
            }
        }

        [Test]
        public void Attach_Throws()
        {
            Attach_Throws_Test(new UndirectedVertexDistanceRecorderObserver<int, IEdge<int>>(_ => 1.0));
        }
    }
}