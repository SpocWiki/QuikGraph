using System;
using JetBrains.Annotations;

namespace QuikGraph.Algorithms
{
    /// <summary> Represents a computation of something with control states. </summary>
    public interface IComputation
    {
        /// <summary> Synchronizer object usable for managing concurrent Operations. </summary>
        [NotNull]
        object SyncRoot { get; }

        /// <inheritdoc cref="ComputationState"/>
        ComputationState State { get; }

        /// <summary> Runs the computation. </summary>
        void Compute();

        /// <summary> Abort the computation. </summary>
        void Abort();

        /// <summary> Event that fires when the computation state changed. </summary>
        event EventHandler StateChanged;

        /// <summary> Event that fires when the computation starts. </summary>
        event EventHandler Started;

        /// <summary> Event that fires when the computation is finished. </summary>
        event EventHandler Finished;

        /// <summary> Event that fires when the computation is aborted. </summary>
        event EventHandler Aborted;
    }
}