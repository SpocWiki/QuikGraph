using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace QuikGraph.Predicates
{
    /// <summary> Extension Methods to build complex Structures </summary>
    public static class ImplicitGraph
    {
        /// <summary> Filters <paramref name="baseGraph"/> by <paramref name="vertexPredicate"/> and <paramref name="edgePredicate"/> </summary>
        /// <returns></returns>
        public static FilteredImplicitGraph<TVertex, TEdge, TGraph> FilteredBy<TVertex, TEdge, TGraph>(
            this TGraph baseGraph,
            [NotNull] Func<TVertex, bool> vertexPredicate,
            [NotNull] Func<TEdge, bool> edgePredicate)
            where TGraph : IImplicitGraph<TVertex, TEdge>
            where TEdge : class, IEdge<TVertex>
            => new FilteredImplicitGraph<TVertex, TEdge, TGraph>(baseGraph, vertexPredicate, edgePredicate);
    }

    /// <summary>
    /// Represents an implicit graph that is filtered with a vertex and an edge predicate.
    /// This means only vertex and edge matching predicates are "accessible".
    /// </summary>
    /// <typeparam name="TVertex">Vertex type.</typeparam>
    /// <typeparam name="TEdge">Edge type.</typeparam>
    /// <typeparam name="TGraph">Graph type.</typeparam>
    public class FilteredImplicitGraph<TVertex, TEdge, TGraph>
        : FilteredImplicitVertexSet<TVertex, TEdge, TGraph>
        , IImplicitGraph<TVertex, TEdge>
        where TEdge : class, IEdge<TVertex>
        where TGraph : IImplicitGraph<TVertex, TEdge>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilteredImplicitGraph{TVertex,TEdge,TGraph}"/> class.
        /// </summary>
        /// <param name="baseGraph">Graph in which applying predicates.</param>
        /// <param name="vertexPredicate">Predicate to match vertex that should be taken into account.</param>
        /// <param name="edgePredicate">Predicate to match edge that should be taken into account.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="baseGraph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="vertexPredicate"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgePredicate"/> is <see langword="null"/>.</exception>
        public FilteredImplicitGraph(
            [NotNull] TGraph baseGraph,
            [NotNull] Func<TVertex, bool> vertexPredicate,
            [NotNull] Func<TEdge, bool> edgePredicate)
            : base(baseGraph, vertexPredicate, edgePredicate)
        {
        }

        #region IImplicitGraph<TVertex,TEdge>

        /// <inheritdoc />
        public int? OutDegree(TVertex vertex) => OutEdges(vertex)?.Count();

        /// <inheritdoc />
        public IEnumerable<TEdge> OutEdges(TVertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            if (VertexPredicate(vertex))
                return BaseGraph.OutEdges(vertex)?.Where(FilterEdge);

            return null;
        }

        /// <inheritdoc />
        public bool TryGetOutEdges(TVertex vertex, out IEnumerable<TEdge> edges)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            if (VertexPredicate(vertex)
                && BaseGraph.TryGetOutEdges(vertex, out IEnumerable<TEdge> outEdges))
            {
                edges = outEdges.Where(FilterEdge);
                return true;
            }

            edges = null;
            return false;
        }

        /// <inheritdoc />
        public TEdge OutEdge(TVertex vertex, int index)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            if (VertexPredicate(vertex))
            {
                return BaseGraph.OutEdges(vertex)?.Where(FilterEdge).ElementAt(index);
            }

            return null;
        }

        #endregion
    }
}