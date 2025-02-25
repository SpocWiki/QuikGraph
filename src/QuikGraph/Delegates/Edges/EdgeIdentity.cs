using JetBrains.Annotations;

namespace QuikGraph
{
    /// <summary>
    /// Delegate to compute the identity of the given <paramref name="edge"/>.
    /// </summary>
    /// <param name="edge">Edge to compute identity.</param>
    /// <returns>The <paramref name="edge"/> identity.</returns>
    [NotNull]
    public delegate string EdgeIdentity<TVertex, in TEdge>([NotNull] TEdge edge)
        where TEdge : IEdge<TVertex>;
}