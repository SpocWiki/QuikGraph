using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Search;
using QuikGraph.Algorithms.Services;
using QuikGraph.Collections;

namespace QuikGraph.Algorithms.ShortestPath
{
    /// <inheritdoc cref="CreateUndirectedDijkstraShortestPathAlgorithm{TVertex,TEdge}"/>
    public static class UndirectedDijkstraShortestPathAlgorithm
    {
        /// <summary> Initializes a new instance of the <see cref="UndirectedDijkstraShortestPathAlgorithm{TVertex,TEdge}"/> class. </summary>
        public static UndirectedDijkstraShortestPathAlgorithm<TVertex, TEdge>
            CreateUndirectedDijkstraShortestPathAlgorithm<TVertex, TEdge>(
                [NotNull] this IUndirectedGraph<TVertex, TEdge> visitedGraph,
                [NotNull] Func<TEdge, double> edgeWeights,
                [CanBeNull] IDistanceRelaxer distanceRelaxer = null,
                [CanBeNull] IAlgorithmComponent host = null) where TEdge : IEdge<TVertex>
            => new UndirectedDijkstraShortestPathAlgorithm<TVertex, TEdge>(visitedGraph, edgeWeights, distanceRelaxer, host);

        /// <summary>
        /// Computes shortest path with the Dijkstra algorithm and gets a function that allows
        /// to get paths in an undirected graph.
        /// </summary>
        /// <remarks>Uses <see cref="UndirectedDijkstraShortestPathAlgorithm{TVertex,TEdge}"/> algorithm.</remarks>
        /// <param name="graph">The graph to visit.</param>
        /// <param name="edgeWeights">Function that computes the weight for a given edge.</param>
        /// <param name="root">Starting vertex.</param>
        /// <returns>A function that allow to get paths starting from <paramref name="root"/> vertex.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeWeights"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="root"/> is not part of <paramref name="graph"/>.</exception>
        [Pure]
        [NotNull]
        public static TryFunc<TVertex, IEnumerable<TEdge>> ShortestPathsDijkstra<TVertex, TEdge>(
            [NotNull] this IUndirectedGraph<TVertex, TEdge> graph,
            [NotNull, InstantHandle] Func<TEdge, double> edgeWeights,
            [NotNull] TVertex root)
            where TEdge : IEdge<TVertex>
        {
            var algorithm = graph.CreateUndirectedDijkstraShortestPathAlgorithm(edgeWeights);
            var predecessorRecorder = new UndirectedVertexPredecessorRecorderObserver<TVertex, TEdge>();
            using (predecessorRecorder.Attach(algorithm))
            {
                algorithm.Compute(root);
            }

            IDictionary<TVertex, TEdge> predecessors = predecessorRecorder.VerticesPredecessors;
            return (TVertex vertex, out IEnumerable<TEdge> edges) => predecessors.TryGetPath(vertex, out edges);
        }

        /// <summary> Computes the minimum spanning tree using Prim algorithm. </summary>
        /// <remarks>Prim algorithm is simply implemented by calling Dijkstra shortest path.</remarks>
        /// <param name="graph">Graph to visit.</param>
        /// <param name="edgeWeights">Function that computes the weight for a given edge.</param>
        /// <returns>Edges part of the minimum spanning tree.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeWeights"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull, ItemNotNull]
        public static IEnumerable<TEdge> MinimumSpanningTreePrim<TVertex, TEdge>(
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

            IDistanceRelaxer distanceRelaxer = DistanceRelaxers.Prim;
            var dijkstra = graph.CreateUndirectedDijkstraShortestPathAlgorithm(edgeWeights, distanceRelaxer);
            var edgeRecorder = new UndirectedVertexPredecessorRecorderObserver<TVertex, TEdge>();
            using (edgeRecorder.Attach(dijkstra))
            {
                dijkstra.Compute();
            }

            return edgeRecorder.VerticesPredecessors.Values;
        }

    }

    /// <summary> A single source shortest path algorithm for undirected graph with positive distances. </summary>
    public sealed class UndirectedDijkstraShortestPathAlgorithm<TVertex, TEdge>
        : UndirectedShortestPathAlgorithmBase<TVertex, TEdge>
        , IUndirectedVertexPredecessorRecorderAlgorithm<TVertex, TEdge>
        , IDistanceRecorderAlgorithm<TVertex>
        where TEdge : IEdge<TVertex>
    {
        private IPriorityQueue<TVertex> _vertexQueue;

        /// <summary> Initializes a new instance of the <see cref="UndirectedDijkstraShortestPathAlgorithm{TVertex,TEdge}"/> class. </summary>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="edgeWeights">Function that computes the weight for a given edge.</param>
        /// <param name="distanceRelaxer">Distance relaxer.</param>
        /// <param name="host">Host to use if set, otherwise use this reference.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeWeights"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="distanceRelaxer"/> is <see langword="null"/>.</exception>
        internal UndirectedDijkstraShortestPathAlgorithm([NotNull] IUndirectedGraph<TVertex, TEdge> visitedGraph,
            [NotNull] Func<TEdge, double> edgeWeights,
            [CanBeNull] IDistanceRelaxer distanceRelaxer = null,
            [CanBeNull] IAlgorithmComponent host = null)
            : base(visitedGraph, edgeWeights, distanceRelaxer, host)
        {
        }

        [Conditional("DEBUG")]
        private void AssertHeap()
        {
            if (_vertexQueue.Count == 0)
                return;

            TVertex top = _vertexQueue.Peek();
            TVertex[] vertices = _vertexQueue.ToArray();
            for (int i = 1; i < vertices.Length; ++i)
            {
                if (GetVertexDistance(top)> GetVertexDistance(vertices[i]))
                    Debug.Assert(false);
            }
        }

        #region Events

        /// <inheritdoc />
        public event VertexAction<TVertex> InitializeVertex;

        /// <inheritdoc />
        public event VertexAction<TVertex> StartVertex;

        /// <inheritdoc />
        public event VertexAction<TVertex> DiscoverVertex;

        /// <summary>
        /// Fired when a vertex is going to be analyzed.
        /// </summary>
        public event VertexAction<TVertex> ExamineVertex;

        /// <summary>
        /// Fired when an edge is going to be analyzed.
        /// </summary>
        public event EdgeAction<TVertex, TEdge> ExamineEdge;

        /// <inheritdoc />
        public event VertexAction<TVertex> FinishVertex;

        /// <summary>
        /// Fired when relax of an edge does not decrease distance.
        /// </summary>
        public event UndirectedEdgeAction<TVertex, TEdge> EdgeNotRelaxed;

        private void OnEdgeNotRelaxed([NotNull] TEdge edge, bool reversed)
        {
            Debug.Assert(edge != null);

            EdgeNotRelaxed?.Invoke(this, new UndirectedEdgeEventArgs<TVertex, TEdge>(edge, reversed));
        }

        private void OnDijkstraTreeEdge(object sender, UndirectedEdgeEventArgs<TVertex, TEdge> args)
        {
            Debug.Assert(args != null);

            bool decreased = Relax(args.Edge, args.Source, args.Target);
            if (decreased)
                OnTreeEdge(args.Edge, args.Reversed);
            else
                OnEdgeNotRelaxed(args.Edge, args.Reversed);
        }

        private void OnGrayTarget(object sender, UndirectedEdgeEventArgs<TVertex, TEdge> args)
        {
            Debug.Assert(args != null);

            bool decreased = Relax(args.Edge, args.Source, args.Target);
            if (decreased)
            {
                _vertexQueue.Update(args.Target);
                AssertHeap();
                OnTreeEdge(args.Edge, args.Reversed);
            }
            else
            {
                OnEdgeNotRelaxed(args.Edge, args.Reversed);
            }
        }

        #endregion

        #region AlgorithmBase<TGraph>

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            double initialDistance = DistanceRelaxer.InitialDistance;
            // Initialize colors and distances
            foreach (TVertex vertex in VisitedGraph.Vertices)
            {
                VerticesColors.Add(vertex, GraphColor.White);
                SetVertexDistance(vertex, initialDistance);
            }

            _vertexQueue = new FibonacciQueue<TVertex, double>(DistancesIndexGetter());
        }

        /// <inheritdoc />
        protected override void InternalCompute()
        {
            if (TryGetRootVertex(out TVertex root))
            {
                AssertRootInGraph(root);
                ComputeFromRoot(root);
            }
            else
            {
                foreach (TVertex vertex in VisitedGraph.Vertices)
                {
                    if (VerticesColors[vertex] == GraphColor.White)
                    {
                        ComputeFromRoot(vertex);
                    }
                }
            }
        }

        #endregion

        private void ComputeFromRoot([NotNull] TVertex rootVertex)
        {
            Debug.Assert(rootVertex != null);
            Debug.Assert(VisitedGraph.ContainsVertex(rootVertex));
            Debug.Assert(VerticesColors[rootVertex] == GraphColor.White);

            VerticesColors[rootVertex] = GraphColor.Gray;
            SetVertexDistance(rootVertex, 0);
            ComputeNoInit(rootVertex);
        }

        private void ComputeNoInit([NotNull] TVertex root)
        {
            Debug.Assert(root != null);
            Debug.Assert(VisitedGraph.ContainsVertex(root));

            UndirectedBreadthFirstSearchAlgorithm<TVertex, TEdge> bfs = null;
            try
            {
                bfs = new UndirectedBreadthFirstSearchAlgorithm<TVertex, TEdge>(VisitedGraph,
                    _vertexQueue,
                    VerticesColors, this);

                bfs.InitializeVertex += InitializeVertex;
                bfs.DiscoverVertex += DiscoverVertex;
                bfs.StartVertex += StartVertex;
                bfs.ExamineEdge += ExamineEdge;
#if DEBUG
                bfs.ExamineEdge += edge => AssertHeap();
#endif
                bfs.ExamineVertex += ExamineVertex;
                bfs.FinishVertex += FinishVertex;

                bfs.TreeEdge += OnDijkstraTreeEdge;
                bfs.GrayTarget += OnGrayTarget;

                bfs.Visit(root);
            }
            finally
            {
                if (bfs != null)
                {
                    bfs.InitializeVertex -= InitializeVertex;
                    bfs.DiscoverVertex -= DiscoverVertex;
                    bfs.StartVertex -= StartVertex;
                    bfs.ExamineEdge -= ExamineEdge;
                    bfs.ExamineVertex -= ExamineVertex;
                    bfs.FinishVertex -= FinishVertex;

                    bfs.TreeEdge -= OnDijkstraTreeEdge;
                    bfs.GrayTarget -= OnGrayTarget;
                }
            }
        }
    }
}