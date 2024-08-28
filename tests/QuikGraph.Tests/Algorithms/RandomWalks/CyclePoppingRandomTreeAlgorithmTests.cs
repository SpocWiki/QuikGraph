using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using QuikGraph.Algorithms.RandomWalks;
using QuikGraph.Algorithms.Search;


namespace QuikGraph.Tests.Algorithms.RandomWalks
{
    /// <summary>
    /// Tests for <see cref="CyclePoppingRandomTreeAlgorithm{TVertex,TEdge}"/>.
    /// </summary>
    [TestFixture]
    internal sealed class CyclePoppingRandomTreeAlgorithmTests : RootedAlgorithmTestsBase
    {
        #region Test helpers

        private static void RunCyclePoppingRandomTreeAndCheck<TVertex, TEdge>(
            [NotNull] IVertexListGraph<TVertex, TEdge> graph,
            [NotNull] TVertex root)
            where TEdge : class, IEdge<TVertex>
        {
            var randomChain = new Random(123456);
            var chain = new NormalizedMarkovEdgeChain<TVertex, TEdge>
            {
                Rand = randomChain
            };
            var randomAlgorithm = new Random(123456);
            var algorithm = new CyclePoppingRandomTreeAlgorithm<TVertex, TEdge>(graph, chain)
            {
                Rand = randomAlgorithm
            };
            algorithm.InitializeVertex += vertex =>
            {
                Assert.AreEqual(GraphColor.White, algorithm.VerticesColors[vertex]);
            };

            algorithm.FinishVertex += vertex =>
            {
                Assert.AreEqual(GraphColor.Black, algorithm.VerticesColors[vertex]);
            };

            algorithm.Compute(root);

            Assert.AreEqual(graph.VertexCount, algorithm.VerticesColors.Count);
            foreach (TVertex vertex in graph.Vertices)
            {
                Assert.AreEqual(GraphColor.Black, algorithm.VerticesColors[vertex]);
            }

            AssertIsTree(root, algorithm.Successors);
        }

        [Pure]
        [NotNull]
        private static IVertexListGraph<TVertex, TEdge> MakeGraph<TVertex, TEdge>(
            [NotNull] TVertex root,
            [NotNull] IDictionary<TVertex, TEdge> successors)
            where TEdge : class, IEdge<TVertex>
        {
            var graph = new AdjacencyGraph<TVertex, TEdge>();
            graph.AddVerticesAndEdgeRange(
                successors
                    .Where(pair => !Equals(pair.Key, root))
                    .Select(pair => pair.Value)
                    .Where(edge => edge != null));
            return graph;
        }

        private static void AssertIsTree<TVertex, TEdge>(
            [NotNull] TVertex root,
            [NotNull] IDictionary<TVertex, TEdge> successors)
            where TEdge : class, IEdge<TVertex>
        {
            IVertexListGraph<TVertex, TEdge> graph = MakeGraph(root, successors);

            var dfs = new DepthFirstSearchAlgorithm<TVertex, TEdge>(graph);
            dfs.BackEdge += _ => Assert.Fail("Random constructed tree contains a cycle.");
            dfs.Compute();
        }

        #endregion

        [Test]
        public void Constructor()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            IMarkovEdgeChain<int, IEdge<int>> markovChain1 = new NormalizedMarkovEdgeChain<int, IEdge<int>>();
            IMarkovEdgeChain<int, IEdge<int>> markovChain2 = new WeightedMarkovEdgeChain<int, IEdge<int>>(new Dictionary<IEdge<int>, double>());

            var algorithm = new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(graph);
            AssertAlgorithmProperties(algorithm, graph);

            algorithm = new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(graph, markovChain1);
            AssertAlgorithmProperties(algorithm, graph, markovChain1);

            algorithm = new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(graph, markovChain2);
            AssertAlgorithmProperties(algorithm, graph, markovChain2);

            algorithm = new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(null, graph, markovChain1);
            AssertAlgorithmProperties(algorithm, graph, markovChain1);

            var random = new Random(123456);
            algorithm.Rand = random;
            AssertAlgorithmProperties(algorithm, graph, markovChain1, random);

            #region Local function

            void AssertAlgorithmProperties<TVertex, TEdge>(
                CyclePoppingRandomTreeAlgorithm<TVertex, TEdge> algo,
                IVertexListGraph<TVertex, TEdge> g,
                IMarkovEdgeChain<TVertex, TEdge> chain = null,
                Random rand = null)
                where TEdge : IEdge<TVertex>
            {
                algo.AssertAlgorithmState(g);
                if (chain is null)
                    Assert.IsNotNull(algo.EdgeChain);
                else
                    Assert.AreSame(chain, algo.EdgeChain);
                if (rand is null)
                    Assert.IsNotNull(algo.Rand);
                else
                    Assert.AreSame(rand, algo.Rand);
                CollectionAssert.IsEmpty(algo.Successors);
                CollectionAssert.IsEmpty(algo.VerticesColors);
            }

            #endregion
        }

        [Test]
        public void Constructor_Throws()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var chain = new NormalizedMarkovEdgeChain<int, IEdge<int>>();

            // ReSharper disable ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(null));

            Assert.Throws<ArgumentNullException>(
                () => new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(graph, null));
            Assert.Throws<ArgumentNullException>(
                () => new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(null, chain));
            Assert.Throws<ArgumentNullException>(
                () => new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(null, null));

            Assert.Throws<ArgumentNullException>(
                () => new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(null, graph, null));
            Assert.Throws<ArgumentNullException>(
                () => new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(null, null, chain));
            Assert.Throws<ArgumentNullException>(
                () => new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(null, null, null));

            var algorithm = new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(graph, chain);
            Assert.Throws<ArgumentNullException>(() => algorithm.Rand = null);
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ObjectCreationAsStatement
        }

        #region Rooted algorithm

        [Test]
        public void TryGetRootVertex()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var chain = new NormalizedMarkovEdgeChain<int, IEdge<int>>();
            var algorithm = new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(graph, chain);
            TryGetRootVertex_Test(algorithm);
        }

        [Test]
        public void SetRootVertex()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var chain = new NormalizedMarkovEdgeChain<int, IEdge<int>>();
            var algorithm = new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(graph, chain);
            SetRootVertex_Test(algorithm);
        }

        [Test]
        public void SetRootVertex_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, IEdge<TestVertex>>();
            var chain = new NormalizedMarkovEdgeChain<TestVertex, IEdge<TestVertex>>();
            var algorithm = new CyclePoppingRandomTreeAlgorithm<TestVertex, IEdge<TestVertex>>(graph, chain);
            SetRootVertex_Null_Should_Throw_ArgumentNullException(algorithm);
        }

        [Test]
        public void ClearRootVertex()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var chain = new NormalizedMarkovEdgeChain<int, IEdge<int>>();
            var algorithm = new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(graph, chain);
            ClearRootVertex_RaisesVertexChanged_Test(algorithm);
        }

        [Test]
        public void ComputeWithoutRoot_Throws()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var chain = new NormalizedMarkovEdgeChain<int, IEdge<int>>();
            ComputeWithoutRoot_Throws_Test(
                () => new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(graph, chain));
        }

        [Test]
        public void ComputeWithRoot()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            var chain = new NormalizedMarkovEdgeChain<int, IEdge<int>>();
            graph.AddVertex(0);
            var algorithm = new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(graph, chain);
            ComputeWithRoot_Test(algorithm);
        }

        [Test]
        public void ComputeWithRoot_Throws()
        {
            var graph = new AdjacencyGraph<TestVertex, IEdge<TestVertex>>();
            var chain = new NormalizedMarkovEdgeChain<TestVertex, IEdge<TestVertex>>();
            ComputeWithUnknownRootOrNull_Throws_Test(
                () => new CyclePoppingRandomTreeAlgorithm<TestVertex, IEdge<TestVertex>>(graph, chain));
        }

        #endregion

        [Test]
        public void GetVertexColor()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdge(Edge.Create(1, 2));
            var chain = new NormalizedMarkovEdgeChain<int, IEdge<int>>();

            var algorithm = new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(graph, chain);
            algorithm.Compute(1);

            Assert.AreEqual(GraphColor.Black, algorithm.GetVertexColor(1));
            Assert.AreEqual(GraphColor.Black, algorithm.GetVertexColor(2));
        }

        [Test]
        public void Repro13160()
        {
            // Create a new graph
            var graph = new BidirectionalGraph<int, IEdge<int>>(false);

            // Adding vertices
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                    graph.AddVertex(i * 3 + j);
            }

            // Adding Width edges
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    graph.AddEdge(
                        Edge.Create(i * 3 + j, i * 3 + j + 1));
                }
            }

            // Adding Length edges
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    graph.AddEdge(
                        Edge.Create(i * 3 + j, (i + 1) * 3 + j));
                }
            }

            // Create cross edges 
            foreach (var edge in graph.Edges)
            {
                graph.AddEdge(Edge.Create(edge.Target, edge.Source));
            }

            // Breaking graph apart
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    if (i == 1)
                        graph.RemoveVertex(i * 3 + j);
                }
            }

            var randomChain = new Random(123456);
            var chain = new NormalizedMarkovEdgeChain<int, IEdge<int>>
            {
                Rand = randomChain
            };
            var randomAlgorithm = new Random(123456);
            var algorithm = new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(graph, chain)
            {
                Rand = randomAlgorithm
            };

            Assert.DoesNotThrow(() => algorithm.Compute(2));
            // Successors is not a spanning tree...
        }

        [Test]
        public void SmallGraphWithCycles()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>();
            graph.AddVerticesAndEdgeRange(
                Edge.Create(0, 1),
                Edge.Create(1, 0),
                Edge.Create(1, 2),
                Edge.Create(2, 1)
            );

            RunCyclePoppingRandomTreeAndCheck(graph, 0);
            RunCyclePoppingRandomTreeAndCheck(graph, 1);
            // Not all root fit, consider using RandomTree rather than rooted one
            //RunCyclePoppingRandomTreeAndCheck(graph, 2);
        }

        [Test]
        public void GraphWithCycles()
        {
            var graph = new AdjacencyGraph<char, IEdge<char>>(true);
            graph.AddVertexRange("12345");
            var edges = new[]
            {
                Edge.Create('1', '2'),
                Edge.Create('1', '3'),
                Edge.Create('1', '4'),
                Edge.Create('2', '1'),
                Edge.Create('2', '3'),
                Edge.Create('2', '4'),
                Edge.Create('3', '1'),
                Edge.Create('3', '2'),
                Edge.Create('3', '5'),
                Edge.Create('5', '2')
            };
            graph.AddEdgeRange(edges);

            foreach (char vertex in graph.Vertices)
            {
                RunCyclePoppingRandomTreeAndCheck(graph, vertex);
            }
        }

        [TestCaseSource(typeof(TestGraphFactory), nameof(TestGraphFactory.GetAdjacencyGraphs_SlowTests), new object[] { 10 })]
        [Category(TestCategories.LongRunning)]
        public void CyclePoppingRandomTree(AdjacencyGraph<string, IEdge<string>> graph)
        {
            foreach (string root in graph.Vertices)
            {
                RunCyclePoppingRandomTreeAndCheck(graph, root);
            }
        }

        [Test]
        public void IsolatedVertices()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>(true);
            graph.AddVertex(0);
            graph.AddVertex(1);

            var algorithm = new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(graph);
            algorithm.RandomTree();
            AssertIsTree(0, algorithm.Successors);
            AssertIsTree(1, algorithm.Successors);
        }

        [Test]
        public void IsolatedVerticesWithRoot()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>(true);
            graph.AddVertex(0);
            graph.AddVertex(1);

            var algorithm = new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(graph);
            algorithm.RandomTreeWithRoot(0);
            AssertIsTree(0, algorithm.Successors);
        }

        [Test]
        public void RootIsNotAccessible()
        {
            var graph = new AdjacencyGraph<int, IEdge<int>>(true);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(Edge.Create(0, 1));

            var algorithm = new CyclePoppingRandomTreeAlgorithm<int, IEdge<int>>(graph);
            algorithm.RandomTreeWithRoot(0);
            AssertIsTree(0, algorithm.Successors);
        }

        [Pure]
        [NotNull]
        public static CyclePoppingRandomTreeAlgorithm<T, IEdge<T>> CreateAlgorithmAndMaybeDoComputation<T>(
            [NotNull] ContractScenario<T> scenario)
        {
            var graph = new AdjacencyGraph<T, IEdge<T>>();
            graph.AddVerticesAndEdgeRange(scenario.EdgesInGraph.Select(e => Edge.Create(e.Source, e.Target)));
            graph.AddVertexRange(scenario.SingleVerticesInGraph);
            var chain = new NormalizedMarkovEdgeChain<T, IEdge<T>>();

            var algorithm = new CyclePoppingRandomTreeAlgorithm<T, IEdge<T>>(graph, chain);

            if (scenario.DoComputation)
                algorithm.Compute(scenario.Root);
            return algorithm;
        }
    }
}