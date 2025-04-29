using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using static QuikGraph.Utils.DisposableHelpers;

namespace QuikGraph.Algorithms.Observers
{
    /// <summary> Extension Methods for <see cref="UndirectedVertexPredecessorRecorderObserver{TVertex, TEdge}"/> </summary>
    public static class UndirectedVertexPredecessorRecorderObserver
    {

        /// <summary> Attaches a new <see cref="UndirectedVertexPredecessorRecorderObserver{TVertex, TEdge}"/> and returns it </summary>
        public static UndirectedVertexPredecessorRecorderObserver<TVertex, TEdge> AttachUndirectedVertexPredecessorRecorderObserver<TVertex, TEdge>
            (this IUndirectedTreeBuilderAlgorithm<TVertex, TEdge> algorithm)
            where TEdge : IEdge<TVertex>
        {
            var predecessors = new UndirectedVertexPredecessorRecorderObserver<TVertex, TEdge>();
            predecessors.Attach(algorithm);
            return predecessors;
        }

        /// <summary> Creates a new <see cref="UndirectedVertexPredecessorRecorderObserver{TVertex, TEdge}"/> and returns it </summary>
        public static UndirectedVertexPredecessorRecorderObserver<TVertex, TEdge> CreateUndirectedVertexPredecessorRecorderObserver<TVertex, TEdge>(
            this IDictionary<TVertex, TEdge> vertexPredecessors)
            where TEdge : IEdge<TVertex>
            => new UndirectedVertexPredecessorRecorderObserver<TVertex, TEdge>(vertexPredecessors);
    }

    /// <summary> Recorder of vertices predecessors (undirected). </summary>
#if SUPPORTS_SERIALIZATION
    [Serializable]
#endif
    public sealed class UndirectedVertexPredecessorRecorderObserver<TVertex, TEdge> :
        IObserver<IUndirectedTreeBuilderAlgorithm<TVertex, TEdge>>, IDisposable
        where TEdge : IEdge<TVertex>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UndirectedVertexPredecessorRecorderObserver{TVertex,TEdge}"/> class.
        /// </summary>
        public UndirectedVertexPredecessorRecorderObserver()
            : this(new Dictionary<TVertex, TEdge>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UndirectedVertexPredecessorRecorderObserver{TVertex,TEdge}"/> class.
        /// </summary>
        /// <param name="verticesPredecessors">Vertices predecessors.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="verticesPredecessors"/> is <see langword="null"/>.</exception>
        public UndirectedVertexPredecessorRecorderObserver(
            [NotNull] IDictionary<TVertex, TEdge> verticesPredecessors)
        {
            VerticesPredecessors = verticesPredecessors ?? throw new ArgumentNullException(nameof(verticesPredecessors));
        }

        /// <summary> predecessors for each <typeparamref name="TVertex"/>.</summary>
        /// <remarks>
        /// Since any sub-Path of a shortest Path is also a shortest Path to the Sub-Node,
        /// you can trace the Steps back from any given Node, unless there is no path to the "root".
        /// </remarks>
        [NotNull]
        public IDictionary<TVertex, TEdge> VerticesPredecessors { get; }

        /// <summary> Typically only a single Algorithm is subscribed! </summary>
        IUndirectedTreeBuilderAlgorithm<TVertex, TEdge> _Algorithms;// = new List<IUndirectedTreeBuilderAlgorithm<TVertex, TEdge>>();

        /// <inheritdoc />
        public IDisposable Attach(IUndirectedTreeBuilderAlgorithm<TVertex, TEdge> algorithm)
        {
            if (algorithm is null)
                throw new ArgumentNullException(nameof(algorithm));
            if (_Algorithms != null)
            {
                throw new InvalidOperationException("Must not subscribe to multiple Observers!");
            }
            _Algorithms = algorithm; //_Algorithms.Add(algorithm);
            algorithm.TreeEdge += OnEdgeDiscovered;
            return Finally(() => algorithm.TreeEdge -= OnEdgeDiscovered);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            var algorithm = _Algorithms;
            //foreach (var algorithm in _Algorithms)
            if (algorithm != null)
            {
                algorithm.TreeEdge -= OnEdgeDiscovered;
            }
            _Algorithms = null; //.Clear();
        }

        private void OnEdgeDiscovered([NotNull] object sender, [NotNull] UndirectedEdgeEventArgs<TVertex, TEdge> args)
        {
            VerticesPredecessors[args.Target] = args.Edge;
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
        public bool TryGetPath([NotNull] TVertex vertex, [ItemNotNull] out IEnumerable<TEdge> path)
            => VerticesPredecessors.TryGetPath(vertex, out path);
    }
}