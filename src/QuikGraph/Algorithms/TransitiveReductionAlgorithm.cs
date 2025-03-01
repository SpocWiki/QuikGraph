using JetBrains.Annotations;

namespace QuikGraph.Algorithms
{
    /// <inheritdoc cref="CreateTransitiveReductionAlgorithm{TVertex,TEdge}"/>
    public static class TransitiveReductionAlgorithm
    {
        /// <summary> Computes the transitive reduction of the given <paramref name="graph"/>. </summary>
        /// <param name="graph">Graph to compute the reduction.</param>
        /// <returns>Transitive graph reduction.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        [NotNull]
        public static BidirectionalGraph<TVertex, TEdge> ComputeTransitiveReduction<TVertex, TEdge>(
            [NotNull] this IEdgeListGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            var algorithm = graph.CreateTransitiveReductionAlgorithm();
            algorithm.Compute();
            return algorithm.TransitiveReduction;
        }

        /// <summary> Creates a new instance of the <see cref="TransitiveReductionAlgorithm{TVertex,TEdge}"/> class. </summary>
        public static TransitiveReductionAlgorithm<TVertex, TEdge> CreateTransitiveReductionAlgorithm<TVertex, TEdge>(
            [NotNull] this IEdgeListGraph<TVertex, TEdge> visitedGraph)
            where TEdge : IEdge<TVertex>
            => new TransitiveReductionAlgorithm<TVertex, TEdge>(visitedGraph);
    }

    /// <summary> Algorithm that computes the transitive reduction of a graph, </summary>
    /// <remarks>
    /// which is another directed graph with the same vertices and as few edges as possible.
    /// </remarks>
    public class TransitiveReductionAlgorithm<TVertex, TEdge> : AlgorithmBase<IEdgeListGraph<TVertex, TEdge>>
        where TEdge : IEdge<TVertex>
    {
        /// <summary> Initializes a new instance of the <see cref="TransitiveReductionAlgorithm{TVertex,TEdge}"/> class. </summary>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
        internal TransitiveReductionAlgorithm([NotNull] IEdgeListGraph<TVertex, TEdge> visitedGraph)
            : base(visitedGraph)
        {
            TransitiveReduction = new BidirectionalGraph<TVertex, TEdge>();
        }

        /// <summary> Transitive reduction graph. </summary>
        [NotNull]
        public BidirectionalGraph<TVertex, TEdge> TransitiveReduction { get; }

        #region AlgorithmBase<TGraph>

        /// <inheritdoc />
        protected override void InternalCompute()
        {
            // Clone the visited graph
            TransitiveReduction.AddVertexRange(VisitedGraph.Vertices);
            TransitiveReduction.AddEdgeRange(VisitedGraph.Edges);

            var algorithmHelper = new TransitiveAlgorithmHelper<TVertex, TEdge>(TransitiveReduction);
            algorithmHelper.InternalCompute((graph, u, v, found, edge) =>
            {
                if (found)
                {
                    graph.RemoveEdge(edge);
                }
            });
        }

        #endregion
    }
}