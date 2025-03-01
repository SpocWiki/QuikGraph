using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#if SUPPORTS_TYPE_FULL_FEATURES
using System.Reflection;
#else
using QuikGraph.Utils;
#endif
#if SUPPORTS_AGGRESSIVE_INLINING
using System.Runtime.CompilerServices;
#endif
using JetBrains.Annotations;
using QuikGraph.Algorithms.Condensation;
using QuikGraph.Algorithms.ConnectedComponents;
using QuikGraph.Algorithms.MaximumFlow;
using QuikGraph.Algorithms.MinimumSpanningTree;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.RandomWalks;
using QuikGraph.Algorithms.RankedShortestPath;
using QuikGraph.Algorithms.Search;
using QuikGraph.Algorithms.ShortestPath;
using QuikGraph.Algorithms.TopologicalSort;
using QuikGraph.Collections;

namespace QuikGraph.Algorithms
{
    /// <summary> Extensions related to algorithms, to run them. </summary>
    public static class AlgorithmExtensions
    {
        /// <summary> Returns the method that implement the access indexer. </summary>
        /// <param name="dictionary">Dictionary on which getting the key access method.</param>
        /// <returns>A function allowing key indexed access.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="dictionary"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull]
        public static Func<TKey, TValue> GetIndexer<TKey, TValue>([NotNull] IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

#if SUPPORTS_TYPE_FULL_FEATURES
            // ReSharper disable once PossibleNullReferenceException, Justification: Dictionary has the [] operator called "Item".
            MethodInfo method = dictionary.GetType().GetProperty("Item").GetGetMethod();
            // ReSharper disable once AssignNullToNotNullAttribute, Justification: Throws if the method is not found.
            return (Func<TKey, TValue>)Delegate.CreateDelegate(typeof(Func<TKey, TValue>), dictionary, method, true);
#else
            return key => dictionary[key];
#endif
        }

        /// <summary>
        /// Gets the vertex identity.
        /// </summary>
        /// <remarks>
        /// Returns more efficient methods for primitive types,
        /// otherwise builds a dictionary.
        /// </remarks>
        /// <param name="graph">The graph.</param>
        /// <returns>A function that computes a vertex identity for the given <paramref name="graph"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull]
        public static VertexIdentity<TVertex> GetVertexIdentity<TVertex>([NotNull] this IVertexSet<TVertex> graph)
        {
            if (graph is null)
                throw new ArgumentNullException(nameof(graph));

            // Simpler identity for primitive types
#if SUPPORTS_TYPE_FULL_FEATURES
            switch (Type.GetTypeCode(typeof(TVertex)))
#else
            switch (TypeUtils.GetTypeCode(typeof(TVertex)))
#endif
            {
                case TypeCode.String:
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return vertex => vertex.ToString();
            }

            // Create dictionary
            var ids = new Dictionary<TVertex, string>(graph.VertexCount);
            return vertex =>
            {
                if (!ids.TryGetValue(vertex, out string id))
                {
                    ids[vertex] = id = ids.Count.ToString();
                }
                return id;
            };
        }

        /// <summary>
        /// Gets the edge identity.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <returns>A function that computes an edge identity for the given <paramref name="graph"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull]
        public static EdgeIdentity<TVertex, TEdge> GetEdgeIdentity<TVertex, TEdge>([NotNull] this IEdgeSet<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            if (graph is null)
                throw new ArgumentNullException(nameof(graph));

            // Create dictionary
            var ids = new Dictionary<TEdge, string>(graph.EdgeCount);
            return edge =>
            {
                if (!ids.TryGetValue(edge, out string id))
                {
                    ids[edge] = id = ids.Count.ToString();
                }
                return id;
            };
        }

        /// <summary>
        /// 
        /// </summary>
        [Pure]
        [NotNull]
        public static TryFunc<TVertex, IEnumerable<TEdge>> RunDirectedRootedAlgorithm<TVertex, TEdge, TAlgorithm>(
            [NotNull] this TAlgorithm algorithm, [NotNull] TVertex source)
            where TEdge : IEdge<TVertex>
            where TAlgorithm : RootedAlgorithmBase<TVertex, IVertexListGraph<TVertex, TEdge>>, ITreeBuilderAlgorithm<TVertex, TEdge>
        {
            Debug.Assert(algorithm != null);

            var predecessorRecorder = algorithm.AttachVertexPredecessorRecorderObserver();
            using (predecessorRecorder)
            {
                algorithm.Compute(source);
            }

            IDictionary<TVertex, TEdge> predecessors = predecessorRecorder.VerticesPredecessors;
            return (TVertex vertex, out IEnumerable<TEdge> edges) => predecessors.TryGetPath(vertex, out edges);
        }

        /// <summary>Computes a breadth first tree and gets a function
        /// that allow to get edges connected to a vertex in a directed graph. </summary>
        /// <remarks>Uses <see cref="BreadthFirstSearchAlgorithm{TVertex,TEdge}"/> algorithm.</remarks>
        /// <param name="graph">The graph to visit.</param>
        /// <param name="root">Starting vertex.</param>
        /// <returns>A function that allow to get edges connected to a given vertex.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="root"/> is not part of <paramref name="graph"/>.</exception>
        [Pure]
        [NotNull]
        public static TryFunc<TVertex, IEnumerable<TEdge>> TreeBreadthFirstSearch<TVertex, TEdge>(
            [NotNull] this IVertexListGraph<TVertex, TEdge> graph,
            [NotNull] TVertex root)
            where TEdge : IEdge<TVertex>
            => graph.CreateBreadthFirstSearchAlgorithm()
                .RunDirectedRootedAlgorithm<TVertex, TEdge, BreadthFirstSearchAlgorithm<TVertex, TEdge>>(root);

        /// <summary> Computes a depth first tree and gets a function
        /// that allow to get edges connected to a vertex in a directed graph. </summary>
        /// <remarks>Uses <see cref="DepthFirstSearchAlgorithm{TVertex,TEdge}"/> algorithm.</remarks>
        /// <param name="graph">The graph to visit.</param>
        /// <param name="root">Starting vertex.</param>
        /// <returns>A function that allow to get edges connected to a given vertex.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="root"/> is not part of <paramref name="graph"/>.</exception>
        [Pure]
        [NotNull]
        public static TryFunc<TVertex, IEnumerable<TEdge>> TreeDepthFirstSearch<TVertex, TEdge>(
            [NotNull] this IVertexListGraph<TVertex, TEdge> graph,
            [NotNull] TVertex root) where TEdge : IEdge<TVertex>
            => graph.CreateDepthFirstSearchAlgorithm()
                .RunDirectedRootedAlgorithm<TVertex, TEdge, DepthFirstSearchAlgorithm<TVertex, TEdge>>(root);

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
            => new CyclePoppingRandomTreeAlgorithm<TVertex, TEdge>(graph, edgeChain)
                .RunDirectedRootedAlgorithm<TVertex, TEdge, CyclePoppingRandomTreeAlgorithm<TVertex, TEdge>>(root);

        /// <summary>
        /// Gets set of sink vertices.
        /// </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <returns>Sink vertices.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull, ItemNotNull]
        public static IEnumerable<TVertex> Sinks<TVertex, TEdge>(
            [NotNull] this IVertexListGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            if (graph is null)
                throw new ArgumentNullException(nameof(graph));
            return graph.Vertices.Where(graph.IsOutEdgesEmpty);
        }

        /// <summary>
        /// Gets set of root vertices.
        /// </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <returns>Root vertices.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull, ItemNotNull]
        public static IEnumerable<TVertex> Roots<TVertex, TEdge>(
            [NotNull] this IVertexListGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            var dfs = graph.CreateDepthFirstSearchAlgorithm();
            var notRoots = new Dictionary<TVertex, bool>(graph.VertexCount);
            dfs.ExamineEdge += edge => notRoots[edge.Target] = false;
            dfs.Compute();

            foreach (TVertex vertex in graph.Vertices)
            {
                if (!notRoots.TryGetValue(vertex, out _))
                    yield return vertex;
            }
        }

        /// <summary>
        /// Gets set of root vertices.
        /// </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <returns>Root vertices.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull, ItemNotNull]
        public static IEnumerable<TVertex> Roots<TVertex, TEdge>(
            [NotNull] this IBidirectionalGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            if (graph is null)
                throw new ArgumentNullException(nameof(graph));
            return graph.Vertices.Where(graph.IsInEdgesEmpty);
        }

        /// <summary>
        /// Gets set of isolated vertices (no incoming nor outcoming vertices).
        /// </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <returns>Root vertices.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull, ItemNotNull]
        public static IEnumerable<TVertex> IsolatedVertices<TVertex, TEdge>(
            [NotNull] this IBidirectionalGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            if (graph is null)
                throw new ArgumentNullException(nameof(graph));
            return graph.Vertices.Where(vertex => graph.Degree(vertex) == 0);
        }

        #region Topological sorts

        /// <summary>
        /// Creates a topological sort of a directed acyclic graph.
        /// </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <returns>Sorted vertices (topological sort).</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="NonAcyclicGraphException">If the input graph has a cycle.</exception>
        [Pure]
        [NotNull, ItemNotNull]
        public static TVertex[] TopologicalSort<TVertex, TEdge>(
            [NotNull] this IVertexListGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            if (graph is null)
                throw new ArgumentNullException(nameof(graph));

            var algorithm = new TopologicalSortAlgorithm<TVertex, TEdge>(graph, graph.VertexCount);
            algorithm.Compute();
            return algorithm.SortedVertices;
        }

        /// <summary>
        /// Creates a topological sort of an undirected acyclic graph.
        /// </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <returns>Sorted vertices (topological sort).</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="NonAcyclicGraphException">If the input graph has a cycle.</exception>
        [Pure]
        [NotNull, ItemNotNull]
        public static TVertex[] TopologicalSort<TVertex, TEdge>(
            [NotNull] this IUndirectedGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            if (graph is null)
                throw new ArgumentNullException(nameof(graph));

            var algorithm = new UndirectedTopologicalSortAlgorithm<TVertex, TEdge>(graph, graph.VertexCount);
            algorithm.Compute();
            return algorithm.SortedVertices;
        }

        #endregion

        #region Connected components

        /// <summary>
        /// Computes the connected components of an undirected graph.
        /// </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <param name="components">Found components.</param>
        /// <returns>Number of component found.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="components"/> is <see langword="null"/>.</exception>
        public static int ConnectedComponents<TVertex, TEdge>(
            [NotNull] this IUndirectedGraph<TVertex, TEdge> graph,
            [NotNull] IDictionary<TVertex, int> components)
            where TEdge : IEdge<TVertex>
        {
            var algorithm = new ConnectedComponentsAlgorithm<TVertex, TEdge>(graph, components);
            algorithm.Compute();
            return algorithm.ComponentCount;
        }

        /// <summary>
        /// Computes the incremental connected components for a growing graph (edge added only).
        /// Each call to the delegate re-computes the component dictionary. The returned dictionary
        /// is shared across multiple calls of the method.
        /// </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <param name="getComponents">A function retrieve components of the <paramref name="graph"/>.</param>
        /// <returns>A <see cref="T:System.IDisposable"/> of the used algorithm.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull]
        public static IDisposable IncrementalConnectedComponents<TVertex, TEdge>(
            [NotNull] this IMutableVertexAndEdgeSet<TVertex, TEdge> graph,
            [NotNull] out Func<KeyValuePair<int, IDictionary<TVertex, int>>> getComponents)
            where TEdge : IEdge<TVertex>
        {
            var incrementalComponents = new IncrementalConnectedComponentsAlgorithm<TVertex, TEdge>(graph);
            incrementalComponents.Compute();
            getComponents = () => incrementalComponents.GetComponents();
            return incrementalComponents;
        }

        /// <summary>
        /// Computes the strongly connected components of a directed graph.
        /// </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <param name="components">Found components.</param>
        /// <returns>Number of component found.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="components"/> is <see langword="null"/>.</exception>
        public static int StronglyConnectedComponents<TVertex, TEdge>(
            [NotNull] this IVertexListGraph<TVertex, TEdge> graph,
            [NotNull] IDictionary<TVertex, int> components)
            where TEdge : IEdge<TVertex>
        {
            var algorithm = new StronglyConnectedComponentsAlgorithm<TVertex, TEdge>(graph, components);
            algorithm.Compute();
            return algorithm.ComponentCount;
        }

        /// <summary>
        /// Computes the weakly connected components of a directed graph.
        /// </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <param name="components">Found components.</param>
        /// <returns>Number of component found.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="components"/> is <see langword="null"/>.</exception>
        public static int WeaklyConnectedComponents<TVertex, TEdge>(
            [NotNull] this IVertexListGraph<TVertex, TEdge> graph,
            [NotNull] IDictionary<TVertex, int> components)
            where TEdge : IEdge<TVertex>
        {
            var algorithm = new WeaklyConnectedComponentsAlgorithm<TVertex, TEdge>(graph, components);
            algorithm.Compute();
            return algorithm.ComponentCount;
        }

        /// <summary>
        /// Condensates the strongly connected components of a directed graph.
        /// </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <returns>The condensed graph.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull]
        public static IMutableBidirectionalGraph<TGraph, CondensedEdge<TVertex, TEdge, TGraph>> CondensateStronglyConnected<TVertex, TEdge, TGraph>(
            [NotNull] this IVertexAndEdgeListGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
            where TGraph : IMutableVertexAndEdgeSet<TVertex, TEdge>, new()
        {
            var algorithm = new CondensationGraphAlgorithm<TVertex, TEdge, TGraph>(graph)
            {
                StronglyConnected = true
            };
            algorithm.Compute();
            return algorithm.CondensedGraph;
        }

        /// <summary> Condensates the weakly connected components of a directed graph. </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <returns>The condensed graph.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull]
        public static IMutableBidirectionalGraph<TGraph, CondensedEdge<TVertex, TEdge, TGraph>> CondensateWeaklyConnected<TVertex, TEdge, TGraph>(
            [NotNull] this IVertexAndEdgeListGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
            where TGraph : IMutableVertexAndEdgeSet<TVertex, TEdge>, new()
        {
            var algorithm = new CondensationGraphAlgorithm<TVertex, TEdge, TGraph>(graph)
            {
                StronglyConnected = false
            };
            algorithm.Compute();
            return algorithm.CondensedGraph;
        }

        #endregion

        /// <summary> Gets odd vertices of the given <paramref name="graph"/>. </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <returns>Enumerable of odd vertices.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull, ItemNotNull]
        public static IEnumerable<TVertex> OddVertices<TVertex, TEdge>(
            [NotNull] this IVertexAndEdgeListGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            if (graph is null)
                throw new ArgumentNullException(nameof(graph));

            var counts = new Dictionary<TVertex, int>(graph.VertexCount);
            foreach (TVertex vertex in graph.Vertices)
            {
                counts.Add(vertex, 0);
            }

            foreach (TEdge edge in graph.Edges)
            {
                ++counts[edge.Source];
                --counts[edge.Target];
            }

            // Odds
            return counts
                .Where(pair => pair.Value % 2 != 0)
                .Select(pair => pair.Key);
        }

        private sealed class DirectedCycleTester<TVertex, TEdge>
            where TEdge : IEdge<TVertex>
        {
            private bool _isDag = true;

            [Pure]
            public bool IsDag([NotNull] IVertexListGraph<TVertex, TEdge> graph)
            {
                Debug.Assert(graph != null);

                var dfs = graph.CreateDepthFirstSearchAlgorithm();
                try
                {
                    dfs.BackEdge += DfsBackEdge;
                    _isDag = true;
                    dfs.Compute();
                    return _isDag;
                }
                finally
                {
                    dfs.BackEdge -= DfsBackEdge;
                }
            }

            private void DfsBackEdge([NotNull] TEdge edge)
            {
                _isDag = false;
            }
        }

        [Pure]
#if SUPPORTS_AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool IsDirectedAcyclicGraphInternal<TVertex, TEdge>(
            [NotNull] this IVertexListGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            return new DirectedCycleTester<TVertex, TEdge>().IsDag(graph);
        }

        /// <summary>
        /// Checks whether the graph is acyclic or not.
        /// </summary>
        /// <remarks>
        /// Builds an <see cref="AdjacencyGraph{TVertex,TEdge}"/> from <paramref name="edges"/>
        /// and performs a depth first search to look for cycles.
        /// </remarks>
        /// <param name="edges">Edges of forming the graph to visit.</param>
        /// <returns>True if the graph contains a cycle, false otherwise.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="edges"/> is <see langword="null"/> or at least one of them is <see langword="null"/>.
        /// </exception>
        [Pure]
        public static bool IsDirectedAcyclicGraph<TVertex, TEdge>(
            [NotNull, ItemNotNull] this IEnumerable<TEdge> edges)
            where TEdge : IEdge<TVertex>
        {
            var graph = new AdjacencyGraph<TVertex, TEdge>();
            graph.AddVerticesAndEdgeRange(edges);
            return IsDirectedAcyclicGraphInternal(graph);
        }

        /// <summary>
        /// Checks whether the <paramref name="graph"/> is acyclic or not.
        /// </summary>
        /// <remarks>
        /// Performs a depth first search to look for cycles.
        /// </remarks>
        /// <param name="graph">Graph to visit.</param>
        /// <returns>True if the graph contains a cycle, false otherwise.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        public static bool IsDirectedAcyclicGraph<TVertex, TEdge>(
            [NotNull] this IVertexListGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            if (graph is null)
                throw new ArgumentNullException(nameof(graph));
            return IsDirectedAcyclicGraphInternal(graph);
        }

        private sealed class UndirectedCycleTester<TVertex, TEdge>
            where TEdge : IEdge<TVertex>
        {
            private bool _hasCycle;

            [Pure]
            public bool HasCycle([NotNull] IUndirectedGraph<TVertex, TEdge> graph)
            {
                Debug.Assert(graph != null);

                var dfs = graph.CreateUndirectedDepthFirstSearchAlgorithm();
                try
                {
                    dfs.BackEdge += DfsBackEdge;
                    dfs.Compute();
                    return _hasCycle;
                }
                finally
                {
                    dfs.BackEdge -= DfsBackEdge;
                }
            }

            private void DfsBackEdge([NotNull] object sender, [NotNull] UndirectedEdgeEventArgs<TVertex, TEdge> args)
            {
                _hasCycle = true;
            }
        }

        [Pure]
#if SUPPORTS_AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool IsUndirectedAcyclicGraphInternal<TVertex, TEdge>(
            [NotNull] this IUndirectedGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            return !new UndirectedCycleTester<TVertex, TEdge>().HasCycle(graph);
        }

        /// <summary>
        /// Checks whether the graph is acyclic or not.
        /// </summary>
        /// <remarks>
        /// Builds an <see cref="UndirectedGraph{TVertex,TEdge}"/> from <paramref name="edges"/>
        /// and performs a depth first search to look for cycles.
        /// </remarks>
        /// <param name="edges">Edges of forming the graph to visit.</param>
        /// <returns>True if the graph contains a cycle, false otherwise.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="edges"/> is <see langword="null"/> or at least one of them is <see langword="null"/>.
        /// </exception>
        [Pure]
        public static bool IsUndirectedAcyclicGraph<TVertex, TEdge>(
            [NotNull, ItemNotNull] this IEnumerable<TEdge> edges)
            where TEdge : IEdge<TVertex>
        {
            var graph = new UndirectedGraph<TVertex, TEdge>();
            graph.AddVerticesAndEdgeRange(edges);
            return IsUndirectedAcyclicGraphInternal(graph);
        }

        /// <summary>
        /// Checks whether the <paramref name="graph"/> is acyclic or not.
        /// </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <returns>True if the graph contains a cycle, false otherwise.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        public static bool IsUndirectedAcyclicGraph<TVertex, TEdge>(
            [NotNull] this IUndirectedGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            if (graph is null)
                throw new ArgumentNullException(nameof(graph));
            return IsUndirectedAcyclicGraphInternal(graph);
        }

        /// <summary>
        /// Given a edge cost map, computes the corresponding predecessor costs.
        /// </summary>
        /// <param name="predecessors">Predecessors map.</param>
        /// <param name="edgeCosts">Costs map.</param>
        /// <param name="target">Target vertex.</param>
        /// <returns>The predecessors cost.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="predecessors"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeCosts"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="target"/> is <see langword="null"/>.</exception>
        [Pure]
        public static double ComputePredecessorCost<TVertex, TEdge>(
            [NotNull] IDictionary<TVertex, TEdge> predecessors,
            [NotNull] IDictionary<TEdge, double> edgeCosts,
            [NotNull] TVertex target)
            where TEdge : IEdge<TVertex>
        {
            if (predecessors is null)
                throw new ArgumentNullException(nameof(predecessors));
            if (edgeCosts is null)
                throw new ArgumentNullException(nameof(edgeCosts));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            double cost = 0;
            TVertex current = target;
            while (predecessors.TryGetValue(current, out TEdge edge))
            {
                cost += edgeCosts[edge];
                current = edge.Source;
            }

            return cost;
        }

        /// <summary>
        /// Computes disjoint sets of the given <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <returns>Found disjoint sets.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull]
        public static IDisjointSet<TVertex> ComputeDisjointSet<TVertex, TEdge>(
            [NotNull] this IUndirectedGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            if (graph is null)
                throw new ArgumentNullException(nameof(graph));

            var sets = new ForestDisjointSet<TVertex>(graph.VertexCount);
            foreach (TVertex vertex in graph.Vertices)
            {
                sets.MakeSet(vertex);
            }

            foreach (TEdge edge in graph.Edges)
            {
                sets.Union(edge.Source, edge.Target);
            }

            return sets;
        }

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

            var kruskal = new KruskalMinimumSpanningTreeAlgorithm<TVertex, TEdge>(graph, edgeWeights);
            var edgeRecorder = new EdgeRecorderObserver<TVertex, TEdge>();
            using (edgeRecorder.Attach(kruskal))
            {
                kruskal.Compute();
            }

            return edgeRecorder.Edges;
        }

        /// <summary>
        /// Computes the maximum flow for a graph with positive capacities and flows
        /// using Edmonds-Karp algorithm.
        /// </summary>
        /// <param name="graph">Graph to visit.</param>
        /// <param name="edgeCapacities">Function that given an edge return the capacity of this edge.</param>
        /// <param name="source">The source vertex.</param>
        /// <param name="sink">The sink vertex.</param>
        /// <param name="flowPredecessors">Function that allow to retrieve flow predecessors.</param>
        /// <param name="edgeFactory">Edge factory method.</param>
        /// <param name="reversedEdgeAugmentorAlgorithm">Algorithm that is in of charge of augmenting the graph (creating missing reversed edges).</param>
        /// <returns>The maximum flow.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeCapacities"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="sink"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeFactory"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="reversedEdgeAugmentorAlgorithm"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="source"/> and <paramref name="sink"/> are the same vertex.</exception>
        public static double MaximumFlow<TVertex, TEdge>(
            [NotNull] this IMutableVertexAndEdgeListGraph<TVertex, TEdge> graph,
            [NotNull] Func<TEdge, double> edgeCapacities,
            [NotNull] TVertex source,
            [NotNull] TVertex sink,
            [NotNull] out TryFunc<TVertex, TEdge> flowPredecessors,
            [NotNull] EdgeFactory<TVertex, TEdge> edgeFactory,
            [NotNull] ReversedEdgeAugmentorAlgorithm<TVertex, TEdge> reversedEdgeAugmentorAlgorithm)
            where TEdge : IEdge<TVertex>
        {
            if (EqualityComparer<TVertex>.Default.Equals(source, sink))
                throw new ArgumentException($"{nameof(source)} and {nameof(sink)} must be different.");

            // Compute maximum flow
            var flow = new EdmondsKarpMaximumFlowAlgorithm<TVertex, TEdge>(
                graph,
                edgeCapacities,
                edgeFactory,
                reversedEdgeAugmentorAlgorithm);
            flow.Compute(source, sink);
            flowPredecessors = flow.Predecessors.TryGetValue;

            return flow.MaxFlow;
        }

        /// <summary>
        /// Computes the transitive close of the given <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph">Graph to compute the closure.</param>
        /// <param name="edgeFactory">Function that create an edge between the 2 given vertices.</param>
        /// <returns>Transitive graph closure.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeFactory"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull]
        public static BidirectionalGraph<TVertex, TEdge> ComputeTransitiveClosure<TVertex, TEdge>(
            [NotNull] this IEdgeListGraph<TVertex, TEdge> graph,
            [NotNull] Func<TVertex, TVertex, TEdge> edgeFactory)
            where TEdge : IEdge<TVertex>
        {
            var algorithm = graph.CreateTransitiveClosureAlgorithm(edgeFactory);
            algorithm.Compute();
            return algorithm.TransitiveClosure;
        }

        /// <summary>
        /// Clones a graph to another graph.
        /// </summary>
        /// <param name="graph">Graph to clone.</param>
        /// <param name="vertexCloner">Delegate to clone a vertex.</param>
        /// <param name="edgeCloner">Delegate to clone an edge.</param>
        /// <param name="clone">Cloned graph.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="vertexCloner"/> is <see langword="null"/> or creates <see langword="null"/> vertex.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeCloner"/> is <see langword="null"/> or creates <see langword="null"/> edge.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="clone"/> is <see langword="null"/>.</exception>
        public static void Clone<TVertex, TEdge>(
            [NotNull] this IVertexAndEdgeListGraph<TVertex, TEdge> graph,
            [NotNull, InstantHandle] Func<TVertex, TVertex> vertexCloner,
            [NotNull, InstantHandle] Func<TEdge, TVertex, TVertex, TEdge> edgeCloner,
            [NotNull] IMutableVertexAndEdgeSet<TVertex, TEdge> clone)
            where TEdge : IEdge<TVertex>
        {
            if (graph is null)
                throw new ArgumentNullException(nameof(graph));
            if (vertexCloner is null)
                throw new ArgumentNullException(nameof(vertexCloner));
            if (edgeCloner is null)
                throw new ArgumentNullException(nameof(edgeCloner));
            if (clone is null)
                throw new ArgumentNullException(nameof(clone));

            clone.Clear();
            var vertexClones = new Dictionary<TVertex, TVertex>(graph.VertexCount);
            foreach (TVertex vertex in graph.Vertices)
            {
                TVertex clonedVertex = vertexCloner(vertex);
                clone.AddVertex(clonedVertex);
                vertexClones.Add(vertex, clonedVertex);
            }

            foreach (TEdge edge in graph.Edges)
            {
                TEdge clonedEdge = edgeCloner(
                    edge,
                    vertexClones[edge.Source],
                    vertexClones[edge.Target]);
                clone.AddEdge(clonedEdge);
            }
        }
    }
}