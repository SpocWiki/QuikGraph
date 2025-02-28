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
    [TestFixture]
    internal sealed class EulerianTrailAlgorithmTests : RootedAlgorithmTestsBase
    {
        #region Test helpers

        private static void ComputeTrails<TVertex, TEdge>(
            [NotNull] IMutableVertexAndEdgeListGraph<TVertex, TEdge> graph,
            [NotNull, InstantHandle] Func<TVertex, TVertex, TEdge> edgeFactory,
            [NotNull, ItemNotNull] out ICollection<TEdge>[] trails,
            [NotNull, ItemNotNull] out TEdge[] circuit)
            where TEdge : IEdge<TVertex>
        {
            trails = [];
            circuit = [];

            int circuitCount = EulerianTrailAlgorithm<TVertex, TEdge>.ComputeEulerianPathCount(graph);
            if (circuitCount == 0)
                return;

            var algorithm = graph.CreateEulerianTrailAlgorithm();
            algorithm.AddTemporaryEdges((s, t) => edgeFactory(s, t));
            algorithm.Compute();
            trails = algorithm.Trails().ToArray();
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
                    edge => edges.Contains(edge));
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
            trails = [];
            circuit = [];

            int circuitCount = EulerianTrailAlgorithm<TVertex, TEdge>.ComputeEulerianPathCount(graph);
            if (circuitCount == 0)
                return;

            TEdge[] graphEdges = graph.Edges.ToArray();

            var algorithm = graph.CreateEulerianTrailAlgorithm();
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

            var algorithm = graph.CreateEulerianTrailAlgorithm();
            AssertAlgorithmProperties(algorithm, graph);

            algorithm = graph.CreateEulerianTrailAlgorithm(null);
            AssertAlgorithmProperties(algorithm, graph);

            #region Local function

            void AssertAlgorithmProperties<TVertex, TEdge>(
                EulerianTrailAlgorithm<TVertex, TEdge> algo,
                IMutableVertexAndEdgeListGraph<TVertex, TEdge> g)
                where TEdge : IEdge<TVertex>
            {
                algo.AssertAlgorithmState(g);
                CollectionAssert.IsEmpty(algo.Circuit);
            }

            #endregion
        }

        [Test]
        public void Constructor_Throws()
        {
            // ReSharper disable ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            IMutableVertexAndEdgeListGraph<int, IEdge<int>> nullGraph = null;
            Assert.Throws<ArgumentNullException>(
                () => nullGraph.CreateEulerianTrailAlgorithm());
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ObjectCreationAsStatement
        }

        #region Rooted algorithm

        [Test]
        public void TryGetRootVertex()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var algorithm = graph.CreateEulerianTrailAlgorithm();
            TryGetRootVertex_Test(algorithm);
        }

        [Test]
        public void SetRootVertex()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var algorithm = graph.CreateEulerianTrailAlgorithm();
            SetRootVertex_Test(algorithm);
        }

        [Test]
        public void SetRootVertex_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, Edge<TestVertex>>();
            var algorithm = graph.CreateEulerianTrailAlgorithm();
            SetRootVertex_Throws_Test(algorithm);
        }

        [Test]
        public void ClearRootVertex()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var algorithm = graph.CreateEulerianTrailAlgorithm();
            ClearRootVertex_Test(algorithm);
        }

        [Test]
        public void ComputeWithoutRoot_Throws()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            ComputeWithoutRoot_NoThrows_Test(
                graph,
                () => graph.CreateEulerianTrailAlgorithm());
        }

        [Test]
        public void ComputeWithRoot()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVertex(0);
            var algorithm = graph.CreateEulerianTrailAlgorithm();
            ComputeWithRoot_Test(algorithm);
        }

        [Test]
        public void ComputeWithRoot_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, Edge<TestVertex>>();
            ComputeWithRoot_Throws_Test(
                () => graph.CreateEulerianTrailAlgorithm());
        }

        #endregion

        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> ComputeEulerianPathCountTestCases
        {
            [UsedImplicitly]
            get
            {
                var emptyGraph = new AdjacencyGraph<int, IEdge<int>>();
                yield return new TestCaseData(emptyGraph)
                {
                    ExpectedResult = 1
                };

                var moreVerticesThanEdgesGraph = new AdjacencyGraph<int, IEdge<int>>();
                moreVerticesThanEdgesGraph.AddVertexRange([1, 2]);
                moreVerticesThanEdgesGraph.AddEdge(Edge.Create(1, 2));
                yield return new TestCaseData(moreVerticesThanEdgesGraph)
                {
                    ExpectedResult = 0
                };

                var sameVerticesAndEdgesCountGraph = new AdjacencyGraph<int, IEdge<int>>();
                sameVerticesAndEdgesCountGraph.AddVertexRange([1, 2]);
                sameVerticesAndEdgesCountGraph.AddEdgeRange(
                [
                    Edge.Create(1, 2),
                    Edge.Create(2, 1)
                ]);
                yield return new TestCaseData(sameVerticesAndEdgesCountGraph)
                {
                    ExpectedResult = 1
                };

                var sameVerticesAndEdgesCountGraph2 = new AdjacencyGraph<int, IEdge<int>>();
                sameVerticesAndEdgesCountGraph2.AddVertexRange([1, 2, 3]);
                sameVerticesAndEdgesCountGraph2.AddEdgeRange(
                [
                    Edge.Create(1, 2),
                    Edge.Create(2, 1),
                    Edge.Create(1, 3)
                ]);
                yield return new TestCaseData(sameVerticesAndEdgesCountGraph2)
                {
                    ExpectedResult = 1
                };

                var moreEdgesThanEdgesGraph = new AdjacencyGraph<int, IEdge<int>>();
                moreEdgesThanEdgesGraph.AddVertexRange([1, 2, 3, 4, 5]);
                moreEdgesThanEdgesGraph.AddEdgeRange(
                [
                    Edge.Create(1, 2),
                    Edge.Create(2, 1),
                    Edge.Create(1, 3),
                    Edge.Create(1, 4),
                    Edge.Create(3, 4),
                    Edge.Create(3, 4),
                    Edge.Create(1, 5)
                ]);
                yield return new TestCaseData(moreEdgesThanEdgesGraph)
                {
                    ExpectedResult = 2
                };
            }
        }

        [TestCaseSource(nameof(ComputeEulerianPathCountTestCases))]
        public int ComputeEulerianPathCount([NotNull] AdjacencyGraph<int, IEdge<int>> graph)
            => EulerianTrailAlgorithm<int, IEdge<int>>.ComputeEulerianPathCount(graph);

        [Test]
        public void ComputeEulerianPathCount_Throws()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => EulerianTrailAlgorithm<int, IEdge<int>>.ComputeEulerianPathCount(null));
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> AddTemporaryEdgesTestCases
        {
            [UsedImplicitly]
            get
            {
                var emptyGraph = new AdjacencyGraph<int, EquatableEdge<int>>();
                yield return new TestCaseData(emptyGraph, new EquatableEdge<int>[0]);

                var evenVerticesGraph = new AdjacencyGraph<int, EquatableEdge<int>>();
                evenVerticesGraph.AddVertexRange([1, 2, 3, 4]);
                evenVerticesGraph.AddEdgeRange(
                [
                    new EquatableEdge<int>(1, 2),
                    new EquatableEdge<int>(1, 3),
                    new EquatableEdge<int>(2, 4),
                    new EquatableEdge<int>(3, 4)
                ]);
                yield return new TestCaseData(evenVerticesGraph, new EquatableEdge<int>[0]);


                var oddVerticesGraph1 = new AdjacencyGraph<int, EquatableEdge<int>>();
                oddVerticesGraph1.AddVertexRange([1, 2, 3]);
                oddVerticesGraph1.AddEdgeRange(
                [
                    new EquatableEdge<int>(1, 2),
                    new EquatableEdge<int>(2, 1),
                    new EquatableEdge<int>(1, 3)
                ]);
                yield return new TestCaseData(
                    oddVerticesGraph1,
                    new[]
                    {
                        new EquatableEdge<int>(1, 3)
                    });


                var oddVerticesGraph2 = new AdjacencyGraph<int, EquatableEdge<int>>();
                oddVerticesGraph2.AddVertexRange([1, 2, 3, 4, 5]);
                oddVerticesGraph2.AddEdgeRange(
                [
                    new EquatableEdge<int>(1, 2),
                    new EquatableEdge<int>(2, 1),
                    new EquatableEdge<int>(1, 4),
                    new EquatableEdge<int>(3, 1),
                    new EquatableEdge<int>(1, 5)
                ]);
                yield return new TestCaseData(
                    oddVerticesGraph2,
                    new[]
                    {
                        new EquatableEdge<int>(1, 4),
                        new EquatableEdge<int>(3, 5)
                    });


                var oddVerticesGraph3 = new AdjacencyGraph<int, EquatableEdge<int>>();
                oddVerticesGraph3.AddVertexRange([1, 2, 3, 4, 5]);
                oddVerticesGraph3.AddEdgeRange(
                [
                    new EquatableEdge<int>(1, 2),
                    new EquatableEdge<int>(2, 1),
                    new EquatableEdge<int>(1, 3),
                    new EquatableEdge<int>(1, 4),
                    new EquatableEdge<int>(3, 4),
                    new EquatableEdge<int>(3, 4),
                    new EquatableEdge<int>(1, 5)
                ]);
                yield return new TestCaseData(
                    oddVerticesGraph3,
                    new[]
                    {
                        new EquatableEdge<int>(1, 3),
                        new EquatableEdge<int>(4, 5)
                    });
            }
        }

        [TestCaseSource(nameof(AddTemporaryEdgesTestCases))]
        public void AddTemporaryEdges(
            [NotNull] AdjacencyGraph<int, EquatableEdge<int>> graph,
            [NotNull, ItemNotNull] EquatableEdge<int>[] expectedTemporaryEdges)
        {
            var algorithm = graph.CreateEulerianTrailAlgorithm();
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

        [Test]
        public void AddTemporaryEdges_Throws()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var algorithm = graph.CreateEulerianTrailAlgorithm();

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
                evenVerticesGraph.AddVertexRange([1, 2, 3, 4]);
                evenVerticesGraph.AddEdgeRange(
                [
                    new EquatableEdge<int>(1, 2),
                    new EquatableEdge<int>(1, 3),
                    new EquatableEdge<int>(2, 4),
                    new EquatableEdge<int>(3, 4)
                ]);
                yield return new TestCaseData(evenVerticesGraph);


                var oddVerticesGraph1 = new AdjacencyGraph<int, EquatableEdge<int>>();
                oddVerticesGraph1.AddVertexRange([1, 2, 3]);
                oddVerticesGraph1.AddEdgeRange(
                [
                    new EquatableEdge<int>(1, 2),
                    new EquatableEdge<int>(2, 1),
                    new EquatableEdge<int>(1, 3)
                ]);
                yield return new TestCaseData(oddVerticesGraph1);


                var oddVerticesGraph2 = new AdjacencyGraph<int, EquatableEdge<int>>();
                oddVerticesGraph2.AddVertexRange([1, 2, 3, 4, 5]);
                oddVerticesGraph2.AddEdgeRange(
                [
                    new EquatableEdge<int>(1, 2),
                    new EquatableEdge<int>(2, 1),
                    new EquatableEdge<int>(1, 4),
                    new EquatableEdge<int>(3, 1),
                    new EquatableEdge<int>(1, 5)
                ]);
                yield return new TestCaseData(oddVerticesGraph2);
            }
        }

        [TestCaseSource(nameof(RemoveTemporaryEdgesTestCases))]
        public void RemoveTemporaryEdges([NotNull] AdjacencyGraph<int, EquatableEdge<int>> graph)
        {
            var algorithm = graph.CreateEulerianTrailAlgorithm();
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
            AdjacencyGraph<string, Edge<string>> graph = TestGraphFactory.LoadGraph(GetGraphFilePath("g.42.34.graphml"));
            // No trails in tests graphs there
            ComputeTrails(
                graph,
                (s, t) => new Edge<string>(s, t),
                out ICollection<Edge<string>>[] trails,
                out Edge<string>[] circuit);
            CollectionAssert.IsEmpty(trails);
            CollectionAssert.IsEmpty(circuit);
        }

        [Test]
        public void SingleEulerianTrailGraph()
        {
            var edge1 = new Edge<char>('b', 'c');
            var edge2 = new Edge<char>('f', 'a');
            var edge3 = new Edge<char>('a', 'b');
            var edge4 = new Edge<char>('c', 'd');
            var edge5 = new Edge<char>('e', 'c');
            var edge6 = new Edge<char>('d', 'e');
            var edge7 = new Edge<char>('c', 'f');

            var graph = new AdjacencyGraph<char, Edge<char>>();
            graph.AddVerticesAndEdgeRange(
            [
                edge1, edge2, edge3, edge4, edge5, edge6, edge7
            ]);

            ComputeTrails(
                graph,
                (s, t) => new Edge<char>(s, t),
                out ICollection<Edge<char>>[] trails,
                out Edge<char>[] circuit);

            Edge<char>[] expectedTrail = [edge3, edge1, edge4, edge6, edge5, edge7, edge2];
            Assert.AreEqual(1, trails.Length);
            Assert.IsTrue(trails[0].IsPath<char, Edge<char>>());
            CollectionAssert.AreEquivalent(expectedTrail, trails[0]);

            Assert.IsTrue(circuit.IsPath<char, Edge<char>>());
            CollectionAssert.AreEquivalent(expectedTrail, circuit);
        }

        [Test]
        public void SingleEulerianTrailGraph2()
        {
            var edge1 = new Edge<char>('b', 'c');
            var edge2 = new Edge<char>('f', 'a');
            var edge3 = new Edge<char>('a', 'b');
            var edge4 = new Edge<char>('c', 'd');
            var edge5 = new Edge<char>('e', 'c');
            var edge6 = new Edge<char>('d', 'e');
            var edge7 = new Edge<char>('c', 'f');
            var edge8 = new Edge<char>('b', 'e');

            var graph = new AdjacencyGraph<char, Edge<char>>();
            graph.AddVerticesAndEdgeRange(
            [
                edge1, edge2, edge3, edge4, edge5, edge6, edge7, edge8
            ]);

            ComputeTrails(
                graph,
                (s, t) => new Edge<char>(s, t),
                out ICollection<Edge<char>>[] trails,
                out Edge<char>[] circuit);

            Edge<char>[] expectedTrail = [edge3, edge1, edge4, edge6, edge5, edge7, edge2];
            Assert.AreEqual(1, trails.Length);
            Assert.IsTrue(trails[0].IsPath<char, Edge<char>>());
            CollectionAssert.AreEquivalent(expectedTrail, trails[0]);

            Assert.IsTrue(circuit.IsPath<char, Edge<char>>());
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
            [
                edge1, edge2, edge3, edge4, edge5, edge6, edge7, edge8, edge9
            ]);

            ComputeTrails(
                graph,
                (s, t) => Edge.Create(s, t),
                out ICollection<IEdge<int>>[] trails,
                out IEdge<int>[] circuit);

            IEdge<int>[] expectedTrail = [edge3, edge7, edge9, edge6, edge5, edge8, edge4, edge1, edge2];
            Assert.AreEqual(1, trails.Length);
            Assert.IsTrue(trails[0].IsPath<int, IEdge<int>>());
            CollectionAssert.AreEquivalent(expectedTrail, trails[0]);

            Assert.IsTrue(circuit.IsPath<int, IEdge<int>>());
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
            [
                edge1, edge2, edge3, edge4, edge5, edge6, edge7, edge8
            ]);

            ComputeTrails(
                graph,
                (s, t) => Edge.Create(s, t),
                out ICollection<IEdge<int>>[] trails,
                out IEdge<int>[] circuit);

            IEdge<int>[] expectedTrail1 = [edge3, edge6, edge8, edge5];
            IEdge<int>[] expectedTrail2 = [edge7, edge4, edge1, edge2];
            Assert.AreEqual(2, trails.Length);
            Assert.IsTrue(trails[0].IsPath<int, IEdge<int>>());
            Assert.IsTrue(trails[1].IsPath<int, IEdge<int>>());
            CollectionAssert.AreEquivalent(expectedTrail1, trails[0]);
            CollectionAssert.AreEquivalent(expectedTrail2, trails[1]);

            Assert.IsTrue(circuit.IsPath<int, IEdge<int>>());
            Assert.AreEqual(
                expectedTrail1.Length + expectedTrail2.Length + 1 /* Temporary edge */,
                circuit.Length);
            foreach (Edge<int> edge in expectedTrail1.Concat(expectedTrail2))
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
            AdjacencyGraph<string, Edge<string>> graph = TestGraphFactory.LoadGraph(GetGraphFilePath("g.10.0.graphml"));
            Assert.Throws<InvalidOperationException>(() =>
            {
                ComputeTrails(
                    graph,
                    graph.Vertices.First(),
                    (s, t) => new Edge<string>(s, t),
                    out _,
                    out _);
            });
        }

        [Test]
        public void SingleRootedEulerianTrailGraph()
        {
            var edge1 = new Edge<char>('b', 'c');
            var edge2 = new Edge<char>('f', 'a');
            var edge3 = new Edge<char>('a', 'b');
            var edge4 = new Edge<char>('c', 'd');
            var edge5 = new Edge<char>('e', 'c');
            var edge6 = new Edge<char>('d', 'e');
            var edge7 = new Edge<char>('c', 'f');
            var edge8 = new Edge<char>('b', 'e');

            var graph = new AdjacencyGraph<char, Edge<char>>();
            graph.AddVerticesAndEdgeRange(
            [
                edge1, edge2, edge3, edge4, edge5, edge6, edge7, edge8
            ]);

            ComputeTrails(
                graph,
                'c',
                (s, t) => new Edge<char>(s, t),
                out ICollection<Edge<char>>[] trails,
                out Edge<char>[] circuit);

            Edge<char>[] expectedTrail = [edge4, edge6, edge5, edge7, edge2, edge3, edge1];
            Assert.AreEqual(1, trails.Length);
            Assert.IsTrue(trails[0].IsPath<char, Edge<char>>());
            CollectionAssert.AreEquivalent(expectedTrail, trails[0]);
            Assert.AreEqual('c', trails[0].ElementAt(0).Source);

            Assert.IsTrue(circuit.IsPath<char, Edge<char>>());
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
            [
                edge1, edge2, edge3, edge4, edge5, edge6, edge7, edge8, edge9
            ]);

            ComputeTrails(
                graph,
                4,
                Edge.Create,
                out ICollection<IEdge<int>>[] trails,
                out IEdge<int>[] circuit);

            IEdge<int>[] expectedTrail = [edge9, edge6, edge5, edge8, edge4, edge1, edge2, edge3, edge7];
            Assert.AreEqual(1, trails.Length);
            Assert.IsTrue(trails[0].IsPath<int, IEdge<int>>());
            CollectionAssert.AreEquivalent(expectedTrail, trails[0]);
            Assert.AreEqual(4, trails[0].ElementAt(0).Source);

            Assert.IsTrue(circuit.IsPath<int, IEdge<int>>());
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
            [
                edge1, edge2, edge3, edge4, edge5, edge6, edge7, edge8
            ]);

            // Root 2
            ComputeTrails(
                graph,
                2,
                (s, t) => new EquatableEdge<int>(s, t),
                out ICollection<EquatableEdge<int>>[] trails,
                out EquatableEdge<int>[] circuit);
            EquatableEdge<int>[] trail1 = [edge2, edge3, edge6, edge8, edge5];
            EquatableEdge<int>[] trail2 = [new EquatableEdge<int>(2, 4), edge7, edge4, edge1];
            CheckTrails(trails, trail1, trail2);

            Assert.IsTrue(circuit.IsPath<int, EquatableEdge<int>>());
            Assert.AreEqual(
                trail1.Length + trail2.Length /* Include temporary edge */,
                circuit.Length);
            foreach (EquatableEdge<int> edge in trail1.Concat(trail2))
            {
                Assert.Contains(edge, circuit);
            }

            // Root 3
            ComputeTrails(
                graph,
                3,
                (s, t) => new EquatableEdge<int>(s, t),
                out trails,
                out circuit);
            trail1 = [edge6, edge8, edge5];
            trail2 = [edge6, edge7, edge4, edge1, edge2, edge3];
            CheckTrails(trails, trail1, trail2);

            Assert.IsTrue(circuit.IsPath<int, EquatableEdge<int>>());
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
                Assert.IsTrue(computedTrails[0].IsPath<int, EquatableEdge<int>>());
                Assert.IsTrue(computedTrails[1].IsPath<int, EquatableEdge<int>>());
                CollectionAssert.AreEquivalent(expectedTrail1, computedTrails[0]);
                CollectionAssert.AreEquivalent(expectedTrail2, computedTrails[1]);
            }

            #endregion
        }

        [Test]
        public void RootedEulerianTrails_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, Edge<TestVertex>>();
            var algorithm = graph.CreateEulerianTrailAlgorithm();
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => algorithm.Trails(null));
        }

        #endregion
    }
}