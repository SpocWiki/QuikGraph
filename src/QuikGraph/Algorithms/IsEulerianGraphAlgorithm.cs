using System;
using System.Collections.Generic;
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

        private struct TrueIndexes
        {
            public TrueIndexes(int? firstIndex, int? secondIndex)
            {
                FirstIndex = firstIndex;
                SecondIndex = secondIndex;
            }

            public int? FirstIndex { get; }
            public int? SecondIndex { get; }
        }

        [Pure]
        private static TrueIndexes FirstAndSecondIndexOfTrue([NotNull] bool[] data)
        {
            // If no true elements returns (null, null)
            // If only one true element, returns (indexOfTrue, null)
            int? firstIndex = null;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i])
                {
                    if (!firstIndex.HasValue)
                    {
                        firstIndex = i;
                    }
                    else
                    {
                        return new TrueIndexes(firstIndex, i);
                    }
                }
            }

            return new TrueIndexes(firstIndex, null);
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
        private bool SatisfiesEulerianCondition([NotNull] TVertex vertex) => _graph.AdjacentDegree(vertex) % 2 == 0;

        /// <summary>
        /// Returns true if the graph is Eulerian, otherwise false.
        /// </summary>
        /// <returns>True if the graph is Eulerian, false otherwise.</returns>
        [Pure]
        public bool IsEulerian()
        {
            var components = NumVerticesInComponent().Where(num => num > 1).Take(2).ToList();
            switch (components.Count)
            {
                case 0: return _graph.VertexCount == 1;
                case 1: return _graph.Vertices.All(SatisfiesEulerianCondition);
                default: return false; // Many components
            }
        }
    }

    /// <inheritdoc cref="IsEulerian{TVertex,TEdge}"/>
    public static class IsEulerianGraphAlgorithm
    {
        /// <summary> Returns true if the <paramref name="graph"/> is Eulerian, otherwise false. </summary>
        [Pure]
        public static bool IsEulerian<TVertex, TEdge>(
            [NotNull] this IUndirectedGraph<TVertex, TEdge> graph)
            where TEdge : IUndirectedEdge<TVertex> => new IsEulerianGraphAlgorithm<TVertex, TEdge>(graph).IsEulerian();
    }
}