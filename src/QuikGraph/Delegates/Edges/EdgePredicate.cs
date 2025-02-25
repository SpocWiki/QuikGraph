using System;
using JetBrains.Annotations;

namespace QuikGraph
{
    /// <summary>
    /// Delegate to perform a check on the given <paramref name="edge"/>.
    /// </summary>
    /// <param name="edge">Edge to check condition.</param>
    /// <returns>True if the <paramref name="edge"/> matches the predicate, false otherwise.</returns>
    /// <remarks>Can be substituted by <seealso cref="Func{TEdge, Boolean}"/></remarks>
    public delegate bool EdgePredicate<TVertex, in TEdge>([NotNull] TEdge edge)
        where TEdge : IEdge<TVertex>;
}