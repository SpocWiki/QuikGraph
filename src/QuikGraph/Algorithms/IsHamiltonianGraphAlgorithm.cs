using System;
using System.Linq;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace QuikGraph.Algorithms
{
    /// <inheritdoc cref="IsHamiltonianGraphAlgorithm{TVertex, TEdge}(IUndirectedGraph{TVertex, TEdge})"/>
    public class IsHamiltonianGraphAlgorithm<TVertex, TEdge>
        where TEdge : IUndirectedEdge<TVertex>
    {
        [NotNull]
        private readonly UndirectedGraph<TVertex, TEdge> _graph;

        private readonly double _threshold;

        /// <summary> checks if the <see cref="_graph"/> is Hamiltonian
        /// (has a path that links all vertices and pass one and only one time by each vertex).
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        public IsHamiltonianGraphAlgorithm([NotNull] IUndirectedGraph<TVertex, TEdge> graph)
        {
            if (graph is null)
                throw new ArgumentNullException(nameof(graph));

            // Create new graph without parallel edges
            var newGraph = new UndirectedGraph<TVertex, TEdge>(
                false,
                graph.EdgeEqualityComparer);
            newGraph.AddVertexRange(graph.Vertices);
            newGraph.AddEdgeRange(graph.Edges);
            newGraph.RemoveEdgeIf(edge => edge.IsSelfEdge(graph.AreVerticesEqual));

            _graph = newGraph;
            _threshold = newGraph.VertexCount / 2.0;
        }

        /// <summary> Gets all vertices permutations. </summary>
        [Pure]
        [NotNull, ItemNotNull]
        public List<List<TVertex>> GetPermutations() => _graph.Vertices.ToList().GetAllPermutations();

        [Pure]
        private bool ExistsInGraph([NotNull, ItemNotNull] List<TVertex> path)
        {
            if (path.Count > 1)
            {
                path.Add(path[0]);      // Make cycle, not simple path
            }

            for (int i = 0; i < path.Count - 1; i++)
            {
                if (!_graph.AdjacentVertices(path[i]).Contains(path[i + 1]))
                {
                    return false;
                }
            }

            return true;
        }

        [Pure]
        private bool SatisfiesDiracTheorem([NotNull] TVertex vertex)
        {
            // Using Dirac's theorem:
            // if |vertices| >= 3 and for any vertex deg(vertex) >= (|vertices| / 2)
            // then graph is Hamiltonian 
            return _graph.AdjacentDegree(vertex) >= _threshold;
        }

        /// <summary>
        /// Returns true if the graph is Hamiltonian, otherwise false.
        /// </summary>
        /// <returns>True if the graph is Hamiltonian, false otherwise.</returns>
        [Pure]
        public bool IsHamiltonian()
        {
            int n = _graph.VertexCount;
            return n == 1
                || (n >= 3 && _graph.Vertices.All(SatisfiesDiracTheorem))
                || GetPermutations().Any(ExistsInGraph);
        }

    }

    /// <summary>
    /// Algorithm that checks if a graph is Hamiltonian
    /// (has a path that links all vertices and pass one and only one time by each vertex).
    /// </summary>
    public static class IsHamiltonianGraphAlgorithm
    {
        /// <summary> Creates a new <see cref="IsHamiltonianGraphAlgorithm"/> </summary>
        public static IsHamiltonianGraphAlgorithm<TVertex, TEdge> CreateHamiltonianGraph<TVertex, TEdge>(this IUndirectedGraph<TVertex, TEdge> graph)
            where TEdge : IUndirectedEdge<TVertex>
            => new IsHamiltonianGraphAlgorithm<TVertex, TEdge>(graph);

        /// <summary>
        /// Returns true if the <paramref name="graph"/> is Hamiltonian, otherwise false.
        /// </summary>
        /// <typeparam name="TVertex">Vertex type.</typeparam>
        /// <typeparam name="TEdge">Edge type.</typeparam>
        /// <param name="graph">Graph to check.</param>
        /// <returns>True if the <paramref name="graph"/> is Hamiltonian, false otherwise.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        public static bool IsHamiltonian<TVertex, TEdge>(
            [NotNull] this IUndirectedGraph<TVertex, TEdge> graph)
            where TEdge : IUndirectedEdge<TVertex>
            => new IsHamiltonianGraphAlgorithm<TVertex, TEdge>(graph).IsHamiltonian();
    }
}
