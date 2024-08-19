﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuikGraph.Tests.Structures;
using static QuikGraph.Tests.GraphTestHelpers;

namespace QuikGraph.Tests.Extensions
{
    /// <summary>
    /// Tests related to <see cref="GraphExtensions"/>.
    /// </summary>
    internal sealed class GraphExtensionsTests : GraphTestsBase
    {
        #region Delegate graphs

        [Test]
        public void ToDelegateIncidenceGraph_TryGetDelegate()
        {
            TryFunc<int, IEnumerable<IEdge<int>>> tryGetEdges =
                (int _, out IEnumerable<IEdge<int>> outEdges) =>
                {
                    outEdges = null;
                    return false;
                };

            var graph = tryGetEdges.ToDelegateIncidenceGraph();
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Assert.IsNull(graph.OutEdges(1));

            var edge12 = Edge.Create(1, 2);
            var edge21 = Edge.Create(2, 1);
            tryGetEdges =
                (int vertex, out IEnumerable<IEdge<int>> outEdges) =>
                {
                    if (vertex == 1)
                    {
                        outEdges = [edge12];
                        return true;
                    }

                    if (vertex == 2)
                    {
                        outEdges = Enumerable.Empty<IEdge<int>>();
                        return true;
                    }

                    outEdges = null;
                    return false;
                };

            graph = tryGetEdges.ToDelegateIncidenceGraph();
            AssertHasOutEdges(graph, 1, [edge12]);
            AssertNoOutEdge(graph, 2);

            // Graph can evolve based on the delegate
            tryGetEdges =
                (int vertex, out IEnumerable<IEdge<int>> outEdges) =>
                {
                    if (vertex == 1)
                    {
                        outEdges = [edge12];
                        return true;
                    }

                    if (vertex == 2)
                    {
                        outEdges = [edge21];
                        return true;
                    }

                    outEdges = null;
                    return false;
                };
            graph = tryGetEdges.ToDelegateIncidenceGraph();
            AssertHasOutEdges(graph, 1, [edge12]);
            AssertHasOutEdges(graph, 2, [edge21]);

            tryGetEdges =
                (int vertex, out IEnumerable<IEdge<int>> outEdges) =>
                {
                    if (vertex == 1)
                    {
                        outEdges = Enumerable.Empty<IEdge<int>>();
                        return true;
                    }

                    if (vertex == 2)
                    {
                        outEdges = [edge21];
                        return true;
                    }

                    outEdges = null;
                    return false;
                };
            graph = tryGetEdges.ToDelegateIncidenceGraph();
            AssertNoOutEdge(graph, 1);
            AssertHasOutEdges(graph, 2, [edge21]);
        }

        [Test]
        public void ToDelegateIncidenceGraph_TryGetDelegate_Throws()
        {
            TryFunc<int, IEnumerable<Edge<int>>> tryGetEdges = null;
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => tryGetEdges.ToDelegateIncidenceGraph());
        }

        [Test]
        public void ToDelegateIncidenceGraph_GetDelegate()
        {
            Func<int, IEnumerable<IEdge<int>>> getEdges = _ => null;

            var graph = getEdges.ToDelegateIncidenceGraph();
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Assert.IsNull(graph.OutEdges(1));

            var edge12 = Edge.Create(1, 2);
            var edge21 = Edge.Create(2, 1);
            getEdges =
                vertex =>
                {
                    if (vertex == 1)
                        return [edge12];

                    if (vertex == 2)
                        return Enumerable.Empty<IEdge<int>>();

                    return null;
                };

            graph = getEdges.ToDelegateIncidenceGraph();
            AssertHasOutEdges(graph, 1, [edge12]);
            AssertNoOutEdge(graph, 2);

            // Graph can evolve based on the delegate
            getEdges =
                vertex =>
                {
                    if (vertex == 1)
                        return [edge12];

                    if (vertex == 2)
                        return [edge21];

                    return null;
                };
            graph = getEdges.ToDelegateIncidenceGraph();
            AssertHasOutEdges(graph, 1, [edge12]);
            AssertHasOutEdges(graph, 2, [edge21]);

            getEdges =
                vertex =>
                {
                    if (vertex == 1)
                        return Enumerable.Empty<IEdge<int>>();

                    if (vertex == 2)
                        return [edge21];

                    return null;
                };
            graph = getEdges.ToDelegateIncidenceGraph();
            AssertNoOutEdge(graph, 1);
            AssertHasOutEdges(graph, 2, [edge21]);
        }

        [Test]
        public void ToDelegateIncidenceGraph_GetDelegate_Throws()
        {
            Func<int, IEnumerable<Edge<int>>> getEdges = null;
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => getEdges.ToDelegateIncidenceGraph());
        }

        [Test]
        public void ToDelegateVertexAndEdgeListGraph()
        {
            var dictionary = new Dictionary<int, IEnumerable<IEdge<int>>>();
            var graph = dictionary.ToDelegateVertexAndEdgeListGraph
            <
                int,
                IEdge<int>,
                IEnumerable<IEdge<int>>
            >();
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge21 = Edge.Create(2, 1);
            dictionary.Add(1, [edge12]);
            AssertHasVertices(graph, [1]);
            AssertNoEdge(graph);    // Vertex 2 is not in graph, so edge is skipped

            dictionary.Add(2, [edge12]);
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12]);

            // Graph can dynamically evolve
            dictionary[2] = [edge21];
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);

            dictionary[1] = Enumerable.Empty<IEdge<int>>();
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge21]);
        }

        [Test]
        public void ToDelegateVertexAndEdgeListGraph_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((Dictionary<int, IEnumerable<IEdge<int>>>)null).ToDelegateVertexAndEdgeListGraph<int, IEdge<int>, IEnumerable<IEdge<int>>>());
        }

        [Test]
        public void ToDelegateVertexAndEdgeListGraph_ConverterEdges()
        {
            var dictionary = new Dictionary<int, int>();
            var graph = dictionary.ToDelegateVertexAndEdgeListGraph(_ => Enumerable.Empty<IEdge<int>>());
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge21 = Edge.Create(2, 1);
            graph = dictionary.ToDelegateVertexAndEdgeListGraph(pair =>
            {
                if (pair.Value == 1)
                    return new[] { edge12 };
                return [edge21];
            });
            AssertEmptyGraph(graph);

            dictionary.Add(1, 1);
            AssertHasVertices(graph, [1]);
            AssertNoEdge(graph);    // Vertex 2 is not in graph, so edge is skipped

            dictionary.Add(2, 1);
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12]);

            // Graph can dynamically evolve
            dictionary[2] = 2;
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);

            dictionary[1] = 2;
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge21]);
        }

        [Test]
        public void ToDelegateVertexAndEdgeListGraph_ConverterEdges_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((Dictionary<int, IEdge<int>>) null).ToDelegateVertexAndEdgeListGraph(pair => new[] { pair.Value }));
            Assert.Throws<ArgumentNullException>(
                () => ((Dictionary<int, IEdge<int>>) null).ToDelegateVertexAndEdgeListGraph((Converter<KeyValuePair<int, IEdge<int>>, IEnumerable<Edge<int>>>)null));

            var dictionary = new Dictionary<int, IEdge<int>>();
            Assert.Throws<ArgumentNullException>(
                () => dictionary.ToDelegateVertexAndEdgeListGraph((Converter<KeyValuePair<int, IEdge<int>>, IEnumerable<Edge<int>>>)null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ToDelegateVertexAndEdgeListGraph_TryGetDelegate()
        {
            var vertices = new List<int>();
            var graph = vertices.ToDelegateVertexAndEdgeListGraph(
                (int _, out IEnumerable<IEdge<int>> outEdges) =>
                {
                    outEdges = null;
                    return false;
                });
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge21 = Edge.Create(2, 1);
            graph = vertices.ToDelegateVertexAndEdgeListGraph(
                (int vertex, out IEnumerable<IEdge<int>> outEdges) =>
                {
                    if (vertex == 1)
                    {
                        outEdges = [edge12];
                        return true;
                    }

                    if (vertex == 2)
                    {
                        outEdges = Enumerable.Empty<IEdge<int>>();
                        return true;
                    }

                    outEdges = null;
                    return false;
                });
            AssertEmptyGraph(graph);

            vertices.Add(1);
            AssertHasVertices(graph, [1]);
            AssertNoEdge(graph);    // Vertex 2 is not in graph, so edge is skipped

            vertices.Add(2);
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12]);

            // Graph can evolve based on the delegate
            graph = vertices.ToDelegateVertexAndEdgeListGraph(
                (int vertex, out IEnumerable<IEdge<int>> outEdges) =>
                {
                    if (vertex == 1)
                    {
                        outEdges = [edge12];
                        return true;
                    }

                    if (vertex == 2)
                    {
                        outEdges = [edge21];
                        return true;
                    }

                    outEdges = null;
                    return false;
                });
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);

            graph = vertices.ToDelegateVertexAndEdgeListGraph(
                (int vertex, out IEnumerable<IEdge<int>> outEdges) =>
                {
                    if (vertex == 1)
                    {
                        outEdges = Enumerable.Empty<IEdge<int>>();
                        return true;
                    }

                    if (vertex == 2)
                    {
                        outEdges = [edge21];
                        return true;
                    }

                    outEdges = null;
                    return false;
                });
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge21]);
        }

        [Test]
        public void ToDelegateVertexAndEdgeListGraph_TryGetDelegate_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToDelegateVertexAndEdgeListGraph(
                    (int _, out IEnumerable<Edge<int>> outEdges) =>
                    {
                        outEdges = null;
                        return false;
                    }));
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToDelegateVertexAndEdgeListGraph((TryFunc<int, IEnumerable<Edge<int>>>)null));

            IEnumerable<int> vertices = Enumerable.Empty<int>();
            Assert.Throws<ArgumentNullException>(
                () => vertices.ToDelegateVertexAndEdgeListGraph((TryFunc<int, IEnumerable<Edge<int>>>)null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ToDelegateVertexAndEdgeListGraph_GetDelegate()
        {
            var vertices = new List<int>();
            var graph = vertices.ToDelegateVertexAndEdgeListGraph<int, IEdge<int>>(_ => null);
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge21 = Edge.Create(2, 1);
            graph = vertices.ToDelegateVertexAndEdgeListGraph(
                vertex =>
                {
                    if (vertex == 1)
                        return [edge12];

                    if (vertex == 2)
                        return Enumerable.Empty<IEdge<int>>();

                    return null;
                });
            AssertEmptyGraph(graph);

            vertices.Add(1);
            AssertHasVertices(graph, [1]);
            AssertNoEdge(graph);    // Vertex 2 is not in graph, so edge is skipped

            vertices.Add(2);
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12]);

            // Graph can evolve based on the delegate
            graph = vertices.ToDelegateVertexAndEdgeListGraph(
                vertex =>
                {
                    if (vertex == 1)
                        return new[] { edge12 };

                    if (vertex == 2)
                        return [edge21];

                    return null;
                });
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);

            graph = vertices.ToDelegateVertexAndEdgeListGraph(
                vertex =>
                {
                    if (vertex == 1)
                        return Enumerable.Empty<IEdge<int>>();

                    if (vertex == 2)
                        return [edge21];

                    return null;
                });
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge21]);
        }

        [Test]
        public void ToDelegateVertexAndEdgeListGraph_GetDelegate_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToDelegateVertexAndEdgeListGraph<int, IEdge<int>>(_ => null));
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToDelegateVertexAndEdgeListGraph((Func<int, IEnumerable<Edge<int>>>)null));

            IEnumerable<int> vertices = Enumerable.Empty<int>();
            Assert.Throws<ArgumentNullException>(
                () => vertices.ToDelegateVertexAndEdgeListGraph((Func<int, IEnumerable<Edge<int>>>)null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ToDelegateBidirectionalIncidenceGraph()
        {
            TryFunc<int, IEnumerable<IEdge<int>>> tryGetOutEdges =
                (int _, out IEnumerable<IEdge<int>> outEdges) =>
                {
                    outEdges = null;
                    return false;
                };
            TryFunc<int, IEnumerable<IEdge<int>>> tryGetInEdges =
                (int _, out IEnumerable<IEdge<int>> inEdges) =>
                {
                    inEdges = null;
                    return false;
                };

            var graph = tryGetOutEdges.ToDelegateBidirectionalIncidenceGraph(tryGetInEdges);
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            Assert.IsNull(graph.OutEdges(1));
            Assert.IsNull(graph.InEdges(1));
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed

            var edge12 = Edge.Create(1, 2);
            tryGetOutEdges =
                (int vertex, out IEnumerable<IEdge<int>> outEdges) =>
                {
                    if (vertex == 1)
                    {
                        outEdges = [edge12];
                        return true;
                    }

                    if (vertex == 2)
                    {
                        outEdges = Enumerable.Empty<IEdge<int>>();
                        return true;
                    }

                    outEdges = null;
                    return false;
                };
            tryGetInEdges =
                (int vertex, out IEnumerable<IEdge<int>> inEdges) =>
                {
                    if (vertex == 1)
                    {
                        inEdges = Enumerable.Empty<IEdge<int>>();
                        return true;
                    }

                    if (vertex == 2)
                    {
                        inEdges = [edge12];
                        return true;
                    }

                    inEdges = null;
                    return false;
                };
            graph = tryGetOutEdges.ToDelegateBidirectionalIncidenceGraph(tryGetInEdges);
            AssertHasOutEdges(graph, 1, [edge12]);
            AssertNoOutEdge(graph, 2);
            AssertNoInEdge(graph, 1);
            AssertHasInEdges(graph, 2, [edge12]);

            // Graph can evolve based on the delegate
            var edge21 = Edge.Create(2, 1);
            var edge23 = Edge.Create(2, 3);
            tryGetOutEdges =
                (int vertex, out IEnumerable<IEdge<int>> outEdges) =>
                {
                    if (vertex == 1)
                    {
                        outEdges = [edge12];
                        return true;
                    }

                    if (vertex == 2)
                    {
                        outEdges = [edge21, edge23];
                        return true;
                    }

                    if (vertex == 3)
                    {
                        outEdges = Enumerable.Empty<IEdge<int>>();
                        return true;
                    }

                    outEdges = null;
                    return false;
                };
            tryGetInEdges =
                (int vertex, out IEnumerable<IEdge<int>> inEdges) =>
                {
                    if (vertex == 1)
                    {
                        inEdges = [edge21];
                        return true;
                    }

                    if (vertex == 2)
                    {
                        inEdges = [edge12];
                        return true;
                    }

                    if (vertex == 3)
                    {
                        inEdges = [edge23];
                        return true;
                    }

                    inEdges = null;
                    return false;
                };
            graph = tryGetOutEdges.ToDelegateBidirectionalIncidenceGraph(tryGetInEdges);
            AssertHasOutEdges(graph, 1, [edge12]);
            AssertHasOutEdges(graph, 2, [edge21, edge23]);
            AssertNoOutEdge(graph, 3);
            AssertHasInEdges(graph, 1, [edge21]);
            AssertHasInEdges(graph, 2, [edge12]);
            AssertHasInEdges(graph, 3, [edge23]);
        }

        [Test]
        public void ToDelegateBidirectionalIncidenceGraph_Throws()
        {
            TryFunc<int, IEnumerable<Edge<int>>> tryGetOutEdges = null;

            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => tryGetOutEdges.ToDelegateBidirectionalIncidenceGraph(null));
            TryFunc<int, IEnumerable<Edge<int>>> tryGetInEdges =
                (int _, out IEnumerable<Edge<int>> inEdges) =>
                {
                    inEdges = null;
                    return false;
                };
            Assert.Throws<ArgumentNullException>(
                () => tryGetOutEdges.ToDelegateBidirectionalIncidenceGraph(tryGetInEdges));

            tryGetOutEdges =
                (int _, out IEnumerable<Edge<int>> outEdges) =>
                {
                    outEdges = null;
                    return false;
                };
            Assert.Throws<ArgumentNullException>(
                () => tryGetOutEdges.ToDelegateBidirectionalIncidenceGraph(null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ToDelegateUndirectedGraph_TryGetDelegate()
        {
            var vertices = new List<int>();
            var graph = vertices.ToDelegateUndirectedGraph(
                (int _, out IEnumerable<IEdge<int>> adjacentEdges) =>
                {
                    adjacentEdges = null;
                    return false;
                });
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge21 = Edge.Create(2, 1);
            graph = vertices.ToDelegateUndirectedGraph(
                (int vertex, out IEnumerable<IEdge<int>> adjacentEdges) =>
                {
                    if (vertex == 1 || vertex == 2)
                    {
                        adjacentEdges = [edge12, edge21];
                        return true;
                    }

                    adjacentEdges = null;
                    return false;
                });
            AssertEmptyGraph(graph);

            vertices.Add(1);
            AssertHasVertices(graph, [1]);
            AssertNoEdge(graph);    // Vertex 2 is not in graph, so edge is skipped

            vertices.Add(2);
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);
            AssertHasAdjacentEdges(graph, 1, [edge12, edge21]);
            AssertHasAdjacentEdges(graph, 2, [edge12, edge21]);

            // Graph can evolve based on the delegate
            vertices.Add(3);
            var edge23 = Edge.Create(2, 3);
            graph = vertices.ToDelegateUndirectedGraph(
                (int vertex, out IEnumerable<IEdge<int>> adjacentEdges) =>
                {
                    if (vertex == 1)
                    {
                        adjacentEdges = [edge12, edge21];
                        return true;
                    }

                    if (vertex == 2)
                    {
                        adjacentEdges = [edge12, edge21, edge23];
                        return true;
                    }

                    if (vertex == 3)
                    {
                        adjacentEdges = [edge23];
                        return true;
                    }

                    adjacentEdges = null;
                    return false;
                });
            AssertHasVertices(graph, [1, 2, 3]);
            AssertHasEdges(graph, [edge12, edge21, edge23]);
            AssertHasAdjacentEdges(graph, 1, [edge12, edge21]);
            AssertHasAdjacentEdges(graph, 2, [edge12, edge21, edge23]);
            AssertHasAdjacentEdges(graph, 3, [edge23]);
        }

        [Test]
        public void ToDelegateUndirectedGraph_TryGetDelegate_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToDelegateUndirectedGraph(
                    (int _, out IEnumerable<Edge<int>> adjacentEdges) =>
                    {
                        adjacentEdges = null;
                        return false;
                    }));
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToDelegateUndirectedGraph((TryFunc<int, IEnumerable<Edge<int>>>)null));

            IEnumerable<int> vertices = Enumerable.Empty<int>();
            Assert.Throws<ArgumentNullException>(
                () => vertices.ToDelegateUndirectedGraph((TryFunc<int, IEnumerable<Edge<int>>>)null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ToDelegateUndirectedGraph_GetDelegate()
        {
            var vertices = new List<int>();
            var graph = vertices.ToDelegateUndirectedGraph<int, IEdge<int>>(_ => null);
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge21 = Edge.Create(2, 1);
            graph = vertices.ToDelegateUndirectedGraph(
                vertex =>
                {
                    if (vertex == 1 || vertex == 2)
                        return new[] { edge12, edge21 };
                    return null;
                });
            AssertEmptyGraph(graph);

            vertices.Add(1);
            AssertHasVertices(graph, [1]);
            AssertNoEdge(graph);    // Vertex 2 is not in graph, so edge is skipped

            vertices.Add(2);
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);
            AssertHasAdjacentEdges(graph, 1, [edge12, edge21]);
            AssertHasAdjacentEdges(graph, 2, [edge12, edge21]);

            // Graph can evolve based on the delegate
            vertices.Add(3);
            var edge23 = Edge.Create(2, 3);
            graph = vertices.ToDelegateUndirectedGraph(
                vertex =>
                {
                    if (vertex == 1)
                        return new[] { edge12, edge21 };

                    if (vertex == 2)
                        return [edge12, edge21, edge23];

                    if (vertex == 3)
                        return [edge23];

                    return null;
                });
            AssertHasVertices(graph, [1, 2, 3]);
            AssertHasEdges(graph, [edge12, edge21, edge23]);
            AssertHasAdjacentEdges(graph, 1, [edge12, edge21]);
            AssertHasAdjacentEdges(graph, 2, [edge12, edge21, edge23]);
            AssertHasAdjacentEdges(graph, 3, [edge23]);
        }

        [Test]
        public void ToDelegateUndirectedGraph_GetDelegate_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToDelegateUndirectedGraph<int, IEdge<int>>(_ => null));
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToDelegateUndirectedGraph((Func<int, IEnumerable<Edge<int>>>)null));

            IEnumerable<int> vertices = Enumerable.Empty<int>();
            Assert.Throws<ArgumentNullException>(
                () => vertices.ToDelegateUndirectedGraph((Func<int, IEnumerable<Edge<int>>>)null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        #endregion

        #region Graphs

        [Test]
        public void ToAdjacencyGraph_EdgeArray()
        {
            int[][] edges = [[], []];
            var graph = edges.ToAdjacencyGraph();
            AssertEmptyGraph(graph);

            edges =
            [
                new[] {1, 2, 3},
                [2, 3, 1]
            ];
            graph = edges.ToAdjacencyGraph();
            AssertHasVertices(graph, [1, 2, 3]);
            AssertHasEdges(
                graph, 
                [
                    new SEquatableEdge<int>(1, 2),
                    new SEquatableEdge<int>(2, 3),
                    new SEquatableEdge<int>(3, 1)
                ]);
        }

        [Test]
        public void ToAdjacencyGraph_EdgeArray_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            int[][] edges = null;
            Assert.Throws<ArgumentNullException>(() => edges.ToAdjacencyGraph());

            edges = [];
            Assert.Throws<ArgumentException>(() => edges.ToAdjacencyGraph());
            edges = [new int[]{ }];
            Assert.Throws<ArgumentException>(() => edges.ToAdjacencyGraph());
            edges = [new int[] { }, [], []];
            Assert.Throws<ArgumentException>(() => edges.ToAdjacencyGraph());

            edges = [new int[] { }, null];
            Assert.Throws<ArgumentNullException>(() => edges.ToAdjacencyGraph());
            edges = [null, new int[] { }];
            Assert.Throws<ArgumentNullException>(() => edges.ToAdjacencyGraph());
            edges = [null, null];
            Assert.Throws<ArgumentNullException>(() => edges.ToAdjacencyGraph());

            edges = [new int[] { }, [1]];
            Assert.Throws<ArgumentException>(() => edges.ToAdjacencyGraph());
            edges = [new[] { 1 }, []];
            Assert.Throws<ArgumentException>(() => edges.ToAdjacencyGraph());
            edges = [new[] { 1, 2 }, [1]];
            Assert.Throws<ArgumentException>(() => edges.ToAdjacencyGraph());
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ToAdjacencyGraph_EdgeSet()
        {
            var edges = new List<IEdge<int>>();
            var graph = edges.ToAdjacencyGraph<int, IEdge<int>>();
            AssertEmptyGraph(graph);

            graph = edges.ToAdjacencyGraph<int, IEdge<int>>(false);
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge21 = Edge.Create(2, 1);
            edges.AddRange([edge12, edge21]);
            graph = edges.ToAdjacencyGraph<int, IEdge<int>>();
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);

            // Graph cannot dynamically evolve
            var edge12Bis = Edge.Create(1, 2);
            edges.Add(edge12Bis);
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);

            graph = edges.ToAdjacencyGraph<int, IEdge<int>>();
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge12Bis, edge21]);

            graph = edges.ToAdjacencyGraph<int, IEdge<int>>(false);
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);
        }

        [Test]
        public void ToAdjacencyGraph_EdgeSet_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<IEdge<int>>)null).ToAdjacencyGraph<int, IEdge<int>>());
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<IEdge<int>>)null).ToAdjacencyGraph<int, IEdge<int>>(false));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ToAdjacencyGraph_VertexPairs()
        {
            var vertices = new List<SEquatableEdge<int>>();
            var graph = vertices.ToAdjacencyGraph();
            AssertEmptyGraph(graph);

            var edge12 = new SEquatableEdge<int>(1, 2);
            var edge23 = new SEquatableEdge<int>(2, 3);
            vertices.AddRange([edge12, edge23]);
            graph = vertices.ToAdjacencyGraph();
            AssertHasVertices(graph, [1, 2, 3]);
            AssertHasEdges(graph, [edge12, edge23]);
        }

        [Test]
        public void ToAdjacencyGraph_VertexPairs_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<SEquatableEdge<int>>)null).ToAdjacencyGraph());
        }

        [Test]
        public void ToAdjacencyGraph_EdgeSetWithFactory()
        {
            var vertices = new List<int>();
            var graph = vertices.ToAdjacencyGraph(
                _ => Enumerable.Empty<IEdge<int>>());
            AssertEmptyGraph(graph);

            graph = vertices.ToAdjacencyGraph(
                _ => Enumerable.Empty<IEdge<int>>(),
                false);
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge12Bis = Edge.Create(1, 2);
            var edge21 = Edge.Create(2, 1);
            vertices.AddRange([1, 2]);
            graph = vertices.ToAdjacencyGraph(
                vertex =>
                {
                    if (vertex == 1)
                        return [edge12, edge12Bis];
                    if (vertex == 2)
                        return [edge21];
                    return Enumerable.Empty<IEdge<int>>();
                });
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge12Bis, edge21]);

            graph = vertices.ToAdjacencyGraph(
                vertex =>
                {
                    if (vertex == 1)
                        return [edge12, edge12Bis];
                    if (vertex == 2)
                        return [edge21];
                    return Enumerable.Empty<IEdge<int>>();
                },
                false);
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);
        }

        [Test]
        public void ToAdjacencyGraph_EdgeSetWithFactory_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToAdjacencyGraph(_ => Enumerable.Empty<Edge<int>>()));
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToAdjacencyGraph<int, IEdge<int>>(null));
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToAdjacencyGraph(_ => Enumerable.Empty<Edge<int>>(), false));
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToAdjacencyGraph<int, IEdge<int>>(null, false));

            IEnumerable<int> vertices = Enumerable.Empty<int>();
            Assert.Throws<ArgumentNullException>(
                () => vertices.ToAdjacencyGraph<int, IEdge<int>>(null));
            Assert.Throws<ArgumentNullException>(
                () => vertices.ToAdjacencyGraph<int, IEdge<int>>(null, false));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ToArrayAdjacencyGraph()
        {
            var wrappedGraph = new AdjacencyGraph<int, IEdge<int>>();
            var graph = wrappedGraph.ToArrayAdjacencyGraph();
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge23 = Edge.Create(2, 3);
            wrappedGraph.AddVerticesAndEdgeRange([edge12, edge23]);
            graph = wrappedGraph.ToArrayAdjacencyGraph();
            AssertHasVertices(graph, [1, 2, 3]);
            AssertHasEdges(graph, [edge12, edge23]);
        }

        [Test]
        public void ToArrayAdjacencyGraph_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((AdjacencyGraph<int, IEdge<int>>)null).ToArrayAdjacencyGraph());
        }

        [Test]
        public void ToBidirectionalGraph_FromDirectedGraph()
        {
            var initialGraph1 = new AdjacencyGraph<int, IEdge<int>>();
            IBidirectionalGraph<int, IEdge<int>> graph = initialGraph1.ToBidirectionalGraph();
            AssertEmptyGraph(graph);

            var initialGraph2 = new BidirectionalGraph<int, IEdge<int>>();
            graph = initialGraph2.ToBidirectionalGraph();
            AssertEmptyGraph(graph);
            Assert.AreSame(initialGraph2, graph);

            var edge12 = Edge.Create(1, 2);
            var edge21 = Edge.Create(2, 1);
            var edge23 = Edge.Create(2, 3);

            // Graph can dynamically evolve but it will not work when dealing with in-edges
            // stuff when the initial is not a bidirectional graph
            initialGraph1.AddVerticesAndEdgeRange([edge12, edge21]);
            initialGraph1.AddVertex(3);
            graph = initialGraph1.ToBidirectionalGraph();
            AssertHasVertices(graph, [1, 2, 3]);
            AssertHasEdges(graph, [edge12, edge21]);
            AssertNoInEdge(graph, 3);

            initialGraph1.AddVerticesAndEdge(edge23);
            AssertHasVertices(graph, [1, 2, 3]);
            AssertHasEdges(graph, [edge12, edge21, edge23]);
            AssertNoInEdge(graph, 3);


            initialGraph2.AddVerticesAndEdgeRange([edge12, edge21]);
            initialGraph2.AddVertex(3);
            graph = initialGraph2.ToBidirectionalGraph();
            AssertHasVertices(graph, [1, 2, 3]);
            AssertHasEdges(graph, [edge12, edge21]);
            AssertNoInEdge(graph, 3);

            initialGraph2.AddVerticesAndEdge(edge23);
            AssertHasVertices(graph, [1, 2, 3]);
            AssertHasEdges(graph, [edge12, edge21, edge23]);
            AssertHasInEdges(graph, 3, [edge23]);
        }

        [Test]
        public void ToBidirectionalGraph_FromDirectedGraph_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IVertexAndEdgeListGraph<int, IEdge<int>>)null).ToBidirectionalGraph());
        }

        [Test]
        public void ToBidirectionalGraph_EdgeSet()
        {
            var edges = new List<IEdge<int>>();
            var graph = edges.ToBidirectionalGraph<int, IEdge<int>>();
            AssertEmptyGraph(graph);

            graph = edges.ToBidirectionalGraph<int, IEdge<int>>(false);
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge21 = Edge.Create(2, 1);
            edges.AddRange([edge12, edge21]);
            graph = edges.ToBidirectionalGraph<int, IEdge<int>>();
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);

            // Graph cannot dynamically evolve
            var edge12Bis = Edge.Create(1, 2);
            edges.Add(edge12Bis);
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);

            graph = edges.ToBidirectionalGraph<int, IEdge<int>>();
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge12Bis, edge21]);

            graph = edges.ToBidirectionalGraph<int, IEdge<int>>(false);
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);
        }

        [Test]
        public void ToBidirectionalGraph_EdgeSet_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<IEdge<int>>)null).ToBidirectionalGraph<int, IEdge<int>>());
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<IEdge<int>>)null).ToBidirectionalGraph<int, IEdge<int>>(false));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ToBidirectionalGraph_VertexPairs()
        {
            var vertices = new List<SEquatableEdge<int>>();
            var graph = vertices.ToBidirectionalGraph();
            AssertEmptyGraph(graph);

            var edge12 = new SEquatableEdge<int>(1, 2);
            var edge23 = new SEquatableEdge<int>(2, 3);
            vertices.AddRange([edge12, edge23]);
            graph = vertices.ToBidirectionalGraph();
            AssertHasVertices(graph, [1, 2, 3]);
            AssertHasEdges(graph, [edge12, edge23]);
        }

        [Test]
        public void ToBidirectionalGraph_VertexPairs_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<SEquatableEdge<int>>)null).ToBidirectionalGraph());
        }

        [Test]
        public void ToBidirectionalGraph_EdgeSetWithFactory()
        {
            var vertices = new List<int>();
            var graph = vertices.ToBidirectionalGraph(
                _ => Enumerable.Empty<IEdge<int>>());
            AssertEmptyGraph(graph);

            graph = vertices.ToBidirectionalGraph(
                _ => Enumerable.Empty<IEdge<int>>(),
                false);
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge12Bis = Edge.Create(1, 2);
            var edge21 = Edge.Create(2, 1);
            vertices.AddRange([1, 2]);
            graph = vertices.ToBidirectionalGraph(
                vertex =>
                {
                    if (vertex == 1)
                        return [edge12, edge12Bis];
                    if (vertex == 2)
                        return [edge21];
                    return Enumerable.Empty<IEdge<int>>();
                });
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge12Bis, edge21]);

            graph = vertices.ToBidirectionalGraph(
                vertex =>
                {
                    if (vertex == 1)
                        return [edge12, edge12Bis];
                    if (vertex == 2)
                        return [edge21];
                    return Enumerable.Empty<IEdge<int>>();
                },
                false);
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);
        }

        [Test]
        public void ToBidirectionalGraph_EdgeSetWithFactory_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToBidirectionalGraph(_ => Enumerable.Empty<Edge<int>>()));
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToBidirectionalGraph<int, IEdge<int>>(null));
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToBidirectionalGraph(_ => Enumerable.Empty<Edge<int>>(), false));
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<int>)null).ToBidirectionalGraph<int, IEdge<int>>(null, false));

            var vertices = Enumerable.Empty<int>();
            Assert.Throws<ArgumentNullException>(
                () => vertices.ToBidirectionalGraph<int, IEdge<int>>(null));
            Assert.Throws<ArgumentNullException>(
                () => vertices.ToBidirectionalGraph<int, IEdge<int>>(null, false));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ToBidirectionalGraph_FromUndirectedGraph()
        {
            var initialGraph = new UndirectedGraph<int, IEdge<int>>();
            var graph = initialGraph.ToBidirectionalGraph();
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge21 = Edge.Create(2, 1);
            var edge23 = Edge.Create(2, 3);

            // Graph cannot dynamically evolve
            initialGraph.AddVerticesAndEdgeRange([edge12, edge21]);
            initialGraph.AddVertex(3);
            graph = initialGraph.ToBidirectionalGraph();
            AssertHasVertices(graph, [1, 2, 3]);
            AssertHasEdges(graph, [edge12, edge21]);
            AssertNoInEdge(graph, 3);

            initialGraph.AddVerticesAndEdge(edge23);
            initialGraph.AddVertex(4);
            AssertHasVertices(graph, [1, 2, 3]);  // Not added
            AssertHasEdges(graph, [edge12, edge21]); // Not added
            AssertNoInEdge(graph, 3);                       // Not added
        }

        [Test]
        public void ToBidirectionalGraph_FromUndirectedGraph_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IUndirectedGraph<int, IEdge<int>>)null).ToBidirectionalGraph());
        }

        [Test]
        public void ToArrayBidirectionalGraph()
        {
            var wrappedGraph = new BidirectionalGraph<int, IEdge<int>>();
            var graph = wrappedGraph.ToArrayBidirectionalGraph();
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge23 = Edge.Create(2, 3);
            wrappedGraph.AddVerticesAndEdgeRange([edge12, edge23]);
            graph = wrappedGraph.ToArrayBidirectionalGraph();
            AssertHasVertices(graph, [1, 2, 3]);
            AssertHasEdges(graph, [edge12, edge23]);
        }

        [Test]
        public void ToArrayBidirectionalGraph_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((BidirectionalGraph<int, IEdge<int>>)null).ToArrayBidirectionalGraph());
        }

        [Test]
        public void ToUndirectedGraph()
        {
            var edges = new List<IEdge<int>>();
            var graph = edges.ToUndirectedGraph<int, IEdge<int>>();
            AssertEmptyGraph(graph);

            graph = edges.ToUndirectedGraph<int, IEdge<int>>(false);
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge21 = Edge.Create(2, 1);
            edges.AddRange([edge12, edge21]);
            graph = edges.ToUndirectedGraph<int, IEdge<int>>();
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);

            // Graph cannot dynamically evolve
            var edge12Bis = Edge.Create(1, 2);
            edges.Add(edge12Bis);
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge21]);

            graph = edges.ToUndirectedGraph<int, IEdge<int>>();
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12, edge12Bis, edge21]);

            graph = edges.ToUndirectedGraph<int, IEdge<int>>(false);
            AssertHasVertices(graph, [1, 2]);
            AssertHasEdges(graph, [edge12]);
        }

        [Test]
        public void ToUndirectedGraph_Throws()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<IEdge<int>>)null).ToUndirectedGraph<int, IEdge<int>>());
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<IEdge<int>>)null).ToUndirectedGraph<int, IEdge<int>>(false));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void ToUndirectedGraph_VertexPairs()
        {
            var vertices = new List<SEquatableEdge<int>>();
            var graph = vertices.ToUndirectedGraph();
            AssertEmptyGraph(graph);

            var edge12 = new SEquatableEdge<int>(1, 2);
            var edge23 = new SEquatableEdge<int>(2, 3);
            vertices.AddRange([edge12, edge23]);
            graph = vertices.ToUndirectedGraph();
            AssertHasVertices(graph, [1, 2, 3]);
            AssertHasEdges(graph, [edge12, edge23]);
        }

        [Test]
        public void ToUndirectedGraph_VertexPairs_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((IEnumerable<SEquatableEdge<int>>)null).ToUndirectedGraph());
        }

        [Test]
        public void ToArrayUndirectedGraph()
        {
            var wrappedGraph = new UndirectedGraph<int, IEdge<int>>();
            var graph = wrappedGraph.ToArrayUndirectedGraph();
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge23 = Edge.Create(2, 3);
            wrappedGraph.AddVerticesAndEdgeRange([edge12, edge23]);
            graph = wrappedGraph.ToArrayUndirectedGraph();
            AssertHasVertices(graph, [1, 2, 3]);
            AssertHasEdges(graph, [edge12, edge23]);
        }

        [Test]
        public void ToArrayUndirectedGraph_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((UndirectedGraph<int, IEdge<int>>)null).ToArrayUndirectedGraph());
        }

        [Test]
        public void ToCompressedRowGraph()
        {
            var wrappedGraph = new AdjacencyGraph<int, IEdge<int>>();
            CompressedSparseRowGraph<int> graph = wrappedGraph.ToCompressedRowGraph();
            AssertEmptyGraph(graph);

            var edge12 = Edge.Create(1, 2);
            var edge23 = Edge.Create(2, 3);
            wrappedGraph.AddVerticesAndEdgeRange([edge12, edge23]);
            graph = wrappedGraph.ToCompressedRowGraph();
            AssertHasVertices(graph, [1, 2, 3]);
            AssertHasEdges(
                graph, 
                [
                    new SEquatableEdge<int>(1, 2),
                    new SEquatableEdge<int>(2, 3)
                ]);
        }

        [Test]
        public void ToCompressedRowGraph_Throws()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => ((AdjacencyGraph<int, IEdge<int>>)null).ToCompressedRowGraph());
        }

        #endregion
    }
}