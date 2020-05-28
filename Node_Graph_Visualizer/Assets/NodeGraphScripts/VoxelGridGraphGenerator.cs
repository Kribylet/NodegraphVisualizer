using System;
using System.Collections.Generic;

namespace Nodegraph_Generator
{
    public static class VoxelGridGraphGenerator
    {
        private const double SquareRootOfThree = 1.73d;
        public static double VoxelDistanceThreshold = 2d;
        public static double mergeThreshold = 3d;
        public static double outerLayerLimit = 0.3d; // Controls how much of the grids approximate radius is considered external as percentage of diameter. (50% diameter == 100% radius)

        /*
         * Generate a NodeGraph for an entire Structure using VoxelStrategy
         */
        public static NodeGraph GenerateNodeGraph(Structure structure)
        {
            VoxelGrid voxelGrid = new VoxelGrid(structure, plane1Normal: Vect3.Right, plane2Normal: Vect3.Up, plane3Normal: Vect3.Forward);
            voxelGrid.FillInternalVolume(structure);

            return GenerateNodeGraph(voxelGrid);
        }

        /*
         * Generate a NodeGraph for a single Component using VoxelStrategy
         */
        public static NodeGraph GenerateNodeGraph(Component component)
        {
            VoxelGrid voxelGrid = new VoxelGrid(component);
            voxelGrid.FillInternalVolume(component);

            return GenerateNodeGraph(voxelGrid);
        }

        private static NodeGraph GenerateNodeGraph(VoxelGrid voxelGrid)
        {
            DistanceGrid distanceGrid = new DistanceGrid(voxelGrid);

            Skeletonize(distanceGrid, voxelGrid);
            MoveGridToFloor(distanceGrid, voxelGrid);

            NodeGraph nodeGraph = CreateNodeGraph(distanceGrid, voxelGrid);
            nodeGraph = MergeNodeGraph(nodeGraph, voxelGrid);
            return nodeGraph;
        }

        private static NodeGraph MergeNodeGraph(NodeGraph nodeGraph, VoxelGrid voxelGrid)
        {
            bool proximity = true;
            while (proximity)
            {
                proximity = false;
                Edge mergableEdge = null;
                foreach (Edge edge in nodeGraph.Edges)
                {
                    if ((nodeGraph.GetCoordinateOfNode(edge.nodeIndex1) - nodeGraph.GetCoordinateOfNode(edge.nodeIndex2)).Length() < mergeThreshold*voxelGrid.resolution)
                    {
                        mergableEdge = edge;
                        proximity = true;
                        break;
                    }
                }

                if (mergableEdge != null)
                {
                    Vect3 centerPoint = (nodeGraph.GetCoordinateOfNode(mergableEdge.nodeIndex1) + nodeGraph.GetCoordinateOfNode(mergableEdge.nodeIndex2))/2d;
                    nodeGraph.MoveNode(nodeGraph.GetNode(mergableEdge.nodeIndex1), centerPoint);
                    nodeGraph.MoveNode(nodeGraph.GetNode(mergableEdge.nodeIndex2), centerPoint);
                }
            }
            nodeGraph.ReIndexNodeGraph();
            return nodeGraph;
        }

        public static void MoveGridToFloor(DistanceGrid distanceGrid, VoxelGrid voxelGrid)
        {
            for (int x = 1; x < voxelGrid.xBound - 1; x++)
            {
                for (int y = 1; y < voxelGrid.yBound - 1; y++)
                {
                    for (int z = 1; z < voxelGrid.zBound - 1; z++)
                    {
                        if (distanceGrid.grid[x][y][z] != 0)
                        {
                            int offsetY = 0;
                            while (voxelGrid.coordinateGrid[x][y + offsetY - 1][z]) offsetY--;
                            distanceGrid.grid[x][y + offsetY][z] = distanceGrid.grid[x][y][z];
                            distanceGrid.grid[x][y][z] = 0;
                        }
                    }
                }
            }
        }

        /**
         * Can be used when visualising in Unity.
         */
        public static DistanceGrid GenerateSkeletalGrid(VoxelGrid voxelGrid)
        {
            DistanceGrid distanceGrid = new DistanceGrid(voxelGrid);

            Skeletonize(distanceGrid, voxelGrid);

            MoveGridToFloor(distanceGrid, voxelGrid);

            return distanceGrid;
        }

        public static void Skeletonize(DistanceGrid distanceGrid, VoxelGrid voxelGrid)
        {
            ThinByDistanceMapping(distanceGrid, voxelGrid);
            ThinToSkeleton(distanceGrid);
        }

        /**
        * Pre processing for the skeletonizing. Removes the outer most layers.
        */
        private static void ThinByDistanceMapping(DistanceGrid distanceGrid, VoxelGrid voxelGrid)
        {
            int outerLayersToRemove = Convert.ToInt32(voxelGrid.ComponentWiseResolutionDivider * outerLayerLimit);
            for (int x = 1; x < distanceGrid.xBound; x++)
            {
                for (int y = 1; y < distanceGrid.yBound; y++)
                {
                    for (int z = 1; z < distanceGrid.zBound; z++)
                    {
                        if (distanceGrid.grid[x][y][z] <= outerLayersToRemove) distanceGrid.grid[x][y][z] = 0;
                    }
                }
            }
        }

        /*
         * Removes voxels from a filled and marked voxelgrid by removing voxels until only "necessary" ones are left.
         * Necessary voxels are ones which are needed to maintain the geometry and topology of the component.
        */
        public static void ThinToSkeleton(DistanceGrid distanceGrid)
        {
            int removedVoxels;
            do
            {
                removedVoxels = 0;

                removedVoxels += DeleteByTemplates(GridTemplate.USWGridTemplates, distanceGrid);
                removedVoxels += DeleteByTemplates(GridTemplate.DNEGridTemplates, distanceGrid);
                removedVoxels += DeleteByTemplates(GridTemplate.USEGridTemplates, distanceGrid);
                removedVoxels += DeleteByTemplates(GridTemplate.DNWGridTemplates, distanceGrid);
                removedVoxels += DeleteByTemplates(GridTemplate.UNEGridTemplates, distanceGrid);
                removedVoxels += DeleteByTemplates(GridTemplate.DSWGridTemplates, distanceGrid);
                removedVoxels += DeleteByTemplates(GridTemplate.UNWGridTemplates, distanceGrid);
                removedVoxels += DeleteByTemplates(GridTemplate.DSEGridTemplates, distanceGrid);
            } while (removedVoxels > 0);

            for (int x = 1; x < distanceGrid.xBound; x++)
            {
                for (int y = 1; y < distanceGrid.yBound; y++)
                {
                    for (int z = 1; z < distanceGrid.zBound; z++)
                    {
                        if (distanceGrid.grid[x][y][z] != 0 &&
                            GridTemplate.NeighborLessTemplate.Matches(distanceGrid.grid, x, y, z))
                        {
                            distanceGrid.grid[x][y][z] = 0;
                        }
                    }
                }
            }
        }

        /*
        * Removes all voxels in a voxelgrid which match any of the given templates in the list.
        */
        private static int DeleteByTemplates(List<GridTemplate> gridTemplates, DistanceGrid distanceGrid)
        {
            List<Point3> coordsToRemove = new List<Point3>();
            for (int x = 1; x < distanceGrid.xBound; x++)
            {
                for (int y = 1; y < distanceGrid.yBound; y++)
                {
                    for (int z = 1; z < distanceGrid.zBound; z++)
                    {
                        if (distanceGrid.grid[x][y][z] != 0)
                        {
                            foreach (GridTemplate gridTemplate in gridTemplates)
                            {
                                if (gridTemplate.Matches(distanceGrid.grid, x, y, z))
                                {
                                    coordsToRemove.Add(new Point3(x, y, z));
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            foreach (var p in coordsToRemove)
            {
                distanceGrid.grid[p.x][p.y][p.z] = 0;
            }
            return coordsToRemove.Count;
        }

        /**
         * Generates a NodeGraph from a skeletonized voxelgrid, by traversing neighboring
         * voxels and creating nodes connected by edges to maintain the topology of the voxelgrid.
        */
        private static NodeGraph CreateNodeGraph(DistanceGrid distanceGrid, VoxelGrid voxelGrid)
        {
            bool[][][] visitedGrid = new bool[distanceGrid.xBound + 1][][];
            for (int i = 0; i < visitedGrid.Length; i++)
            {
                visitedGrid[i] = new bool[distanceGrid.yBound + 1][];

                for (int j = 0; j < visitedGrid[i].Length; j++)
                {
                    visitedGrid[i][j] = new bool[distanceGrid.zBound + 1];
                }
            }

            NodeGraph nodeGraph = new NodeGraph();

            Queue<(Point3, Point3)> startPoints = GetStartPoints(distanceGrid);

            Point3 initialPoint = startPoints.Peek().Item1;

            nodeGraph.AddNode(voxelGrid.VoxelPositionAsCoordinate(initialPoint));
            visitedGrid[initialPoint.x][initialPoint.y][initialPoint.z] = true;

            //Breadth-first traversing of the Voxels
            while (startPoints.Count > 0)
            {
                (Point3 startPoint, Point3 currentPoint) = startPoints.Dequeue();

                //Special case for when connecting to an allready visited node
                if (visitedGrid[currentPoint.x][currentPoint.y][currentPoint.z])
                {
                    Vect3 pos1 = voxelGrid.VoxelPositionAsCoordinate(startPoint);
                    Vect3 pos2 = voxelGrid.VoxelPositionAsCoordinate(currentPoint);

                    Node startNode = nodeGraph.GetNode(pos1);
                    Node currentNode = nodeGraph.GetNode(pos2);

                    if (nodeGraph.GetNode(pos1) == null || nodeGraph.GetNode(pos2) == null) continue;

                    bool shouldLink = true;

                    foreach (var neighbor in currentNode.neighbors)
                    {
                        if (neighbor.nodeIndex == startNode.index)
                        {
                            shouldLink = false;
                        }
                        else
                        {
                            Node neighborNode = nodeGraph.GetNode(neighbor.nodeIndex);
                            foreach (var nextNeighbor in neighborNode.neighbors)
                            {
                                if (nextNeighbor.nodeIndex == startNode.index)
                                {
                                    shouldLink = false;
                                }
                            }
                        }

                        if (!shouldLink) break;
                    }
                    if (shouldLink) nodeGraph.LinkNodes(pos1, pos2);
                    else continue;
                }


                Point3 cameFrom = startPoint;

                List<Point3> pathPoints = new List<Point3>(){startPoint};

                Vect3 startCoordinate = voxelGrid.VoxelPositionAsCoordinate(startPoint);
                Vect3 currentCoordinate = voxelGrid.VoxelPositionAsCoordinate(currentPoint);

                List<Vect3> intermediateCoords = new List<Vect3>();

                List<Point3> neighbors = new List<Point3>();

                bool distanceWasValid = false;

                //Traversing untill the Nodeplacement would make the Edge to far away from the voxels it represents,
                //an already visited node is reached, an intersection is found or it's a dead end.
                while (DistancesAreValid(startCoordinate, currentCoordinate, intermediateCoords, voxelGrid.resolution))
                {
                    // Node we found was visited
                    if (visitedGrid[currentPoint.x][currentPoint.y][currentPoint.z])
                    {
                        // Ensures nothing happens after we break
                        distanceWasValid = true;
                        break;
                    }

                    neighbors = distanceGrid.Get26AdjacentNeighbors(currentPoint);

                    if (neighbors.Count != 2)
                    {
                        visitedGrid[currentPoint.x][currentPoint.y][currentPoint.z] = true;
                        distanceWasValid = true;
                        break;
                    }

                    // Only one new neighbor
                    intermediateCoords.Add(voxelGrid.VoxelPositionAsCoordinate(currentPoint));

                    Point3 temp = currentPoint;
                    currentPoint = neighbors[0] == cameFrom ? neighbors[1] : neighbors[0];
                    currentCoordinate = voxelGrid.VoxelPositionAsCoordinate(currentPoint);
                    cameFrom = temp;

                    visitedGrid[cameFrom.x][cameFrom.y][cameFrom.z] = true;
                }

                double maxDistanceValue = 0;

                foreach (var point in pathPoints)
                {
                    maxDistanceValue += distanceGrid.grid[point.x][point.y][point.z]*voxelGrid.resolution * SquareRootOfThree; // Distance is 26-distance from edge
                    // double distanceValue = distanceGrid.grid[point.x][point.y][point.z]*voxelGrid.resolution*2;
                    // maxDistanceValue = distanceValue > maxDistanceValue ? distanceValue : maxDistanceValue;
                }

                maxDistanceValue /= (double) pathPoints.Count;

                double minWidth = maxDistanceValue;
                double minHeight = maxDistanceValue;

                if (!distanceWasValid)
                {
                    nodeGraph.AddNode(intermediateCoords[intermediateCoords.Count - 1], minWidth, minHeight, nodeGraph.GetNode(startCoordinate).index);
                    visitedGrid[cameFrom.x][cameFrom.y][cameFrom.z] = true;
                    if (!visitedGrid[currentPoint.x][currentPoint.y][currentPoint.z]) startPoints.Enqueue((cameFrom, currentPoint));
                }
                else
                {
                    nodeGraph.AddNode(currentCoordinate, minWidth, minHeight, nodeGraph.GetNode(startCoordinate).index);
                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor == cameFrom ||
                            visitedGrid[neighbor.x][neighbor.y][neighbor.z]) continue;
                        startPoints.Enqueue((currentPoint, neighbor));
                    }
                }
            }
            return nodeGraph;
        }

        /*
         * Return all possible start points for nodgraph.
         */
        private static Queue<(Point3, Point3)> GetStartPoints(DistanceGrid distanceGrid)
        {
            // First is start point, second is the immediate neighbor to go to
            Queue<(Point3, Point3)> startPoints = new Queue<(Point3, Point3)>();

            bool done = false;

            // Find a "starting point" to traverse from.
            // A voxel is a "starting point" if it only has one "26-neighbor"
            for (int x = 1; x < distanceGrid.xBound; x++)
            {
                for (int y = 1; y < distanceGrid.yBound; y++)
                {
                    for (int z = 1; z < distanceGrid.zBound; z++)
                    {
                        if (distanceGrid.grid[x][y][z] == 0) continue;

                        // If coordinate is a valid startpoint
                        if (distanceGrid.Get26AdjacentNeighbors(x, y, z).Count == 1)
                        {
                            Point3 nextPoint = new Point3();
                            for (int xi = -1; xi <= 1; xi++)
                            {
                                for (int yi = -1; yi <= 1; yi++)
                                {
                                    for (int zi = -1; zi <= 1; zi++)
                                    {
                                        if (xi == 0 && yi == 0 && zi == 0) continue;
                                        if (distanceGrid.grid[x + xi][y + yi][z + zi] > 0)
                                        {
                                            nextPoint = new Point3(x + xi, y + yi, z + zi);
                                        }
                                    }
                                }
                            }

                            startPoints.Enqueue((new Point3(x, y, z), nextPoint));
                            done = true;
                            break;
                        }
                    }
                    if (done) break;
                }
                if (done) break;
            }
            return startPoints;
        }

        /**
         * Returns true if none of the intermediate points are futher away from the line, start --> current, than the threshold.
         */
        private static bool DistancesAreValid(Vect3 startCoordinate, Vect3 currentCoordinate, List<Vect3> intermediatePoints, double resolution)
        {
            foreach (var intermediatePoint in intermediatePoints)
            {
                double distance = Vect3.FindShortestDistanceLinePoint(startCoordinate, currentCoordinate, intermediatePoint);
                if (distance > resolution*VoxelDistanceThreshold) return false;
            }

            return true;
        }
    }
}
