using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuikGraph.Algorithms.VertexColoring;


namespace QuikGraph.Tests.Algorithms.GraphColoring
{
    /// <summary>
    /// Tests for <see cref="VertexColoringAlgorithm{TVertex,TEdge}"/>.
    /// </summary>
    [TestFixture]
    internal sealed class VertexColoringAlgorithmTests
    {
        [Test]
        public void Constructor()
        {
            var graph = new UndirectedGraph<int, IEdge<int>>();
            var algorithm = new VertexColoringAlgorithm<int, IEdge<int>>(graph);
            algorithm.AssertAlgorithmState(graph);
            CollectionAssert.IsEmpty(algorithm.Colors);
        }

        [Test]
        public void Constructor_Throws()
        {
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(
                () => new VertexColoringAlgorithm<int, IEdge<int>>(null));
        }

        [Test]
        public void VertexColoringEmptyGraph()
        {
            var graph = new UndirectedGraph<char, Edge<char>>(true);
            var algorithm = new VertexColoringAlgorithm<char, Edge<char>>(graph);
            algorithm.Compute();

            IDictionary<char, int?> coloredVertices = algorithm.Colors;

            // Graph doesn't have first vertex color
            Assert.IsFalse(coloredVertices.Values.Contains(1));

            // Expecting to no get any color
            Assert.AreEqual(0, coloredVertices.Values.Count);
        }

        [Test]
        public void VertexColoringNoEdge()
        {
            /* 
                                      (1)
                                                     
            Generate empty graph: (0)     (3) (4)
                                                     
                                      (2)
            */
            UndirectedGraph<char, Edge<char>> graph = CreateTestGraph();
            var algorithm = new VertexColoringAlgorithm<char, Edge<char>>(graph);
            algorithm.Compute();

            IDictionary<char, int?> coloredVertices = algorithm.Colors;

            // Graph doesn't have first vertex color
            Assert.IsFalse(coloredVertices.Values.Contains(1));

            int?[] result = coloredVertices.Values.ToArray();

            // Expecting to get only 1 color
            Assert.AreEqual(1, result.Max() + 1);

            // Not equal to null 
            foreach (int? color in result)
            {
                Assert.AreNotEqual(null, color);
            }

            // and corresponding colors of vertices
            Assert.AreEqual(0, result[0]); // 0 vertex = 0 color
            Assert.AreEqual(0, result[1]); // 1 vertex = 0 color
            Assert.AreEqual(0, result[2]); // 2 vertex = 0 color
            Assert.AreEqual(0, result[3]); // 3 vertex = 0 color
            Assert.AreEqual(0, result[4]); // 4 vertex = 0 color

            #region Local function

            UndirectedGraph<char, Edge<char>> CreateTestGraph()
            {
                var g = new UndirectedGraph<char, Edge<char>>(true);

                g.AddVertex('0'); // 1 Vertex
                g.AddVertex('1'); // 2 Vertex
                g.AddVertex('2'); // 3 Vertex
                g.AddVertex('3'); // 4 Vertex
                g.AddVertex('4'); // 5 Vertex

                return g;
            }

            #endregion
        }

        /// <summary> Generate undirected simple graph </summary>
        /// <remarks>
        /// <pre>
        ///     (1)
        ///    / | \ 
        /// (0)  |  (3)-(4)
        ///    \ | /
        ///     (2)
        /// </pre>
        /// </remarks>
        [Test]
        public void VertexColoringSimpleGraph()
        {
            var graph = CreateTestGraph();
            var algorithm = new VertexColoringAlgorithm<char, IEdge<char>>(graph);
            algorithm.Compute();

            IDictionary<char, int?> coloredVertices = algorithm.Colors;

            // Graph doesn't have third vertex color
            Assert.IsFalse(coloredVertices.Values.Contains(3));

            int?[] result = coloredVertices.Values.ToArray();

            // Expecting to get 3 different colors
            Assert.AreEqual(3, result.Max() + 1);

            // Not equal to null 
            foreach (int? color in result)
            {
                Assert.AreNotEqual(null, color);
            }

            // and corresponding colors of vertices
            Assert.AreEqual(0, result[0]); // 0 vertex = 0 color
            Assert.AreEqual(1, result[1]); // 1 vertex = 1 color
            Assert.AreEqual(2, result[2]); // 2 vertex = 2 color
            Assert.AreEqual(0, result[3]); // 3 vertex = 0 color
            Assert.AreEqual(1, result[4]); // 4 vertex = 1 color

            #region Local function

            UndirectedGraph<char, IEdge<char>> CreateTestGraph()
            {
                var g = new UndirectedGraph<char, IEdge<char>>(true);

                g.AddVertex('0'); // 1 Vertex
                g.AddVertex('1'); // 2 Vertex
                g.AddVertex('2'); // 3 Vertex
                g.AddVertex('3'); // 4 Vertex
                g.AddVertex('4'); // 5 Vertex

                g.AddEdge(Edge.Create('0', '1')); // 1 Edge
                g.AddEdge(Edge.Create('0', '2')); // 2 Edge
                g.AddEdge(Edge.Create('1', '2')); // 3 Edge
                g.AddEdge(Edge.Create('1', '3')); // 4 Edge
                g.AddEdge(Edge.Create('2', '3')); // 5 Edge
                g.AddEdge(Edge.Create('3', '4')); // 6 Edge

                return g;
            }

            #endregion
        }

        [Test]
        public void VertexColoringGraph()
        {
            /* 
                                                  (2)      (7)-(5)
                                                 /   \     /
            Generate undirected some graph:    (1)   (4)-(0)
                                                 \   /
                                             (6)  (3)
            
            (this graph has a minimum number of vertex colors only if to swap (1) and (4) vertices)
            */
            var graph = CreateTestGraph();
            var algorithm = new VertexColoringAlgorithm<char, IEdge<char>>(graph);
            algorithm.Compute();

            IDictionary<char, int?> coloredVertices = algorithm.Colors;

            // Graph doesn't have third vertex color
            Assert.IsFalse(coloredVertices.Values.Contains(3));

            int?[] result = coloredVertices.Values.ToArray();

            // Expecting to get 3 different colors
            Assert.AreEqual(3, result.Max() + 1);

            // Not equal to null 
            foreach (int? color in result)
            {
                Assert.AreNotEqual(null, color);
            }

            // And corresponding colors of vertices
            Assert.AreEqual(0, result[0]); // 0 vertex = 0 color
            Assert.AreEqual(0, result[1]); // 1 vertex = 0 color
            Assert.AreEqual(1, result[2]); // 2 vertex = 1 color
            Assert.AreEqual(1, result[3]); // 3 vertex = 1 color
            Assert.AreEqual(2, result[4]); // 4 vertex = 2 color
            Assert.AreEqual(0, result[5]); // 5 vertex = 0 color
            Assert.AreEqual(0, result[6]); // 6 vertex = 0 color
            Assert.AreEqual(1, result[7]); // 7 vertex = 1 color

            #region Local function

            UndirectedGraph<char, IEdge<char>> CreateTestGraph()
            {
                var g = new UndirectedGraph<char, IEdge<char>>(true);

                g.AddVertex('0'); // 1 Vertex
                g.AddVertex('1'); // 2 Vertex
                g.AddVertex('2'); // 3 Vertex
                g.AddVertex('3'); // 4 Vertex
                g.AddVertex('4'); // 5 Vertex
                g.AddVertex('5'); // 6 Vertex
                g.AddVertex('6'); // 7 Vertex
                g.AddVertex('7'); // 8 Vertex

                g.AddEdge(Edge.Create('0', '4')); // 1 Edge
                g.AddEdge(Edge.Create('1', '2')); // 2 Edge
                g.AddEdge(Edge.Create('1', '3')); // 3 Edge
                g.AddEdge(Edge.Create('2', '4')); // 4 Edge
                g.AddEdge(Edge.Create('3', '4')); // 5 Edge
                g.AddEdge(Edge.Create('5', '7')); // 6 Edge
                g.AddEdge(Edge.Create('7', '0')); // 7 Edge

                return g;
            }

            #endregion
        }

        /// <summary> Generate undirected full graph </summary>
        /// <remarks>
        /// <pre>
        ///    _____(2)_____
        ///   /    / | \    \
        /// (0)-(1)--+--(4)-(5) 
        ///   \    \ | /    /
        ///    \____(3)____/
        ///  + edges: (0-4), (0-5) and (1-5)
        /// </pre>
        /// </remarks>
        [Test]
        public void VertexColoringCompleteGraph()
        {
            var graph = CreateTestGraph();
            var algorithm = new VertexColoringAlgorithm<char, IEdge<char>>(graph);
            algorithm.Compute();

            IDictionary<char, int?> coloredVertices = algorithm.Colors;

            // Graph doesn't have sixth vertex color
            Assert.IsFalse(coloredVertices.Values.Contains(6));

            int?[] result = coloredVertices.Values.ToArray();

            // Expecting to get 6 different colors
            Assert.AreEqual(6, result.Max() + 1);

            // Not equal to null 
            foreach (int? color in result)
            {
                Assert.AreNotEqual(null, color);
            }

            // and corresponding colors of vertices
            Assert.AreEqual(0, result[0]); // 0 vertex = 0 color
            Assert.AreEqual(1, result[1]); // 1 vertex = 1 color
            Assert.AreEqual(2, result[2]); // 2 vertex = 2 color
            Assert.AreEqual(3, result[3]); // 3 vertex = 3 color
            Assert.AreEqual(4, result[4]); // 4 vertex = 4 color
            Assert.AreEqual(5, result[5]); // 5 vertex = 5 color

            #region Local function

            UndirectedGraph<char, IEdge<char>> CreateTestGraph()
            {
                var g = new UndirectedGraph<char, IEdge<char>>(true);

                g.AddVertex('0'); // 1 Vertex
                g.AddVertex('1'); // 2 Vertex
                g.AddVertex('2'); // 3 Vertex
                g.AddVertex('3'); // 4 Vertex
                g.AddVertex('4'); // 5 Vertex
                g.AddVertex('5'); // 6 Vertex

                g.AddEdge(Edge.Create('0', '1')); // 1  Edge
                g.AddEdge(Edge.Create('0', '2')); // 2  Edge
                g.AddEdge(Edge.Create('0', '3')); // 3  Edge
                g.AddEdge(Edge.Create('0', '4')); // 4  Edge
                g.AddEdge(Edge.Create('0', '5')); // 5  Edge
                g.AddEdge(Edge.Create('1', '2')); // 6  Edge
                g.AddEdge(Edge.Create('1', '3')); // 7  Edge
                g.AddEdge(Edge.Create('1', '4')); // 8  Edge
                g.AddEdge(Edge.Create('1', '5')); // 9  Edge
                g.AddEdge(Edge.Create('2', '3')); // 10 Edge
                g.AddEdge(Edge.Create('2', '4')); // 11 Edge
                g.AddEdge(Edge.Create('2', '5')); // 12 Edge
                g.AddEdge(Edge.Create('3', '4')); // 13 Edge
                g.AddEdge(Edge.Create('3', '5')); // 14 Edge
                g.AddEdge(Edge.Create('4', '5')); // 15 Edge

                return g;
            }

            #endregion
        }

        [Test]
        public void VertexColoringBipartiteGraph()
        {
            /*                                   
                                                 (3)
                                                / 
                                             (1)-(4)
                                                X     
            Generate undirected empty graph: (0)-(5)    + edges: (1-6) and (2-4)
                                                /     
                                             (2)-(6)
            
            */

            var graph = CreateTestGraph();
            var algorithm = new VertexColoringAlgorithm<char, IEdge<char>>(graph);
            algorithm.Compute();

            IDictionary<char, int?> coloredVertices = algorithm.Colors;

            // Graph doesn't have second vertex color
            Assert.IsFalse(coloredVertices.Values.Contains(2));

            int?[] result = coloredVertices.Values.ToArray();

            // Expecting to get 2 different colors
            Assert.AreEqual(2, result.Max() + 1);

            // Not equal to null 
            foreach (int? color in result)
            {
                Assert.AreNotEqual(null, color);
            }

            // and corresponding colors of vertices
            Assert.AreEqual(0, result[0]); // 0 vertex = 0 color
            Assert.AreEqual(0, result[1]); // 1 vertex = 0 color
            Assert.AreEqual(0, result[2]); // 2 vertex = 0 color
            Assert.AreEqual(1, result[3]); // 3 vertex = 1 color
            Assert.AreEqual(1, result[4]); // 4 vertex = 1 color
            Assert.AreEqual(1, result[5]); // 5 vertex = 1 color
            Assert.AreEqual(1, result[6]); // 6 vertex = 1 color

            #region Local function

            UndirectedGraph<char, IEdge<char>> CreateTestGraph()
            {
                var g = new UndirectedGraph<char, IEdge<char>>(true);

                g.AddVertex('0'); // 1 Vertex
                g.AddVertex('1'); // 2 Vertex
                g.AddVertex('2'); // 3 Vertex
                g.AddVertex('3'); // 4 Vertex
                g.AddVertex('4'); // 5 Vertex
                g.AddVertex('5'); // 6 Vertex
                g.AddVertex('6'); // 7 Vertex

                g.AddEdge(Edge.Create('0', '4')); // 1 Edge
                g.AddEdge(Edge.Create('0', '5')); // 2 Edge
                g.AddEdge(Edge.Create('1', '3')); // 3 Edge
                g.AddEdge(Edge.Create('1', '4')); // 4 Edge
                g.AddEdge(Edge.Create('1', '5')); // 5 Edge
                g.AddEdge(Edge.Create('1', '6')); // 6 Edge
                g.AddEdge(Edge.Create('2', '5')); // 7 Edge
                g.AddEdge(Edge.Create('2', '6')); // 8 Edge
                g.AddEdge(Edge.Create('2', '4')); // 9 Edge

                return g;
            }

            #endregion
        }
    }
}