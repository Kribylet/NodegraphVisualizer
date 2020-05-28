using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace Nodegraph_Generator
{
    [Serializable()]
    [DataContract()]
    public class NodeGraph
    {
        private const double NODE_RADIUS = 15;

        [DataMember(Order=0)]
        private List<Node> nodes = new List<Node>();
        [DataMember(Order=1)]
        private List<Edge> edges = new List<Edge>();

        public List<int> componentIndices = new List<int>();
        public List<Node> Nodes {
            get {return nodes;}

            private set{}
        }

        public List<Edge> Edges {
            get {return edges;}

            private set{}
        }

        public List<Node> NodesDeepCopy {
            get {
                List<Node> returnList = new List<Node>();

                foreach (Node node in nodes)
                {
                    returnList.Add(node.DeepCopy());
                }

                return returnList;
            }

            private set{}
        }

        public List<Edge> EdgesDeepCopy {
            get {
                List<Edge> returnList = new List<Edge>();

                foreach (Edge edge in edges)
                {
                    returnList.Add(edge.DeepCopy());
                }

                return returnList;
            }

            private set { }
        }

        private int nodeIndexCounter = 0;

        private int edgeIndexCounter = 0;

        public NodeGraph() {}

        public NodeGraph DeepCopy()
        {
            using (MemoryStream stream = new MemoryStream())
            {

                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(stream, this);

                stream.Position = 0;

                return (NodeGraph) formatter.Deserialize(stream);

            }
        }

        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                NodeGraph other = (NodeGraph)obj;

                return System.Linq.Enumerable.SequenceEqual(nodes, other.nodes) &&
                       System.Linq.Enumerable.SequenceEqual(edges, other.edges);
            }
        }

        /*
         * Generates a simple text description of a NodeGraph structure.
         */
        public override string ToString()
        {
            String nodeGraphString = "";
            foreach (Edge edge in edges)
            {
                nodeGraphString += "[" + edge.nodeIndex1 + "->" + edge.nodeIndex2 + "](" + edge.index + ")\n";
            }

            nodeGraphString += "\n";


            foreach (Node node in nodes)
            {
                nodeGraphString += "Node [" + node.index + "] " + node.coordinate.ToString() + "\n";
            }

            return nodeGraphString;
        }

        public void SetEdgeWidth(int index, double width)
        {
            GetEdge(index).width = width;
        }

        public void SetEdgeWidth(int nodeIndex1, int nodeIndex2, double width)
        {
            SetEdgeWidth(GetEdge(nodeIndex1, nodeIndex2).index, width);
        }

        public void SetEdgeWidth(Node node1, Node node2, double width){
            SetEdgeWidth(GetEdge(node1, node2).index, width);
        }

        public double GetEdgeWidth(int edgeIndex)
        {
            return GetEdge(edgeIndex).width;
        }

        public double GetEdgeWidth(int nodeIndex1, int nodeIndex2)
        {
            return GetEdgeWidth(GetEdge(nodeIndex1, nodeIndex2).index);
        }

        public double GetEdgeWidth(Node node1, Node node2){
            return GetEdgeWidth(GetEdge(node1, node2).index);
        }

        public double GetEdgeWidth(Vect3 coordinate1, Vect3 coordinate2)
        {
            return GetEdgeWidth(GetEdge(coordinate1, coordinate2).index);
        }

        public void SetEdgeHeight(int index, double height)
        {
            GetEdge(index).height = height;
        }

        public void SetEdgeHeight(int nodeIndex1, int nodeIndex2, double height)
        {
            SetEdgeHeight(GetEdge(nodeIndex1, nodeIndex2).index, height);
        }

        public void SetEdgeHeight(Node node1, Node node2, double height){
            SetEdgeHeight(GetEdge(node1, node2).index, height);
        }

        public override int GetHashCode()
        {
            return this.nodes.GetHashCode() ^ this.edges.GetHashCode() << 2 ^ nodeIndexCounter >> 3 ^ edgeIndexCounter;
        }


        /*
         * Retrieve a the coordinate of a node in the graph.
         */
        public Vect3 GetCoordinateOfNode(int index)
        {
            Node nodeFound = GetNodeByIndex(index);
            if (nodeFound == null) {return null;}
            return nodeFound.coordinate;
        }

        /*
         * Retrieve a reference to a Node in the nodegraph specified by index, if it exists. Otherwise returns null.
         */
        public Node GetNode(int index) {
            Node nodeFound = GetNodeByIndex(index);
            if (nodeFound == null) {return null;}
            return nodeFound;
        }

        /*
         * Retrieve a reference to a Node in the nodegraph specified by Node, if it exists. Otherwise returns null.
         */
        public Node GetNode(Node node) {
            Node nodeFound = GetNodeByCoordinate(node.coordinate);
            if (nodeFound == null) {return null;}
            return nodeFound;
        }

        /*
         * Retrieve a reference to a Node in the nodegraph specified by coordinate, if it exists. Otherwise returns null.
         */
        public Node GetNode(Vect3 coordinate) {
            Node nodeFound = GetNodeByCoordinate(coordinate);
            if (nodeFound == null) {return null;}
            return nodeFound;
        }

        /*
         * Retrieve a reference to an Edge in the nodegraph that match provided nodes, if it exists. Otherwise returns null.
         */
        public Edge GetEdge(Node node1, Node node2) {
            return GetEdge(node1.coordinate, node2.coordinate);
        }

        /*
         * Retrieve a reference to an Edge in the nodegraph that links nodes specified by coordinates, if it exists. Otherwise returns null.
         */
        public Edge GetEdge(Vect3 coordinate1, Vect3 coordinate2) {

            Node node1 = GetNodeByCoordinate(coordinate1);
            Node node2 = GetNodeByCoordinate(coordinate2);

            if (node1 == null || node2 == null) {return null;}

            return GetEdgeBetweenIndices(node1.index, node2.index);
        }

        /*
         * Retrieve a reference to an Edge in the nodegraph that links nodes specified by indices, if it exists. Otherwise returns null.
         */
        public Edge GetEdge(int nodeIndex1, int nodeIndex2) {
            return GetEdgeBetweenIndices(nodeIndex1, nodeIndex2);
        }

        /*
         * Retrieve a reference to an Edge in the nodegraph by index, if it exists. Otherwise returns null.
         */
        public Edge GetEdge(int edgeIndex)
        {
            Edge edge = GetEdgeByIndex(edgeIndex);
            if (edge == null) {return null;}
            return edge;
        }

        /*
         * Retrieves a Node that matches a given coordinate.
         */
        private Node GetNodeByCoordinate(Vect3 coordinate) {
            foreach (Node n in nodes) {
                if (n.coordinate.Equals(coordinate)) {
                    return n;
                }
            }
            return null;
        }

        /*
         * Retrieves a Node that has a given index in this NodeGraph.
         */
        private Node GetNodeByIndex(int index) {
            foreach (Node n in nodes) {
                if (n.index.Equals(index)) {
                    return n;
                }
            }
            return null;
        }

        private Edge GetEdgeByIndex(int index)
        {
            foreach(Edge e in edges)
            {
                if (e.index.Equals(index))
                {
                    return e;
                }
            }
            return null;
        }

        /*
         * Adds a Node without any link. If a node at the same position already exists in the NodeGraph, no node is added.
         */
        public bool AddNode(Node node) {
            return AddNode(node.coordinate);
        }

        /*
         * Adds a Node by coordinate without any link. If a node at the same position already exists in the NodeGraph, no node is added.
         */
        public bool AddNode(Vect3 coordinate) {
            Node internalNode = GetNodeByCoordinate(coordinate);
            if (internalNode == null) {
                Node node = new Node(coordinate);
                node.index = nodeIndexCounter++;
                nodes.Add(node);
                return true;
            }
            else
            {
                return false;
            }
        }

        /*
         * Adds a Node and links it to the provided neighbor nodes.
         */
        public bool AddNode(Node node, params Node[] neighbors) {
            if (!AddNode(node)) {return false;}
            foreach (Node neighbor in neighbors) {
                LinkNodes(node, neighbor);
            }
            return true;
        }

        /*
         * Adds a Node and links it to the provided neighbor nodes.
         */
        public bool AddNode(Node node, params (Node, double)[] neighborPairs) {
            if (!AddNode(node)) {return false;}
            foreach ((Node, double) pair in neighborPairs) {
                LinkNodes(node, pair.Item1, pair.Item2);
            }
            return true;
        }

        /*
         * Adds a Node and links it to Nodes with given coordinates, if they exist.
         */
        public bool AddNode(Node node, params Vect3[] neighborCoordinates) {
            return AddNode(node.coordinate, neighborCoordinates);
        }

        /*
         * Creates a Node at given coordinates and links it to Nodes with the remaining given coordinates, if they exist.
         */
        public bool AddNode(Vect3 coordinate, params Vect3[] neighborCoordinates) {
            bool result = AddNode(coordinate);
            foreach (Vect3 neighborCoordinate in neighborCoordinates) {
                Node neighborNode = GetNodeByCoordinate(neighborCoordinate);

                if (neighborNode == null) {
                    throw new NullNodeException("Tried to link node to neighbor that does not exist in the node graph.");
                }
                result = LinkNodes(coordinate, neighborNode.coordinate) || result;
            }
            return result;
        }

        /*
         * Adds a Node and links it to Nodes with given coordinates, if they exist.
         */
        public bool AddNode(Node node, params (Vect3, double)[] neighborPairs) {
            if (!AddNode(node)) {return false;}
            foreach ((Vect3, double) neighborPair in neighborPairs) {
                Node neighborNode = GetNodeByCoordinate(neighborPair.Item1);

                if (neighborNode == null) {
                    throw new NullNodeException("Tried to link node to neighbor that does not exist in the node graph.");
                }
                LinkNodes(node, neighborNode, neighborPair.Item2);
            }
            return true;
        }

        /*
         * Adds a Node and links it to Nodes with given nodegraph indices, if they exist.
         */
        public bool AddNode(Node node, params int[] neighborIndices) {
            if (!AddNode(node)) {return false;}

            foreach (int neighborIndex in neighborIndices) {
                Node neighborNode = GetNodeByIndex(neighborIndex);

                if (neighborNode == null) {
                    throw new NullNodeException("Tried to link node to neighbor that does not exist in the node graph.");
                }

                LinkNodes(node, neighborNode);
            }
            return true;
        }

        /*
         * Adds a Node and links it to Nodes with given nodegraph indices, if they exist.
         */
        public bool AddNode(Node node, params (int, double)[] neighborPairs) {
            if (!AddNode(node)) {return false;}

            foreach ((int, double) neighborPair in neighborPairs) {
                Node neighborNode = GetNodeByIndex(neighborPair.Item1);

                if (neighborNode == null) {
                    throw new NullNodeException("Tried to link node to neighbor that does not exist in the node graph.");
                }

                LinkNodes(node, neighborNode, neighborPair.Item2);
            }
            return true;
        }

        /*
         * Adds a Node with a width and height
         * and links it to Nodes with given nodegraph indices, if they exist.
         * Always returns true.
         */
        public bool AddNode(Vect3 coordinate, double width, double height, params int[] neighborIndices)
        {
            Node internalNode = GetNodeByCoordinate(coordinate);
            if (internalNode == null)
            {
                Node newNode = new Node(coordinate)
                {
                    index = nodeIndexCounter++
                };
                nodes.Add(newNode);
                internalNode = newNode;
            }
            foreach (int neighbourIndex in neighborIndices)
            {
                LinkNodes(internalNode.index, neighbourIndex, width, height);
            }
            return true;
        }

        /*
         * Move a Node in the NodeGraph to specified coordinate.
         */
        public Node MoveNode(Node node, Vect3 coordinate) {
            return MoveNode(node.coordinate, coordinate);
        }

        /*
         * Move a Node in the NodeGraph that exists at a given coordinate to another coordinate.
         */
        public Node MoveNode(Vect3 nodeCoordinate, Vect3 targetCoordinate) {
            Node internalNode = GetNodeByCoordinate(nodeCoordinate);
            if (internalNode == null) {return null;}

            RelocateNode(internalNode, targetCoordinate);
            return internalNode;
        }

        /*
         * Performs Node relocation for MoveNode if it is trivial, otherwise starts a Node merge.
         */
        private void RelocateNode(Node nodeToMove, Vect3 targetCoordinate) {
            Node nodeToMerge = GetNodeByCoordinate(targetCoordinate);
            if (nodeToMerge == null)
            {
                nodeToMove.coordinate = targetCoordinate;
            }
            else if (nodeToMerge == nodeToMove) {return;}
            else
            {
                MergeNodes(nodeToMerge, nodeToMove);
            }
        }

        /*
         * Combines two nodes into one by moving all edges from one to the other.
         */
        private void MergeNodes(Node nodeToMerge, Node nodeToMove)
        {

            Edge mergeToMoveEdge = GetEdge(nodeToMerge, nodeToMove);
            if (mergeToMoveEdge != null)
            {
                // When moving nodeToMove to nodeToMerge's position, smallest width of edges to all neighbor's of nodeToMove is changed
                // to smallest width of edge from neighbor to nodeToMove and edge from nodeToMove to nodeToMerge.
                double mergeToMoveWidth = mergeToMoveEdge.width > 0 ? mergeToMoveEdge.width : double.MaxValue;
                double mergeToMoveHeight = mergeToMoveEdge.width > 0 ? mergeToMoveEdge.width : double.MaxValue;

                foreach(NodeEdgePair pair in nodeToMove.neighbors){
                    SetEdgeWidth(pair.edgeIndex, Math.Min(mergeToMoveWidth, GetEdgeWidth(pair.edgeIndex)));
                    SetEdgeHeight(pair.edgeIndex, Math.Min(mergeToMoveHeight, GetEdgeWidth(pair.edgeIndex)));
                }
            }

            // Add all neighbors of nodeToMerge to nodeToMove
            for (int i = 0; i < nodeToMerge.neighbors.Count; i++)
            {
                LinkNodes(nodeToMove.index, nodeToMerge.neighbors[i].nodeIndex, GetEdge(nodeToMerge.neighbors[i].edgeIndex).width, GetEdge(nodeToMerge.neighbors[i].edgeIndex).height);
            }

            UnlinkAndDeleteNode(nodeToMerge);

            // Change coordinate rather than relink to nodeToMerge because this preserves NodeGraph indexing.
            nodeToMove.coordinate = nodeToMerge.coordinate;
        }

        /*
         * Unify two nodes, merging all there edges to one node
         * Keeping the node that has the lowest Y-value
         */
        public Node UnifyNodes(Node node1, Node node2)
        {
            Node nodeToKeep;
            Node nodeToMerge;
            if (node1.coordinate.y <= node2.coordinate.y)
            {
                nodeToKeep = node1;
                nodeToMerge = node2;
            }
            else
            {
                nodeToKeep = node2;
                nodeToMerge = node1;
            }

            foreach (NodeEdgePair neighbor in nodeToMerge.neighbors)
            {
                if (neighbor.nodeIndex != nodeToKeep.index)
                {
                    Edge neighborEdge = GetEdge(neighbor.edgeIndex);
                    AddEdge(nodeToKeep.index, neighbor.nodeIndex, neighborEdge.width, neighborEdge.height);
                }
            }
            UnlinkAndDeleteNode(nodeToMerge);
            return nodeToKeep;
        }

        /*
         * Removes a Node if it exists in the node graph. Return value is true if a Node was successfully removed.
         */
        public bool RemoveNode(Node node) {
            return RemoveNode(node.coordinate);
        }

        /*
         * Removes a Node with matching coordinates if it exists in the node graph. Return value is true if a Node was successfully removed.
         */
        public bool RemoveNode(Vect3 coordinate) {
            Node node = GetNodeByCoordinate(coordinate);

            if (node == null) {return false;}

            UnlinkAndDeleteNode(node);
            return true;
        }

        /*
         * Removes a Node with a given index if it exists in the node graph. Return value is true if a Node was successfully removed.
         */
        public bool RemoveNode(int nodeIndex) {
            Node node = GetNodeByIndex(nodeIndex);

            if (node == null) {return false;}

            UnlinkAndDeleteNode(node);
            return true;
        }

        /*
         * Add an edge betwwen two nodes, given an width and height
         */
        public Edge AddEdge(int nodeIndex1, int nodeIndex2, double width, double height)
        {
            Edge edge = new Edge(nodeIndex1, nodeIndex2, edgeIndexCounter++, width, height);
            edges.Add(edge);
            Node node1 = GetNode(nodeIndex1);
            Node node2 = GetNode(nodeIndex2);
            node1.neighbors.Add(new NodeEdgePair(nodeIndex2, edge.index));
            node2.neighbors.Add(new NodeEdgePair(nodeIndex1, edge.index));
            return edge;
        }


        /*
         * Helper function that removes a specified Node and ensures all Edges that links to it are removed throughout the NodeGraph.
         */
        private void UnlinkAndDeleteNode(Node nodeToDelete)
        {
            var edgesToRemove = new List<Edge>();
            var edgeIndicesToRemove = new List<int>();

            nodes.Remove(nodeToDelete);

            foreach (Edge edge in edges) {
                if (edge.nodeIndex1 == nodeToDelete.index ||
                    edge.nodeIndex2 == nodeToDelete.index)
                {
                    edgesToRemove.Add(edge);
                }
            }

            foreach (Node node in nodes) {

                var pairsToRemove = new List<NodeEdgePair>();

                foreach (NodeEdgePair pair in node.neighbors) {
                    foreach (Edge edgeToRemove in edgesToRemove) {
                        if (pair.edgeIndex == edgeToRemove.index) {
                            pairsToRemove.Add(pair);
                            continue;
                        }
                    }
                }

                foreach (NodeEdgePair pair in pairsToRemove) {
                    node.neighbors.Remove(pair);
                }
            }

            foreach (Edge edge in edgesToRemove) {
                edges.Remove(edge);
            }
        }

        /*
         * Removes an Edge if it exists in the node graph. Return value is true if an Edge was successfully removed.
         */
        public bool RemoveEdge(Node node1, Node node2) {
            return RemoveEdge(node1.index, node2.index);
        }

        /*
         * Removes an Edge that links nodes at two specified coordinates if such an Edge exists in the node graph.
         * Return value is true if an Edge was successfully removed.
         */
        public bool RemoveEdge(Vect3 coordinate1, Vect3 coordinate2) {

            Node node1 = GetNodeByCoordinate(coordinate1);
            Node node2 = GetNodeByCoordinate(coordinate2);

            if (node1 == null || node2 == null) {return false;}

            return RemoveEdge(node1.index, node2.index);
        }

        /*
         * Removes an Edge specified by an index if it exists in the node graph.
         * Return value is true if an Edge was successfully removed.
         */
        public bool RemoveEdge(int edgeIndex) {

            Edge edge = GetEdgeByIndex(edgeIndex);

            if (edge == null) {return false;}

            DeleteEdge(edge);
            return true;
        }

        /*
         * Removes an Edge that links nodes with specified indices if such an Edge exists in the node graph.
         * Return value is true if an Edge was successfully removed.
         */
        public bool RemoveEdge(int nodeIndex1, int nodeIndex2) {
            Edge edgeToRemove = GetEdgeBetweenIndices(nodeIndex1, nodeIndex2);

            if (edgeToRemove == null) {return false;}

            DeleteEdge(edgeToRemove);
            return true;
        }

        /*
         * Removes a given Edge if that Edge exists in the node graph.
         * Return value is true if an Edge was successfully removed.
         */
        public bool RemoveEdge(Edge edge) {
            if (!edges.Contains(edge)) {return false;}

            DeleteEdge(edge);
            return true;
        }

        /*
         * Helper function that handles edge deletion, makes sure all references to the edge in the NodeGraph are removed.
         */
        private void DeleteEdge(Edge edgeToRemove)
        {
            edges.Remove(edgeToRemove);

            Node node1 = GetNodeByIndex(edgeToRemove.nodeIndex1);
            Node node2 = GetNodeByIndex(edgeToRemove.nodeIndex2);

            // Find the NodeEdgePair that contains the Edge we removed and remove it from the
            // NodeEdgePair list of the nodes.
            node1.neighbors.Remove(node1.neighbors.Find(x => x.edgeIndex == edgeToRemove.index));
            node2.neighbors.Remove(node2.neighbors.Find(x => x.edgeIndex == edgeToRemove.index));
        }

        /* Retrieves an Edge that links two nodes specified by indices, if it exists. Otherwise returns null. */
        private Edge GetEdgeBetweenIndices(int nodeIndex1, int nodeIndex2)
        {
            foreach (Edge edge in edges) {
                if (edge.nodeIndex1 == nodeIndex1 && edge.nodeIndex2 == nodeIndex2 ||
                    edge.nodeIndex1 == nodeIndex2 && edge.nodeIndex2 == nodeIndex1) {
                        return edge;
                    }
            }
            return null;
        }

        /*
         * Creates an Edge between two specified Nodes.
         */
        public bool LinkNodes(Node start, Node end, double width = -1, double height = -1)
        {
            return LinkNodes(start.coordinate, end.coordinate, width, height);
        }

        /*
         * Creates an Edge between two Nodes specified by their coordinates.
         */
        public bool LinkNodes(Vect3 startCoordinate, Vect3 endCoordinate, double width = -1, double height = -1) {
            Node linkStart = GetNodeByCoordinate(startCoordinate);
            Node linkEnd = GetNodeByCoordinate(endCoordinate);

            if (linkStart == null)
            {
                throw new NullNodeException("Attempted to link non-existent start node.");
            }

            if (linkEnd == null)
            {
                throw new NullNodeException("Attempted to link non-existent end node.");
            }

            if (linkStart == linkEnd) {return false;}

            return CreateNodeLink(linkStart.index, linkEnd.index, width, height);
        }

        /*
         * Creates an Edge between two Nodes specified by their NodeGraph indices.
         */
        public bool LinkNodes(int startIndex, int endIndex, double width = -1, double height = -1) {
            Node linkStart = GetNodeByIndex(startIndex);
            Node linkEnd = GetNodeByIndex(endIndex);

            if (linkStart == null)
            {
                throw new NullNodeException("Attempted to link non-existent start node.");
            }

            if (linkEnd == null)
            {
                throw new NullNodeException("Attempted to link non-existent end node.");
            }

            if (linkStart == linkEnd) {return false;}

            return CreateNodeLink(linkStart.index, linkEnd.index, width, height);
        }

        /*
         * Helper function that creates an Edge between two existing Nodes.
         */
        private bool CreateNodeLink(int startIndex, int endIndex, double width, double height)
        {
            if (AreLinked(startIndex, endIndex)) {
                if (GetEdge(startIndex, endIndex).width == -1 && width != -1)
                {
                    SetEdgeWidth(startIndex, endIndex, width);
                    SetEdgeHeight(startIndex, endIndex, height);

                    return true;
                }
                return false;
            }

            Edge edge = new Edge(startIndex, endIndex, edgeIndexCounter++, width, height);
            edges.Add(edge);

            Node node1 = GetNodeByIndex(startIndex);
            Node node2 = GetNodeByIndex(endIndex);

            node1.neighbors.Add(new NodeEdgePair(node2.index, edge.index));
            node2.neighbors.Add(new NodeEdgePair(node1.index, edge.index));
            return true;
        }

        /*
         * Reindexes the Node and Edge indexing from 0. When removing elements, the list indexing and
         * NodeGraph indexing values used for XML generation later may become detached. Without arguments,
         * this method reorders Node and Edge indices so that they match the C# list indexing. When offsets N, M,
         * are specified the NodeGraph is indexed as if there were N number of existing Nodes and M number of
         * existing Edges, which can be useful for merging two NodeGraphs.
         */
        public void ReIndexNodeGraph() {
            ReIndexNodeGraph(/*nodeOffset*/ 0, /*edgeOffset*/ 0);
        }

        /*
         * Reindexes the Node and Edge indexing from 0. When removing elements, the list indexing and
         * NodeGraph indexing values used for XML generation later may become detached. Without arguments,
         * this method reorders Node and Edge indices so that they match the C# list indexing. When offsets N, M,
         * are specified the NodeGraph is indexed as if there were N number of existing Nodes and M number of
         * existing Edges, which can be useful for merging two NodeGraphs.
         */
        public void ReIndexNodeGraph(int nodeOffset, int edgeOffset) {
            nodeIndexCounter = nodeOffset;
            edgeIndexCounter = edgeOffset;

            int tempIndex;

            foreach (Node outerNode in nodes) {
                tempIndex = outerNode.index;
                outerNode.index = nodeIndexCounter++;

                foreach (Edge edge in edges) {
                    if (edge.nodeIndex1 == tempIndex) {
                        edge.nodeIndex1 = outerNode.index;
                    }

                    if (edge.nodeIndex2 == tempIndex) {
                        edge.nodeIndex2 = outerNode.index;
                    }
                }

                foreach (Node innerNode in nodes) {
                    // Node can't have NodeEdgePairs to itself
                    if (innerNode == outerNode) {continue;}

                    foreach (NodeEdgePair pair in innerNode.neighbors)
                    {
                        if (pair.nodeIndex == tempIndex) {
                            pair.nodeIndex = outerNode.index;
                        }
                    }
                }
            }

            foreach (Edge edge in edges) {
                tempIndex = edge.index;
                edge.index = edgeIndexCounter++;

                foreach (Node node in nodes) {
                    foreach (NodeEdgePair pair in node.neighbors)
                    {
                        if (pair.edgeIndex == tempIndex) {
                            pair.edgeIndex = edge.index;
                        }
                    }
                }
            }
        }

        /*
         * Checks whether there is an Edge connecting two Nodes in the NodeGraph.
         */
        public bool AreLinked(Node node1, Node node2) {
            return AreLinked(node1.coordinate, node2.coordinate);
        }

        /*
         * Checks whether there is an Edge connecting two Nodes in the NodeGraph, where
         * Nodes are specified by coordinates.
         */
        public bool AreLinked(Vect3 coordinate1, Vect3 coordinate2) {
            Node node1 = GetNodeByCoordinate(coordinate1);
            Node node2 = GetNodeByCoordinate(coordinate2);

            if (node1 == null || node2 == null) {return false;}

            return AreLinked(node1.index, node2.index);
        }

        /*
         * Checks whether there is an Edge connecting two Nodes in the NodeGraph, where
         * Nodes are specified by NodeGraph indices.
         */
        public bool AreLinked(int nodeIndex1, int nodeIndex2) {

            if (nodeIndex1 == nodeIndex2) {return false;}

            foreach (Edge edge in edges) {
                if ((edge.nodeIndex1 == nodeIndex1 &&
                    edge.nodeIndex2 == nodeIndex2) ||
                    (edge.nodeIndex1 == nodeIndex2 &&
                    edge.nodeIndex2 == nodeIndex1))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * Returns a node index that is at the collision point
         * If no point on collision point, create a new node a the point and
         * link with rest of graph.
         */
        public int GetCollisionNodeIndex(Vect3 collisionPoint, Edge edge)
        {
            if (PointCloseToNode(collisionPoint, GetNode(edge.nodeIndex1)))
            {
                return edge.nodeIndex1;
            }
            else if (PointCloseToNode(collisionPoint, GetNode(edge.nodeIndex2)))
            {
                return edge.nodeIndex2;
            }
            else if(PointOnEdge(collisionPoint, edge))
            {
                int node1 = edge.nodeIndex1;
                int node2 = edge.nodeIndex2;
                RemoveEdge(edge);
                if (!AddNode(collisionPoint, edge.width, edge.height, node1, node2))
                {
                    throw new Exception("Couldn't add node when collision is on edge");
                }
                return nodeIndexCounter - 1;
            }
            else
            {
                throw new Exception("Wants collsion node, collision outside off edge");
            }
        }

        /*
         * True if a point is with in NODE_RADIUS, a constant.
         */
        public bool PointCloseToNode(Vect3 point, Node node)
        {
            Vect3 nodeCoordXZ = new Vect3(node.coordinate.x, 0, node.coordinate.z);
            Vect3 pointXZ = new Vect3(point.x, 0, point.z);
            return (nodeCoordXZ - pointXZ).Length() < NODE_RADIUS;
        }

        /*
         * True if a point exists on an en edge
         */
        public bool PointOnEdge(Vect3 point, Edge edge)
        {
            Node node1 = GetNode(edge.nodeIndex1);
            Node node2 = GetNode(edge.nodeIndex2);
            if (PointCloseToNode(point, GetNode(node1.index)) ||
                PointCloseToNode(point, GetNode(node2.index)))
            {
                return true;
            }
            else
            {
                return point.x <= Math.Max(node1.coordinate.x, node2.coordinate.x) &&
                       point.x >= Math.Min(node1.coordinate.x, node2.coordinate.x) &&
                       point.z <= Math.Max(node1.coordinate.z, node2.coordinate.z) &&
                       point.z >= Math.Min(node1.coordinate.z, node2.coordinate.z);
            }
        }
    }
}
