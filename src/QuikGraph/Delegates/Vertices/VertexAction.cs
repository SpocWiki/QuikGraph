using JetBrains.Annotations;

namespace QuikGraph
{
    /// <summary>
    /// Delegate to perform an action involving the <paramref name="vertex"/>.
    /// </summary>
    /// <param name="vertex">Vertex to treat.</param>
    public delegate void VertexAction<in TVertex>([NotNull] TVertex vertex);
}