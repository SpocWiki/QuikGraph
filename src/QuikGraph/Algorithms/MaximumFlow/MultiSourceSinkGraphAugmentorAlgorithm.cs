using JetBrains.Annotations;
using QuikGraph.Algorithms.Services;

namespace QuikGraph.Algorithms.MaximumFlow
{
    /// <summary>
    /// Multi source and sink graph augmentor algorithm.
    /// </summary>
    public sealed class MultiSourceSinkGraphAugmentorAlgorithm<TVertex, TEdge>
        : GraphAugmentorAlgorithmBase<TVertex, TEdge, IMutableBidirectionalGraph<TVertex, TEdge>>
        where TEdge : IEdge<TVertex>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSourceSinkGraphAugmentorAlgorithm{TVertex,TEdge}"/> class.
        /// </summary>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="vertexFactory">Vertex factory method.</param>
        /// <param name="edgeFactory">Edge factory method.</param>
        /// <param name="host">Host to use if set, otherwise use this reference.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="vertexFactory"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeFactory"/> is <see langword="null"/>.</exception>
        public MultiSourceSinkGraphAugmentorAlgorithm([NotNull] IMutableBidirectionalGraph<TVertex, TEdge> visitedGraph,
            [NotNull] VertexFactory<TVertex> vertexFactory,
            [NotNull] EdgeFactory<TVertex, TEdge> edgeFactory, [CanBeNull] IAlgorithmComponent host = null)
            : base(visitedGraph, vertexFactory, edgeFactory, host)
        {
        }

        /// <inheritdoc />
        protected override void AugmentGraph()
        {
            foreach (TVertex vertex in VisitedGraph.Vertices)
            {
                ThrowIfCancellationRequested();

                // Is source
                if (VisitedGraph.IsInEdgesEmpty(vertex))
                {
                    AddAugmentedEdge(SuperSource, vertex);
                }

                // Is sink
                if (VisitedGraph.IsOutEdgesEmpty(vertex))
                {
                    AddAugmentedEdge(vertex, SuperSink);
                }
            }
        }
    }
}