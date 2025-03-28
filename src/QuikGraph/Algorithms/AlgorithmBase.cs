﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
#if SUPPORTS_AGGRESSIVE_INLINING
using System.Runtime.CompilerServices;
#endif
using JetBrains.Annotations;
using QuikGraph.Algorithms.Services;

namespace QuikGraph.Algorithms
{
    /// <summary> Base class for all graph algorithms. </summary>
    /// <inheritdoc cref="IComputation"/>
    /// <inheritdoc cref="IAlgorithm{TGraph}"/>
    /// <inheritdoc cref="IAlgorithmComponent"/>
#if SUPPORTS_SERIALIZATION
    [Serializable]
#endif
    public abstract class AlgorithmBase<TGraph> : IAlgorithm<TGraph>, IAlgorithmComponent
        //where TGraph : IGraph<TVertex, TEdge>
    {
        /// <inheritdoc />
        public TGraph VisitedGraph { get; }

        /// <summary>
        /// Initializes a new <see cref="AlgorithmBase{TGraph}"/> class (with optional host).
        /// </summary>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="host">Host to use if set, otherwise use this reference.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
        protected AlgorithmBase([NotNull] TGraph visitedGraph, [CanBeNull] IAlgorithmComponent host = null)
        {
            if (visitedGraph == null)
                throw new ArgumentNullException(nameof(visitedGraph));

            VisitedGraph = visitedGraph;
            _algorithmServices = new AlgorithmServices(host ?? this);
        }

        #region IComputation

        /// <inheritdoc />
        public object SyncRoot { get; } = new object();

        private volatile ComputationState _state = ComputationState.NotRunning;

        /// <inheritdoc />
        public ComputationState State
        {
            get
            {
                lock (SyncRoot)
                {
                    return _state;
                }
            }
        }

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">Something went wrong when running the algorithm.</exception>
        public void Compute()
        {
            BeginComputation();

            try
            {
                Initialize();

                InternalCompute();
            }
            catch (OperationCanceledException)
            {
                // Just catch it to clean and end computing.
            }
            finally
            {
                Clean();

                EndComputation();
            }
        }

        /// <inheritdoc />
        public void Abort()
        {
            bool raise = false;
            lock (SyncRoot)
            {
                if (_state == ComputationState.Running)
                {
                    _state = ComputationState.PendingAbortion;
                    Services.CancelManager.Cancel();
                    raise = true;
                }
            }

            if (raise)
            {
                OnStateChanged(EventArgs.Empty);
            }
        }

        /// <inheritdoc />
        public event EventHandler StateChanged;

        /// <summary> Called on algorithm state changed. </summary>
        /// <param name="args"><see cref="F:EventArgs.Empty"/>.</param>
        protected virtual void OnStateChanged([NotNull] EventArgs args)
        {
            Debug.Assert(args != null);

            StateChanged?.Invoke(this, args);
        }

        /// <inheritdoc />
        public event EventHandler Started;

        /// <inheritdoc cref="IComputation.Started"/>
        protected virtual void OnStarted([NotNull] EventArgs args)
        {
            Debug.Assert(args != null);

            Started?.Invoke(this, args);
        }

        /// <inheritdoc />
        public event EventHandler Finished;

        /// <inheritdoc cref="IComputation.Finished"/>
        protected virtual void OnFinished([NotNull] EventArgs args)
        {
            Debug.Assert(args != null);

            Finished?.Invoke(this, args);
        }

        /// <inheritdoc />
        public event EventHandler Aborted;

        /// <inheritdoc cref="IComputation.Aborted"/>
        protected virtual void OnAborted([NotNull] EventArgs args)
        {
            Debug.Assert(args != null);

            Aborted?.Invoke(this, args);
        }

        #endregion

        #region IAlgorithmComponent

        [NotNull]
        private readonly AlgorithmServices _algorithmServices;

        /// <inheritdoc />
        public IAlgorithmServices Services => _algorithmServices;

        /// <inheritdoc />
        public T GetService<T>()
        {
            if (!TryGetService(out T service))
                throw new InvalidOperationException("Service not found.");
            return service;
        }

        /// <inheritdoc />
        public bool TryGetService<T>(out T service)
        {
            if (TryGetService(typeof(T), out object serviceObject))
            {
                service = (T)serviceObject;
                return true;
            }

            service = default(T);
            return false;
        }

        [CanBeNull]
        private Dictionary<Type, object> _services;

        /// <summary> Tries to get the <paramref name="service"/> with given <paramref name="serviceType"/>. </summary>
        /// <returns>True if the service was found, false otherwise.</returns>
        [Pure]
        [ContractAnnotation("=> true, service:notnull;=> false, service:null")]
        protected virtual bool TryGetService([NotNull] Type serviceType, out object service)
        {
            if (serviceType is null)
                throw new ArgumentNullException(nameof(serviceType));

            lock (SyncRoot)
            {
                if (_services is null)
                {
                    _services = new Dictionary<Type, object>();
                }

                if (_services.TryGetValue(serviceType, out service))
                    return service != null;

                if (serviceType == typeof(ICancelManager))
                {
                    _services[serviceType] = service = new CancelManager();
                }
                else
                {
                    _services[serviceType] = null;
                }

                return service != null;
            }
        }

        #endregion

        /// <summary> Throws if a cancellation of the algorithm was requested. </summary>
        /// <exception cref="T:System.OperationCanceledException">
        /// If the algorithm cancellation service indicates <see cref="ICancelManager.IsCancelling"/> is true.
        /// </exception>
#if SUPPORTS_AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected void ThrowIfCancellationRequested()
        {
            if (_algorithmServices.CancelManager.IsCancelling)
                throw new OperationCanceledException("Algorithm aborted.");
        }

        private void BeginComputation()
        {
            Debug.Assert(
                State == ComputationState.NotRunning
                || State == ComputationState.Finished
                || State == ComputationState.Aborted);

            lock (SyncRoot)
            {
                _state = ComputationState.Running;
                Services.CancelManager.ResetCancel();
                OnStarted(EventArgs.Empty);
                OnStateChanged(EventArgs.Empty);
            }
        }

        /// <summary> Called on algorithm initialization step. </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary> Algorithm compute step. </summary>
        protected abstract void InternalCompute();

        /// <summary> Called on algorithm cleanup step. </summary>
        protected virtual void Clean()
        {
        }

        private void EndComputation()
        {
            Debug.Assert(
                State == ComputationState.Running
                ||
                State == ComputationState.PendingAbortion);

            lock (SyncRoot)
            {
                switch (_state)
                {
                    case ComputationState.Running:
                        _state = ComputationState.Finished;
                        OnFinished(EventArgs.Empty);
                        break;
                    case ComputationState.PendingAbortion:
                        _state = ComputationState.Aborted;
                        OnAborted(EventArgs.Empty);
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                Services.CancelManager.ResetCancel();
                OnStateChanged(EventArgs.Empty);
            }
        }
    }
}