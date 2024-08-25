using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace QuikGraph
{
    /// <summary>
    /// A delegate-based directed implicit graph data structure.
    /// </summary>
    /// <typeparam name="TVertex">Vertex type.</typeparam>
    /// <typeparam name="TEdge">Edge type.</typeparam>
    public class DelegateImplicitGraph<TVertex, TEdge> : IImplicitGraph<TVertex, TEdge>
        where TEdge : class, IEdge<TVertex>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateImplicitGraph{TVertex,TEdge}"/> class.
        /// </summary>
        /// <param name="tryGetOutEdges">Getter of out-edges.</param>
        /// <param name="allowParallelEdges">
        /// Indicates if parallel edges are allowed.
        /// Note that get of edges is delegated so you may have bugs related
        /// to parallel edges due to the delegated implementation.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="tryGetOutEdges"/> is <see langword="null"/>.</exception>
        public DelegateImplicitGraph(
            [NotNull] Func<TVertex, IEnumerable<TEdge>> tryGetOutEdges,
            bool allowParallelEdges = true)
        {
            _tryGetOutEdgesFunc = tryGetOutEdges ?? throw new ArgumentNullException(nameof(tryGetOutEdges));
            AllowParallelEdges = allowParallelEdges;
        }

        /// <summary>
        /// Getter of out-edges.
        /// </summary>
        [NotNull]
        private readonly Func<TVertex, IEnumerable<TEdge>> _tryGetOutEdgesFunc;

        #region IGraph<TVertex,TEdge>

        /// <inheritdoc />
        public bool IsDirected => true;

        /// <inheritdoc />
        public bool AllowParallelEdges { get; }

        #endregion

        #region IImplicitGraph<TVertex,TEdge>

        /// <inheritdoc />
        public int? OutDegree(TVertex vertex) => OutEdges(vertex)?.Count();

        [Pure]
        [ItemNotNull]
        [CanBeNull]
        internal virtual IEnumerable<TEdge> OutEdgesInternal([NotNull] TVertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            return _tryGetOutEdgesFunc(vertex);
        }

        /// <inheritdoc />
        public IEnumerable<TEdge> OutEdges(TVertex vertex) => OutEdgesInternal(vertex);

        /// <inheritdoc />
        public TEdge OutEdge(TVertex vertex, int index) => OutEdges(vertex)?.ElementAt(index);

        [Pure]
        internal virtual bool ContainsVertexInternal([NotNull] TVertex vertex) => _tryGetOutEdgesFunc(vertex) != null;

        /// <inheritdoc />
        public bool ContainsVertex(TVertex vertex) => ContainsVertexInternal(vertex);

        #endregion
    }
}