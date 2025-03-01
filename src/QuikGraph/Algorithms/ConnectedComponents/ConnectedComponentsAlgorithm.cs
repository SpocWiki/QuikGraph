using System.Collections.Generic;
using JetBrains.Annotations;
using QuikGraph.Algorithms.Search;
using QuikGraph.Algorithms.Services;

namespace QuikGraph.Algorithms.ConnectedComponents
{
    /// <inheritdoc cref="CreateConnectedComponentsAlgorithm{TVertex,TEdge}"/>
    public static class ConnectedComponentsAlgorithm
    {
        /// <summary> Initializes a new instance of the <see cref="ConnectedComponentsAlgorithm{TVertex,TEdge}"/> class. </summary>
        public static ConnectedComponentsAlgorithm<TVertex, TEdge>
            CreateConnectedComponentsAlgorithm<TVertex, TEdge>(
                [NotNull] this IUndirectedGraph<TVertex, TEdge> visitedGraph,
                [CanBeNull] IDictionary<TVertex, int> components = null,
                [CanBeNull] IAlgorithmComponent host = null) where TEdge : IEdge<TVertex>
            => new ConnectedComponentsAlgorithm<TVertex, TEdge>(visitedGraph, components, host);
    }

    /// <summary> computes connected components of a graph. </summary>
    public sealed class ConnectedComponentsAlgorithm<TVertex, TEdge>
        : AlgorithmBase<IUndirectedGraph<TVertex, TEdge>>
        , IConnectedComponentAlgorithm<TVertex, TEdge, IUndirectedGraph<TVertex, TEdge>>
        where TEdge : IEdge<TVertex>
    {
        /// <summary> Initializes a new instance of the <see cref="ConnectedComponentsAlgorithm{TVertex,TEdge}"/> class. </summary>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="components">Graph components.</param>
        /// <param name="host">Host to use if set, otherwise use this reference.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="components"/> is <see langword="null"/>.</exception>
        internal ConnectedComponentsAlgorithm([NotNull] IUndirectedGraph<TVertex, TEdge> visitedGraph,
            [CanBeNull] IDictionary<TVertex, int> components = null,
            [CanBeNull] IAlgorithmComponent host = null)
            : base(visitedGraph, host)
        {
            Components = components ?? new Dictionary<TVertex, int>();
        }

        #region AlgorithmBase<TGraph>

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            Components.Clear();
            ComponentCount = 0;
        }

        /// <inheritdoc />
        protected override void InternalCompute()
        {
            if (VisitedGraph.VertexCount == 0)
                return;

            ComponentCount = -1;
            UndirectedDepthFirstSearchAlgorithm<TVertex, TEdge> dfs = null;
            try
            {
                dfs = VisitedGraph.CreateUndirectedDepthFirstSearchAlgorithm(null, this);

                dfs.StartVertex += OnStartVertex;
                dfs.DiscoverVertex += OnVertexDiscovered;
                dfs.Compute();
                ++ComponentCount;
            }
            finally
            {
                if (dfs != null)
                {
                    dfs.StartVertex -= OnStartVertex;
                    dfs.DiscoverVertex -= OnVertexDiscovered;
                }
            }
        }

        #endregion

        #region IConnectedComponentAlgorithm<TVertex,TEdge,TGraph>

        /// <inheritdoc />
        public int ComponentCount { get; private set; }

        /// <inheritdoc />
        public IDictionary<TVertex, int> Components { get; }

        #endregion

        private void OnStartVertex([NotNull] TVertex vertex)
        {
            ++ComponentCount;
        }

        private void OnVertexDiscovered([NotNull] TVertex vertex)
        {
            Components[vertex] = ComponentCount;
        }
    }
}