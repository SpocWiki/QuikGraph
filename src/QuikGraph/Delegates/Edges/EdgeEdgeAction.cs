using JetBrains.Annotations;

namespace QuikGraph
{
    /// <summary>
    /// Delegate to perform an action involving the 2 edges.
    /// </summary>
    /// <param name="edge">First edge.</param>
    /// <param name="targetEdge">Second edge.</param>
    public delegate void EdgeEdgeAction<TVertex, in TEdge>([NotNull] TEdge edge, [NotNull] TEdge targetEdge)
        where TEdge : IEdge<TVertex>;
}