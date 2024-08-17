#if SUPPORTS_SERIALIZATION || SUPPORTS_CLONEABLE
using System;
#endif
using System.Collections.Generic;
using JetBrains.Annotations;

namespace QuikGraph.Collections
{
    /// <summary> Stores a list of edges. </summary>
#if SUPPORTS_SERIALIZATION
    [Serializable]
#endif
    public sealed class EdgeList<TVertex, TEdge> : List<TEdge>, IEdgeList<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        /// <summary> Initializes a new instance of the <see cref="EdgeList{TVertex,TEdge}"/> class. </summary>
        public EdgeList()
        {
        }

        /// <summary> Initializes a new instance of the <see cref="EdgeList{TVertex,TEdge}"/> class. </summary>
        /// <param name="capacity">List capacity.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="capacity"/> is negative.</exception>
        public EdgeList(int capacity)
            : base(capacity)
        {
        }

        /// <inheritdoc />
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
        public EdgeList([NotNull] EdgeList<TVertex, TEdge> other)
            : base(other)
        {
        }

        /// <summary> Clones this edge list. </summary>
        [NotNull]
        public EdgeList<TVertex, TEdge> Clone() => new EdgeList<TVertex, TEdge>(this);

        /// <inheritdoc />
        IEdgeList<TVertex, TEdge> IEdgeList<TVertex, TEdge>.Clone() => Clone();

#if SUPPORTS_CLONEABLE
        /// <inheritdoc />
        object ICloneable.Clone() => Clone();
#endif
    }
}