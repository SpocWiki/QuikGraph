﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using QuikGraph.Algorithms.Search;
using QuikGraph.Algorithms.Services;

namespace QuikGraph.Algorithms.ConnectedComponents
{
    /// <summary> Extension Methods for <see cref="ConnectedComponentsAlgorithm{TVertex,TEdge}"/> </summary>
    public static class ConnectedComponentsAlgorithm{

        /// <summary> Creates an <see cref="ConnectedComponentsAlgorithm{TVertex,TEdge}"/> for <paramref name="undirectedGraph"/> </summary>
        public static ConnectedComponentsAlgorithm<TVertex, TEdge> CreateConnectedComponentsAlgorithm<TVertex, TEdge>(this IUndirectedGraph<TVertex, TEdge> undirectedGraph)
            where TEdge : IEdge<TVertex> => new ConnectedComponentsAlgorithm<TVertex, TEdge>(undirectedGraph);

        /// <summary> Aggregates the Number of Nodes for each Component </summary>
        /// <remarks>
        /// This is faster than aggregating the Number of Edges and since each connected Component with N+1 Vertices
        /// has at least N Edges. 
        /// </remarks>
        public static int[] NumVerticesInComponent<TVertex, TEdge>(this ConnectedComponentsAlgorithm<TVertex, TEdge> componentsAlgorithm)
            where TEdge : IEdge<TVertex>
        {
            var numVerticesInComponent = new int[componentsAlgorithm.ComponentCount];
            foreach (KeyValuePair<TVertex, int> indexOfVertex in componentsAlgorithm.ComponentIndex)
            {
                ++numVerticesInComponent[indexOfVertex.Value];
            }

            return numVerticesInComponent;
        }

        /// <summary> Aggregates the Number of Nodes for each Component </summary>
        /// <remarks>
        /// This is faster than aggregating the Number of Edges and since each connected Component with N+1 Vertices
        /// has at least N Edges. 
        /// </remarks>
        public static int[] NumEdgesInComponent<TVertex, TEdge>(this ConnectedComponentsAlgorithm<TVertex, TEdge> componentsAlgorithm)
            where TEdge : IEdge<TVertex>
        {
            var numVerticesInComponent = new int[componentsAlgorithm.ComponentCount];
            foreach (KeyValuePair<TVertex, int> indexOfVertex in componentsAlgorithm.ComponentIndex)
            {
                numVerticesInComponent[indexOfVertex.Value] += componentsAlgorithm.VisitedGraph.AdjacentEdges(indexOfVertex.Key)?.Count() ?? 0;
            }

            return numVerticesInComponent;
        }

    }
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
            : this(null, visitedGraph, components)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedComponentsAlgorithm{TVertex,TEdge}"/> class.
        /// </summary>
        /// <param name="host">Host to use if set, otherwise use this reference.</param>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="components">Graph components.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="components"/> is <see langword="null"/>.</exception>
        public ConnectedComponentsAlgorithm(
            [CanBeNull] IAlgorithmComponent host,
            [NotNull] IUndirectedGraph<TVertex, TEdge> visitedGraph,
            [NotNull] IDictionary<TVertex, int> components)
            : base(visitedGraph, host)
        {
            ComponentIndex = components ?? throw new ArgumentNullException(nameof(components));
        }

        #region AlgorithmBase<TGraph>

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            ComponentIndex.Clear();
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
                dfs = new UndirectedDepthFirstSearchAlgorithm<TVertex, TEdge>(
                    this,
                    VisitedGraph,
                    new Dictionary<TVertex, GraphColor>(VisitedGraph.VertexCount));

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
        public IDictionary<TVertex, int> ComponentIndex { get; }

        #endregion

        private void OnStartVertex([NotNull] TVertex vertex)
        {
            ++ComponentCount;
        }

        private void OnVertexDiscovered([NotNull] TVertex vertex)
        {
            ComponentIndex[vertex] = ComponentCount;
        }
    }
}