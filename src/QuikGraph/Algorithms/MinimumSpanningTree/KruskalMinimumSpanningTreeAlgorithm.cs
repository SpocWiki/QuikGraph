using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Services;
using QuikGraph.Collections;

namespace QuikGraph.Algorithms.MinimumSpanningTree
{
    /// <inheritdoc cref="CreateKruskalMinimumSpanningTreeAlgorithm{TVertex,TEdge}"/>
    public static class KruskalMinimumSpanningTreeAlgorithm
    {
        /// <summary> Computes the minimum spanning tree using Kruskal algorithm. </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <param name="edgeWeights">Function that computes the weight for a given edge.</param>
        /// <returns>Edges part of the minimum spanning tree.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeWeights"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull, ItemNotNull]
        public static IEnumerable<TEdge> MinimumSpanningTreeKruskal<TVertex, TEdge>(
            [NotNull] this IUndirectedGraph<TVertex, TEdge> graph,
            [NotNull, InstantHandle] Func<TEdge, double> edgeWeights)
            where TEdge : IEdge<TVertex>
        {
            if (graph is null)
                throw new ArgumentNullException(nameof(graph));
            if (edgeWeights is null)
                throw new ArgumentNullException(nameof(edgeWeights));

            if (graph.VertexCount == 0)
                return Enumerable.Empty<TEdge>();

            var kruskal = graph.CreateKruskalMinimumSpanningTreeAlgorithm(edgeWeights);
            var edgeRecorder = new EdgeRecorderObserver<TVertex, TEdge>();
            using (edgeRecorder.Attach(kruskal))
            {
                kruskal.Compute();
            }

            return edgeRecorder.Edges;
        }

        /// <summary> Initializes a new instance of the <see cref="KruskalMinimumSpanningTreeAlgorithm{TVertex,TEdge}"/> class. </summary>
        public static KruskalMinimumSpanningTreeAlgorithm<TVertex, TEdge> CreateKruskalMinimumSpanningTreeAlgorithm<
            TVertex, TEdge>(
            [NotNull] this IUndirectedGraph<TVertex, TEdge> visitedGraph,
            [NotNull] Func<TEdge, double> edgeWeights,
            [CanBeNull] IAlgorithmComponent host = null)
            where TEdge : IEdge<TVertex>
            => new KruskalMinimumSpanningTreeAlgorithm<TVertex, TEdge>(visitedGraph, edgeWeights, host);
    }

    /// <summary> Kruskal minimum spanning tree algorithm implementation. </summary>
    public sealed class KruskalMinimumSpanningTreeAlgorithm<TVertex, TEdge>
        : AlgorithmBase<IUndirectedGraph<TVertex, TEdge>>
        , IMinimumSpanningTreeAlgorithm<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        [NotNull]
        private readonly Func<TEdge, double> _edgeWeights;

        /// <summary> Initializes a new instance of the <see cref="KruskalMinimumSpanningTreeAlgorithm{TVertex,TEdge}"/> class. </summary>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="edgeWeights">Function that computes the weight for a given edge.</param>
        /// <param name="host">Host to use if set, otherwise use this reference.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeWeights"/> is <see langword="null"/>.</exception>
        internal KruskalMinimumSpanningTreeAlgorithm([NotNull] IUndirectedGraph<TVertex, TEdge> visitedGraph,
            [NotNull] Func<TEdge, double> edgeWeights, [CanBeNull] IAlgorithmComponent host = null)
            : base(visitedGraph, host)
        {
            _edgeWeights = edgeWeights ?? throw new ArgumentNullException(nameof(edgeWeights));
        }

        /// <summary>
        /// Fired when an edge is going to be analyzed.
        /// </summary>
        public event EdgeAction<TVertex, TEdge> ExamineEdge;

        private void OnExamineEdge([NotNull] TEdge edge)
        {
            Debug.Assert(edge != null);

            ExamineEdge?.Invoke(edge);
        }

        #region ITreeBuilderAlgorithm<TVertex,TEdge>

        /// <inheritdoc />
        public event EdgeAction<TVertex, TEdge> TreeEdge;

        private void OnTreeEdge([NotNull] TEdge edge)
        {
            Debug.Assert(edge != null);

            TreeEdge?.Invoke(edge);
        }

        #endregion

        #region AlgorithmBase<TGraph>

        /// <inheritdoc />
        protected override void InternalCompute()
        {
            var sets = new ForestDisjointSet<TVertex>(VisitedGraph.VertexCount);
            foreach (TVertex vertex in VisitedGraph.Vertices)
            {
                sets.MakeSet(vertex);
            }

            ThrowIfCancellationRequested();

            var queue = new BinaryQueue<TEdge, double>(_edgeWeights);
            foreach (TEdge edge in VisitedGraph.Edges)
            {
                queue.Enqueue(edge);
            }

            ThrowIfCancellationRequested();

            while (queue.Count > 0)
            {
                TEdge edge = queue.Dequeue();
                OnExamineEdge(edge);

                if (!sets.AreInSameSet(edge.Source, edge.Target))
                {
                    OnTreeEdge(edge);
                    sets.Union(edge.Source, edge.Target);
                }
            }
        }

        #endregion
    }
}