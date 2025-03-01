#if SUPPORTS_SERIALIZATION
using System;
#endif

namespace QuikGraph.Collections
{
    /// <inheritdoc cref="IQueue{T}" />
#if SUPPORTS_SERIALIZATION
    [Serializable]
#endif
    public sealed class Queue<T> : System.Collections.Generic.Queue<T>, IQueue<T>
    {
        //public Queue() {}

        /// <summary> Creates a new Queue with minimum <paramref name="capacity"/> </summary>
        /// <param name="capacity"></param>
        public Queue(int capacity) : base(capacity){ }
    }
}