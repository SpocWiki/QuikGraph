using System.Collections.Generic;
using JetBrains.Annotations;

namespace QuikGraph
{
    /// <summary> A directed graph that efficiently traverses both <see cref="InEdges"/> and <see cref="IImplicitGraph{TVertex,TEdge}.OutEdges"/>. </summary>
    /// <remarks>
    /// Adds Methods to efficiently query both <see cref="InEdges"/> and <see cref="IImplicitGraph{TVertex,TEdge}.OutEdges"/>.
    /// </remarks>
    public interface IBidirectionalIncidenceGraph<TVertex, TEdge> : IIncidenceGraph<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        /// <summary> Gets the number of in-edges of <paramref name="vertex"/>. </summary>
        /// <returns>The number of in-edges pointing towards <paramref name="vertex"/>.</returns>
        [Pure]
        int InDegree([NotNull] TVertex vertex);

        /// <summary> Gets the collection of in-edges of <paramref name="vertex"/>. </summary>
        /// <returns>The collection of in-edges of <paramref name="vertex"/>.</returns>
        [Pure]
        [NotNull, ItemNotNull]
        IEnumerable<TEdge> InEdges([NotNull] TVertex vertex);

        /// <summary> Tries to get the in-edges of <paramref name="vertex"/>. </summary>
        /// <param name="vertex">The vertex.</param>
        /// <param name="edges">In-edges.</param>
        /// <returns>True if <paramref name="vertex"/> was found or/and in-edges were found, false otherwise.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="vertex"/> is <see langword="null"/>.</exception>
        [Pure]
        [ContractAnnotation("=> true, edges:notnull;=> false, edges:null")]
        bool TryGetInEdges([NotNull] TVertex vertex, [ItemNotNull] out IEnumerable<TEdge> edges);

        /// <summary> Gets the in-edge at location <paramref name="index"/>. </summary>
        /// <param name="vertex">The vertex.</param>
        /// <param name="index">The index.</param>
        /// <returns>The in-edge at position <paramref name="index"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="vertex"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">No vertex at <paramref name="index"/>.</exception>
        /// <exception cref="VertexNotFoundException"><paramref name="vertex"/> is not part of the graph.</exception>
        [Pure]
        [NotNull]
        TEdge InEdge([NotNull] TVertex vertex, int index);

        /// <summary>
        /// Gets the degree of <paramref name="vertex"/>, i.e.
        /// the sum of the out-degree and in-degree of <paramref name="vertex"/>.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <returns>The sum of OutDegree and InDegree of <paramref name="vertex"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="vertex"/> is <see langword="null"/>.</exception>
        /// <exception cref="VertexNotFoundException"><paramref name="vertex"/> is not part of the graph.</exception>
        [Pure]
        int Degree([NotNull] TVertex vertex);
    }
}