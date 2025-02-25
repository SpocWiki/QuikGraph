using JetBrains.Annotations;

namespace QuikGraph
{
    /// <summary>
    /// Delegate for an handler dealing with a edge.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="args">Event arguments.</param>
    public delegate void EdgeEventHandler<TVertex, TEdge>([NotNull] object sender, [NotNull] EdgeEventArgs<TVertex, TEdge> args) 
        where TEdge : IEdge<TVertex>;
}