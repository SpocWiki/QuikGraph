using System.Linq;
using JetBrains.Annotations;

namespace QuikGraph
{
    /// <summary> Extension Methods on <see cref="IImplicitGraph{TVertex,TEdge}"/> </summary>
    public static class ImplicitGraphExtensions
    {
        /// <summary> Determines whether <paramref name="vertex"/> has no out-edges. </summary>
        /// <returns>True if <paramref name="vertex"/> has no out-edges, false otherwise.</returns>
        [Pure]
        public static bool IsOutEdgesEmpty<TVertex, TEdge>(this IImplicitGraph<TVertex, TEdge> graph
            , TVertex vertex) where TEdge : IEdge<TVertex> => !graph.OutEdges(vertex).Any(); //OutDegree(vertex) == 0;

        /// <summary> Determines whether <paramref name="vertex"/> has no in-edges. </summary>
        /// <returns>True if <paramref name="vertex"/> has no in-edges, false otherwise.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="vertex"/> is <see langword="null"/>.</exception>
        /// <exception cref="VertexNotFoundException"><paramref name="vertex"/> is not part of the graph.</exception>
        [Pure]
        public static bool IsInEdgesEmpty<TVertex, TEdge>(this IBidirectionalIncidenceGraph<TVertex, TEdge> graph
            , TVertex vertex) where TEdge : IEdge<TVertex> => !graph.InEdges(vertex).Any(); //InDegree(vertex) == 0;

    }
}