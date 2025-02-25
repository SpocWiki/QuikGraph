using System;
using System.Diagnostics;
using JetBrains.Annotations;
using QuikGraph.Constants;

namespace QuikGraph
{
    /// <inheritdoc cref="Create{TVertex}"/>
    public static class Edge
    {
        /// <summary> Creates a new <see cref="Edge{TVertex}"/> from <paramref name="source"/> to <paramref name="target"/> </summary>
        /// <remarks>
        /// A Factory Method allows to centrally change the Types of Edges to create.
        /// It can also be registered with an IoC Container to dynamically create new Edges. 
        /// </remarks>
        public static IEdge<TVertex> Create<TVertex>(TVertex source, TVertex target) => new Edge<TVertex>(source, target);
    }

    /// <summary>
    /// The default <see cref="IEdge{TVertex}"/> implementation (directed edge).
    /// </summary>
#if SUPPORTS_SERIALIZATION
    [Serializable]
#endif
    [DebuggerDisplay("{" + nameof(Source) + "}->{" + nameof(Target) + "}")]
    public class Edge<TVertex> : IEdge<TVertex>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Edge{TVertex}"/> class.
        /// </summary>
        /// <param name="source">The source vertex.</param>
        /// <param name="target">The target vertex.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="target"/> is <see langword="null"/>.</exception>
        public Edge([NotNull] TVertex source, [NotNull] TVertex target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            Source = source;
            Target = target;
        }

        /// <inheritdoc />
        public TVertex Source { get; }

        /// <inheritdoc />
        public TVertex Target { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(EdgeConstants.EdgeFormatString, Source, Target);
        }
    }
}