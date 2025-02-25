using JetBrains.Annotations;

namespace QuikGraph
{
    /// <summary>
    /// Delegate to compute the identity of the given <paramref name="vertex"/>.
    /// </summary>
    /// <param name="vertex">Vertex to compute identity.</param>
    /// <returns>The <paramref name="vertex"/> identity.</returns>
    [NotNull]
    public delegate string VertexIdentity<in TVertex>([NotNull] TVertex vertex);
}