﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using QuikGraph.Algorithms;
using static QuikGraph.Tests.Algorithms.AlgorithmTestHelpers;
using static QuikGraph.Tests.GraphTestHelpers;

namespace QuikGraph.Tests.Algorithms
{
    /// <summary>
    /// Tests for <see cref="TransitiveClosureAlgorithm{TVertex,TEdge}"/>.
    /// </summary>
    [TestFixture]
    internal sealed class TransitiveClosureAlgorithmTests
    {
        [Test]
        public void Constructor()
        {
            var graph = new AdjacencyGraph<int, Edge<int>>();
            var algorithm = new TransitiveClosureAlgorithm<int, Edge<int>>(graph, (v1, v2) => new Edge<int>(v1, v2));
            AssertAlgorithmState(algorithm, graph);
            Assert.IsNotNull(algorithm.TransitiveClosure);
        }

        [Test]
        public void Constructor_Throws()
        {
            var graph = new AdjacencyGraph<int, Edge<int>>();
            // ReSharper disable ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new TransitiveClosureAlgorithm<int, IEdge<int>>(null, (v1, v2) => Edge.Create(v1, v2)));
            Assert.Throws<ArgumentNullException>(() => new TransitiveClosureAlgorithm<int, IEdge<int>>(graph, null));
            Assert.Throws<ArgumentNullException>(() => new TransitiveClosureAlgorithm<int, IEdge<int>>(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore ObjectCreationAsStatement
        }

        [Test]
        public void TransitiveClosure_ValueType()
        {
            // Test 1
            var graph = new AdjacencyGraph<int, SEquatableEdge<int>>();
            graph.AddVerticesAndEdgeRange(new[]
            {
                new SEquatableEdge<int>(1, 2),
                new SEquatableEdge<int>(2, 3)
            });

            BidirectionalGraph<int, SEquatableEdge<int>> result = graph.ComputeTransitiveClosure((u, v) => new SEquatableEdge<int>(u, v));
            AssertHasVertices(result, [1, 2, 3]);
            var expected = new List<SEquatableEdge<int>>
            {
                new(1, 2),
                new(1, 3),
                new(2, 3)
            }.AsReadOnly();
            AssertHasEdges(result, expected);

            var result2 = result.ComputeTransitiveClosure((u, v)
                => new SEquatableEdge<int>(u, v));
            AssertHasEdges(result2, expected);
        }

        [Test]
        public void TransitiveClosure_ValueType2()
        {
            var graph = new AdjacencyGraph<int, SEquatableEdge<int>>();
            graph.AddVerticesAndEdgeRange(
                new List<SEquatableEdge<int>>
                {
                    new(1, 2),
                    new(2, 3),
                    new(3, 4),
                    new(3, 5)
                }.AsReadOnly());

            var result = graph.ComputeTransitiveClosure((u, v)
                => new SEquatableEdge<int>(u, v));
            AssertHasVertices(result, [1, 2, 3, 4, 5]);
            var expected = new SEquatableEdge<int>[] {
                new(1, 2),
                new(1, 3),
                new(1, 4),
                new(1, 5),
                new(2, 3),
                new(2, 4),
                new(2, 5),
                new(3, 4),
                new(3, 5)
            };
            AssertHasEdges(result, expected);

            // Idempotency:
            var result2 = result.ComputeTransitiveClosure((u, v)
                => new SEquatableEdge<int>(u, v));
            AssertHasEdges(result2, expected);
        }

        [Test]
        public void TransitiveClosure_ReferenceType()
        {
            var graph = new AdjacencyGraph<int, EquatableEdge<int>>();
            graph.AddVerticesAndEdgeRange(new[]
            {
                new EquatableEdge<int>(1, 2),
                new EquatableEdge<int>(2, 3)
            });

            BidirectionalGraph<int, EquatableEdge<int>> result = graph.ComputeTransitiveClosure((u, v) => new EquatableEdge<int>(u, v));
            AssertHasVertices(result, [1, 2, 3]);
            var expected = new List<EquatableEdge<int>>
            {
                new(1, 2),
                new(1, 3),
                new(2, 3)
            }.AsReadOnly();
            AssertHasEdges(
                result,
                expected);

            // Idempotency:
            var result2 = result.ComputeTransitiveClosure((u, v)
                => new EquatableEdge<int>(u, v));
            AssertHasEdges(result2, expected);
        }

        [Test]
        public void TransitiveClosure_ReferenceType2()
        {
            var graph = new AdjacencyGraph<int, EquatableEdge<int>>();
            graph.AddVerticesAndEdgeRange(
                new List<EquatableEdge<int>>
                {
                    new(1, 2),
                    new(2, 3),
                    new(3, 4),
                    new(3, 5)
                }.AsReadOnly());

            var result = graph.ComputeTransitiveClosure((u, v) => new EquatableEdge<int>(u, v));
            AssertHasVertices(result, [1, 2, 3, 4, 5]);
            var expected = new List<EquatableEdge<int>>
            {
                new(1, 2),
                new(1, 3),
                new(1, 4),
                new(1, 5),
                new(2, 3),
                new(2, 4),
                new(2, 5),
                new(3, 4),
                new(3, 5)
            }.AsReadOnly();

            AssertHasEdges(result, expected);

            // Idempotency:
            var result2 = result.ComputeTransitiveClosure((u, v)
                => new EquatableEdge<int>(u, v));
            AssertHasEdges(result2, expected);
        }

        [Test]
        public void TransitiveClosure_IsolatedVertices()
        {
            const string vertex1 = "/test";
            const string vertex2 = "/test/123";
            const string vertex3 = "/test/456";
            const string vertex4 = "/test/notlinked";
            var edge12 = new EquatableEdge<string>(vertex1, vertex2);
            var edge23 = new EquatableEdge<string>(vertex2, vertex3);

            var graph = new AdjacencyGraph<string, EquatableEdge<string>>();
            graph.AddVertexRange(new[] { vertex1, vertex2, vertex3, vertex4 });
            graph.AddEdgeRange(new[] { edge12, edge23 });

            BidirectionalGraph<string, EquatableEdge<string>> result = graph.ComputeTransitiveClosure((u, v) => new EquatableEdge<string>(u, v));
            AssertHasVertices(result, new[] { vertex1, vertex2, vertex3, vertex4 });
            AssertHasEdges(
                result,
                new[]
                {
                    edge12,
                    new EquatableEdge<string>(vertex1, vertex3),
                    edge23
                });
        }
    }
}