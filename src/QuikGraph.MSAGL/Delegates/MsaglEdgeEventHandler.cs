using JetBrains.Annotations;

namespace QuikGraph.MSAGL
{
    /// <summary>
    /// Delegate for an handler dealing with a edge.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="args">Event arguments.</param>
    public delegate void MsaglEdgeEventHandler<TVertex, TEdge>(
        [NotNull] object sender,
        [NotNull] MsaglEdgeEventArgs<TVertex, TEdge> args)
        where TEdge : IEdge<TVertex>;
}