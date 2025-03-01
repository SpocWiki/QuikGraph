using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace QuikGraph.Algorithms.Condensation
{
    /// <inheritdoc cref="CreateEdgeMergeCondensationGraphAlgorithm{TVertex,TEdge}"/>
    public static class EdgeMergeCondensationGraphAlgorithmX
    {
        /// <summary> Condensates the given bidirectional directed graph. </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <param name="vertexPredicate">Vertex predicate used to filter the vertices to put in the condensed graph.</param>
        /// <returns>The condensed graph.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="vertexPredicate"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull]
        public static IMutableBidirectionalGraph<TVertex, MergedEdge<TVertex, TEdge>> CondensateEdges<TVertex, TEdge>(
            [NotNull] this IBidirectionalGraph<TVertex, TEdge> graph,
            [NotNull] VertexPredicate<TVertex> vertexPredicate)
            where TEdge : IEdge<TVertex>
        {
            var condensedGraph = new BidirectionalGraph<TVertex, MergedEdge<TVertex, TEdge>>();
            var algorithm = graph.CreateEdgeMergeCondensationGraphAlgorithm(condensedGraph, vertexPredicate);
            algorithm.Compute();
            return condensedGraph;
        }

        /// <summary> Initializes a new instance of the <see cref="EdgeMergeCondensationGraphAlgorithm{TVertex,TEdge}"/> class. </summary>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="condensedGraph">Graph that will contain the condensation of the <paramref name="visitedGraph"/>.</param>
        /// <param name="vertexPredicate">Vertex predicate used to filter the vertices to put in the condensed graph.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="condensedGraph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="vertexPredicate"/> is <see langword="null"/>.</exception>
        public static EdgeMergeCondensationGraphAlgorithm<TVertex, TEdge> CreateEdgeMergeCondensationGraphAlgorithm<
            TVertex, TEdge>(
            [NotNull] this IBidirectionalGraph<TVertex, TEdge> visitedGraph,
            [NotNull] IMutableBidirectionalGraph<TVertex, MergedEdge<TVertex, TEdge>> condensedGraph,
            [NotNull] VertexPredicate<TVertex> vertexPredicate) where TEdge : IEdge<TVertex>
            => new EdgeMergeCondensationGraphAlgorithm<TVertex, TEdge>(visitedGraph, condensedGraph, vertexPredicate);
    }

    /// <summary> Algorithm that condensate edges of a graph. </summary>
    public sealed class EdgeMergeCondensationGraphAlgorithm<TVertex, TEdge> : AlgorithmBase<IBidirectionalGraph<TVertex, TEdge>>
        where TEdge : IEdge<TVertex>
    {
        /// <summary> Initializes a new instance of the <see cref="EdgeMergeCondensationGraphAlgorithm{TVertex,TEdge}"/> class. </summary>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="condensedGraph">Graph that will contain the condensation of the <paramref name="visitedGraph"/>.</param>
        /// <param name="vertexPredicate">Vertex predicate used to filter the vertices to put in the condensed graph.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="condensedGraph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="vertexPredicate"/> is <see langword="null"/>.</exception>
        internal EdgeMergeCondensationGraphAlgorithm(
            [NotNull] IBidirectionalGraph<TVertex, TEdge> visitedGraph,
            [NotNull] IMutableBidirectionalGraph<TVertex, MergedEdge<TVertex, TEdge>> condensedGraph,
            [NotNull] VertexPredicate<TVertex> vertexPredicate)
            : base(visitedGraph)
        {
            CondensedGraph = condensedGraph ?? throw new ArgumentNullException(nameof(condensedGraph));
            VertexPredicate = vertexPredicate ?? throw new ArgumentNullException(nameof(vertexPredicate));
        }

        /// <summary>
        /// Condensed graph.
        /// </summary>
        [NotNull]
        public IMutableBidirectionalGraph<TVertex, MergedEdge<TVertex, TEdge>> CondensedGraph { get; }

        /// <summary>
        /// Vertex predicate used to filter the vertices to put in the condensed graph.
        /// </summary>
        [NotNull]
        public VertexPredicate<TVertex> VertexPredicate { get; }

        #region AlgorithmBase<TGraph>

        /// <inheritdoc />
        protected override void InternalCompute()
        {
            // Adding vertices to the new graph
            // and pushing filtered vertices in queue
            var filteredVertices = new Queue<TVertex>();
            foreach (TVertex vertex in VisitedGraph.Vertices)
            {
                CondensedGraph.AddVertex(vertex);
                if (!VertexPredicate(vertex))
                {
                    filteredVertices.Enqueue(vertex);
                }
            }

            // Adding all edges
            foreach (TEdge edge in VisitedGraph.Edges)
            {
                var mergedEdge = new MergedEdge<TVertex, TEdge>(edge.Source, edge.Target);
                mergedEdge.Edges.Add(edge);

                CondensedGraph.AddEdge(mergedEdge);
            }

            // Remove vertices
            while (filteredVertices.Count > 0)
            {
                TVertex filteredVertex = filteredVertices.Dequeue();

                // Do the cross product between in-edges and out-edges
                MergeVertex(filteredVertex);
            }
        }

        #endregion

        private void MergeVertex([NotNull] TVertex vertex)
        {
            Debug.Assert(vertex != null);

            // Get in-edges and out-edges
            var inEdges = new List<MergedEdge<TVertex, TEdge>>(CondensedGraph.InEdges(vertex));
            var outEdges = new List<MergedEdge<TVertex, TEdge>>(CondensedGraph.OutEdges(vertex));

            // Remove vertex
            CondensedGraph.RemoveVertex(vertex);

            // Add condensed edges
            foreach (MergedEdge<TVertex, TEdge> inEdge in inEdges)
            {
                if (EqualityComparer<TVertex>.Default.Equals(inEdge.Source, vertex))
                    continue;

                foreach (MergedEdge<TVertex, TEdge> outEdge in outEdges)
                {
                    if (EqualityComparer<TVertex>.Default.Equals(outEdge.Target, vertex))
                        continue;

                    var newEdge = inEdge.Merge(outEdge);
                    CondensedGraph.AddEdge(newEdge);
                }
            }
        }
    }
}