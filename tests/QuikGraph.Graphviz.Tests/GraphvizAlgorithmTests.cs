﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using NUnit.Framework;
using QuikGraph.Graphviz.Dot;

namespace QuikGraph.Graphviz.Tests
{
    /// <summary>
    /// Tests related to <see cref="GraphvizAlgorithm{TVertex,TEdge}"/>.
    /// </summary>
    [TestFixture]
    internal class GraphvizAlgorithmTests
    {
        #region Test classes

        private class TestDotEngine : IDotEngine
        {
            public string ExpectedDot { get; set; }

            /// <inheritdoc />
            public string Run(GraphvizImageType imageType, string dot, string outputFilePath)
            {
                Assert.AreEqual(ExpectedDot, dot);
                return outputFilePath;
            }
        }

        #endregion

        [Test]
        public void Constructor()
        {
            var graph = new AdjacencyGraph<int, Edge<int>>();
            var otherGraph = new AdjacencyGraph<int, Edge<int>>();
            var algorithm = new GraphvizAlgorithm<int, Edge<int>>(graph);
            AssertAlgorithmProperties(algorithm, graph);

            algorithm = new GraphvizAlgorithm<int, Edge<int>>(graph, GraphvizImageType.Fig);
            AssertAlgorithmProperties(algorithm, graph, GraphvizImageType.Fig);

            algorithm = new GraphvizAlgorithm<int, Edge<int>>(graph, GraphvizImageType.Ps);
            AssertAlgorithmProperties(algorithm, graph, GraphvizImageType.Ps);

            algorithm = new GraphvizAlgorithm<int, Edge<int>>(graph, GraphvizImageType.Hpgl);
            algorithm.ImageType = GraphvizImageType.Gd;
            AssertAlgorithmProperties(algorithm, graph, GraphvizImageType.Gd);

            algorithm = new GraphvizAlgorithm<int, Edge<int>>(graph);
            algorithm.VisitedGraph = otherGraph;
            AssertAlgorithmProperties(algorithm, otherGraph);

            #region Local function

            void AssertAlgorithmProperties<TVertex, TEdge>(
                GraphvizAlgorithm<TVertex, TEdge> algo,
                IEdgeListGraph<TVertex, TEdge> treatedGraph,
                GraphvizImageType imageType = GraphvizImageType.Png)
                where TEdge : IEdge<TVertex>
            {
                Assert.AreSame(treatedGraph, algo.VisitedGraph);
                Assert.IsNotNull(algo.GraphFormat);
                Assert.IsNotNull(algo.CommonVertexFormat);
                Assert.IsNotNull(algo.CommonEdgeFormat);
                Assert.AreEqual(imageType, algo.ImageType);
                Assert.IsNull(algo.Output);
            }

            #endregion
        }

        [Test]
        public void Constructor_Throws()
        {
            // ReSharper disable ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new GraphvizAlgorithm<int, Edge<int>>(null));
            Assert.Throws<ArgumentNullException>(() => new GraphvizAlgorithm<int, Edge<int>>(null, GraphvizImageType.Gif));

            var graph = new AdjacencyGraph<int, Edge<int>>();
            var algorithm = new GraphvizAlgorithm<int, Edge<int>>(graph);
            Assert.Throws<ArgumentNullException>(() => algorithm.VisitedGraph = null);
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ObjectCreationAsStatement
        }

        [Test]
        [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
        public void FormatHandlers()
        {
            var graph = new AdjacencyGraph<int, Edge<int>>();
            var algorithm = new GraphvizAlgorithm<int, Edge<int>>(graph);
            algorithm.FormatVertex += NoVertexOnFormatVertex;
            algorithm.FormatEdge += NoEdgeOnFormatEdge;
            algorithm.FormatCluster += NoClusterOnFormatCluster;

            // Empty graph
            algorithm.Generate();

            // Only vertices
            graph.AddVertexRange(new[] { 1, 2 });
            algorithm = new GraphvizAlgorithm<int, Edge<int>>(graph);
            List<int> notFormattedVertices = RegisterOnFormatVertex(algorithm, graph.Vertices);
            algorithm.FormatEdge += NoEdgeOnFormatEdge;
            algorithm.FormatCluster += NoClusterOnFormatCluster;

            algorithm.Generate();

            CollectionAssert.IsEmpty(notFormattedVertices);

            // With edges
            graph.AddVerticesAndEdgeRange(new[]
            {
                new Edge<int>(1, 2),
                new Edge<int>(2, 3),
                new Edge<int>(3, 1)
            });
            algorithm = new GraphvizAlgorithm<int, Edge<int>>(graph);
            notFormattedVertices = RegisterOnFormatVertex(algorithm, graph.Vertices);
            List<Edge<int>> notFormattedEdges = RegisterOnFormatEdge(algorithm, graph.Edges);
            algorithm.FormatCluster += NoClusterOnFormatCluster;

            algorithm.Generate();

            CollectionAssert.IsEmpty(notFormattedVertices);
            CollectionAssert.IsEmpty(notFormattedEdges);

            // With no cluster
            var clusteredGraph = new ClusteredAdjacencyGraph<int, Edge<int>>(graph);
            algorithm = new GraphvizAlgorithm<int, Edge<int>>(clusteredGraph);
            notFormattedVertices = RegisterOnFormatVertex(algorithm, clusteredGraph.Vertices);
            notFormattedEdges = RegisterOnFormatEdge(algorithm, clusteredGraph.Edges);
            algorithm.FormatCluster += NoClusterOnFormatCluster;

            algorithm.Generate();

            CollectionAssert.IsEmpty(notFormattedVertices);
            CollectionAssert.IsEmpty(notFormattedEdges);

            // With clusters
            ClusteredAdjacencyGraph<int, Edge<int>> subGraph1 = clusteredGraph.AddCluster();
            subGraph1.AddVertexRange(new[] { 4, 5 });
            ClusteredAdjacencyGraph<int, Edge<int>> subGraph2 = clusteredGraph.AddCluster();
            subGraph2.AddVerticesAndEdge(new Edge<int>(1, 6));
            algorithm = new GraphvizAlgorithm<int, Edge<int>>(clusteredGraph);
            notFormattedVertices = RegisterOnFormatVertex(algorithm, clusteredGraph.Vertices);
            notFormattedEdges = RegisterOnFormatEdge(algorithm, clusteredGraph.Edges);
            List<IVertexAndEdgeListGraph<int, Edge<int>>> notFormattedClusters = RegisterOnFormatCluster(
                algorithm,
                new[] { subGraph1, subGraph2 });

            algorithm.Generate();

            CollectionAssert.IsEmpty(notFormattedVertices);
            CollectionAssert.IsEmpty(notFormattedEdges);
            CollectionAssert.IsEmpty(notFormattedClusters);

            #region Local functions

            void NoVertexOnFormatVertex(object sender, FormatVertexEventArgs<int> args)
            {
                Assert.Fail($"{nameof(GraphvizAlgorithm<object, Edge<object>>.FormatVertex)} called while no vertex in graph.");
            }

            List<TVertex> RegisterOnFormatVertex<TVertex, TEdge>(GraphvizAlgorithm<TVertex, TEdge> algo, IEnumerable<TVertex> vertices)
                where TEdge : IEdge<TVertex>
            {
                var verticesList = new List<TVertex>(vertices);
                algo.FormatVertex += (sender, args) =>
                {
                    Assert.IsTrue(verticesList.Remove(args.Vertex));
                };
                return verticesList;
            }

            void NoEdgeOnFormatEdge(object sender, FormatEdgeEventArgs<int, Edge<int>> args)
            {
                Assert.Fail($"{nameof(GraphvizAlgorithm<object, Edge<object>>.FormatEdge)} called while no edge in graph.");
            }

            List<TEdge> RegisterOnFormatEdge<TVertex, TEdge>(GraphvizAlgorithm<TVertex, TEdge> algo, IEnumerable<TEdge> edges)
                where TEdge : IEdge<TVertex>
            {
                var edgeList = new List<TEdge>(edges);
                algo.FormatEdge += (sender, args) =>
                {
                    Assert.IsTrue(edgeList.Remove(args.Edge));
                };
                return edgeList;
            }

            void NoClusterOnFormatCluster(object sender, FormatClusterEventArgs<int, Edge<int>> args)
            {
                Assert.Fail($"{nameof(GraphvizAlgorithm<object, Edge<object>>.FormatCluster)} called while no cluster in graph.");
            }

            List<IVertexAndEdgeListGraph<TVertex, TEdge>> RegisterOnFormatCluster<TVertex, TEdge>(GraphvizAlgorithm<TVertex, TEdge> algo, IEnumerable<IVertexAndEdgeListGraph<TVertex, TEdge>> clusters)
                where TEdge : IEdge<TVertex>
            {
                var clusterList = new List<IVertexAndEdgeListGraph<TVertex, TEdge>>(clusters);
                algo.FormatCluster += (sender, args) =>
                {
                    Assert.IsTrue(clusterList.Remove(args.Cluster));
                };
                return clusterList;
            }

            #endregion
        }

        [Test]
        public void GenerateSameDot()
        {
            var graph = new AdjacencyGraph<int, Edge<int>>();

            // Empty graph
            TestGenerate(graph);

            // Only vertices
            graph.AddVertexRange(new[] { 1, 2 });
            TestGenerate(graph);

            // With edges
            graph.AddVerticesAndEdgeRange(new[]
            {
                new Edge<int>(1, 2),
                new Edge<int>(2, 3),
                new Edge<int>(3, 1)
            });
            TestGenerate(graph);

            // With no cluster
            var clusteredGraph = new ClusteredAdjacencyGraph<int, Edge<int>>(graph);
            TestGenerate(clusteredGraph);

            // With clusters
            ClusteredAdjacencyGraph<int, Edge<int>> subGraph1 = clusteredGraph.AddCluster();
            subGraph1.AddVertexRange(new[] { 4, 5 });
            ClusteredAdjacencyGraph<int, Edge<int>> subGraph2 = clusteredGraph.AddCluster();
            subGraph2.AddVerticesAndEdge(new Edge<int>(1, 6));
            TestGenerate(clusteredGraph);

            #region Local function

            void TestGenerate<TVertex, TEdge>(IEdgeListGraph<TVertex, TEdge> g)
                where TEdge : IEdge<TVertex>
            {
                var algorithm = new GraphvizAlgorithm<TVertex, TEdge>(g);
                string generatedDot = algorithm.Generate();
                Assert.IsNotEmpty(generatedDot);

                var dotEngine = new TestDotEngine { ExpectedDot = generatedDot };
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                algorithm.Generate(dotEngine, "NotSaved.dot");
            }

            #endregion
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<TestCaseData> GenerateTestCases
        {
            [UsedImplicitly]
            get
            {
                // Empty graphs
                var graph = new AdjacencyGraph<int, Edge<int>>();
                yield return new TestCaseData(graph)
                {
                    ExpectedResult = 
                        "digraph G {" + Environment.NewLine
                        + "}"
                };

                var undirectedGraph = new UndirectedGraph<int, Edge<int>>();
                yield return new TestCaseData(undirectedGraph)
                {
                    ExpectedResult =
                        "graph G {" + Environment.NewLine
                        + "}"
                };

                // Only vertices
                graph = new AdjacencyGraph<int, Edge<int>>();
                graph.AddVertexRange(new[] { 1, 2, 3 });
                yield return new TestCaseData(graph)
                {
                    ExpectedResult =
                        "digraph G {" + Environment.NewLine
                        + "0;" + Environment.NewLine
                        + "1;" + Environment.NewLine
                        + "2;" + Environment.NewLine
                        + "}"
                };

                undirectedGraph = new UndirectedGraph<int, Edge<int>>();
                undirectedGraph.AddVertexRange(new[] { 1, 2, 3 });
                yield return new TestCaseData(undirectedGraph)
                {
                    ExpectedResult =
                        "graph G {" + Environment.NewLine
                        + "0;" + Environment.NewLine
                        + "1;" + Environment.NewLine
                        + "2;" + Environment.NewLine
                        + "}"
                };

                // With edges
                graph = new AdjacencyGraph<int, Edge<int>>();
                graph.AddVerticesAndEdgeRange(new[]
                {
                    new Edge<int>(1, 2),
                    new Edge<int>(2, 3),
                    new Edge<int>(3, 1)
                });
                yield return new TestCaseData(graph)
                {
                    ExpectedResult =
                        "digraph G {" + Environment.NewLine
                        + "0;" + Environment.NewLine
                        + "1;" + Environment.NewLine
                        + "2;" + Environment.NewLine
                        + "0 -> 1;" + Environment.NewLine
                        + "1 -> 2;" + Environment.NewLine
                        + "2 -> 0;" + Environment.NewLine
                        + "}"
                };

                undirectedGraph = new UndirectedGraph<int, Edge<int>>();
                undirectedGraph.AddVerticesAndEdgeRange(new[]
                {
                    new Edge<int>(1, 2),
                    new Edge<int>(2, 3),
                    new Edge<int>(3, 1)
                });
                yield return new TestCaseData(undirectedGraph)
                {
                    ExpectedResult =
                        "graph G {" + Environment.NewLine
                        + "0;" + Environment.NewLine
                        + "1;" + Environment.NewLine
                        + "2;" + Environment.NewLine
                        + "0 -- 1;" + Environment.NewLine
                        + "1 -- 2;" + Environment.NewLine
                        + "2 -- 0;" + Environment.NewLine
                        + "}"
                };

                // With no cluster
                var wrappedGraph = new AdjacencyGraph<int, Edge<int>>();
                wrappedGraph.AddVertexRange(new[] { 1, 2 });
                var clusteredGraph = new ClusteredAdjacencyGraph<int, Edge<int>>(wrappedGraph);
                yield return new TestCaseData(clusteredGraph)
                {
                    ExpectedResult =
                        "digraph G {" + Environment.NewLine
                        + "0;" + Environment.NewLine
                        + "1;" + Environment.NewLine
                        + "}"
                };

                // With clusters
                wrappedGraph = new AdjacencyGraph<int, Edge<int>>();
                wrappedGraph.AddVertexRange(new[] { 1, 2 });
                clusteredGraph = new ClusteredAdjacencyGraph<int, Edge<int>>(wrappedGraph);
                ClusteredAdjacencyGraph<int, Edge<int>> subGraph1 = clusteredGraph.AddCluster();
                subGraph1.AddVerticesAndEdgeRange(new[]
                {
                    new Edge<int>(3, 4),
                    new Edge<int>(4, 1)
                });
                ClusteredAdjacencyGraph<int, Edge<int>> subGraph2 = clusteredGraph.AddCluster();
                subGraph2.AddVerticesAndEdge(new Edge<int>(1, 5));
                yield return new TestCaseData(clusteredGraph)
                {
                    ExpectedResult =
                        "digraph G {" + Environment.NewLine
                        + "subgraph cluster1 {" + Environment.NewLine
                        + "2;" + Environment.NewLine
                        + "3;" + Environment.NewLine
                        + "0;" + Environment.NewLine
                        + "2 -> 3;" + Environment.NewLine
                        + "3 -> 0;" + Environment.NewLine
                        + "}" + Environment.NewLine
                        + "subgraph cluster2 {" + Environment.NewLine
                        + "4;" + Environment.NewLine
                        + "0 -> 4;" + Environment.NewLine
                        + "}" + Environment.NewLine
                        + "1;" + Environment.NewLine
                        + "}"
                };
            }
        }

        [TestCaseSource(nameof(GenerateTestCases))]
        public string Generate([NotNull] IEdgeListGraph<int, Edge<int>> graph)
        {
            var algorithm = new GraphvizAlgorithm<int, Edge<int>>(graph);
            return algorithm.Generate();
        }

        [Test]
        public void GenerateWithFormats()
        {
            var graph = new AdjacencyGraph<int, Edge<int>>();
            graph.AddVerticesAndEdgeRange(new[]
            {
                new Edge<int>(1, 2),
                new Edge<int>(1, 3),
                new Edge<int>(3, 2),
                new Edge<int>(3, 4),
                new Edge<int>(4, 6),
                new Edge<int>(5, 2),
                new Edge<int>(5, 5)
            });
            graph.AddVertex(7);
            var clusteredGraph = new ClusteredAdjacencyGraph<int, Edge<int>>(graph);
            ClusteredAdjacencyGraph<int, Edge<int>> subGraph1 = clusteredGraph.AddCluster();
            subGraph1.AddVertexRange(new[] { 8, 9, 10 });
            ClusteredAdjacencyGraph<int, Edge<int>> subGraph2 = clusteredGraph.AddCluster();
            subGraph2.AddVerticesAndEdgeRange(new[]
            {
                new Edge<int>(11, 12),
                new Edge<int>(11, 13),
                new Edge<int>(12, 13)
            });

            var algorithm = new GraphvizAlgorithm<int, Edge<int>>(clusteredGraph);
            algorithm.GraphFormat.Name = "MyGraph";
            algorithm.GraphFormat.NodeSeparation = 2;
            algorithm.GraphFormat.FontColor = GraphvizColor.Red;
            algorithm.CommonVertexFormat.Url = "https://myurl.com";
            algorithm.CommonVertexFormat.FillColor = GraphvizColor.LightYellow;
            algorithm.CommonVertexFormat.Style = GraphvizVertexStyle.Filled;
            algorithm.CommonEdgeFormat.Direction = GraphvizEdgeDirection.Back;
            algorithm.CommonEdgeFormat.ToolTip = "Edge";

            algorithm.FormatCluster += (sender, args) =>
            {
                args.GraphFormat.Label = args.Cluster == subGraph1
                    ? "Only Vertices cluster"
                    : "Triangle cluster";
            };

            algorithm.FormatVertex += (sender, args) =>
            {
                if (args.Vertex == 2 || args.Vertex == 11)
                {
                    args.VertexFormat.Label = "Special Node";
                }
            };

            algorithm.FormatEdge += (sender, args) =>
            {
                if (args.Edge.Source ==  args.Edge.Target)
                {
                    args.EdgeFormat.StrokeColor = GraphvizColor.Gold;
                }
            };

            string dot = algorithm.Generate();
            string expectedDot = "digraph MyGraph {" + Environment.NewLine
            + "fontcolor=\"#FF0000FF\"; nodesep=2;" + Environment.NewLine
            + "node [style=filled, fillcolor=\"#FFFFE0FF\", URL=\"https://myurl.com\"];" + Environment.NewLine
            + "edge [dir=back, tooltip=\"Edge\"];" + Environment.NewLine
            + "subgraph cluster1 {" + Environment.NewLine
            + "label=\"Only Vertices cluster\"" + Environment.NewLine
            + "7 [label=\"8\"];" + Environment.NewLine
            + "8 [label=\"9\"];" + Environment.NewLine
            + "9 [label=\"10\"];" + Environment.NewLine
            + "}" + Environment.NewLine
            + "subgraph cluster2 {" + Environment.NewLine
            + "label=\"Triangle cluster\"" + Environment.NewLine
            + "10 [label=\"Special Node\"];" + Environment.NewLine
            + "11 [label=\"12\"];" + Environment.NewLine
            + "12 [label=\"13\"];" + Environment.NewLine
            + "10 -> 11;" + Environment.NewLine
            + "10 -> 12;" + Environment.NewLine
            + "11 -> 12;" + Environment.NewLine
            + "}" + Environment.NewLine
            + "0 [label=\"1\"];" + Environment.NewLine
            + "1 [label=\"Special Node\"];" + Environment.NewLine
            + "2 [label=\"3\"];" + Environment.NewLine
            + "3 [label=\"4\"];" + Environment.NewLine
            + "4 [label=\"6\"];" + Environment.NewLine
            + "5 [label=\"5\"];" + Environment.NewLine
            + "6 [label=\"7\"];" + Environment.NewLine
            + "0 -> 1;" + Environment.NewLine
            + "0 -> 2;" + Environment.NewLine
            + "2 -> 1;" + Environment.NewLine
            + "2 -> 3;" + Environment.NewLine
            + "3 -> 4;" + Environment.NewLine
            + "5 -> 1;" + Environment.NewLine
            + "5 -> 5 [color=\"#FFD700FF\"];" + Environment.NewLine
            + "}";
            Assert.AreEqual(expectedDot, dot);
        }
    }
}