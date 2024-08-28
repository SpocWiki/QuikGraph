using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using QuikGraph.Algorithms;
using static QuikGraph.Tests.QuikGraphUnitTestsHelpers;

namespace QuikGraph.Tests.Algorithms
{
    /// <summary> Tests for <see cref="EulerianTrailAlgorithm{TVertex,TEdge}"/>. </summary>
    /// <inheritdoc cref="EulerianTrailAlgorithm{TVertex,TEdge}"/>
    [TestFixture]
    internal sealed class EulerianTrailAlgorithmTests : RootedAlgorithmTestsBase
    {
        #region Test helpers

        public static void ComputeTrailsAndCheck<TVertex, TEdge>(
            [NotNull] IMutableVertexAndEdgeListGraph<TVertex, TEdge> graph,
            [NotNull, InstantHandle] Func<TVertex, TVertex, TEdge> edgeFactory,
            [NotNull, ItemNotNull] out ICollection<TEdge>[] trails,
            [NotNull, ItemNotNull] out TEdge[] circuit)
            where TEdge : IEdge<TVertex>
        {
            if (!graph.TryComputeTrails(edgeFactory, out trails, out circuit))
            {
                return;
            }

            // make sure all the edges are in the trail
            var edges = new HashSet<TEdge>();
            foreach (TEdge edge in graph.Edges)
                Assert.IsTrue(edges.Add(edge));

            foreach (ICollection<TEdge> trail in trails)
            {
                Assert.AreEqual(graph.EdgeCount, edges.Count);
                QuikGraphAssert.TrueForAll(trail, edges.Contains);
            }
        }

        private static void ComputeTrails<TVertex, TEdge>(
            [NotNull] IMutableVertexAndEdgeListGraph<TVertex, TEdge> graph,
            [NotNull] TVertex root,
            [NotNull, InstantHandle] Func<TVertex, TVertex, TEdge> edgeFactory,
            [NotNull, ItemNotNull] out ICollection<TEdge>[] trails,
            [NotNull, ItemNotNull] out TEdge[] circuit)
            where TEdge : IEdge<TVertex>
        {
            trails = new ICollection<TEdge>[0];
            circuit = new TEdge[0];

            int circuitCount = graph.ComputeEulerianPathCount();
            if (circuitCount == 0)
                return;

            TEdge[] graphEdges = graph.Edges.ToArray();

            var algorithm = new EulerianTrailAlgorithm<TVertex, TEdge>(graph);
            algorithm.AddTemporaryEdges((s, t) => edgeFactory(s, t));
            TEdge[] augmentedGraphEdges = graph.Edges.ToArray();
            Assert.GreaterOrEqual(augmentedGraphEdges.Length, graphEdges.Length);
            TEdge[] temporaryEdges = augmentedGraphEdges.Except(graphEdges).ToArray();
            Assert.AreEqual(augmentedGraphEdges.Length - graphEdges.Length, temporaryEdges.Length);

            algorithm.Compute();
            trails = algorithm.Trails(root).ToArray();

            algorithm.RemoveTemporaryEdges();
            Assert.IsNotNull(algorithm.Circuit);
            circuit = algorithm.Circuit;

            // Lets make sure all the edges are in the trail
            var edges = new HashSet<TEdge>();
            foreach (TEdge edge in graph.Edges)
                Assert.IsTrue(edges.Add(edge));

            foreach (ICollection<TEdge> trail in trails)
            {
                Assert.AreEqual(graph.EdgeCount, edges.Count);
                QuikGraphAssert.TrueForAll(
                    trail,
                    // Edge in graph or part of temporary ones but is a root
                    edge => edges.Contains(edge)
                            || (temporaryEdges.Contains(edge) && Equals(edge.Source, root)));
            }
        }

        #endregion

        [Test]
        public void Constructor()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();

            var algorithm = new EulerianTrailAlgorithm<int, IEdge<int>>(graph);
            AssertAlgorithmProperties(algorithm, graph);

            algorithm = new EulerianTrailAlgorithm<int, IEdge<int>>(graph, null);
            AssertAlgorithmProperties(algorithm, graph);
            return;

            void AssertAlgorithmProperties<TVertex, TEdge>(
                EulerianTrailAlgorithm<TVertex, TEdge> algo,
                IMutableVertexAndEdgeListGraph<TVertex, TEdge> g)
                where TEdge : IEdge<TVertex>
            {
                algo.AssertAlgorithmState(g);
                CollectionAssert.IsEmpty(algo.Circuit);
            }
        }

        [Test]
        public void Constructor_Throws()
        {
            // ReSharper disable ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => new EulerianTrailAlgorithm<int, IEdge<int>>(null));
            Assert.Throws<ArgumentNullException>(
                () => new EulerianTrailAlgorithm<int, IEdge<int>>(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ObjectCreationAsStatement
        }

        #region Rooted algorithm

        /// <inheritdoc cref="RootedAlgorithmTestsBase.TryGetRootVertex_Test{TVertex,TGraph}"/>
        [Test]
        public void TryGetRootVertex()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var algorithm = new EulerianTrailAlgorithm<int, IEdge<int>>(graph);
            TryGetRootVertex_Test(algorithm);
        }

        /// <inheritdoc cref="RootedAlgorithmTestsBase.SetRootVertex_Test{TGraph}(RootedAlgorithmBase{int, TGraph})"/>
        [Test]
        public void SetRootVertex()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var algorithm = new EulerianTrailAlgorithm<int, IEdge<int>>(graph);
            SetRootVertex_Test(algorithm);
        }

        /// <inheritdoc cref="RootedAlgorithmTestsBase.SetRootVertex_Null_Should_Throw_ArgumentNullException{TVertex,TGraph}"/>
        [Test]
        public void SetRootVertex_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, IEdge<TestVertex>>();
            var algorithm = new EulerianTrailAlgorithm<TestVertex, IEdge<TestVertex>>(graph);
            SetRootVertex_Null_Should_Throw_ArgumentNullException(algorithm);
        }

        /// <inheritdoc cref="RootedAlgorithmTestsBase.ClearRootVertex_RaisesVertexChanged_Test{TVertex,TGraph}"/>
        [Test]
        public void ClearRootVertex()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var algorithm = new EulerianTrailAlgorithm<int, IEdge<int>>(graph);
            ClearRootVertex_RaisesVertexChanged_Test(algorithm);
        }

        /// <inheritdoc cref="RootedAlgorithmTestsBase.ComputeWithoutRoot_ShouldNotThrow_Test{TGraph}"/>
        [Test]
        public void ComputeWithoutRoot_Throws()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            ComputeWithoutRoot_ShouldNotThrow_Test(graph, ()
                => new EulerianTrailAlgorithm<int, IEdge<int>>(graph));
        }

        /// <inheritdoc cref="RootedAlgorithmTestsBase.ComputeWithRoot_Test{TVertex,TGraph}"/>
        [Test]
        public void ComputeWithRoot()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVertex(0);
            var algorithm = new EulerianTrailAlgorithm<int, IEdge<int>>(graph);
            ComputeWithRoot_Test(algorithm);
        }

        /// <inheritdoc cref="RootedAlgorithmTestsBase.ComputeWithUnknownRootOrNull_Throws_Test{TVertex,TGraph}"/>
        [Test]
        public void ComputeWithRoot_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, IEdge<TestVertex>>();
            ComputeWithUnknownRootOrNull_Throws_Test(()
                => new EulerianTrailAlgorithm<TestVertex, IEdge<TestVertex>>(graph));
        }

        #endregion Rooted algorithm

        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> ComputeEulerianPathCountTestCases
        {
            [UsedImplicitly]
            get
            {
                var emptyGraph = new AdjacencyGraph<int, IEdge<int>>();
                yield return new TestCaseData(emptyGraph) { ExpectedResult = 1 };

                var moreVerticesThanEdgesGraph = new AdjacencyGraph<int, IEdge<int>>();
                moreVerticesThanEdgesGraph.AddVertexRange( 1, 2 );
                moreVerticesThanEdgesGraph.AddEdge(Edge.Create(1, 2));
                yield return new TestCaseData(moreVerticesThanEdgesGraph) { ExpectedResult = 0 };


                var sameVerticesAndEdgesCountGraph = new AdjacencyGraph<int, IEdge<int>>();
                sameVerticesAndEdgesCountGraph.AddVertexRange( 1, 2 );
                sameVerticesAndEdgesCountGraph.AddEdgeRange(
                    Edge.Create(1, 2),
                    Edge.Create(2, 1)
                );
                yield return new TestCaseData(sameVerticesAndEdgesCountGraph) { ExpectedResult = 1 };


                var sameVerticesAndEdgesCountGraph2 = new AdjacencyGraph<int, IEdge<int>>();
                sameVerticesAndEdgesCountGraph2.AddVertexRange( 1, 2, 3 );
                sameVerticesAndEdgesCountGraph2.AddEdgeRange(
                    Edge.Create(1, 2),
                    Edge.Create(2, 1),
                    Edge.Create(1, 3)
                );
                yield return new TestCaseData(sameVerticesAndEdgesCountGraph2) { ExpectedResult = 1 };


                var moreEdgesThanEdgesGraph = new AdjacencyGraph<int, IEdge<int>>();
                moreEdgesThanEdgesGraph.AddVertexRange( 1, 2, 3, 4, 5 );
                moreEdgesThanEdgesGraph.AddEdgeRange(
                    Edge.Create(1, 5),
                    Edge.Create(1, 2),
                    Edge.Create(2, 1),

                    Edge.Create(1, 3),
                    Edge.Create(1, 4),
                    Edge.Create(3, 4),
                    Edge.Create(3, 4)
                );
                yield return new TestCaseData(moreEdgesThanEdgesGraph) { ExpectedResult = 2 };
            }
        }

        /// <inheritdoc cref="VertexAndEdgeListGraphX.ComputeEulerianPathCount{TVertex,TEdge}"/>
        [TestCaseSource(nameof(ComputeEulerianPathCountTestCases))]
        public int ComputeEulerianPathCount([NotNull] AdjacencyGraph<int, IEdge<int>> graph)
            => graph.ComputeEulerianPathCount();

        /// <inheritdoc cref="VertexAndEdgeListGraphX.ComputeEulerianPathCount{TVertex,TEdge}"/>
        [Test]
        public void ComputeEulerianPathCount_Throws() => Assert.Throws<ArgumentNullException>(()
            => _ = ComputeEulerianPathCount(null!));

        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> AddTemporaryEdgesTestCases
        {
            [UsedImplicitly]
            get
            {
                var emptyGraph = new AdjacencyGraph<int, EquatableEdge<int>>();
                yield return new TestCaseData(emptyGraph, new EquatableEdge<int>[0]);


                var evenVerticesGraph = new AdjacencyGraph<int, EquatableEdge<int>>();
                evenVerticesGraph.AddVertexRange( 1, 2, 3, 4 );
                evenVerticesGraph.AddEdgeRange(
                    new EquatableEdge<int>(1, 2),
                    new EquatableEdge<int>(1, 3),
                    new EquatableEdge<int>(2, 4),
                    new EquatableEdge<int>(3, 4)
                );
                yield return new TestCaseData(evenVerticesGraph, new EquatableEdge<int>[0]);


                var oddVerticesGraph1 = new AdjacencyGraph<int, EquatableEdge<int>>();
                oddVerticesGraph1.AddVertexRange( 1, 2, 3 );
                oddVerticesGraph1.AddEdgeRange(
                    new EquatableEdge<int>(1, 2),
                    new EquatableEdge<int>(2, 1),
                    new EquatableEdge<int>(1, 3)
                );
                yield return new TestCaseData(oddVerticesGraph1, new[] { new EquatableEdge<int>(1, 3) });


                var oddVerticesGraph2 = new AdjacencyGraph<int, EquatableEdge<int>>();
                oddVerticesGraph2.AddVertexRange( 1, 2, 3, 4, 5 );
                oddVerticesGraph2.AddEdgeRange(
                    new EquatableEdge<int>[] {
                        new (1, 2),
                        new (2, 1),
                        new (1, 4),
                        new (3, 1),
                        new (1, 5)
                    }
                );
                yield return new TestCaseData(oddVerticesGraph2,
                    new EquatableEdge<int>[] {
                        new (1, 4),
                        new (3, 5)
                        }
                    );


                var oddVerticesGraph3 = new AdjacencyGraph<int, EquatableEdge<int>>();
                oddVerticesGraph3.AddVertexRange( 1, 2, 3, 4, 5 );
                oddVerticesGraph3.AddEdgeRange(
                    new EquatableEdge<int>[]{
                        new (1, 2),
                        new (2, 1),
                        new (1, 3),
                        new (1, 4),
                        new (3, 4),
                        new (3, 4),
                        new (1, 5)
                    }
                );
                yield return new TestCaseData(oddVerticesGraph3,
                        new EquatableEdge<int>[] {
                            new (1, 3),
                            new (4, 5)
                        }
                    );
            }
        }

        /// <inheritdoc cref="EulerianTrailAlgorithm{TVertex,TEdge}.AddTemporaryEdges"/>
        [TestCaseSource(nameof(AddTemporaryEdgesTestCases))]
        public void Test_AddTemporaryEdges(
            [NotNull] AdjacencyGraph<int, EquatableEdge<int>> graph,
            [NotNull, ItemNotNull] EquatableEdge<int>[] expectedTemporaryEdges)
        {
            var algorithm = new EulerianTrailAlgorithm<int, EquatableEdge<int>>(graph);
            int edgeCount = graph.EdgeCount;
            EquatableEdge<int>[] tmpEdges = algorithm.AddTemporaryEdges(
                (source, target) => new EquatableEdge<int>(source, target));
            CollectionAssert.AreEquivalent(expectedTemporaryEdges, tmpEdges);

            Assert.AreEqual(edgeCount + tmpEdges.Length, graph.EdgeCount);
            EquatableEdge<int>[] graphEdges = graph.Edges.ToArray();
            foreach (EquatableEdge<int> edge in tmpEdges)
            {
                Assert.Contains(edge, graphEdges);
            }
        }

        /// <inheritdoc cref="EulerianTrailAlgorithm{TVertex,TEdge}.AddTemporaryEdges"/>
        [Test]
        public void AddTemporaryEdges_Throws()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var algorithm = new EulerianTrailAlgorithm<int, IEdge<int>>(graph);

            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => algorithm.AddTemporaryEdges(null));
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> RemoveTemporaryEdgesTestCases
        {
            [UsedImplicitly]
            get
            {
                var emptyGraph = new AdjacencyGraph<int, EquatableEdge<int>>();
                yield return new TestCaseData(emptyGraph);


                var evenVerticesGraph = new AdjacencyGraph<int, EquatableEdge<int>>();
                evenVerticesGraph.AddVertexRange( 1, 2, 3, 4 );
                evenVerticesGraph.AddEdgeRange(
                    new EquatableEdge<int>(1, 2),
                    new EquatableEdge<int>(1, 3),
                    new EquatableEdge<int>(2, 4),
                    new EquatableEdge<int>(3, 4)
                );
                yield return new TestCaseData(evenVerticesGraph);


                var oddVerticesGraph1 = new AdjacencyGraph<int, EquatableEdge<int>>();
                oddVerticesGraph1.AddVertexRange( 1, 2, 3 );
                oddVerticesGraph1.AddEdgeRange(
                    new EquatableEdge<int>(1, 2),
                    new EquatableEdge<int>(2, 1),
                    new EquatableEdge<int>(1, 3)
                );
                yield return new TestCaseData(oddVerticesGraph1);


                var oddVerticesGraph2 = new AdjacencyGraph<int, EquatableEdge<int>>();
                oddVerticesGraph2.AddVertexRange( 1, 2, 3, 4, 5 );
                oddVerticesGraph2.AddEdgeRange(
                    new EquatableEdge<int>(1, 2),
                    new EquatableEdge<int>(2, 1),
                    new EquatableEdge<int>(1, 4),
                    new EquatableEdge<int>(3, 1),
                    new EquatableEdge<int>(1, 5)
                );
                yield return new TestCaseData(oddVerticesGraph2);
            }
        }

        /// <inheritdoc cref="EulerianTrailAlgorithm{TVertex,TEdge}.RemoveTemporaryEdges"/>
        [TestCaseSource(nameof(RemoveTemporaryEdgesTestCases))]
        public void RemoveTemporaryEdges([NotNull] AdjacencyGraph<int, EquatableEdge<int>> graph)
        {
            var algorithm = new EulerianTrailAlgorithm<int, EquatableEdge<int>>(graph);
            int edgeCount = graph.EdgeCount;
            EquatableEdge<int>[] tmpEdges = algorithm.AddTemporaryEdges(
                (source, target) => new EquatableEdge<int>(source, target));
            Assert.AreEqual(edgeCount + tmpEdges.Length, graph.EdgeCount);

            algorithm.RemoveTemporaryEdges();
            Assert.AreEqual(edgeCount, graph.EdgeCount);
        }

        #region Trails

        [Test]
        public void NotEulerianTrailGraph()
        {
            var graph = TestGraphFactory.LoadGraph(GetGraphFilePath("g.42.34.graphml"));
            // No trails in tests graphs there
            ComputeTrailsAndCheck(graph, Edge.Create,
                out ICollection<IEdge<string>>[] trails,
                out IEdge<string>[] circuit);
            CollectionAssert.IsEmpty(trails);
            CollectionAssert.IsEmpty(circuit);
        }

        [Test]
        public void SingleEulerianTrailGraph()
        {
            var edge1 = Edge.Create('b', 'c');
            var edge2 = Edge.Create('f', 'a');
            var edge3 = Edge.Create('a', 'b');
            var edge4 = Edge.Create('c', 'd');
            var edge5 = Edge.Create('e', 'c');
            var edge6 = Edge.Create('d', 'e');
            var edge7 = Edge.Create('c', 'f');

            var graph = new AdjacencyGraph<char, IEdge<char>>();
            graph.AddVerticesAndEdgeRange(
                edge1, edge2, edge3, edge4, edge5, edge6, edge7
            );

            ComputeTrailsAndCheck(graph, Edge.Create, out var trails, out var circuit);

            IEdge<char>[] expectedTrail = { edge3, edge1, edge4, edge6, edge5, edge7, edge2 };
            Assert.AreEqual(1, trails.Length);
            Assert.IsTrue(trails[0].Cast<IEdge<char>>().IsPath());
            CollectionAssert.AreEquivalent(expectedTrail, trails[0]);

            Assert.IsTrue(circuit.IsPath());
            CollectionAssert.AreEquivalent(expectedTrail, circuit);
        }

        [Test]
        public void SingleEulerianTrailGraph2()
        {
            var edge1 = Edge.Create('b', 'c');
            var edge2 = Edge.Create('f', 'a');
            var edge3 = Edge.Create('a', 'b');
            var edge4 = Edge.Create('c', 'd');
            var edge5 = Edge.Create('e', 'c');
            var edge6 = Edge.Create('d', 'e');
            var edge7 = Edge.Create('c', 'f');
            var edge8 = Edge.Create('b', 'e');

            var graph = new AdjacencyGraph<char, IEdge<char>>();
            graph.AddVerticesAndEdgeRange(
                edge1, edge2, edge3, edge4, edge5, edge6, edge7, edge8
            );

            ComputeTrailsAndCheck(graph, Edge.Create, out var trails, out var circuit);

            IEdge<char>[] expectedTrail = { edge3, edge1, edge4, edge6, edge5, edge7, edge2 };
            Assert.AreEqual(1, trails.Length);
            Assert.IsTrue(trails[0].Cast<IEdge<char>>().IsPath());
            CollectionAssert.AreEquivalent(expectedTrail, trails[0]);

            Assert.IsTrue(circuit.IsPath());
            CollectionAssert.AreEquivalent(expectedTrail, circuit);
        }

        [Test]
        public void SingleEulerianTrailGraph3()
        {
            var edge1 = Edge.Create(1, 2);
            var edge2 = Edge.Create(2, 1);
            var edge3 = Edge.Create(1, 3);
            var edge4 = Edge.Create(3, 1);
            var edge5 = Edge.Create(2, 4);
            var edge6 = Edge.Create(4, 2);
            var edge7 = Edge.Create(3, 4);
            var edge8 = Edge.Create(4, 3);
            var edge9 = Edge.Create(4, 4);

            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
                edge1, edge2, edge3, edge4, edge5, edge6, edge7, edge8, edge9
            );

            ComputeTrailsAndCheck(
                graph,
                Edge.Create,
                out ICollection<IEdge<int>>[] trails,
                out IEdge<int>[] circuit);

            IEdge<int>[] expectedTrail = { edge3, edge7, edge9, edge6, edge5, edge8, edge4, edge1, edge2 };
            Assert.AreEqual(1, trails.Length);
            Assert.IsTrue(trails[0].IsPath());
            CollectionAssert.AreEquivalent(expectedTrail, trails[0]);

            Assert.IsTrue(circuit.IsPath());
            CollectionAssert.AreEquivalent(expectedTrail, circuit);
        }

        [Test]
        public void MultipleEulerianTrailsGraph()
        {
            var edge1 = Edge.Create(1, 2);
            var edge2 = Edge.Create(2, 1);

            var edge3 = Edge.Create(1, 3);
            var edge4 = Edge.Create(3, 1);

            var edge5 = Edge.Create(4, 2);

            var edge6 = Edge.Create(3, 4);
            var edge7 = Edge.Create(4, 3);

            var edge8 = Edge.Create(4, 4);

            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
                edge1, edge2, edge3, edge4, edge5, edge6, edge7, edge8
            );

            ComputeTrailsAndCheck(graph, Edge.Create, out ICollection<IEdge<int>>[] trails, out IEdge<int>[] circuit);

            IEdge<int>[] expectedTrail1 = { edge3, edge6, edge8, edge5 };
            IEdge<int>[] expectedTrail2 = { edge7, edge4, edge1, edge2 };
            Assert.AreEqual(2, trails.Length);
            Assert.IsTrue(trails[0].IsPath());
            Assert.IsTrue(trails[1].IsPath());
            CollectionAssert.AreEquivalent(expectedTrail1, trails[0]);
            CollectionAssert.AreEquivalent(expectedTrail2, trails[1]);

            Assert.IsTrue(circuit.IsPath());
            Assert.AreEqual(
                expectedTrail1.Length + expectedTrail2.Length + 1 /* Temporary edge */,
                circuit.Length);
            foreach (var edge in expectedTrail1.Concat(expectedTrail2))
            {
                Assert.Contains(edge, circuit);
            }
            // + Temporary edge
            Assert.IsNotNull(circuit.FirstOrDefault(e => e.Source == 2 && e.Target == 4));
        }

        #endregion

        #region Rooted trails

        [Test]
        public void RootedNotEulerianTrailGraph_Throws()
        {
            var graph = TestGraphFactory.LoadGraph(GetGraphFilePath("g.10.0.graphml"));
            Assert.Throws<InvalidOperationException>(() =>
            {
                ComputeTrails(graph, graph.Vertices.First(), Edge.Create, out _, out _);
            });
        }

        [Test]
        public void SingleRootedEulerianTrailGraph()
        {
            var edge1 = Edge.Create('b', 'c');
            var edge2 = Edge.Create('f', 'a');
            var edge3 = Edge.Create('a', 'b');
            var edge4 = Edge.Create('c', 'd');
            var edge5 = Edge.Create('e', 'c');
            var edge6 = Edge.Create('d', 'e');
            var edge7 = Edge.Create('c', 'f');
            var edge8 = Edge.Create('b', 'e');

            var graph = new AdjacencyGraph<char, IEdge<char>>();
            graph.AddVerticesAndEdgeRange(
                edge1, edge2, edge3, edge4, edge5, edge6, edge7, edge8
            );

            ComputeTrails(graph, 'c', Edge.Create, out var trails, out var circuit);

            IEdge<char>[] expectedTrail = { edge4, edge6, edge5, edge7, edge2, edge3, edge1 };
            Assert.AreEqual(1, trails.Length);
            Assert.IsTrue(trails[0].Cast<IEdge<char>>().IsPath());
            CollectionAssert.AreEquivalent(expectedTrail, trails[0]);
            Assert.AreEqual('c', trails[0].ElementAt(0).Source);

            Assert.IsTrue(circuit.IsPath());
            CollectionAssert.AreEquivalent(expectedTrail, circuit);
        }

        [Test]
        public void SingleRootedEulerianTrailGraph2()
        {
            var edge1 = Edge.Create(1, 2);
            var edge2 = Edge.Create(2, 1);

            var edge3 = Edge.Create(1, 3);
            var edge4 = Edge.Create(3, 1);

            var edge5 = Edge.Create(2, 4);
            var edge6 = Edge.Create(4, 2);

            var edge7 = Edge.Create(3, 4);
            var edge8 = Edge.Create(4, 3);

            var edge9 = Edge.Create(4, 4);

            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
                edge1, edge2, edge3, edge4, edge5, edge6, edge7, edge8, edge9
            );

            ComputeTrails(
                graph,
                4,
                Edge.Create,
                out ICollection<IEdge<int>>[] trails,
                out IEdge<int>[] circuit);

            IEdge<int>[] expectedTrail = { edge9, edge6, edge5, edge8, edge4, edge1, edge2, edge3, edge7 };
            Assert.AreEqual(1, trails.Length);
            Assert.IsTrue(trails[0].IsPath());
            CollectionAssert.AreEquivalent(expectedTrail, trails[0]);
            Assert.AreEqual(4, trails[0].ElementAt(0).Source);

            Assert.IsTrue(circuit.IsPath());
            CollectionAssert.AreEquivalent(expectedTrail, circuit);
        }

        [Test]
        public void MultipleRootedEulerianTrailsGraph()
        {
            var edge1 = new EquatableEdge<int>(1, 2);
            var edge2 = new EquatableEdge<int>(2, 1);
            var edge3 = new EquatableEdge<int>(1, 3);
            var edge4 = new EquatableEdge<int>(3, 1);
            var edge5 = new EquatableEdge<int>(4, 2);
            var edge6 = new EquatableEdge<int>(3, 4);
            var edge7 = new EquatableEdge<int>(4, 3);
            var edge8 = new EquatableEdge<int>(4, 4);

            var graph = new AdjacencyGraph<int, EquatableEdge<int>>();
            graph.AddVerticesAndEdgeRange(
                edge1, edge2, edge3, edge4, edge5, edge6, edge7, edge8
            );

            // Root 2
            ComputeTrails(
                graph,
                2,
                (s, t) => new EquatableEdge<int>(s, t),
                out ICollection<EquatableEdge<int>>[] trails,
                out EquatableEdge<int>[] circuit);
            EquatableEdge<int>[] trail1 = { edge2, edge3, edge6, edge8, edge5 };
            EquatableEdge<int>[] trail2 = { new EquatableEdge<int>(2, 4), edge7, edge4, edge1 };
            CheckTrails(trails, trail1, trail2);

            Assert.IsTrue(circuit.IsPath());
            Assert.AreEqual(
                trail1.Length + trail2.Length /* Include temporary edge */,
                circuit.Length);
            foreach (EquatableEdge<int> edge in trail1.Concat(trail2))
            {
                Assert.Contains(edge, circuit);
            }

            // Root 3
            ComputeTrails(graph, 3, (s, t) => new EquatableEdge<int>(s, t),
                out trails, out circuit);
            trail1 = new[] { edge6, edge8, edge5 };
            trail2 = new[] { edge6, edge7, edge4, edge1, edge2, edge3 };
            CheckTrails(trails, trail1, trail2);

            Assert.IsTrue(circuit.IsPath());
            Assert.AreEqual(
                trail1.Concat(trail2).Distinct().Count() /* Edge present in both paths */ + 1 /* One temporary edge */,
                circuit.Length);
            foreach (EquatableEdge<int> edge in trail1.Concat(trail2))
            {
                Assert.Contains(edge, circuit);
            }
            // + Temporary edge
            Assert.IsNotNull(circuit.FirstOrDefault(e => e.Source == 2 && e.Target == 4));

            #region Local function

            void CheckTrails(
                IList<ICollection<EquatableEdge<int>>> computedTrails,
                IEnumerable<EquatableEdge<int>> expectedTrail1,
                IEnumerable<EquatableEdge<int>> expectedTrail2)
            {
                Assert.AreEqual(2, computedTrails.Count);
                Assert.IsTrue(computedTrails[0].Cast<IEdge<int>>().IsPath());
                Assert.IsTrue(computedTrails[1].Cast<IEdge<int>>().IsPath());
                CollectionAssert.AreEquivalent(expectedTrail1, computedTrails[0]);
                CollectionAssert.AreEquivalent(expectedTrail2, computedTrails[1]);
            }

            #endregion
        }

        [Test]
        public void RootedEulerianTrails_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, IEdge<TestVertex>>();
            var algorithm = new EulerianTrailAlgorithm<TestVertex, IEdge<TestVertex>>(graph);
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => algorithm.Trails(null));
        }

        #endregion
    }
}