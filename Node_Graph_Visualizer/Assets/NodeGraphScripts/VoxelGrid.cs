using System;
using System.Collections.Generic;

namespace Nodegraph_Generator
{
    /**
     * 3-Dimensional grid of pixels called Voxels.
     * Voxels are used for representing where meshes are in space.
     *
     */
    public class VoxelGrid
    {
        private struct VoxelSpan
        {
            public int minX;
            public int maxX;
            public int minY;
            public int maxY;
            public int minZ;
            public int maxZ;

            public VoxelSpan(List<Vect3> points, VoxelGrid voxelGrid, int borderOffset = 0, bool givenAsLocal = false)
            {
                if (!givenAsLocal)
                {
                    points = voxelGrid.ConvertToLocalCoordinates(points);
                }

                double lowestX = Double.PositiveInfinity;
                double lowestY = Double.PositiveInfinity;
                double lowestZ = Double.PositiveInfinity;
                double highestX = Double.NegativeInfinity;
                double highestY = Double.NegativeInfinity;
                double highestZ = Double.NegativeInfinity;

                foreach (var bound in points)
                {
                    lowestX = bound.x < lowestX ? bound.x : lowestX;
                    lowestY = bound.y < lowestY ? bound.y : lowestY;
                    lowestZ = bound.z < lowestZ ? bound.z : lowestZ;
                    highestX = bound.x > highestX ? bound.x : highestX;
                    highestY = bound.y > highestY ? bound.y : highestY;
                    highestZ = bound.z > highestZ ? bound.z : highestZ;
                }


                minX = Math.Max(0, Convert.ToInt32(Math.Floor(lowestX / voxelGrid.resolution)) - borderOffset);
                minY = Math.Max(0, Convert.ToInt32(Math.Floor(lowestY / voxelGrid.resolution)) - borderOffset);
                minZ = Math.Max(0, Convert.ToInt32(Math.Floor(lowestZ / voxelGrid.resolution)) - borderOffset);

                maxX = Math.Min(Convert.ToInt32(Math.Floor(highestX / voxelGrid.resolution)) + borderOffset, voxelGrid.xBound);
                maxY = Math.Min(Convert.ToInt32(Math.Floor(highestY / voxelGrid.resolution)) + borderOffset, voxelGrid.yBound);
                maxZ = Math.Min(Convert.ToInt32(Math.Floor(highestZ / voxelGrid.resolution)) + borderOffset, voxelGrid.zBound);
            }

            public override string ToString()
            {
                return "{" + minX + ", " + minY + ", " + minZ + ", " + maxX + ", " + maxY + ", " + maxZ + "}";
            }
        }

        // Voxelspan expansion beyond calculated limits for FillInternalVolume
        private const int BorderOffset = 2;
        // Constant used for deciding the resolution of the Voxelgrid
        public static double DefaultComponentWiseResolutionDivider = 7d;
        public static double OrientedBBoxExpansionFactor = 0.05d;
        public double ComponentWiseResolutionDivider;
        public Vect3 voxelStartCoordinate;
        public Vect3 voxelMiddleStartCoordinate;
        public double resolution;
        public double halfResolution;
        public List<Vect3> resolutionOffsets;
        public bool[][][] coordinateGrid;
        public Dictionary<int, OrientedBBox> componentOBBs = new Dictionary<int, OrientedBBox>();

        public OrientedBBox orientedBbox;

        public int xBound;
        public int yBound;
        public int zBound;

        public VoxelGrid() { }

        public VoxelGrid(Component component, Vect3 plane1Normal = null, Vect3 plane2Normal = null, Vect3 plane3Normal = null)
        {
            orientedBbox = new OrientedBBox(component, plane1Normal, plane2Normal, plane3Normal);
            componentOBBs[component.index] = orientedBbox;

            SetupVoxelGrid();

            this.CreateShellFromFaces(component);
        }

        public VoxelGrid(Structure structure, Vect3 plane1Normal = null, Vect3 plane2Normal = null, Vect3 plane3Normal = null)
        {
            orientedBbox = new OrientedBBox(structure, plane1Normal, plane2Normal, plane3Normal);

            SetupVoxelGrid(structure);

            foreach (Component component in structure.components)
            {
                CreateShellFromFaces(component);
            }
        }

        private void SetupVoxelGrid(Structure structure = null)
        {
            ComponentWiseResolutionDivider = DefaultComponentWiseResolutionDivider;
            double rightLength;
            double heightLength;
            double depthLength;
            if (resolution == 0)
            {
                if (structure != null)
                {
                    double shortestSide = 0;
                    foreach (var component in structure.components)
                    {
                        OrientedBBox componentOBB = new OrientedBBox(component);

                        rightLength = (componentOBB.localMaxX - componentOBB.localOrigin).Length();
                        heightLength = (componentOBB.localMaxY - componentOBB.localOrigin).Length();
                        depthLength = (componentOBB.localMaxZ - componentOBB.localOrigin).Length();

                        shortestSide += Math.Min(rightLength, Math.Min(heightLength, depthLength));

                        componentOBBs[component.index] = componentOBB;
                    }

                    shortestSide /= structure.components.Count;

                    resolution = shortestSide / ComponentWiseResolutionDivider;
                }
                else
                {
                    rightLength = (orientedBbox.localMaxX - orientedBbox.localOrigin).Length();
                    heightLength = (orientedBbox.localMaxY - orientedBbox.localOrigin).Length();
                    depthLength = (orientedBbox.localMaxZ - orientedBbox.localOrigin).Length();
                    double shortestSide = Math.Min(rightLength, Math.Min(heightLength, depthLength));
                    resolution = shortestSide / ComponentWiseResolutionDivider;
                }
            }

            halfResolution = resolution / 2d;

            rightLength = (orientedBbox.localMaxX - orientedBbox.localOrigin).Length();
            heightLength = (orientedBbox.localMaxY - orientedBbox.localOrigin).Length();
            depthLength = (orientedBbox.localMaxZ - orientedBbox.localOrigin).Length();


            xBound = Convert.ToInt32(Math.Round(rightLength / resolution)) + 4 + 1;
            yBound = Convert.ToInt32(Math.Round(heightLength / resolution)) + 4 + 1;
            zBound = Convert.ToInt32(Math.Round(depthLength / resolution)) + 4 + 1;

            voxelStartCoordinate = orientedBbox.localOrigin - (orientedBbox.localX * 2d * resolution +
                                                               orientedBbox.localY * 2d  * resolution +
                                                               orientedBbox.localZ * 2d * resolution);

            // Include offset to move start position to the first cube center.
            voxelMiddleStartCoordinate = orientedBbox.localOrigin - (orientedBbox.localX * 1.5d * resolution +
                                                                     orientedBbox.localY * 1.5d * resolution +
                                                                     orientedBbox.localZ * 1.5d * resolution);

            coordinateGrid = new bool[xBound][][];
            for (int i = 0; i < coordinateGrid.Length; i++)
            {
                coordinateGrid[i] = new bool[yBound][];

                for (int j = 0; j < coordinateGrid[i].Length; j++)
                {
                    coordinateGrid[i][j] = new bool[zBound];
                }
            }

            // Resolution offsets are based around the Voxel center. This allows faster intersection calculations.
            resolutionOffsets = new List<Vect3>
            {
                new Vect3(-halfResolution, -halfResolution, -halfResolution),
                new Vect3(-halfResolution, -halfResolution, halfResolution),
                new Vect3(-halfResolution, halfResolution, -halfResolution),
                new Vect3(-halfResolution, halfResolution, halfResolution),
                new Vect3(halfResolution, -halfResolution, -halfResolution),
                new Vect3(halfResolution, -halfResolution, halfResolution),
                new Vect3(halfResolution, halfResolution, -halfResolution),
                new Vect3(halfResolution, halfResolution, halfResolution)
            };
        }

        public List<Vect3> ConvertToLocalCoordinates(List<Vect3> globalCoordinates)
        {
            List<Vect3> localCoordinates = new List<Vect3>();

            foreach (Vect3 globalCoordinate in globalCoordinates)
            {
                localCoordinates.Add(orientedBbox.GetLocalCoordinate(globalCoordinate - voxelStartCoordinate));
            }

            return localCoordinates;
        }

        public List<Vect3> ConvertFaceToLocal(Face face, Component component)
        {
            List<Vect3> facePositions = new List<Vect3>
            {
                component.GetCoordinate(face.vertexIndices[0]),
                component.GetCoordinate(face.vertexIndices[1]),
                component.GetCoordinate(face.vertexIndices[2])
            };

            return ConvertToLocalCoordinates(facePositions);
        }

        public bool HasEmptyNeighbor(int originX, int originY, int originZ)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        if (!coordinateGrid[originX + x][originY + y][originZ + z])
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /*
         * Fills VoxelGrid from component faces using a digital differential analyzer method.
         */
        public void CreateShellFromFaces(Component component)
        {
            foreach (Face face in component.faces)
            {
                List<Vect3> trianglePoints = ConvertFaceToLocal(face, component);

                Vect3 v1 = trianglePoints[0] / resolution;
                Vect3 v2 = trianglePoints[1] / resolution;
                Vect3 v3 = trianglePoints[2] / resolution;

                // v2->v3 should be shortest triangle side

                double v2v1length = (v2 - v1).Length();
                double v3v2length = (v3 - v2).Length();
                if (v3v2length > v2v1length)
                {
                    Vect3 temp = v1;
                    v1 = v3;
                    v3 = temp;
                }

                double v3v1length = (v3 - v1).Length();
                if (v3v2length > v3v1length)
                {
                    Vect3 temp = v2;
                    v2 = v1;
                    v1 = temp;
                }

                Vect3 lp1 = v1;
                Vect3 lp2 = v1;

                Vect3 d2 = v2 - v1;
                Vect3 d3 = v3 - v1;

                double d2step = d2.Length();
                double d3step = d3.Length();

                double outerstep = d2step > d3step ? d2step : d3step;

                outerstep *= 2;

                d2 /= outerstep;
                d3 /= outerstep;

                int xi, yi, zi;

                xi = Convert.ToInt32(lp1.x);
                yi = Convert.ToInt32(lp1.y);
                zi = Convert.ToInt32(lp1.z);

                coordinateGrid[xi][yi][zi] = true;

                lp1 += d2;
                lp2 += d3;

                int k = 0;
                while (k <= outerstep)
                {
                    Vect3 d = lp2 - lp1;

                    double step, x, y, z;
                    int i;

                    step = d.Length();

                    step *= 2;

                    if (step != 0) d /= step;

                    x = lp1.x;
                    y = lp1.y;
                    z = lp1.z;
                    i = 0;

                    yi = Convert.ToInt32(y);
                    zi = Convert.ToInt32(z);

                    while (i <= step)
                    {
                        xi = Convert.ToInt32(x);
                        coordinateGrid[xi][yi][zi] = true;
                        yi = Convert.ToInt32(y);
                        coordinateGrid[xi][yi][zi] = true;
                        zi = Convert.ToInt32(z);
                        coordinateGrid[xi][yi][zi] = true;

                        x += d.x;
                        y += d.y;
                        z += d.z;
                        i += 1;
                    }

                    lp1 += d2;
                    lp2 += d3;
                    k += 1;
                }
            }
        }

        public void FillInternalVolume(Structure structure)
        {
            foreach (Component component in structure.components)
            {
                FillInternalVolume(component);
            }
        }

        /*
         * Fills internal volume of voxels by finding points in the voxel grid captured
         * by a mesh. Relies on the original meshes to perform odd/even polygon capture calculation.
         *
         * https://en.wikipedia.org/wiki/Point_in_polygon
         */
        public void FillInternalVolume(Component component)
        {
            OrientedBBox componentOBB = componentOBBs[component.index];

            List<Vect3> points = new List<Vect3>();

            foreach (var vertex in component.vertices)
            {
                points.Add(vertex.coordinate);
            }

            VoxelSpan componentVoxelSpan = new VoxelSpan(points, this, BorderOffset);

            for (int x = componentVoxelSpan.minX; x < componentVoxelSpan.maxX; x++)
            {
                for (int z = componentVoxelSpan.minZ; z < componentVoxelSpan.maxZ; z++)
                {
                    for (int y = componentVoxelSpan.minY; y < componentVoxelSpan.maxY; y++)
                    {
                        if (!coordinateGrid[x][y][z] && coordinateGrid[x][Math.Max(0,y-1)][z])
                        {
                            Vect3 globalPos = voxelStartCoordinate +
                                                orientedBbox.localX * x * resolution +
                                                orientedBbox.localY * y * resolution +
                                                orientedBbox.localZ * z * resolution;

                            if (!componentOBB.ContainsGlobalCoordinate(globalPos, OrientedBBoxExpansionFactor)) continue;

                            int intersects = 0;
                            foreach (Face face in component.faces)
                            {
                                Vect3 triA = component.GetCoordinate(face.vertexIndices[0]);
                                Vect3 triB = component.GetCoordinate(face.vertexIndices[1]);
                                Vect3 triC = component.GetCoordinate(face.vertexIndices[2]);

                                // if (intersect_triangle(globalPos, Vect3.Up, triA, triB, triC))
                                if (rayIntersectsTriangle(globalPos, Vect3.Up, triA, triB, triC))
                                {
                                    intersects++;
                                }
                            }

                            if ((intersects % 2) != 0)
                            {
                                int rise = 0;
                                while (!coordinateGrid[x][y+rise][z])
                                {
                                    coordinateGrid[x][y+rise][z] = true;
                                    rise++;
                                }
                            }
                            else if (intersects == 0)
                            {
                                // If there is no intersecting surface above whatsoever, this column cannot
                                // contain points that are inside a polygon.
                                break;
                            }
                        }
                    }
                }
            }
        }


        /*
         * Calculates Line/Face intersections using the Möller-Trumbore intersection algorithm.
         *
         * https://en.wikipedia.org/wiki/Möller–Trumbore_intersection_algorithm
         */
        public static bool rayIntersectsTriangle(Vect3 rayOrigin,
                                            Vect3 rayVector,
                                            Vect3 vertex0,
                                            Vect3 vertex1,
                                            Vect3 vertex2) {
        double EPSILON = 1e-6;
        Vect3 edge1 = vertex1 - vertex0;
        Vect3 edge2 = vertex2 - vertex0;
        double a, f, u, v;
        Vect3 h = Vect3.Cross(rayVector, edge2);
        a = Vect3.Dot(edge1, h);
        if (a > -EPSILON && a < EPSILON) {
            return false;    // This ray is parallel to this triangle.
        }
        f = 1.0 / a;
        Vect3 s = rayOrigin - vertex0;
        u = f * Vect3.Dot(s,h);
        if (u < 0.0 || u > 1.0) {
            return false;
        }
        Vect3 q = Vect3.Cross(s, edge1);
        v = f * Vect3.Dot(rayVector, q);
        if (v < 0.0 || u + v > 1.0) {
            return false;
        }
        double t = f * Vect3.Dot(edge2, q);

        return t > EPSILON;
    }

        private void FillColumnY(int x, int z, int firstYFilled, int lastYFilled)
        {
            for (int y = firstYFilled; y <= lastYFilled; y++)
            {
                coordinateGrid[x][y][z] = true;
            }
        }

        public Vect3 VoxelPositionAsCoordinate(Point3 point)
        {
            return VoxelPositionAsCoordinate(point.x, point.y, point.z);
        }


        /*
         * Retrieves the lowest filled voxel grid position that shares the same XZ-coordinates
         * as the provided voxel coordinate.
         */
        public Vect3 LowestPositionAtCoordinate(Point3 point)
        {
            int lower = 0;
            while (coordinateGrid[point.x][point.y-lower-1][point.z])
            {
                lower++;
            }
            int lowestY = point.y - lower;
            return VoxelPositionAsCoordinate(point.x, lowestY, point.z);
        }

        public Vect3 VoxelPositionAsCoordinate(int x, int y, int z)
        {
            Vect3 cubeCoordinate = voxelStartCoordinate +
                                        orientedBbox.localX * x * resolution +
                                        orientedBbox.localY * y * resolution +
                                        orientedBbox.localZ * z * resolution;

            return cubeCoordinate;
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
                VoxelGrid other = (VoxelGrid)obj;

                return this.resolution == other.resolution
                       && this.voxelStartCoordinate == other.voxelStartCoordinate
                       && this.coordinateGrid.Length == other.coordinateGrid.Length
                       && this.coordinateGrid[0].Length == other.coordinateGrid[0].Length
                       && this.coordinateGrid[0][0].Length == other.coordinateGrid[0][0].Length
                       && System.Linq.Enumerable.SequenceEqual(this.coordinateGrid, other.coordinateGrid);
            }
        }

        public override int GetHashCode()
        {
            return this.voxelStartCoordinate.GetHashCode() ^ this.resolution.GetHashCode();
        }
    }
}