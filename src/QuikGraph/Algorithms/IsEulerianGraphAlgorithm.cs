using System;
using System.Linq;
using JetBrains.Annotations;
using QuikGraph.Algorithms.ConnectedComponents;

namespace QuikGraph.Algorithms
{
    /// <summary> Algorithm that checks if the undirected <see cref="_graph"/> is Eulerian.
    /// (i.e. has a path using all edges one and only one time).
    /// </summary>
    public class IsEulerianGraphAlgorithm<TVertex, TEdge>
        where TEdge : IUndirectedEdge<TVertex>
    {
        [NotNull]
        private readonly UndirectedGraph<TVertex, TEdge> _graph;

        /// <summary>
        /// Initializes a new instance of the <see cref="IsEulerianGraphAlgorithm{TVertex,TEdge}"/> class.
        /// </summary>
        /// <param name="graph">Graph to check; is not modified, but copied.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        public IsEulerianGraphAlgorithm([NotNull] IUndirectedGraph<TVertex, TEdge> graph)
        {
            if (graph is null)
                throw new ArgumentNullException(nameof(graph));

            // Create new graph without parallel edges
            var newGraph = new UndirectedGraph<TVertex, TEdge>(false, graph.EdgeEqualityComparer);
            newGraph.AddVertexRange(graph.Vertices);
            newGraph.AddEdgeRange(graph.Edges);
            newGraph.RemoveEdgeIf(edge => edge.IsSelfEdge(graph.AreVerticesEqual));

            _graph = newGraph;
        }

        /// <summary> Gets the components except for single Nodes in the current graph. </summary>
        [Pure]
        public int[] NumVerticesInComponent()
        {
            var undirectedGraph = _graph;
            ConnectedComponentsAlgorithm<TVertex, TEdge> componentsAlgorithm = undirectedGraph.CreateConnectedComponentsAlgorithm();
            componentsAlgorithm.Compute();

            return componentsAlgorithm.NumVerticesInComponent();
        }

        [Pure]
        private bool HasEvenDegree([NotNull] TVertex vertex) => _graph.AdjacentDegree(vertex) % 2 == 0;

        /// <summary> A graph is an Eulerian circuit, if it has a single Component, where each Vertex <see cref="HasEvenDegree"/>. </summary>
        /// <returns>True if the graph is Eulerian, false otherwise.</returns>
        [Pure]
        public bool IsEulerian()
        {
            if (!_graph.Vertices.All(HasEvenDegree))
            {
                return false;
            }
            var components = NumVerticesInComponent().Where(num => num > 1).Take(2).ToList();
            switch (components.Count)
            {
                case 0: return _graph.VertexCount == 1;
                case 1: return true;
                default: return false; // Many components
            }
        }
    }

    /// <inheritdoc cref="IsEulerian{TVertex,TEdge}"/>
    public static class IsEulerianGraphAlgorithm
    {
        /// <summary> Creates a new <see cref="IsEulerianGraphAlgorithm{TVertex,TEdge}"/> </summary>
        public static IsEulerianGraphAlgorithm<TVertex, TEdge> CreateEulerianGraphAlgorithm<TVertex, TEdge>(
            this IUndirectedGraph<TVertex, TEdge> graph)
            where TEdge : IUndirectedEdge<TVertex> => new IsEulerianGraphAlgorithm<TVertex, TEdge>(graph);

        /// <summary> Returns true if the <paramref name="graph"/> is Eulerian, otherwise false. </summary>
        /// <inheritdoc cref="IsEulerianGraphAlgorithm{TVertex,TEdge}.IsEulerian"/>
        [Pure]
        public static bool IsEulerian<TVertex, TEdge>(
            [NotNull] this IUndirectedGraph<TVertex, TEdge> graph)
            where TEdge : IUndirectedEdge<TVertex> => CreateEulerianGraphAlgorithm(graph).IsEulerian();
    }
}