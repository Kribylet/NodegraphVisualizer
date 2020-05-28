using System;
using System.Collections.Generic;

namespace Nodegraph_Generator
{
    /**
    * Static class containing functions for creating a nodegraph from a component.
    */
    public static class ComponentGraphGenerator
    {
        private const double AngleEqualityTolerance = .1d;
        private const double PLACE_HOLDER_HEIGHT = 700;

        /*
        * Generates a nodegraph from a component by identifing the floor faces and a linked list of the floor vertices.
        * Split that list in two, one list for each long side of the component. Then creating a node
        * by connecting the corresponding vertex in each linkedlist and placing that node between the two vertices.
        */
        public static NodeGraph GenerateComponentNodeGraph(Component component)
        {
            List<int> floorFaces = GetFloorFaces(component);
            LinkedList<int> floorSideVertices = GetShellVertices(component, floorFaces);
            RemoveRedundantPoints(ref floorSideVertices, component);
            (LinkedList<int>, LinkedList<int>) sides = SplitLinkedList(component, floorSideVertices);
            NodeGraph nodeGraph = CreateNodeGraphComponent(component, sides);
            nodeGraph.componentIndices.Add(component.index);
            return nodeGraph;
        }

        /*
         * Finds and removes redundant points in a circular list of vertices that describe an exterior ring of a mesh.
         *
         * Examines the list three elements at a time. If the vector formed by the first and second element has the same
         * direction as the the vector formed by the second and third element, the second element is redundant and removed.
         */
        private static void RemoveRedundantPoints(ref LinkedList<int> floorSideVertices, Component component)
        {

            Vect3 firstPoint;
            Vect3 secondPoint;
            Vect3 thirdPoint;

            Vect3 firstToSecond;;
            Vect3 secondToThird;

            //Handle midlist cases
            var current = floorSideVertices.First.Next;
            while (current != floorSideVertices.Last)
            {
                if (current == floorSideVertices.First) {current = current.Next; continue;}

                firstPoint = component.GetCoordinate(current.Previous.Value);
                secondPoint = component.GetCoordinate(current.Value);
                thirdPoint = component.GetCoordinate(current.Next.Value);

                firstToSecond = secondPoint - firstPoint;
                secondToThird = thirdPoint - secondPoint;

                if (firstToSecond.AngleTo(secondToThird) < AngleEqualityTolerance)
                {
                    current = current.Previous;
                    floorSideVertices.Remove(current.Next);
                }
                else
                {
                    current = current.Next;
                }
            }
            //Noncircular list data structure for circular list

            // Compare secondToLast->Last->First, if same, remove last, repeat check
            firstPoint = component.GetVertex(floorSideVertices.Last.Previous.Value).coordinate;
            secondPoint = component.GetVertex(floorSideVertices.Last.Value).coordinate;
            thirdPoint = component.GetVertex(floorSideVertices.First.Value).coordinate;

            firstToSecond = secondPoint - firstPoint;
            secondToThird = thirdPoint - secondPoint;

            while (firstToSecond.AngleTo(secondToThird) < AngleEqualityTolerance)
            {
                floorSideVertices.RemoveLast();
                firstPoint = component.GetVertex(floorSideVertices.Last.Previous.Value).coordinate;
                secondPoint = component.GetVertex(floorSideVertices.Last.Value).coordinate;
                thirdPoint = component.GetVertex(floorSideVertices.First.Value).coordinate;

                firstToSecond = secondPoint - firstPoint;
                secondToThird = thirdPoint - secondPoint;
            }

            // Compare Last->->First->SecondAfterFirst, if same, remove first, repeat check
            firstPoint = component.GetVertex(floorSideVertices.Last.Value).coordinate;
            secondPoint = component.GetVertex(floorSideVertices.First.Value).coordinate;
            thirdPoint = component.GetVertex(floorSideVertices.First.Next.Value).coordinate;

            firstToSecond = secondPoint - firstPoint;
            secondToThird = thirdPoint - secondPoint;

            while (firstToSecond.AngleTo(secondToThird) < AngleEqualityTolerance)
            {
                floorSideVertices.RemoveFirst();
                firstPoint = component.GetVertex(floorSideVertices.Last.Value).coordinate;
                secondPoint = component.GetVertex(floorSideVertices.First.Value).coordinate;
                thirdPoint = component.GetVertex(floorSideVertices.First.Next.Value).coordinate;

                firstToSecond = secondPoint - firstPoint;
                secondToThird = thirdPoint - secondPoint;
            }
        }

        /*
         * Gets all the floor faces of a component.
         */
        public static List<int> GetFloorFaces(Component component) {

            int lowestVertex = FindLowestVertex(component);
            int startFloorFace = FindStartFloorFace(component, lowestVertex);
            List<int> floorFaces = FindAllLinkedPlaneFaces(component, startFloorFace, 10.0d);

            return floorFaces;
        }

        /*
         * Finds lowest Vertex in the component.
         */
        public static int FindLowestVertex(Component component)
        {
            if (component.vertices.Count == 0)
            {
                throw new EmptyListException("Tried to find lowest point in a component without vertices.");
            }

            int currLowestVertex = 0;

            for (int i = 1; i < component.vertices.Count; i++)
            {
                if (component.GetVertex(i).coordinate.y < component.GetVertex(currLowestVertex).coordinate.y)
                {
                    currLowestVertex = i;
                }
            }

            return currLowestVertex;

        }

        /*
         * Retrieves the most downward facing face connected to a Vertex.
         */
        public static int FindStartFloorFace(Component component, int lowestVertex)
        {
            int currStartFloorFace;
            try
            {
                currStartFloorFace = component.GetVertex(lowestVertex).faceIndices[0];

                foreach(int faceIndex in component.GetVertex(lowestVertex).faceIndices)
                {
                    if(component.GetFace(faceIndex).normal.AngleTo(Vect3.Down) <
                        component.GetFace(currStartFloorFace).normal.AngleTo(Vect3.Down))
                    {
                        currStartFloorFace = faceIndex;
                    }
                }

            }
            catch (ArgumentOutOfRangeException)
            {
                throw new EmptyListException("Vertex exists but has an empty face list.");
            }

            return currStartFloorFace;
        }

        /*
         * Find all faces that are linked such that the normal from face to face never changes more than angleTolerance.
         */
        public static List<int> FindAllLinkedPlaneFaces(Component component, int faceInt, double angleTolerance)
        {
            var valid = new List<int>(){faceInt};
            int i = 0;

            while (i < valid.Count)
            {
                foreach(int neighborIndex in component.GetNeighbouringFacesViaEdges(valid[i]))
                {
                    if (!valid.Contains(neighborIndex) && component.GetFace(valid[i]).normal.AngleTo(
                         component.GetFace(neighborIndex).normal) < angleTolerance)
                    {
                        valid.Add(neighborIndex);
                    }
                }

                i++;
            }

            return valid;
        }

        /*
         * Returns a linked list of the vertices representing the shell of a face list.
         */
        public static LinkedList<int> GetShellVertices(Component component, List<int> floorFaces)
        {
            var shellPairs = new List<(int, int)>();

            for (int i = 0; i < floorFaces.Count; i++)
            {
                Face outerFace = component.GetFace(floorFaces[i]);
                bool foundPair01 = false;
                bool foundPair02 = false;
                bool foundPair12 = false;

                for (int j = 0; j < floorFaces.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    Face innerFace = component.GetFace(floorFaces[j]);

                    if (Array.Exists(innerFace.vertexIndices, index => index == outerFace.vertexIndices[0]) &&
                        Array.Exists(innerFace.vertexIndices, index => index == outerFace.vertexIndices[1]))
                    {
                        foundPair01 = true;
                    }
                    if (Array.Exists(innerFace.vertexIndices, index => index == outerFace.vertexIndices[0]) &&
                        Array.Exists(innerFace.vertexIndices, index => index == outerFace.vertexIndices[2]))
                    {
                        foundPair02 = true;
                    }
                    if (Array.Exists(innerFace.vertexIndices, index => index == outerFace.vertexIndices[1]) &&
                        Array.Exists(innerFace.vertexIndices, index => index == outerFace.vertexIndices[2]))
                    {
                        foundPair12 = true;
                    }
                }

                if (!foundPair01)
                {
                    shellPairs.Add((outerFace.vertexIndices[0], outerFace.vertexIndices[1]));
                }
                if (!foundPair02)
                {
                    shellPairs.Add((outerFace.vertexIndices[0], outerFace.vertexIndices[2]));
                }
                if (!foundPair12)
                {
                    shellPairs.Add((outerFace.vertexIndices[1], outerFace.vertexIndices[2]));
                }
            }

            /* Make shellPairs a Linked List */
            var linkedList = new LinkedList<int>();
            linkedList.AddFirst(shellPairs[0].Item1);
            linkedList.AddLast(shellPairs[0].Item2);
            shellPairs.RemoveAt(0);
            var count = 0;

            while (shellPairs.Count > 0)
            {
                foreach ((int, int) pair in shellPairs)
                {
                    if (pair.Item1 == linkedList.Last.Value)
                    {
                        if (linkedList.Contains(pair.Item2))
                        {
                            count++;
                            if (count > 1)
                            {
                                throw new Exception("Shellpair-list is not empty when it should be.");
                            }
                        }
                        else
                        {
                            linkedList.AddLast(pair.Item2);
                        }
                        shellPairs.Remove(pair);
                        break;
                    }
                    else if (pair.Item2 == linkedList.Last.Value)
                    {
                        if (linkedList.Contains(pair.Item1))
                        {
                            count++;
                            if (count > 1)
                            {
                                throw new Exception("ShellPair-list is not empty when it should be.");
                            }
                        }
                        else
                        {
                            linkedList.AddLast(pair.Item1);
                        }
                        shellPairs.Remove(pair);
                        break;
                    }
                }
            }
            return linkedList;
        }

        /*
        * Returns two linked lists, each list represent the vertices on one long side of the component.
        */
        public static (LinkedList<int>, LinkedList<int>) SplitLinkedList(Component component, LinkedList<int> linkedList)
        {
            var listFront = new LinkedList<int>();
            var listBack = new LinkedList<int>();

            /*Special case, with just 4 vertices.*/
            if (linkedList.Count == 4)
            {
                return FourVerticesCase(component, linkedList);
            }
            else
            {
                LinkedListNode<int> current = linkedList.First.Next;
                while (current != linkedList.Last)
                {
                    double angleDiff = GetAngleBetweenVertices(component, current);

                    if (angleDiff > 80 && angleDiff < 100)
                    {
                        double angleDiffNext = GetAngleBetweenVertices(component, current.Next);

                        if (angleDiffNext > 80 && angleDiffNext < 100)
                        {
                            linkedList = RearangeLinkedList(linkedList, current.Next);
                        }
                        else
                        {
                            linkedList = RearangeLinkedList(linkedList, current);
                        }
                        break;
                    }
                    current = current.Next;
                }

                /* Fill front */
                LinkedListNode<int> temp = linkedList.First.Next;
                listFront.AddLast(linkedList.First.Value);

                while (temp != linkedList.Last)
                {

                    listFront.AddLast(temp.Value);
                    double angleDiff = GetAngleBetweenVertices(component, temp);

                    if (angleDiff > 80 && angleDiff < 100)
                    {
                        break;
                    }
                    temp = temp.Next;
                }

                /* Fill Back */
                temp = linkedList.Last.Previous;
                listBack.AddLast(linkedList.Last.Value);
                while (temp != linkedList.First)
                {
                    listBack.AddLast(temp.Value);
                    double angleDiff = GetAngleBetweenVertices(component, temp);
                    if (angleDiff > 80 && angleDiff < 100)
                    {
                        break;
                    }
                    temp = temp.Previous;
                }
            }

            return (listFront, listBack);
        }

        /*
        * Returns two linked lists, each list represent one long side of the component. This is for the special case when the component only has 4 vertices.
        */
        private static (LinkedList<int>, LinkedList<int>) FourVerticesCase(Component component, LinkedList<int> linkedList)
        {
            var list1 = new LinkedList<int>();
            var list2 = new LinkedList<int>();

            Vertex beforeVertex = component.GetVertex(linkedList.First.Value);
            Vertex middleVertex = component.GetVertex(linkedList.First.Next.Value);
            Vertex afterVertex = component.GetVertex(linkedList.First.Next.Next.Value);

            list1.AddFirst(linkedList.First.Value);

            if ((beforeVertex.coordinate - middleVertex.coordinate).Length() >
                (afterVertex.coordinate - middleVertex.coordinate).Length())
            {
                list1.AddLast(linkedList.First.Next.Value);
                list2.AddFirst(linkedList.Last.Value);
                list2.AddLast(linkedList.Last.Previous.Value);

            }
            else
            {
                list1.AddLast(linkedList.Last.Value);
                list2.AddFirst(linkedList.First.Next.Value);
                list2.AddLast(linkedList.Last.Previous.Value);
            }

            return (list1, list2);
        }

        /**
         * Creates the NodeGraph given the component and both of its sides of floor-vertices.
         * Expects it to have two sides consisting of atleast two vertices each.
         * When calculating the minimum width of each edge, assumes that the component is narrowest at floor level.
         */
        public static NodeGraph CreateNodeGraphComponent(Component component, (LinkedList<int>, LinkedList<int>) linkedListPair)
        {

            if (linkedListPair.Item1.Count < 2 || linkedListPair.Item2.Count < 2)
            {
                throw new Exception("Each side of the component needs at least 2 vertices.");
            }

            var nodeGraph = new NodeGraph();
            int count = 0;
            LinkedList<int> shortestList;
            LinkedList<int> longestList;

            if (linkedListPair.Item1.Count < linkedListPair.Item2.Count)
            {
                shortestList = linkedListPair.Item1;
                longestList = linkedListPair.Item2;
            }
            else
            {
                shortestList = linkedListPair.Item2;
                longestList = linkedListPair.Item1;
            }

            // Handle first Node as special case
            int closestVertexIndex = ClosestPointInOtherList(longestList.First.Value, shortestList, component);
            (Vertex, Vertex) vertexPair = (component.GetVertex(longestList.First.Value),
                                            component.GetVertex(closestVertexIndex));
            longestList.RemoveFirst();
            Node newNode = new Node(Vect3.MiddlePoint(vertexPair.Item1.coordinate, vertexPair.Item2.coordinate));
            nodeGraph.AddNode(newNode);

            (Vertex, Vertex) oldVertexPair = vertexPair;

            foreach (int vertexIndex in longestList)
            {
                closestVertexIndex = ClosestPointInOtherList(vertexIndex, shortestList, component);
                vertexPair = (component.GetVertex(vertexIndex),
                              component.GetVertex(closestVertexIndex));

                newNode = new Node(Vect3.MiddlePoint(vertexPair.Item1.coordinate, vertexPair.Item2.coordinate));

                nodeGraph.AddNode(newNode, count);
                nodeGraph.SetEdgeWidth(count, getMinimalWidth(oldVertexPair, vertexPair));
                nodeGraph.SetEdgeHeight(count, PLACE_HOLDER_HEIGHT);
                oldVertexPair = vertexPair;
                count++;
            }

            return nodeGraph;
        }

        private static (Vertex, Vertex) getVertexPair(Component component, (LinkedList<int>, LinkedList<int>) linkedListPair)
        {
            (Vertex, Vertex) vertexPair = (component.GetVertex(linkedListPair.Item1.First.Value), component.GetVertex(linkedListPair.Item2.First.Value));
            linkedListPair.Item1.RemoveFirst();
            linkedListPair.Item2.RemoveFirst();
            return vertexPair;
        }

        private static int ClosestPointInOtherList(int vertexIndex, LinkedList<int> otherList, Component component)
        {
            double closestDistance = Double.PositiveInfinity;
            int closestVertexIndex = -1;
            Vect3 vertexCoord = component.GetVertex(vertexIndex).coordinate;
            foreach (int otherVertexIndex in otherList)
            {
                Vect3 otherVertexCoord = component.GetVertex(otherVertexIndex).coordinate;
                double currentDistance = (vertexCoord - otherVertexCoord).LengthSquared();

                if (currentDistance < closestDistance)
                {
                    closestDistance = currentDistance;
                    closestVertexIndex = otherVertexIndex;
                }
            }

            if (closestVertexIndex == -1) {throw new Exception("otherList was empty");}

            return closestVertexIndex;
        }

        /*
         * Moves a vertix to last place in the linked list returns the list.
         */
        private static LinkedList<int> RearangeLinkedList(LinkedList<int> linkedList, LinkedListNode<int> newFirst)
        {
            while (linkedList.First != newFirst)
            {
                LinkedListNode<int> tempFirst = linkedList.First;
                linkedList.RemoveFirst();
                linkedList.AddLast(tempFirst);
            }
            return linkedList;
        }

        /*
         * Returns the angle that is created when drawing "edges" between 3 vertices.
         */
        private static double GetAngleBetweenVertices(Component component, LinkedListNode<int> mark)
        {
            Vertex vBack = component.GetVertex(mark.Previous.Value);
            Vertex vMiddle = component.GetVertex(mark.Value);
            Vertex vFront = component.GetVertex(mark.Next.Value);
            return (vMiddle.coordinate - vBack.coordinate).AngleTo(vFront.coordinate - vMiddle.coordinate);
        }


        /**
         * Returns the minimal width for an edge in a component, given the two sides containing the edge.
         */
        static double getMinimalWidth((Vertex v1, Vertex v2) oldVertexPair, (Vertex v1, Vertex v2) vertexPair)
        {
            double p11 = Vect3.FindShortestDistanceLinePoint(oldVertexPair.v2, vertexPair.v2, vertexPair.v1);
            double p12 = Vect3.FindShortestDistanceLinePoint(oldVertexPair.v2, vertexPair.v2, oldVertexPair.v1);
            double p21 = Vect3.FindShortestDistanceLinePoint(oldVertexPair.v1, vertexPair.v1, vertexPair.v2);
            double p22 = Vect3.FindShortestDistanceLinePoint(oldVertexPair.v1, vertexPair.v1, oldVertexPair.v2);
            return Math.Min(Math.Min(p11, p12), Math.Min(p21, p22));
        }
    }
}
