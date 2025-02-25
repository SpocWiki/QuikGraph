using JetBrains.Annotations;

namespace QuikGraph.Graphviz
{
    /// <summary>
    /// Delegate for a clustered graph formatting event.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="args">Event arguments.</param>
    public delegate void FormatClusterEventHandler<TVertex, TEdge>(
        [NotNull] object sender,
        [NotNull] FormatClusterEventArgs<TVertex, TEdge> args)
        where TEdge : IEdge<TVertex>;
}