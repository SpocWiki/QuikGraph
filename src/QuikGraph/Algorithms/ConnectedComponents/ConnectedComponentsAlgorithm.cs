﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using QuikGraph.Algorithms.Search;
using QuikGraph.Algorithms.Services;

namespace QuikGraph.Algorithms.ConnectedComponents
{
    /// <summary> computes connected components of a graph. </summary>
    public sealed class ConnectedComponentsAlgorithm<TVertex, TEdge>
        : AlgorithmBase<IUndirectedGraph<TVertex, TEdge>>
        , IConnectedComponentAlgorithm<TVertex, TEdge, IUndirectedGraph<TVertex, TEdge>>
        where TEdge : IEdge<TVertex>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedComponentsAlgorithm{TVertex,TEdge}"/> class.
        /// </summary>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
        public ConnectedComponentsAlgorithm([NotNull] IUndirectedGraph<TVertex, TEdge> visitedGraph)
            : this(visitedGraph, new Dictionary<TVertex, int>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedComponentsAlgorithm{TVertex,TEdge}"/> class.
        /// </summary>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="components">Graph components.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="components"/> is <see langword="null"/>.</exception>
        public ConnectedComponentsAlgorithm(
            [NotNull] IUndirectedGraph<TVertex, TEdge> visitedGraph,
            [NotNull] IDictionary<TVertex, int> components)
            : this(visitedGraph, components, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedComponentsAlgorithm{TVertex,TEdge}"/> class.
        /// </summary>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="components">Graph components.</param>
        /// <param name="host">Host to use if set, otherwise use this reference.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="components"/> is <see langword="null"/>.</exception>
        public ConnectedComponentsAlgorithm([NotNull] IUndirectedGraph<TVertex, TEdge> visitedGraph,
            [NotNull] IDictionary<TVertex, int> components, [CanBeNull] IAlgorithmComponent host = null)
            : base(visitedGraph, host)
        {
            Components = components ?? throw new ArgumentNullException(nameof(components));
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
                dfs = new UndirectedDepthFirstSearchAlgorithm<TVertex, TEdge>(VisitedGraph,
                    new Dictionary<TVertex, GraphColor>(VisitedGraph.VertexCount), this);

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