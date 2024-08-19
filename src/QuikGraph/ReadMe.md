# QuikGraph

This Library defines Interfaces and Algorithms to solve Graph-Problems. 
Graphs are discrete Topologies and many real-world Problems 
can be abstracted down to this Level, which makes the Algorithms widely applicable. 

## Definitions: 

| Term          | Definition                                                                                                                                                                                                                                                                           |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Graph         | A __graph__ is a collection of Vertices and Edges. <br><br>In this Library it implements a generic Interface `IGraph<TVertex,TEdge>` <br>where `TEdge : IEdge<TVertex> { TVertex Source; TVertex Target; }`                                                                          |
| (dense)       | A __dense graph__  has up to \|Vertices\|²  Edges                                                                                                                                                                                                                                    |
| (sparse)      | A __sparse graph__  fewer Edges than \|Vertices\| * Log(\|Vertices\|)                                                                                                                                                                                                                |
| Vertex        | __Vertices__ (AKA __Nodes__) are simple objects <br>which have names (Identity) and sometimes other properties. <br><br>In this Library Vertices can be any Type with an Identity Relation <br>implemented as `IComparable<TVertex>`.                                                |
| Edge          | An __edge__ is a (directed, i.e. ordered) connection <br>from the __Source__ Vertex to the __Target__ Vertex. <br><br>It is modeled as an `IEdge<TVertex>`<br>                                                                                                                       |
| (reverse)     | The __reverse__ (AKA transpose) Edge runs in the opposite Direction <br>from the __Target__ Vertex to the __Source__ Vertex.<br>`ReversedBidirectionalGraph` wraps a Graph and exposes it by swapping `InEdges` and `OutEdges`.                                                      |
| (adjacent)    | Union of  `InEdges` and `OutEdges`.                                                                                                                                                                                                                                                  |
| (Properties)  | Edges can also have Properties, mostly a `Length` or `Cost`.<br>The Edges impose a discrete Topology on the Graph.                                                                                                                                                                   |
| Direction     | - __Directed__ Graphs (AKA `Digraph`) consist of only directed Edges. <br>- The __reverse__ Graph consists of the same Vertices but __reversed Edges__. <br>- __Undirected__ Graphs can be modeled as the Union of a Graph and its Reverse, <br>  i.e. all Edges can be be traversed |
| Path          | A Path is a contiguous sequence of Edges, <br>i.e. the __Target__ Vertex of the previous Edge is the __Source__ Vertex of the next Edge.                                                                                                                                             |
| (simple)      | A simple path is a path in which no vertex is repeated. <br>                                                                                                                                                                                                                         |
| Cycle         | A Cycle is a simple path, except the first and last Nodes are the same. <br>                                                                                                                                                                                                         |
| Component     | A connected Component of a Graph is a Sub-Graph <br>with a path from every node to every other node in the Component                                                                                                                                                                 |
| Tree          | A graph with no cycles is called a tree.<br>Prove by induction that a tree on V vertices has exactly V - 1 edges.                                                                                                                                                                    |
| Diamond       | A Diamond is                                                                                                                                                                                                                                                                         |
| Forest        | A group of disconnected trees is called a forest.                                                                                                                                                                                                                                    |
| Spanning Tree | A spanning tree of a Component is a sub-graph that <br>contains all the vertices but only enough of the edges to form a tree.                                                                                                                                                        |

## Examples 

### Geographical Navigation 

- The Vertices are geographical Places and 
- the Edge Length is usually the 'Distance' between these Places. 

### Electrical Wiring 

- The Vertices are electronic Components and 
- the Edge Properties can often be neglected, 
  unless for high Frequencies or Currencies 
  where Impedance, Capacity and Resistance become relevant. 

### Finite State-Machines 

Graphs can be used to visualize finite State-Machines with 
- the States as Vertices and 
- the Transitions as Edges. 

## Visualization 
 
One can draw a graph by marking points for the vertices and
drawing lines connecting them for the edges. 

The Placement of the Vertices is usually arbitrary, 
but it considerably reduces Confusion when Crossings of Edges are minimized. 

## Samples 

### Sample Vertices are named `a`,`b`,`c`,... 

### Sample Edges are just ordered Pairs of Vertices: `ab`, `ed`, ... 

### Paths are just Strings of Vertices `abfd`... 

### V = |Nodes| = Number of Vertices 

### E = |Edges| = Number of Edges  

## Data Structures 

There are two primary ways to efficiently represent Graphs in Programs. 
The choice between them determines Storage and Efficiency of Algorithms. 
For most efficient Representation the Vertices are 'numbered', 
which can be achieved by storing the Vertices in a List
and storing the Position in an `Index` Property of each Vertex. 

### Adjacency Matrix for dense Graphs 

Modelled by a 2D Array of `float` or `bool` Values 
representing the Length or (reciprocal) Cost of the Connection. 
The 2 Indices of the Array are determined from the `Index` Property of each Vertex 

This is very efficient both in Access in O(1) and Storage, but only for dense Graphs. 
For large Graphs the O(V²) Storage becomes a Problem. 

### Adjacency List for sparse Graphs 

This is the preferred solution for sparse Graphs, even large ones. 
It is modelled by a jagged, compressed 2D Array of Vertices or Vertex Indices. 
This requires O(E) Storage, but Access is typically O(V Log V). 
The first Index is determined from the `Index` Property of the `Source` Vertex. 
The second Index is usually determined by BiSection of a sorted List or by a Hash-Table. 


### Additional Information

Extra information associated with each vertex can be accommodated by using auxiliary arrays indexed by vertex number (or by making adj an array of records in the adjacency structure representation). Extra information associated with each edge can be put in the adjacency list nodes (or in an array a of records in the adjacency matrix representation), or in auxiliary arrays indexed by edge number (this requires numbering the edges). 



