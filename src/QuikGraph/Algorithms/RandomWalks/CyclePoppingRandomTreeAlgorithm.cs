using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using QuikGraph.Algorithms.Services;
using QuikGraph.Utils;

namespace QuikGraph.Algorithms.RandomWalks
{
    /// <inheritdoc cref="CreateCyclePoppingRandomTreeAlgorithm{TVertex,TEdge}"/>
    public static class CyclePoppingRandomTreeAlgorithm
    {
        /// <summary> Computes a cycle popping tree
        /// and gets a function that allow to get edges connected to a vertex in a directed graph. </summary>
        /// <remarks>Uses <see cref="CyclePoppingRandomTreeAlgorithm{TVertex,TEdge}"/> algorithm and
        /// <see cref="NormalizedMarkovEdgeChain{TVertex,TEdge}"/>.</remarks>
        /// <param name="graph">The graph to visit.</param>
        /// <param name="root">Starting vertex.</param>
        /// <returns>A function that allow to get edges connected to a given vertex.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="root"/> is not part of <paramref name="graph"/>.</exception>
        [Pure]
        [NotNull]
        public static TryFunc<TVertex, IEnumerable<TEdge>> TreeCyclePoppingRandom<TVertex, TEdge>(
            [NotNull] this IVertexListGraph<TVertex, TEdge> graph,
            [NotNull] TVertex root) where TEdge : IEdge<TVertex>
            => graph.TreeCyclePoppingRandom(root, new NormalizedMarkovEdgeChain<TVertex, TEdge>());

        /// <summary> Computes a cycle popping tree
        /// and gets a function that allow to get edges connected to a vertex in a directed graph. </summary>
        /// <remarks>Uses <see cref="CyclePoppingRandomTreeAlgorithm{TVertex,TEdge}"/> algorithm.</remarks>
        /// <param name="graph">The graph to visit.</param>
        /// <param name="root">Starting vertex.</param>
        /// <param name="edgeChain">Markov edge chain.</param>
        /// <returns>A function that allow to get edges connected to a given vertex.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeChain"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="root"/> is not part of <paramref name="graph"/>.</exception>
        /// <exception cref="T:System.InvalidOperationException">Something went wrong when running the algorithm.</exception>
        [Pure]
        [NotNull]
        public static TryFunc<TVertex, IEnumerable<TEdge>> TreeCyclePoppingRandom<TVertex, TEdge>(
            [NotNull] this IVertexListGraph<TVertex, TEdge> graph,
            [NotNull] TVertex root,
            [NotNull] IMarkovEdgeChain<TVertex, TEdge> edgeChain)
            where TEdge : IEdge<TVertex>
            => graph.CreateCyclePoppingRandomTreeAlgorithm<TVertex, TEdge>(edgeChain)
                .RunDirectedRootedAlgorithm<TVertex, TEdge, CyclePoppingRandomTreeAlgorithm<TVertex, TEdge>>(root);

        /// <summary> Initializes a new instance of the <see cref="CyclePoppingRandomTreeAlgorithm{TVertex,TEdge}"/> class. </summary>
        public static CyclePoppingRandomTreeAlgorithm<TVertex, TEdge>
            CreateCyclePoppingRandomTreeAlgorithm<TVertex, TEdge>(
                [NotNull] this IVertexListGraph<TVertex, TEdge> visitedGraph,
                [CanBeNull] IMarkovEdgeChain<TVertex, TEdge> edgeChain = null,
                [CanBeNull] IAlgorithmComponent host = null) where TEdge : IEdge<TVertex>
            => new CyclePoppingRandomTreeAlgorithm<TVertex, TEdge>(visitedGraph, edgeChain, host);

    }

    /// <summary> Wilson-Propp Cycle-Popping algorithm for Random Tree Generation. </summary>
    public sealed class CyclePoppingRandomTreeAlgorithm<TVertex, TEdge>
        : RootedAlgorithmBase<TVertex, IVertexListGraph<TVertex, TEdge>>
        , IVertexColorizerAlgorithm<TVertex>
        , ITreeBuilderAlgorithm<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        /// <summary> Initializes a new instance of the <see cref="CyclePoppingRandomTreeAlgorithm{TVertex,TEdge}"/> class. </summary>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="edgeChain">Edge chain strategy to use.</param>
        /// <param name="host">Host to use if set, otherwise use this reference.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeChain"/> is <see langword="null"/>.</exception>
        internal CyclePoppingRandomTreeAlgorithm([NotNull] IVertexListGraph<TVertex, TEdge> visitedGraph,
            [CanBeNull] IMarkovEdgeChain<TVertex, TEdge> edgeChain = null, [CanBeNull] IAlgorithmComponent host = null)
            : base(visitedGraph, host)
        {
            EdgeChain = edgeChain ?? new NormalizedMarkovEdgeChain<TVertex, TEdge>();
        }

        /// <summary>
        /// Stores vertices associated to their colors (treatment state).
        /// </summary>
        [NotNull]
        public IDictionary<TVertex, GraphColor> VerticesColors { get; } = new Dictionary<TVertex, GraphColor>();

        #region IVertexColorizerAlgorithm<TVertex>

        /// <inheritdoc />
        public GraphColor GetVertexColor(TVertex vertex)
        {
            if (VerticesColors.TryGetValue(vertex, out GraphColor color))
                return color;
            throw new VertexNotFoundException();
        }

        #endregion

        /// <summary>
        /// Edge chain strategy for the random walk.
        /// </summary>
        [NotNull]
        public IMarkovEdgeChain<TVertex, TEdge> EdgeChain { get; }

        [NotNull]
        private Random _rand = new CryptoRandom((int)DateTime.Now.Ticks);

        /// <summary>
        /// Gets or sets the random number generator used in <see cref="RandomTree"/>.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException">Set value is <see langword="null"/>.</exception>
        [NotNull]
        public Random Rand
        {
            get => _rand;
            set => _rand = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Map vertices associated to their edge successors.
        /// </summary>
        [NotNull]
        public IDictionary<TVertex, TEdge> Successors { get; } = new Dictionary<TVertex, TEdge>();

        #region Events

        /// <summary>
        /// Fired when a vertex is initialized.
        /// </summary>
        public event VertexAction<TVertex> InitializeVertex;

        private void OnInitializeVertex([NotNull] TVertex vertex)
        {
            Debug.Assert(vertex != null);

            InitializeVertex?.Invoke(vertex);
        }

        /// <summary>
        /// Fired when a vertex is treated and considered as in the random tree.
        /// </summary>
        public event VertexAction<TVertex> FinishVertex;

        private void OnFinishVertex([NotNull] TVertex vertex)
        {
            Debug.Assert(vertex != null);

            FinishVertex?.Invoke(vertex);
        }

        /// <inheritdoc />
        public event EdgeAction<TVertex, TEdge> TreeEdge;

        private void OnTreeEdge([NotNull] TEdge edge)
        {
            Debug.Assert(edge != null);

            TreeEdge?.Invoke(edge);
        }

        /// <summary>
        /// Fired when a vertex is removed from the random tree.
        /// </summary>
        public event VertexAction<TVertex> ClearTreeVertex;

        private void OnClearTreeVertex([NotNull] TVertex vertex)
        {
            Debug.Assert(vertex != null);

            ClearTreeVertex?.Invoke(vertex);
        }

        #endregion

        #region AlgorithmBase<TGraph>

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            Successors.Clear();
            VerticesColors.Clear();
            foreach (TVertex vertex in VisitedGraph.Vertices)
            {
                VerticesColors.Add(vertex, GraphColor.White);
                OnInitializeVertex(vertex);
            }
        }

        /// <inheritdoc />
        protected override void InternalCompute()
        {
            TVertex root = GetAndAssertRootInGraph();

            // Process root
            ClearTree(root);
            SetInTree(root);

            foreach (TVertex vertex in VisitedGraph.Vertices)
            {
                ThrowIfCancellationRequested();

                // First pass: exploration
                Explore(vertex);

                // Second pass: coloration
                Colorize(vertex);
            }
        }

        private void Explore([NotNull] TVertex vertex)
        {
            Debug.Assert(vertex != null);

            var visitedEdges = new Dictionary<TEdge, int>();
            TVertex current = vertex;
            while (NotInTree(current) && TryGetSuccessor(visitedEdges, current, out TEdge successor))
            {
                visitedEdges[successor] = 0;
                Tree(current, successor);
                if (!TryGetNextInTree(current, out current))
                    break;
            }
        }

        [Pure]
        private bool Explore(double eps, [NotNull] TVertex vertex, ref int numRoots)
        {
            Debug.Assert(vertex != null);

            var visited = new Dictionary<TEdge, int>();
            TVertex current = vertex;
            while (NotInTree(current))
            {
                if (Chance(eps))
                {
                    ClearTree(current);
                    SetInTree(current);
                    ++numRoots;
                    if (numRoots > 1)
                        return false;
                }
                else
                {
                    if (!TryGetSuccessor(visited, current, out TEdge successor))
                        break;

                    visited[successor] = 0;
                    Tree(current, successor);
                    if (!TryGetNextInTree(current, out current))
                        break;
                }
            }

            return true;
        }

        private void Colorize([NotNull] TVertex vertex)
        {
            Debug.Assert(vertex != null);

            TVertex current = vertex;
            while (NotInTree(current))
            {
                SetInTree(current);
                if (!TryGetNextInTree(current, out current))
                    break;
            }
        }

        #endregion

        [Pure]
        private bool NotInTree([NotNull] TVertex vertex)
        {
            return VerticesColors[vertex] == GraphColor.White;
        }

        private void SetInTree([NotNull] TVertex vertex)
        {
            VerticesColors[vertex] = GraphColor.Black;
            OnFinishVertex(vertex);
        }

        [Pure]
        private bool TryGetSuccessor([NotNull] IDictionary<TEdge, int> visited, [NotNull] TVertex vertex, out TEdge successor)
        {
            IEnumerable<TEdge> outEdges = VisitedGraph.OutEdges(vertex);
            IEnumerable<TEdge> edges = outEdges.Where(edge => !visited.ContainsKey(edge));
            return EdgeChain.TryGetSuccessor(edges, vertex, out successor);
        }

        private void Tree([NotNull] TVertex vertex, [NotNull] TEdge next)
        {
            Debug.Assert(vertex != null);
            Debug.Assert(next != null);

            Successors[vertex] = next;
            OnTreeEdge(next);
        }

        [Pure]
        private bool TryGetNextInTree([NotNull] TVertex vertex, out TVertex next)
        {
            if (Successors.TryGetValue(vertex, out TEdge nextEdge))
            {
                next = nextEdge.Target;
                return true;
            }

            next = default(TVertex);
            return false;
        }

        [Pure]
        private bool Chance(double eps)
        {
            return Rand.NextDouble() <= eps;
        }

        private void ClearTree([NotNull] TVertex vertex)
        {
            Successors[vertex] = default(TEdge);
            OnClearTreeVertex(vertex);
        }

        [Pure]
        private bool Attempt(double epsilon)
        {
            Initialize();
            int numRoots = 0;

            foreach (TVertex vertex in VisitedGraph.Vertices)
            {
                ThrowIfCancellationRequested();

                // First pass: exploration
                if (!Explore(epsilon, vertex, ref numRoots))
                    return false;

                // Second pass: coloration
                Colorize(vertex);
            }

            return true;
        }

        /// <summary>
        /// Runs a random tree generation starting at <paramref name="root"/> vertex.
        /// </summary>
        /// <param name="root">Tree starting vertex.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="root"/> is part of <see cref="AlgorithmBase{TGraph}.VisitedGraph"/>.</exception>
        public void RandomTreeWithRoot([NotNull] TVertex root)
        {
            if (!VisitedGraph.ContainsVertex(root))
                throw new ArgumentException("The vertex must be in the graph.", nameof(root));

            SetRootVertex(root);
            Compute();
        }

        /// <summary>
        /// Runs a random tree generation.
        /// </summary>
        public void RandomTree()
        {
            double epsilon = 1;
            bool success;
            do
            {
                ThrowIfCancellationRequested();

                epsilon /= 2;
                success = Attempt(epsilon);
            } while (!success);
        }
    }
}