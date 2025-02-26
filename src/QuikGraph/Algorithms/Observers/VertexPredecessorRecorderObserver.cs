using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace QuikGraph.Algorithms.Observers
{
    /// <inheritdoc cref="CreateVertexPredecessorRecorderObserver{TVertex,TEdge}"/>
    public static class VertexPredecessorRecorderObserverX
    {
        /// <summary> Creates a new instance of the <see cref="VertexDistanceRecorderObserver{TVertex,TEdge}"/> class. </summary>
        public static VertexPredecessorRecorderObserver<TVertex, TEdge> CreateVertexPredecessorRecorderObserver<TVertex,
            TEdge>([CanBeNull] this IDictionary<TVertex, TEdge> verticesPredecessors) where TEdge : IEdge<TVertex>
            => new VertexPredecessorRecorderObserver<TVertex, TEdge>(verticesPredecessors);

        /// <summary> Attaches a new VertexDistanceRecorderObserver </summary>
        public static VertexPredecessorRecorderObserver<TVertex, TEdge> AttachVertexPredecessorRecorderObserver<TVertex, TEdge>(
            this ITreeBuilderAlgorithm<TVertex, TEdge> shortestPath
            , [CanBeNull] IDictionary<TVertex, TEdge> verticesPredecessors = null) where TEdge : IEdge<TVertex>
        {
            var ret = verticesPredecessors.CreateVertexPredecessorRecorderObserver();
            ret.Attach(shortestPath);
            return ret;
        }
    }

    /// <summary> Recorder of vertices predecessors (undirected). </summary>
#if SUPPORTS_SERIALIZATION
    [Serializable]
#endif
    public sealed class VertexPredecessorRecorderObserver<TVertex, TEdge>
        : IObserver<ITreeBuilderAlgorithm<TVertex, TEdge>>, IDisposable
        where TEdge : IEdge<TVertex>
    {
        /// <summary> Initializes a new instance of the <see cref="VertexPredecessorRecorderObserver{TVertex,TEdge}"/> class. </summary>
        /// <param name="verticesPredecessors">Vertices predecessors.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="verticesPredecessors"/> is <see langword="null"/>.</exception>
        internal VertexPredecessorRecorderObserver([CanBeNull] IDictionary<TVertex, TEdge> verticesPredecessors = null)
        {
            VerticesPredecessors = verticesPredecessors ?? new Dictionary<TVertex, TEdge>();
        }

        /// <summary> Vertices predecessors. </summary>
        [NotNull]
        public IDictionary<TVertex, TEdge> VerticesPredecessors { get; }

        private ITreeBuilderAlgorithm<TVertex, TEdge> _algorithm;

        /// <inheritdoc />
        public IDisposable Attach(ITreeBuilderAlgorithm<TVertex, TEdge> algorithm)
        {
            if (_algorithm != null)
                throw new InvalidOperationException("Already attached to " + _algorithm);

            _algorithm = algorithm.ShouldNotBeNull();
            algorithm.TreeEdge += OnEdgeDiscovered;
            return this;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _algorithm.TreeEdge -= OnEdgeDiscovered;
        }

        private void OnEdgeDiscovered([NotNull] TEdge edge)
        {
            Debug.Assert(edge != null);

            VerticesPredecessors[edge.Target] = edge;
        }

        /// <summary>
        /// Tries to get the predecessor path, if reachable.
        /// </summary>
        /// <param name="vertex">Path ending vertex.</param>
        /// <param name="path">Path to the ending vertex.</param>
        /// <returns>True if a path was found, false otherwise.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="vertex"/> is <see langword="null"/>.</exception>
        [Pure]
        [ContractAnnotation("=> true, path:notnull;=> false, path:null")]
        public bool TryGetPath([NotNull] TVertex vertex, [ItemNotNull] out IEnumerable<TEdge> path) => VerticesPredecessors.TryGetPath(vertex, out path);

    }
}