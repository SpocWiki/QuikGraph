using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace QuikGraph.Algorithms.Observers
{
    /// <inheritdoc cref="CreateVertexDistanceRecorderObserver{TVertex,TEdge}"/>
    public static class VertexDistanceRecorderObserverX
    {
        /// <summary> throws when <paramref name="item"/> is null </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static T ShouldNotBeNull<T>([CanBeNull] this T item, //[CallerArgumentExpression("item")]
            string argName = "") where T : class
        {
            if (item == null)
                throw new ArgumentNullException(argName);
            return item;
        }

        /// <summary> Creates a new instance of the <see cref="VertexDistanceRecorderObserver{TVertex,TEdge}"/> class. </summary>
        /// <param name="edgeWeights">Function that computes the weight for a given edge.</param>
        /// <param name="distanceRelaxer">Distance relaxer.</param>
        /// <param name="distances">Distances per vertex.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeWeights"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="distanceRelaxer"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="distances"/> is <see langword="null"/>.</exception>
        public static VertexDistanceRecorderObserver<TVertex, TEdge> CreateVertexDistanceRecorderObserver<TVertex,
            TEdge>(
            [NotNull] this Func<TEdge, double> edgeWeights,
            [CanBeNull] IDistanceRelaxer distanceRelaxer = null,
            [CanBeNull] IDictionary<TVertex, double> distances = null) where TEdge : IEdge<TVertex>
            => new VertexDistanceRecorderObserver<TVertex, TEdge>(edgeWeights, distanceRelaxer, distances);

    /// <inheritdoc cref="CreateVertexDistanceRecorderObserver{TVertex,TEdge}"/>
        public static VertexDistanceRecorderObserver<TVertex, IEdge<TVertex>> CreateVertexDistanceRecorderObserver<TVertex>(
            [NotNull] this Func<IEdge<TVertex>, double> edgeWeights,
            [CanBeNull] IDistanceRelaxer distanceRelaxer = null,
            [CanBeNull] IDictionary<TVertex, double> distances = null)
            => new VertexDistanceRecorderObserver<TVertex, IEdge<TVertex>>(edgeWeights, distanceRelaxer, distances);

        /// <summary> Attaches a new VertexDistanceRecorderObserver </summary>
        public static VertexDistanceRecorderObserver<TVertex, TEdge> AttachVertexDistanceRecorderObserver<TVertex, TEdge>(
            this ITreeBuilderAlgorithm<TVertex, TEdge> shortestPath
            , Func<TEdge, double> reversedEdgeWeights) where TEdge : IEdge<TVertex>
        {
            var ret = reversedEdgeWeights.CreateVertexDistanceRecorderObserver<TVertex, TEdge>();
            ret.Attach(shortestPath);
            return ret;
        }
    }

    /// <summary> A distance recorder for <see cref="ITreeBuilderAlgorithm{TVertex,TEdge}"/> algorithms. </summary>
    public sealed class VertexDistanceRecorderObserver<TVertex, TEdge>
        : IObserver<ITreeBuilderAlgorithm<TVertex, TEdge>>, IDisposable
        where TEdge : IEdge<TVertex>
    {
        /// <summary> Initializes a new instance of the <see cref="VertexDistanceRecorderObserver{TVertex,TEdge}"/> class. </summary>
        /// <param name="edgeWeights">Function that computes the weight for a given edge.</param>
        /// <param name="distanceRelaxer">Distance relaxer.</param>
        /// <param name="distances">Distances per vertex.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edgeWeights"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="distanceRelaxer"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="distances"/> is <see langword="null"/>.</exception>
        internal VertexDistanceRecorderObserver(
            [NotNull] Func<TEdge, double> edgeWeights,
            [CanBeNull] IDistanceRelaxer distanceRelaxer = null,
            [CanBeNull] IDictionary<TVertex, double> distances = null)
        {
            EdgeWeights = edgeWeights ?? throw new ArgumentNullException(nameof(edgeWeights));
            DistanceRelaxer = distanceRelaxer ?? DistanceRelaxers.EdgeShortestDistance;
            Distances = distances ?? new Dictionary<TVertex, double>();
        }

        /// <summary> Distance relaxer. </summary>
        [NotNull]
        public IDistanceRelaxer DistanceRelaxer { get; }

        /// <summary> Function that computes the weight for a given edge. </summary>
        [NotNull]
        public Func<TEdge, double> EdgeWeights { get; }

        /// <summary> Distances per vertex. </summary>
        [NotNull]
        public IDictionary<TVertex, double> Distances { get; }

        private ITreeBuilderAlgorithm<TVertex, TEdge> _algorithm;

        /// <inheritdoc />
        public IDisposable Attach(ITreeBuilderAlgorithm<TVertex, TEdge> algorithm)
        {
            if (_algorithm != null)
                throw new InvalidOperationException("Already attached to " + _algorithm);

            _algorithm = algorithm.ShouldNotBeNull();
            algorithm.TreeEdge += OnEdgeDiscovered;

            return this;
        }

        private void OnEdgeDiscovered([NotNull] TEdge edge)
        {
            Debug.Assert(edge != null);

            if (!Distances.TryGetValue(edge.Source, out double sourceDistance))
            {
                Distances[edge.Source] = sourceDistance = DistanceRelaxer.InitialDistance;
            }
            Distances[edge.Target] = DistanceRelaxer.Combine(sourceDistance, EdgeWeights(edge));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _algorithm.TreeEdge -= OnEdgeDiscovered;
            _algorithm = null;
        }
    }
}