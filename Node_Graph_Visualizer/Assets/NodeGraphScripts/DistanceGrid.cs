using System;
using System.Collections.Generic;
using System.Text;

namespace Nodegraph_Generator
{
    /*
     * An Grid with representation for temperature.
     */
    public class DistanceGrid
    {
        public int[][][] grid;

        public int xBound;
        public int yBound;
        public int zBound;

        public DistanceGrid() { }

        /*
         * Creates a DistanceGrid based on a VoxelGrid
         */
        public DistanceGrid(VoxelGrid voxelGrid)
        {
            CreateStartGrid(voxelGrid);
            MarkExternal(voxelGrid);
            MarkInternal(voxelGrid);
        }

        private void CreateStartGrid(VoxelGrid voxelGrid)
        {
            xBound = voxelGrid.xBound;
            yBound = voxelGrid.yBound;
            zBound = voxelGrid.zBound;


            grid = new int[xBound][][];
            for (int i = 0; i < grid.Length; i++)
            {
                grid[i] = new int[yBound][];

                for (int j = 0; j < grid[i].Length; j++)
                {
                    grid[i][j] = new int[zBound];
                }
            }
        }

        /*
        * Marks each exposed voxel as 1.
        */
        public void MarkExternal(VoxelGrid voxelGrid)
        {
            for (int x = 1; x < xBound - 1; x++)
            {
                for (int y = 1; y < yBound - 1; y++)
                {
                    for (int z = 1; z < zBound - 1; z++)
                    {
                        if (voxelGrid.coordinateGrid[x][y][z] && voxelGrid.HasEmptyNeighbor(x, y, z))
                        {
                            grid[x][y][z] = 1;
                        }
                    }
                }
            }
        }

        /*
        * Numbers each voxel depending on its distance in voxels to its closest exposed voxel.
        */
        public void MarkInternal(VoxelGrid voxelGrid)
        {
            List<(Point3, int)> valuesToSet = new List<(Point3, int)>();
            int smallestNeighborValue;

            do
            {
                valuesToSet.Clear();
                for (int x = 1; x < xBound - 1; x++)
                {
                    for (int y = 1; y < yBound - 1; y++)
                    {
                        for (int z = 1; z < zBound - 1; z++)
                        {
                            if (voxelGrid.coordinateGrid[x][y][z] && grid[x][y][z] == 0)
                            {
                                smallestNeighborValue = GetSmallestNeighborValue(x, y, z);

                                if (smallestNeighborValue == 0) continue;
                                valuesToSet.Add((new Point3(x, y, z), smallestNeighborValue + 1));
                            }
                        }
                    }
                }
                foreach (var valueToSet in valuesToSet)
                {
                    Point3 p = valueToSet.Item1;
                    grid[p.x][p.y][p.z] = valueToSet.Item2;
                }
            } while (valuesToSet.Count > 0);
        }

        private int GetSmallestNeighborValue(int originX, int originY, int originZ)
        {
            int smallestSeen = Int32.MaxValue;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        int currentNeighborValue = grid[originX + x][originY + y][originZ + z];
                        if (currentNeighborValue != 0)
                        {
                            smallestSeen = currentNeighborValue < smallestSeen ? currentNeighborValue : smallestSeen;
                        }
                    }
                }
            }

            if (smallestSeen == Int32.MaxValue) return 0;
            return smallestSeen;
        }

        public List<Point3> Get26AdjacentNeighbors(Point3 point)
        {
            return Get26AdjacentNeighbors(point.x, point.y, point.z);
        }

        public List<Point3> Get26AdjacentNeighbors(int originX, int originY, int originZ)
        {
            List<Point3> neighbors = new List<Point3>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        if (grid[originX + x][originY + y][originZ + z] != 0)
                        {
                            if (x == 0 && y == 0 && z == 0) continue;
                            neighbors.Add(new Point3(originX + x, originY + y, originZ + z));
                        }
                    }
                }
            }
            return neighbors;
        }
    }
}