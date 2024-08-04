using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using QuikGraph.Algorithms;

namespace QuikGraph.Tests.Algorithms.Contracts
{
    /// <summary>
    /// Tests related to <see cref="IDistancesCollection{TVertex}.GetDistances"/>.
    /// </summary>
    internal sealed class DistancesCollectionGetDistancesContract : DistancesCollectionContractBase
    {
        public DistancesCollectionGetDistancesContract([NotNull] Type algorithmToTest)
            : base(algorithmToTest)
        {
        }

        [Test]
        public void DistancesForAllVerticesInGraphReturnedWhenAlgorithmHasBeenRun()
        {
            var scenario = new ContractScenario<int>
            {
                EdgesInGraph = [Edge.Create(1, 2)],
                SingleVerticesInGraph = [3],
                AccessibleVerticesFromRoot = [2],
                Root = 1,
                DoComputation = true
            };

            IDistancesCollection<int> algorithm = CreateAlgorithmAndMaybeDoComputation(scenario);

            IEnumerable<KeyValuePair<int, double>> distances = algorithm.GetDistances();
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, distances.Select(pair => pair.Key));
        }

        [Test]
        public void EmptyCollectionReturned_WhenAlgorithmHasNotYetBeenRun()
        {
            var scenario = new ContractScenario<int>
            {
                EdgesInGraph = [Edge.Create(1, 2)],
                SingleVerticesInGraph = [3],
                AccessibleVerticesFromRoot = [2],
                Root = 1,
                DoComputation = false
            };

            IDistancesCollection<int> algorithm = CreateAlgorithmAndMaybeDoComputation(scenario);

            IEnumerable<KeyValuePair<int, double>> distances = algorithm.GetDistances();
            CollectionAssert.IsEmpty(distances);
        }
    }
}