using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Services;

namespace QuikGraph.Algorithms.ShortestPath
{
    /// <inheritdoc cref="CreateBellmanFordShortestPathAlgorithm{TVertex,TEdge}"/>
    public static class BellmanFordShortestPathAlgorithm
    {
        /// <summary> Computes shortest path with the Bellman Ford algorithm
        /// and gets a function that allows to get paths in a directed graph. </summary>
        /// <remarks>Uses <see cref="BellmanFordShortestPathAlgorithm{TVertex,TEdge}"/> algorithm.</remarks>
        /// <param name="graph">The graph to visit.</param>
        /// <param name="edgeWeights">Function that computes the weight for a given edge.</param>
        /// <param name="root">Starting vertex.</param>
        /// <param name="hasNegativeCycle">Indicates if a negative cycle has been found or not.</param>
        /// <returns>A function that allow to get paths starting from <paramref name="root"/> vertex.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeWeights"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="root"/> is not part of <paramref name="graph"/>.</exception>
        [Pure]
        [NotNull]
        public static TryFunc<TVertex, IEnumerable<TEdge>> ShortestPathsBellmanFord<TVertex, TEdge>(
            [NotNull] this IVertexAndEdgeListGraph<TVertex, TEdge> graph,
            [NotNull, InstantHandle] Func<TEdge, double> edgeWeights,
            [NotNull] TVertex root,
            out bool hasNegativeCycle)
            where TEdge : IEdge<TVertex>
        {
            var algorithm = graph.CreateBellmanFordShortestPathAlgorithm(edgeWeights);
            var predecessorRecorder = algorithm.AttachVertexPredecessorRecorderObserver();
            using (predecessorRecorder)
            {
                algorithm.Compute(root);
            }

            hasNegativeCycle = algorithm.FoundNegativeCycle;

            IDictionary<TVertex, TEdge> predecessors = predecessorRecorder.VerticesPredecessors;
            return (TVertex vertex, out IEnumerable<TEdge> edges) => predecessors.TryGetPath(vertex, out edges);
        }

        /// <summary> Creates a new instance of the <see cref="BellmanFordShortestPathAlgorithm{TVertex,TEdge}"/> class. </summary>
        public static BellmanFordShortestPathAlgorithm<TVertex, TEdge> CreateBellmanFordShortestPathAlgorithm<TVertex, TEdge>(
            [NotNull] this IVertexAndEdgeListGraph<TVertex, TEdge> visitedGraph,
            [NotNull] Func<TEdge, double> edgeWeights,
            [CanBeNull] IDistanceRelaxer distanceRelaxer = null,
            [CanBeNull] IAlgorithmComponent host = null) where TEdge : IEdge<TVertex>
            => new BellmanFordShortestPathAlgorithm<TVertex, TEdge>(visitedGraph, edgeWeights, distanceRelaxer, host);
    }

    /// <summary> Bellman Ford shortest path algorithm. </summary>
    /// <remarks>
    /// <para>
    /// The Bellman-Ford algorithm solves the single-source shortest paths
    /// problem for a graph with both positive and negative edge weights.
    /// </para>
    /// <para>
    /// If you only need to solve the shortest paths problem for positive
    /// edge weights, Dijkstra's algorithm provides a more efficient
    /// alternative.
    /// </para>
    /// <para>
    /// If all the edge weights are all equal to one then breadth-first search
    /// provides an even more efficient alternative.
    /// </para>
    /// </remarks>
    public sealed class BellmanFordShortestPathAlgorithm<TVertex, TEdge>
        : ShortestPathAlgorithmBase<TVertex, TEdge, IVertexAndEdgeListGraph<TVertex, TEdge>>
        where TEdge : IEdge<TVertex>
    {
        /// <summary> Initializes a new instance of the <see cref="BellmanFordShortestPathAlgorithm{TVertex,TEdge}"/> class. </summary>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="edgeWeights">Function that computes the weight for a given edge.</param>
        /// <param name="distanceRelaxer">Distance relaxer.</param>
        /// <param name="host">Host to use if set, otherwise use this reference.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeWeights"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="distanceRelaxer"/> is <see langword="null"/>.</exception>
        internal BellmanFordShortestPathAlgorithm([NotNull] IVertexAndEdgeListGraph<TVertex, TEdge> visitedGraph,
            [NotNull] Func<TEdge, double> edgeWeights,
            [CanBeNull] IDistanceRelaxer distanceRelaxer = null,
            [CanBeNull] IAlgorithmComponent host = null)
            : base(visitedGraph, edgeWeights, distanceRelaxer ?? DistanceRelaxers.ShortestDistance, host)
        {
        }

        /// <summary>
        /// Indicates if a negative cycle was found in the graph.
        /// </summary>
        public bool FoundNegativeCycle { get; private set; }

        #region Events

        /// <summary>
        /// Fired on each vertex in the graph before the start of the algorithm.
        /// </summary>
        public event VertexAction<TVertex> InitializeVertex;

        private void OnInitializeVertex([NotNull] TVertex vertex)
        {
            Debug.Assert(vertex != null);

            InitializeVertex?.Invoke(vertex);
        }

        /// <summary>
        /// Fired on every edge in the graph (|V| times).
        /// </summary>
        public event EdgeAction<TVertex, TEdge> ExamineEdge;

        private void OnExamineEdge([NotNull] TEdge edge)
        {
            Debug.Assert(edge != null);

            ExamineEdge?.Invoke(edge);
        }

        /// <summary>
        /// Fired if the distance label for a target vertex is not decreased.
        /// </summary>
        public event EdgeAction<TVertex, TEdge> EdgeNotRelaxed;

        private void OnEdgeNotRelaxed([NotNull] TEdge edge)
        {
            Debug.Assert(edge != null);

            EdgeNotRelaxed?.Invoke(edge);
        }

        /// <summary>
        /// Fired during the second stage of the algorithm,
        /// during the test of whether each edge was minimized.
        /// If the edge is minimized then this event is raised.
        /// </summary>
        public event EdgeAction<TVertex, TEdge> EdgeMinimized;

        private void OnEdgeMinimized([NotNull] TEdge edge)
        {
            Debug.Assert(edge != null);

            EdgeMinimized?.Invoke(edge);
        }

        /// <summary>
        /// Fired during the second stage of the algorithm,
        /// during the test of whether each edge was minimized.
        /// If the edge was not minimized, this event is raised.
        /// This happens when there is a negative cycle in the graph.
        /// </summary>
        public event EdgeAction<TVertex, TEdge> EdgeNotMinimized;

        private void OnEdgeNotMinimized([NotNull] TEdge edge)
        {
            Debug.Assert(edge != null);

            EdgeNotMinimized?.Invoke(edge);
        }

        #endregion

        #region AlgorithmBase<TGraph>

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            FoundNegativeCycle = false;

            // Initialize colors and distances
            VerticesColors.Clear();
            foreach (TVertex vertex in VisitedGraph.Vertices)
            {
                VerticesColors[vertex] = GraphColor.White;
                SetVertexDistance(vertex, double.PositiveInfinity);
                OnInitializeVertex(vertex);
            }

            if (!TryGetRootVertex(out TVertex root))
            {
                // Try to fallback on first vertex, will throw if the graph is empty
                root = VisitedGraph.Vertices.First();
            }
            else if (!VisitedGraph.ContainsVertex(root))
            {
                throw new VertexNotFoundException("Root vertex is not part of the graph.");
            }

            SetVertexDistance(root, 0);
        }

        /// <summary>
        /// Applies the Bellman Ford algorithm.
        /// </summary>
        /// <remarks>
        /// Does not initialize the predecessor and distance map.
        /// </remarks>
        protected override void InternalCompute()
        {
            // Getting the number of vertices
            int nbVertices = VisitedGraph.VertexCount;
            for (int k = 0; k < nbVertices; ++k)
            {
                bool atLeastOneTreeEdge = false;
                foreach (TEdge edge in VisitedGraph.Edges)
                {
                    OnExamineEdge(edge);

                    if (Relax(edge))
                    {
                        atLeastOneTreeEdge = true;
                        OnTreeEdge(edge);
                    }
                    else
                    {
                        OnEdgeNotRelaxed(edge);
                    }
                }

                if (!atLeastOneTreeEdge)
                    break;
            }

            foreach (TEdge edge in VisitedGraph.Edges)
            {
                double edgeWeight = Weights(edge);
                if (DistanceRelaxer.Compare(
                        DistanceRelaxer.Combine(GetVertexDistance(edge.Source), edgeWeight),
                        GetVertexDistance(edge.Target)) < 0)
                {
                    OnEdgeMinimized(edge);
                    FoundNegativeCycle = true;
                    return;
                }

                OnEdgeNotMinimized(edge);
            }

            FoundNegativeCycle = false;
        }

        /// <inheritdoc />
        protected override void Clean()
        {
            base.Clean();

            foreach (TVertex vertex in VisitedGraph.Vertices)
            {
                VerticesColors[vertex] = GraphColor.Black;
            }
        }

        #endregion
    }
}