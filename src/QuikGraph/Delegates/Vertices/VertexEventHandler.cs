using JetBrains.Annotations;

namespace QuikGraph
{
    /// <summary>
    /// Delegate to for an handler dealing with a vertex.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="args">Event arguments.</param>
    public delegate void VertexEventHandler<TVertex>([NotNull] object sender, [NotNull] VertexEventArgs<TVertex> args);
}