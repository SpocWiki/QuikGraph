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

        /// <summary> checks if the <see cref="_graph"/> is Hamiltonian
        /// (has a path that links all vertices and pass one and only one time by each vertex).
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        public IsHamiltonianGraphAlgorithm([NotNull] IUndirectedGraph<TVertex, TEdge> graph)
        {
            _graph = graph.RemoveParallelEdges();
        }

        /// <summary> Gets all vertices permutations. </summary>
        [Pure]
        [NotNull, ItemNotNull]
        public List<List<TVertex>> GetAllVertexPermutations() => _graph.Vertices.ToList().GetAllPermutations();

        /// <inheritdoc cref="GraphExtensions.IsHamiltonian{TVertex,TEdge}"/>
        public bool IsHamiltonian() => _graph.IsHamiltonian();
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

        /// <summary> Returns true if the <paramref name="graph"/> is Hamiltonian, otherwise false. </summary>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        [Pure]
        public static bool IsHamiltonian<TVertex, TEdge>(
            [NotNull] this IUndirectedGraph<TVertex, TEdge> graph)
            where TEdge : IUndirectedEdge<TVertex>
            => new IsHamiltonianGraphAlgorithm<TVertex, TEdge>(graph).IsHamiltonian();
    }
}
