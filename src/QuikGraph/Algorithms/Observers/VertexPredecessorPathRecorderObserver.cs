using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using static QuikGraph.Utils.DisposableHelpers;

namespace QuikGraph.Algorithms.Observers
{
    /// <summary> Recorder of <see cref="VerticesPredecessors"/>> to build up paths. </summary>
#if SUPPORTS_SERIALIZATION
    [Serializable]
#endif
    public sealed class VertexPredecessorPathRecorderObserver<TVertex, TEdge> :
        IObserver<IVertexPredecessorRecorderAlgorithm<TVertex, TEdge>>
        where TEdge : IEdge<TVertex>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VertexPredecessorPathRecorderObserver{TVertex,TEdge}"/> class.
        /// </summary>
        public VertexPredecessorPathRecorderObserver()
            : this(new Dictionary<TVertex, TEdge>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexPredecessorPathRecorderObserver{TVertex,TEdge}"/> class.
        /// </summary>
        /// <param name="verticesPredecessors">Vertices predecessors.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="verticesPredecessors"/> is <see langword="null"/>.</exception>
        public VertexPredecessorPathRecorderObserver([NotNull] IDictionary<TVertex, TEdge> verticesPredecessors)
        {
            VerticesPredecessors = verticesPredecessors ?? throw new ArgumentNullException(nameof(verticesPredecessors));
        }

        /// <summary> Predecessor Edges indexed by their <see cref="IEdge{TVertex}.Target"/> Vertices. </summary>
        [NotNull]
        public IDictionary<TVertex, TEdge> VerticesPredecessors { get; }

        /// <summary> Terminal, i.e. Path-ending vertices. </summary>
        [NotNull, ItemNotNull]
        public ICollection<TVertex> EndPathVertices { get; } = new List<TVertex>();

        /// <summary> Enumerates the paths from all <see cref="EndPathVertices"/>. </summary>
        [Pure]
        [NotNull, ItemNotNull]
        public IEnumerable<List<TEdge>> AllPaths() => EndPathVertices
                .Select(vertex => VerticesPredecessors.TryGetPath(vertex, out List<TEdge> path) ? path : null)
                .Where(path => path != null);

        #region IObserver<TAlgorithm>

        /// <inheritdoc />
        public IDisposable Attach(IVertexPredecessorRecorderAlgorithm<TVertex, TEdge> algorithm)
        {
            if (algorithm is null)
                throw new ArgumentNullException(nameof(algorithm));

            algorithm.TreeEdge += OnEdgeDiscovered;
            algorithm.FinishVertex += OnVertexFinished;
            return Finally(() =>
            {
                algorithm.TreeEdge -= OnEdgeDiscovered;
                algorithm.FinishVertex -= OnVertexFinished;
            });
        }

        #endregion

        private void OnEdgeDiscovered([NotNull] TEdge edge)
        {
            Debug.Assert(edge != null);

            VerticesPredecessors[edge.Target] = edge;
        }

        private void OnVertexFinished([NotNull] TVertex vertex)
        {
            Debug.Assert(vertex != null);

            if (!VerticesPredecessors.Values.Any(edge => EqualityComparer<TVertex>.Default.Equals(edge.Source, vertex)))
            {
                EndPathVertices.Add(vertex);
            }
        }
    }
}