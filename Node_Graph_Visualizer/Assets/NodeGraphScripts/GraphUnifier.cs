using System;
using System.Collections.Generic;

namespace Nodegraph_Generator
{
    /*
     * Class for unifying smaller graphs into one big.
     */
    public static class GraphUnifier
    {
        /*
         * Takes in a list of nodeGraphs and unify them into one nodeGraph.
         */
        public static NodeGraph UnifyGraphs(List<NodeGraph> unifyingGraphs)
        {
            NodeGraph baseGraph = unifyingGraphs[0];
            unifyingGraphs.Remove(baseGraph);

            // Define Variables
            var joiningEdges = new Stack<Edge>();
            int newNodeIndex1;
            int newNodeIndex2;
            Edge joiningEdge;
            Edge baseEdge;
            var edgesAdded = new List<Edge>();
            Vect3 collisionPoint;
            int baseNodeIndex;
            int extraJoiningNodeIndex;
            Edge extraJoiningEdge1;
            Edge extraJoiningEdge2;

            // Unify each graph with the base graph
            foreach (NodeGraph joiningGraph in unifyingGraphs)
            {
                // Fill edge stack of joining edges
                joiningEdges.Clear();
                for (int i = joiningGraph.Edges.Count-1; i>=0; i--)
                {
                    joiningEdges.Push(joiningGraph.GetEdge(i));
                }

                newNodeIndex1 = -1;
                newNodeIndex2 = -1;
                edgesAdded.Clear();

                // Join edges LOOP
                // Find which node indexes that the edge should have.
                // If -1 a new node needs to be created.
                while (joiningEdges.Count > 0)
                {
                    joiningEdge = joiningEdges.Pop();
                    newNodeIndex2 = newNodeIndex1;
                    newNodeIndex1 = -1;
                    for (int i = 0; i < baseGraph.Edges.Count; i++)
                    {
                        baseEdge = baseGraph.GetEdge(i);
                        if (baseEdge == null || edgesAdded.Contains(baseEdge)) { continue; }

                        // If base edge and joining edge has a collision, find where on the base graph the collision is.
                        // To find out if a new node needs to be created or of the graph should join an exisiting node.
                        if (EdgesCollide (baseEdge, joiningEdge, baseGraph, joiningGraph))
                        {
                            collisionPoint = GetEdgesCollidingPoint(baseEdge, joiningEdge, baseGraph, joiningGraph);

                            if (collisionPoint != null)
                            {
                                if (joiningGraph.PointCloseToNode(collisionPoint, joiningGraph.GetNode(joiningEdge.nodeIndex1)))
                                {
                                    baseNodeIndex = baseGraph.GetCollisionNodeIndex(collisionPoint, baseEdge);
                                    if (newNodeIndex1 == -1)
                                    {
                                        newNodeIndex1 = baseNodeIndex;
                                    }
                                    else if (newNodeIndex1 != baseNodeIndex)
                                    {
                                        newNodeIndex1 = baseGraph.UnifyNodes(baseGraph.GetNode(newNodeIndex1), baseGraph.GetNode(baseNodeIndex)).index;
                                            baseGraph.MoveNode(baseGraph.GetNode(newNodeIndex1), baseGraph.GetNode(baseNodeIndex).coordinate);
                                    }
                                }

                                else if (joiningGraph.PointCloseToNode(collisionPoint, joiningGraph.GetNode(joiningEdge.nodeIndex2)))
                                {
                                    baseNodeIndex = baseGraph.GetCollisionNodeIndex(collisionPoint, baseEdge);
                                    if (newNodeIndex2 == -1)
                                    {
                                        newNodeIndex2 = baseNodeIndex;
                                    }
                                    else if (newNodeIndex2 != baseNodeIndex)
                                    {
                                        newNodeIndex2 = baseGraph.UnifyNodes(baseGraph.GetNode(newNodeIndex2), baseGraph.GetNode(baseNodeIndex)).index;
                                    }
                                }

                                else if (joiningGraph.PointOnEdge(collisionPoint, joiningEdge))
                                {
                                    joiningGraph.AddNode(collisionPoint);
                                    extraJoiningNodeIndex = joiningGraph.GetNode(collisionPoint).index;
                                    extraJoiningEdge1 = joiningGraph.AddEdge(joiningEdge.nodeIndex1, extraJoiningNodeIndex,
                                                                      joiningEdge.width, joiningEdge.height);
                                    extraJoiningEdge2 = joiningGraph.AddEdge(extraJoiningNodeIndex, joiningEdge.nodeIndex2,
                                                                      joiningEdge.width, joiningEdge.height);
                                    joiningEdge = extraJoiningEdge2;
                                    joiningEdges.Push(extraJoiningEdge1);
                                    baseNodeIndex = baseGraph.GetCollisionNodeIndex(collisionPoint, baseEdge);
                                    newNodeIndex1 = baseNodeIndex;
                                }
                            }
                        }
                    }

                    // After base graph edge loop. Add Edge to base graph correctly.
                    if (newNodeIndex1 == -1)
                    {
                        baseGraph.AddNode(joiningGraph.GetNode(joiningEdge.nodeIndex1));
                        newNodeIndex1 = baseGraph.GetNode(joiningGraph.GetNode(joiningEdge.nodeIndex1).coordinate).index;
                    }
                    if (newNodeIndex2 == -1)
                    {
                        baseGraph.AddNode(joiningGraph.GetNode(joiningEdge.nodeIndex2));
                        newNodeIndex2 = baseGraph.GetNode(joiningGraph.GetNode(joiningEdge.nodeIndex2).coordinate).index;
                    }

                    if (newNodeIndex1 == newNodeIndex2)
                    {
                        throw new Exception("Same new node index, " + newNodeIndex1);
                    }
                    edgesAdded.Add(baseGraph.AddEdge(newNodeIndex1, newNodeIndex2, joiningEdge.width, joiningEdge.height));
                }
                baseGraph.ReIndexNodeGraph();
            }
            return baseGraph;
        }


        /*
         * Check if two edges collide with eachother.
         */
        public static bool EdgesCollide(Edge edge1, Edge edge2, NodeGraph nodeGraph1, NodeGraph nodeGraph2)
        {
            // Set up Edge1
            Node edge1Node1 = nodeGraph1.GetNode(edge1.nodeIndex1);
            Node edge1Node2 = nodeGraph1.GetNode(edge1.nodeIndex2);
            Vect3 edge1Forward = (edge1Node2.coordinate - edge1Node1.coordinate).GetNormalized();
            Vect3 edge1Right = Vect3.Cross(edge1Forward, Vect3.Up).GetNormalized();
            Vect3 edge1Up = Vect3.Cross(edge1Right, edge1Forward).GetNormalized();
            AxisBBox bBox1 = GetEdgeBoundingBox(edge1, edge1Node1.coordinate, edge1Node2.coordinate,
                                                                    edge1Right, edge1Up);
            // Set up Edge2
            Node edge2Node1 = nodeGraph2.GetNode(edge2.nodeIndex1);
            Node edge2Node2 = nodeGraph2.GetNode(edge2.nodeIndex2);
            Vect3 edge2Forward = (edge2Node2.coordinate - edge2Node1.coordinate).GetNormalized();
            Vect3 edge2Right = Vect3.Cross(edge2Forward, Vect3.Up).GetNormalized();
            Vect3 edge2Up = Vect3.Cross( edge2Right, edge2Forward).GetNormalized();
            AxisBBox bBox2 = GetEdgeBoundingBox(edge2, edge2Node1.coordinate, edge2Node2.coordinate,
                                                                    edge2Right, edge2Up);
            // Check BoundingBoxes
            if (bBox1.Overlaps(bBox2))
            {
                if (IsParallel2D((edge1Node1.coordinate.x, edge1Node1.coordinate.z), (edge1Node2.coordinate.x, edge1Node2.coordinate.z),
                                 (edge2Node1.coordinate.x, edge2Node1.coordinate.z), (edge2Node2.coordinate.x, edge2Node2.coordinate.z)))
                {
                    return ParallelEdgesCollide(edge1, edge2, nodeGraph1, nodeGraph2);
                }
                else
                {
                    Vect3 edge1RightWall1 = edge1Node1.coordinate + ((edge1.width / 2) * edge1Right);
                    Vect3 edge1RightWall2 = edge1Node2.coordinate + ((edge1.width / 2) * edge1Right);
                    Vect3 edge2RightWall1 = edge2Node1.coordinate + ((edge2.width / 2) * edge2Right);
                    Vect3 edge2RightWall2 = edge2Node2.coordinate + ((edge2.width / 2) * edge2Right);

                    Vect3 edge1LeftWall1 = edge1Node1.coordinate - ((edge1.width / 2) * edge1Right);
                    Vect3 edge1LeftWall2 = edge1Node2.coordinate - ((edge1.width / 2) * edge1Right);
                    Vect3 edge2LeftWall1 = edge2Node1.coordinate - ((edge2.width / 2) * edge2Right);
                    Vect3 edge2LeftWall2 = edge2Node2.coordinate - ((edge2.width / 2) * edge2Right);
                    // Check all Possible wall collisions.
                    return WallsIntersect(edge1, edge2, edge1RightWall1, edge1RightWall2, edge2RightWall1, edge2RightWall2) ||
                       WallsIntersect(edge1, edge2, edge1RightWall1, edge1RightWall2, edge2LeftWall1, edge2LeftWall2) ||
                       WallsIntersect(edge1, edge2, edge1LeftWall1, edge1LeftWall2, edge2RightWall1, edge2RightWall2) ||
                       WallsIntersect(edge1, edge2, edge1LeftWall1, edge1LeftWall2, edge2LeftWall1, edge2LeftWall2)
                       ||
                       WallsIntersect(edge1, edge2, edge1RightWall1, edge1LeftWall1, edge2RightWall1, edge2RightWall2) ||
                       WallsIntersect(edge1, edge2, edge1RightWall1, edge1LeftWall1, edge2LeftWall1, edge2LeftWall2) ||
                       WallsIntersect(edge1, edge2, edge1RightWall2, edge1LeftWall2, edge2RightWall1, edge2RightWall2) ||
                       WallsIntersect(edge1, edge2, edge1RightWall2, edge1LeftWall2, edge2LeftWall1, edge2LeftWall2)
                       ||
                       WallsIntersect(edge1, edge2, edge1RightWall1, edge1RightWall2, edge2RightWall1, edge2LeftWall1) ||
                       WallsIntersect(edge1, edge2, edge1RightWall1, edge1RightWall2, edge2RightWall2, edge2LeftWall2) ||
                       WallsIntersect(edge1, edge2, edge1LeftWall1, edge1LeftWall2, edge2RightWall1, edge2LeftWall1) ||
                       WallsIntersect(edge1, edge2, edge1LeftWall1, edge1LeftWall2, edge2RightWall2, edge2LeftWall2)
                       ||
                       WallsIntersect(edge1, edge2, edge1RightWall1, edge1LeftWall1, edge2RightWall1, edge2LeftWall1) ||
                       WallsIntersect(edge1, edge2, edge1RightWall1, edge1LeftWall1, edge2RightWall2, edge2LeftWall2) ||
                       WallsIntersect(edge1, edge2, edge1RightWall2, edge1LeftWall2, edge2RightWall1, edge2LeftWall1) ||
                       WallsIntersect(edge1, edge2, edge1RightWall2, edge1LeftWall2, edge2RightWall2, edge2LeftWall2);
                }
            }
            else
            {
                return false;
            }
        }

        /*
         * Check if two parallel edges collide.
         */
        public static bool ParallelEdgesCollide(Edge edge1, Edge edge2, NodeGraph graph1, NodeGraph graph2)
        {
            Node edge1Node1 = graph1.GetNode(edge1.nodeIndex1);
            Node edge1Node2 = graph1.GetNode(edge1.nodeIndex2);
            Node edge2Node1 = graph2.GetNode(edge2.nodeIndex1);
            Node edge2Node2 = graph2.GetNode(edge2.nodeIndex2);
            return graph1.PointCloseToNode(edge2Node1.coordinate, edge1Node1) || graph1.PointCloseToNode(edge2Node2.coordinate, edge1Node1) ||
                   graph1.PointCloseToNode(edge2Node1.coordinate, edge1Node2) || graph1.PointCloseToNode(edge2Node2.coordinate, edge1Node2);
        }

        // Check if two edge walls intersect.
        public static bool WallsIntersect(Edge edge1, Edge edge2, Vect3 edge1WallStart, Vect3 edge1WallEnd, Vect3 edge2WallStart, Vect3 edge2WallEnd)
        {
            (double x, double z) = GetIntersectionPoint2D((edge1WallStart.x, edge1WallStart.z),
                                                          (edge1WallEnd.x, edge1WallEnd.z),
                                                          (edge2WallStart.x, edge2WallStart.z),
                                                          (edge2WallEnd.x, edge2WallEnd.z));
            // Check if 2D point is inside both walls.
            if (x <= Math.Max(edge1WallStart.x, edge1WallEnd.x) && x >= Math.Min(edge1WallStart.x, edge1WallEnd.x) &&
                z <= Math.Max(edge1WallStart.z, edge1WallEnd.z) && z >= Math.Min(edge1WallStart.z, edge1WallEnd.z) &&
                x <= Math.Max(edge2WallStart.x, edge2WallEnd.x) && x >= Math.Min(edge2WallStart.x, edge2WallEnd.x) &&
                z <= Math.Max(edge2WallStart.z, edge2WallEnd.z) && z >= Math.Min(edge2WallStart.z, edge2WallEnd.z))

            {
                double edge1Y = GetY2D((edge1WallStart.x, edge1WallStart.z),
                                       (edge1WallEnd.x, edge1WallEnd.z), x);
                double edge2Y = GetY2D((edge2WallStart.x, edge2WallStart.z),
                                       (edge2WallEnd.x, edge2WallEnd.z), x);
                // Check if Y collision point is inside both walls.
                return (edge1Y >= edge2Y && edge1Y <= edge2Y + edge2.height) ||
                       (edge2Y >= edge1Y && edge2Y <= edge1Y + edge1.height);
            }
            else
            {
                return false;
            }
        }

        /*
         * Get bounding box of edge to quickly eliminate impossible collisions.
         */
        public static AxisBBox GetEdgeBoundingBox(Edge edge, Vect3 node1, Vect3 node2, Vect3 right, Vect3 up)
        {
            Vect3 rightWall1 = node1 + ((edge.width / 2) * right);
            Vect3 leftWall1 = node1 - ((edge.width / 2) * right);
            Vect3 roof1 = node1 + edge.height * up;

            Vect3 rightWall2 = node2 + ((edge.width / 2) * right);
            Vect3 leftWall2 = node2 - ((edge.width / 2) * right);
            Vect3 roof2 = node2 + edge.height * up;

            return new AxisBBox(Math.Max(Math.Max(rightWall1.x, leftWall1.x),
                                         Math.Max(rightWall2.x, leftWall2.x)),
                                Math.Max(roof1.y, roof2.y),
                                Math.Max(Math.Max(rightWall1.z, leftWall1.z),
                                         Math.Max(rightWall2.z, leftWall2.z)),
                                Math.Min(Math.Min(rightWall1.x, leftWall1.x),
                                         Math.Min(rightWall2.x, leftWall2.x)),
                                Math.Min(node1.y, node2.y),
                                Math.Min(Math.Min(rightWall1.z, leftWall1.z),
                                         Math.Min(rightWall2.z, leftWall2.z)));
        }

        /*
         * Check if two lines are parallel in 2D
         */
        public static bool IsParallel2D((double, double) p1, (double, double) p2, (double, double) p3, (double, double) p4)
        {
            double angleDiff = (new Vect3(p1.Item1 - p2.Item1, p1.Item2 - p2.Item2, 0).AngleTo(new Vect3(p3.Item1 - p4.Item1, p3.Item2 - p4.Item2, 0)));
            return angleDiff < 1 || angleDiff > 179;
        }

        /*
         * Returns a 2D intersection point.
         */
        public static (double, double) GetIntersectionPoint2D((double, double) p1, (double, double) p2, (double, double) p3, (double, double) p4)
        {
            double p1x = p1.Item1; double p1y = p1.Item2;
            double p2x = p2.Item1; double p2y = p2.Item2;
            double p3x = p3.Item1; double p3y = p3.Item2;
            double p4x = p4.Item1; double p4y = p4.Item2;

            double x = (p3y - p3x * ((p3y - p4y) / (p3x - p4x)) - p1y + p1x * ((p1y - p2y) / (p1x - p2x))) /
                        (((p1y - p2y) / (p1x - p2x)) - ((p3y - p4y) / (p3x - p4x)));

            return (x, GetY2D(p1, p2, x));
        }

        /*
         * Returns the Y of a line given an X-value.
         */
        public static double GetY2D((double, double) p1, (double, double) p2, double x)
        {
            double p1x = p1.Item1; double p1y = p1.Item2;
            double p2x = p2.Item1; double p2y = p2.Item2;
            return ((p1y - p2y) / (p1x - p2x)) * x + p1y - (p1x * ((p1y - p2y) / (p1x - p2x)));
        }

        /*
         * Get the closest point between two lines.
         */
        public static (double, double) GetClosestPoint2D((double, double) p1, (double, double) p2, (double, double) p3, (double, double) p4)
        {
            Vect3 point1 = new Vect3(p1.Item1, p1.Item2, 0);
            Vect3 point2 = new Vect3(p2.Item1, p2.Item2, 0);
            Vect3 point3 = new Vect3(p3.Item1, p3.Item2, 0);
            Vect3 point4 = new Vect3(p4.Item1, p4.Item2, 0);

            double length13 = (point1 - point3).Length();
            double length14 = (point1 - point4).Length();
            double length23 = (point2 - point3).Length();
            double length24 = (point2 - point4).Length();

            double minLength = Math.Min(Math.Min(length13, length14),
                                        Math.Min(length23, length24));
            if (minLength == length13)
            {
                return ((p1.Item1 + p3.Item1)/2, (p1.Item2 + p3.Item2)/2);
            }
            else if (minLength == length14)
            {
                return ((p1.Item1 + p4.Item1)/2, (p1.Item2 + p4.Item2)/2);
            }
            else if (minLength == length23)
            {
                return ((p2.Item1 + p3.Item1)/2, (p2.Item2 + p3.Item2)/2);
            }
            else
            {
                return ((p2.Item1 + p4.Item1)/2, (p2.Item2 + p4.Item2)/2);
            }
        }

        /*
         * Get the point where two edges collide.
         */
        public static Vect3 GetEdgesCollidingPoint(Edge edge1, Edge edge2, NodeGraph nodeGraph1, NodeGraph nodeGraph2)
        {
            Node edge1Node1 = nodeGraph1.GetNode(edge1.nodeIndex1);
            Node edge1Node2 = nodeGraph1.GetNode(edge1.nodeIndex2);

            Node edge2Node1 = nodeGraph2.GetNode(edge2.nodeIndex1);
            Node edge2Node2 = nodeGraph2.GetNode(edge2.nodeIndex2);

            double x; double z;

            if (IsParallel2D((edge1Node1.coordinate.x, edge1Node1.coordinate.z),
                             (edge1Node2.coordinate.x, edge1Node2.coordinate.z),
                             (edge2Node1.coordinate.x, edge2Node1.coordinate.z),
                             (edge2Node2.coordinate.x, edge2Node2.coordinate.z)))
            {
                (x, z) = GetClosestPoint2D((edge1Node1.coordinate.x, edge1Node1.coordinate.z),
                                                         (edge1Node2.coordinate.x, edge1Node2.coordinate.z),
                                                         (edge2Node1.coordinate.x, edge2Node1.coordinate.z),
                                                         (edge2Node2.coordinate.x, edge2Node2.coordinate.z));
            }
            else
            {
                (x, z) = GetIntersectionPoint2D((edge1Node1.coordinate.x, edge1Node1.coordinate.z),
                                              (edge1Node2.coordinate.x, edge1Node2.coordinate.z),
                                              (edge2Node1.coordinate.x, edge2Node1.coordinate.z),
                                              (edge2Node2.coordinate.x, edge2Node2.coordinate.z));
            }

            double y1 = GetY2D((edge1Node1.coordinate.x, edge1Node1.coordinate.y),
                               (edge1Node2.coordinate.x, edge1Node2.coordinate.y), x);
            double y2 = GetY2D((edge2Node1.coordinate.x, edge2Node1.coordinate.y),
                               (edge2Node2.coordinate.x, edge2Node2.coordinate.y), x);

            Vect3 collisionPoint = new Vect3(x, Math.Min(y1, y2), z);
            if (nodeGraph1.PointOnEdge(collisionPoint, edge1) && nodeGraph2.PointOnEdge(collisionPoint, edge2))
            {
                return collisionPoint;
            }
            else
            {
                return null;
            }
        }
    }
}
