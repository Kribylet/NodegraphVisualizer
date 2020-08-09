using System;
using System.Collections.Generic;



namespace Nodegraph_Generator
{
    public struct Plane
    {
        public Vect3 offset;
        public Vect3 normal;

        public Plane(Vect3 offset, Vect3 normal)
        {
            this.offset = offset;
            this.normal = normal;
        }
    }

    public struct Triangle
    {
        List<Vect3> trianglePoints;
        Vect3 normal;

        public Triangle(Vect3 point1, Vect3 point2, Vect3 point3, Vect3 normal)
        {
            trianglePoints = new List<Vect3>();
            trianglePoints.Add(point1);
            trianglePoints.Add(point2);
            trianglePoints.Add(point3);
            this.normal = normal;
        }
    }

    /*
     * Represents an axis-aligned bounding box in the global
     * coordinate system.
     */
    public class AxisBBox
    {
        public double highestX = Double.NegativeInfinity;
        public double highestY = Double.NegativeInfinity;
        public double highestZ = Double.NegativeInfinity;
        public double lowestX = Double.PositiveInfinity;
        public double lowestY = Double.PositiveInfinity;
        public double lowestZ = Double.PositiveInfinity;

        private bool generatedCornerCoordinates = false;
        private List<Vect3> _cornerCoordinates;
        public List<Vect3> cornerCoordinates
        {
            get
            {
                if (!generatedCornerCoordinates)
                {
                    _cornerCoordinates = new List<Vect3>()
                    {
                        new Vect3(lowestX, lowestY, lowestZ),
                        new Vect3(lowestX, lowestY, highestZ),
                        new Vect3(lowestX, highestY, lowestZ),
                        new Vect3(lowestX, highestY, highestZ),
                        new Vect3(highestX, lowestY, lowestZ),
                        new Vect3(highestX, lowestY, highestZ),
                        new Vect3(highestX, highestY, lowestZ),
                        new Vect3(highestX, highestY, highestZ)
                    };
                    generatedCornerCoordinates = true;
                }
                return _cornerCoordinates;
            }
            private set {}
        }

        public AxisBBox() {}

        public AxisBBox(double highestX, double highestY, double highestZ,
                    double lowestX, double lowestY, double lowestZ)
        {
            this.highestX = highestX;
            this.highestY = highestY;
            this.highestZ = highestZ;

            this.lowestX = lowestX;
            this.lowestY = lowestY;
            this.lowestZ = lowestZ;
        }

        public AxisBBox(Component c)
        {
            highestX = c.highestXValue;
            lowestX = c.lowestXValue;

            highestY = c.highestYValue;
            lowestY = c.lowestXValue;

            highestZ = c.highestZValue;
            lowestZ = c.lowestZValue;
        }

        /*
         *  Checks whether two axis aligned bounding boxes overlap.
         */
        public bool Overlaps(AxisBBox other)
        {
            //component entirely left of other bounding box
            if (highestX < other.lowestX) return false;
            //component entirely right of other bounding box
            if (lowestX > other.highestX) return false;

            //component entirely below other bounding box
            if (highestY < other.lowestY) return false;
            //component entirely above other bounding box
            if (lowestY > other.highestY) return false;

            //component entirely behind other bounding box
            if (highestZ < other.lowestZ) return false;
            //component entirely ahead of other bounding box
            return !(lowestZ > other.highestZ);
        }

        /*
         *  Checks if a box contains an other box
         */
        public bool Contains(AxisBBox other)
        {
            return lowestX <= other.lowestX && other.highestX <= highestX &&
                   lowestY <= other.lowestY && other.highestY <= highestY &&
                   lowestZ <= other.lowestZ && other.highestZ <= highestZ;
        }

        public void Reset()
        {
            this.highestX = Double.NegativeInfinity;
            this.highestY = Double.NegativeInfinity;
            this.highestZ = Double.NegativeInfinity;

            this.lowestX = Double.PositiveInfinity;
            this.lowestY = Double.PositiveInfinity;
            this.lowestZ = Double.PositiveInfinity;

            generatedCornerCoordinates = false;
        }

        public void Expand(AxisBBox other)
        {
            this.highestX = this.highestX > other.highestX ? this.highestX : other.highestX;
            this.highestY = this.highestY > other.highestY ? this.highestY : other.highestY;
            this.highestZ = this.highestZ > other.highestZ ? this.highestZ : other.highestZ;

            this.lowestX = this.lowestX < other.lowestX ? this.lowestX : other.lowestX;
            this.lowestY = this.lowestY < other.lowestY ? this.lowestY : other.lowestY;
            this.lowestZ = this.lowestZ < other.lowestZ ? this.lowestZ : other.lowestZ;

            generatedCornerCoordinates = false;
        }

        public void Expand(List<Vect3> points)
        {
            foreach (var point in points)
            {
            this.highestX = point.x > highestX ? point.x : highestX;
            this.highestY = point.y > highestY ? point.y : highestY;
            this.highestZ = point.z > highestZ ? point.z : highestZ;

            this.lowestX = point.x < lowestX ? point.x : lowestX;
            this.lowestY = point.y < lowestY ? point.y : lowestY;
            this.lowestZ = point.z < lowestZ ? point.z : lowestZ;
            }

            generatedCornerCoordinates = false;
        }

        public AxisBBox Offset(Vect3 offset)
        {
            return new AxisBBox(highestX - offset.x,
                            highestY - offset.y,
                            highestZ - offset.z,
                            lowestX - offset.x,
                            lowestY - offset.y,
                            lowestZ - offset.z);
        }
    }

    /*
     * An oriented bounding box with a local coordinate system.
     *
     * By default, calculates an optimal enclosing box for a given
     * component. Base vectors can be defined manually.
     *
     * Contains utility methods for translating between the local
     * and glocal coordinate systems.
     */
    public class OrientedBBox
    {
        public Vect3 plane1Normal = null;
        public Vect3 plane1Offset = null;
        private Vect3 antiPlane1Offset = null;
        public Vect3 plane2Normal = null;
        public Vect3 plane2Offset = null;
        private Vect3 antiPlane2Offset = null;
        public Vect3 plane3Normal = null;
        public Vect3 plane3Offset = null;
        private Vect3 antiPlane3Offset = null;

        private double[,] T;
        private double[,] Ti;
        private double[,] normalT;

        public Vect3 localOrigin;
        public Vect3 localX;
        public Vect3 localY;
        public Vect3 localZ;
        public Vect3 localMaxX;
        public Vect3 localMaxY;
        public Vect3 localMaxZ;
        public Vect3 localMaxXZ;
        public Vect3 localMaxXY;
        public Vect3 localMaxYZ;
        public Vect3 localMaxXYZ;

        bool normalsGenerated = false;
        Dictionary<int, (Vect3, Vect3)> _allNormals;

        public Dictionary<int, (Vect3, Vect3)> allNormals
        {
            get
            {
                if (!normalsGenerated)
                {
                    _allNormals = new Dictionary<int, (Vect3, Vect3)>()
                    {
                        {1, (plane1Normal, plane1Offset)},
                        {2, (plane2Normal, plane2Offset)},
                        {3, (plane3Normal, plane3Offset)},
                        {-1, (plane1Normal * -1, antiPlane1Offset)},
                        {-2, (plane2Normal * -1, antiPlane2Offset)},
                        {-3, (plane3Normal * -1, antiPlane3Offset)},
                    };
                    normalsGenerated = true;
                }
                return _allNormals;
            }
            private set {}
        }

        bool pointsGenerated = false;
        List<Vect3> _allPoints;

        public List<Vect3> allPoints
        {
            get
            {
                if (!pointsGenerated)
                {
                    _allPoints = new List<Vect3>()
                    {
                        localOrigin,
                        localMaxX,
                        localMaxY,
                        localMaxZ,
                        localMaxXY,
                        localMaxXZ,
                        localMaxYZ,
                        localMaxXYZ
                    };
                    pointsGenerated = true;
                }
                return _allPoints;
            }
            private set {}
        }

        private bool planesGenerated = false;
        private List<Plane> _allPlanes;

        public List<Plane> allPlanes
        {
            get
            {
                if (!planesGenerated)
                {
                    _allPlanes = new List<Plane>()
                    {
                        new Plane(localOrigin, localX * -1),
                        new Plane(localOrigin, localY * -1),
                        new Plane(localOrigin, localZ * -1),
                        new Plane(localMaxXYZ, localX),
                        new Plane(localMaxXYZ, localY),
                        new Plane(localMaxXYZ, localZ)
                    };
                    planesGenerated = true;
                }
                return _allPlanes;

            }

            private set {}
        }

        private bool trianglesGenerated = false;
        private List<Triangle> _triangles;

        public List<Triangle> asTriangles
        {
            get
            {
                if (!trianglesGenerated)
                {
                    _triangles = new List<Triangle>()
                    {
                        new Triangle(localOrigin, localMaxX, localMaxXY, localX * -1),
                        new Triangle(localOrigin, localMaxY, localMaxXY, localX * -1),

                        new Triangle(localOrigin, localMaxX, localMaxXZ, localY * -1),
                        new Triangle(localOrigin, localMaxZ, localMaxXZ, localY * -1),

                        new Triangle(localOrigin, localMaxY, localMaxYZ, localZ * -1),
                        new Triangle(localOrigin, localMaxZ, localMaxYZ, localZ * -1),

                        new Triangle(localMaxX, localMaxXY, localMaxXYZ, localX),
                        new Triangle(localMaxX, localMaxXZ, localMaxXYZ, localX),

                        new Triangle(localMaxY, localMaxXY, localMaxXYZ, localY),
                        new Triangle(localMaxY, localMaxYZ, localMaxXYZ, localY),

                        new Triangle(localMaxZ, localMaxXZ, localMaxXYZ, localZ),
                        new Triangle(localMaxZ, localMaxYZ, localMaxXYZ, localZ),
                    };
                    trianglesGenerated = true;
                }
                return _triangles;
            }

            private set {}
        }

        private bool middleCalculated = false;
        private Vect3 _middle;

        public Vect3 globalMiddle
        {
            get
            {
                if (!middleCalculated)
                {
                    _middle = new Vect3();
                    foreach (var point in allPoints)
                    {
                        _middle += point;
                    }
                    _middle /= allPoints.Count;
                    middleCalculated = true;
                }
                return _middle;
            }

            private set {}
        }

        private bool axesGenerated = false;
        private Dictionary<Axes, Vect3> _localAxes;

        public Dictionary<Axes, Vect3> LocalAxes
        {
            get
            {
                if (!axesGenerated)
                {
                    _localAxes = new Dictionary<Axes, Vect3>()
                    {
                        {Axes.PositiveX, localX},
                        {Axes.PositiveY, localY},
                        {Axes.PositiveZ, localZ},

                        {Axes.NegativeX, localX * -1d},
                        {Axes.NegativeY, localY * -1d},
                        {Axes.NegativeZ, localZ * -1d}

                    };
                    axesGenerated = true;
                }
                return _localAxes;
            }

            private set {}
        }

        public OrientedBBox(Component component, Vect3 plane1Normal = null, Vect3 plane2Normal = null, Vect3 plane3Normal = null)
        {
            this.plane1Normal = plane1Normal;
            this.plane2Normal = plane2Normal;
            this.plane3Normal = plane3Normal;

            List<Plane> candidatePlanes = new List<Plane>();

            foreach (Face face in component.faces)
            {
                candidatePlanes.Add(new Plane(component.GetCoordinate(face.vertexIndices[0]), face.normal));
            }

            List<Vect3> points = new List<Vect3>();
            foreach (Vertex vertex in component.vertices)
            {
                points.Add(vertex.coordinate);
            }

            FindEnclosingPlanes(candidatePlanes, points);
            GenerateBounds();
        }

        public OrientedBBox(Structure structure, Vect3 plane1Normal = null, Vect3 plane2Normal = null, Vect3 plane3Normal = null)
        {
            this.plane1Normal = plane1Normal;
            this.plane2Normal = plane2Normal;
            this.plane3Normal = plane3Normal;

            List<Plane> candidatePlanes = new List<Plane>();
            List<Vect3> points = new List<Vect3>();

            foreach (Component component in structure.components)
            {
                foreach (Face face in component.faces)
                {
                    candidatePlanes.Add(new Plane(component.GetCoordinate(face.vertexIndices[0]), face.normal));
                }
                foreach (Vertex vertex in component.vertices)
                {
                    points.Add(vertex.coordinate);
                }
            }
            FindEnclosingPlanes(candidatePlanes, points);
            GenerateBounds();
        }

        private void GenerateBounds()
        {
            int mostAlignedLeftKey = 0;
            int mostAlignedDownKey = 0;
            int mostAlignedBackKey = 0;

            double smallestLeftAngle = Double.PositiveInfinity;
            double smallestDownAngle = Double.PositiveInfinity;
            double smallestBackAngle = Double.PositiveInfinity;

            var availableNormals = new Dictionary<int, (Vect3, Vect3)>(allNormals);

            foreach (var normalOffset in availableNormals)
            {
                double currentSmallestLeftAngle = normalOffset.Value.Item1.AngleTo(Vect3.Left);
                if (currentSmallestLeftAngle < smallestLeftAngle)
                {
                    mostAlignedLeftKey = normalOffset.Key;
                    smallestLeftAngle = currentSmallestLeftAngle;
                }
            }

            availableNormals.Remove(mostAlignedLeftKey);
            availableNormals.Remove(-mostAlignedLeftKey);

            foreach (var normalOffset in availableNormals)
            {
                 double currentSmallestDownAngle = normalOffset.Value.Item1.AngleTo(Vect3.Down);
                 if (currentSmallestDownAngle < smallestDownAngle)
                 {
                     mostAlignedDownKey = normalOffset.Key;
                     smallestDownAngle = currentSmallestDownAngle;
                 }
            }

            availableNormals.Remove(mostAlignedDownKey);
            availableNormals.Remove(-mostAlignedDownKey);

            foreach (var normalOffset in availableNormals)
            {
                double currentSmallestBackAngle = normalOffset.Value.Item1.AngleTo(Vect3.Backward);
                if (currentSmallestBackAngle < smallestBackAngle)
                {
                    mostAlignedBackKey = normalOffset.Key;
                    smallestBackAngle = currentSmallestBackAngle;
                }
            }

            Vect3 mostAlignedLeft = allNormals[mostAlignedLeftKey].Item1;
            Vect3 mostAlignedDown = allNormals[mostAlignedDownKey].Item1;
            Vect3 mostAlignedBack = allNormals[mostAlignedBackKey].Item1;
            Vect3 leastAlignedLeft = allNormals[mostAlignedLeftKey*-1].Item1;
            Vect3 leastAlignedDown = allNormals[mostAlignedDownKey*-1].Item1;
            Vect3 leastAlignedBack = allNormals[mostAlignedBackKey*-1].Item1;

            Vect3 mostAlignedLeftOffset = allNormals[mostAlignedLeftKey].Item2;
            Vect3 mostAlignedDownOffset = allNormals[mostAlignedDownKey].Item2;
            Vect3 mostAlignedBackOffset = allNormals[mostAlignedBackKey].Item2;
            Vect3 leastAlignedLeftOffset = allNormals[mostAlignedLeftKey*-1].Item2;
            Vect3 leastAlignedDownOffset = allNormals[mostAlignedDownKey*-1].Item2;
            Vect3 leastAlignedBackOffset = allNormals[mostAlignedBackKey*-1].Item2;

            localOrigin = GeneratePlaneIntersectionPoint(mostAlignedLeft, mostAlignedDown, mostAlignedBack,
                                                  mostAlignedLeftOffset, mostAlignedDownOffset, mostAlignedBackOffset);

            localMaxX = GeneratePlaneIntersectionPoint(leastAlignedLeft, mostAlignedDown, mostAlignedBack,
                                                  leastAlignedLeftOffset, mostAlignedDownOffset, mostAlignedBackOffset);

            localMaxY = GeneratePlaneIntersectionPoint(mostAlignedLeft, leastAlignedDown, mostAlignedBack,
                                                   mostAlignedLeftOffset, leastAlignedDownOffset, mostAlignedBackOffset);

            localMaxZ = GeneratePlaneIntersectionPoint(mostAlignedLeft, mostAlignedDown, leastAlignedBack,
                                                  mostAlignedLeftOffset, mostAlignedDownOffset, leastAlignedBackOffset);

            // Generate the rest of the points by using the 3 direction vectors and starting point.
            localX = localMaxX - localOrigin;
            localY = localMaxY - localOrigin;
            localZ = localMaxZ - localOrigin;

            localMaxXZ = localMaxX + localZ;
            localMaxXY = localMaxX + localY;
            localMaxYZ = localMaxZ + localY;
            localMaxXYZ = localMaxXZ + localY;

            if (localX != Vect3.Zero) localX = localX.GetNormalized();
            if (localY != Vect3.Zero) localY = localY.GetNormalized();
            if (localZ != Vect3.Zero) localZ = localZ.GetNormalized();

            T = new double[3,3]
            {
                {localX.x,localX.y,localX.z},
                {localY.x,localY.y,localY.z},
                {localZ.x,localZ.y,localZ.z}
            };

            Ti = Util.Invert(T);

            normalT = Util.Transpose(Ti);
        }

        public Vect3 GetLocalCoordinate(Vect3 globalCoordinate)
        {
            return Util.Dot(T, globalCoordinate);
        }

        public Vect3 GetLocalNormal(Vect3 normal)
        {
            return Util.Dot(normalT, normal);
        }

        public Vect3 GetGlobalCoordinate(Vect3 localCoordinate)
        {
            return Util.Dot(Ti, localCoordinate);
        }

        public bool ContainsGlobalCoordinate(Vect3 globalCoordinate, double expansionFactor = 0)
        {
            double planeDistanceSign;

            double permissibleDistance = (localOrigin - localMaxXYZ).Length()*expansionFactor;

            foreach (Plane plane in allPlanes)
            {
                planeDistanceSign = Vect3.Dot(globalCoordinate, plane.normal) - Vect3.Dot(plane.normal, plane.offset);
                if (planeDistanceSign > permissibleDistance) return false;
            }

            return true;
        }

        private Vect3 GeneratePlaneIntersectionPoint(Vect3 p1, Vect3 p2, Vect3 p3, Vect3 offset1, Vect3 offset2, Vect3 offset3)
        {
            double[,] lHS = new double[3,3]
            {
                {p1.x,p1.y,p1.z},
                {p2.x,p2.y,p2.z},
                {p3.x,p3.y,p3.z}
            };
            double[,] rHS = new double[3,1]
            {
                {Vect3.Dot(p1,offset1)},
                {Vect3.Dot(p2,offset2)},
                {Vect3.Dot(p3,offset3)}
            };

            double[,] point = Util.Solve(lHS, rHS);

            return new Vect3(point[0,0], point[1,0], point[2,0]);
        }

        private void FindEnclosingPlanes(List<Plane> candidatePlanes, List<Vect3> points)
        {
            FindPlane1(candidatePlanes, points);
            FindPlane2(candidatePlanes, points);
            FindPlane3(candidatePlanes, points);
        }

        private void FindPlane1(List<Plane> candidatePlanes, List<Vect3> points)
        {

            double greatestNegativeDistance = Double.PositiveInfinity;
            double greatestPositiveDistance = Double.NegativeInfinity;

            if (plane1Normal != null)
            {
                Plane plane = new Plane(points[0], plane1Normal);

                FindPlaneDistances(out greatestPositiveDistance, out greatestNegativeDistance,
                                    points, plane.normal, plane.offset);

                plane1Offset = plane.offset + plane.normal*greatestPositiveDistance;
                antiPlane1Offset = plane.offset + plane.normal*greatestNegativeDistance;
            }
            else
            {
                double smallestTotalDistanceMeasured = Double.PositiveInfinity;
                double currentSmallestDistance = Double.PositiveInfinity;
                foreach (Plane plane in candidatePlanes)
                {
                    if (plane.normal == Vect3.Zero) continue;
                    FindPlaneDistances(out greatestPositiveDistance, out greatestNegativeDistance,
                                    points, plane.normal, plane.offset);

                    currentSmallestDistance = (Math.Abs(greatestPositiveDistance) + Math.Abs(greatestNegativeDistance));

                    if (currentSmallestDistance < smallestTotalDistanceMeasured)
                    {
                        smallestTotalDistanceMeasured = currentSmallestDistance;
                        plane1Normal = plane.normal;
                        plane1Offset = plane.offset + plane.normal*greatestPositiveDistance;
                        antiPlane1Offset = plane.offset + plane.normal*greatestNegativeDistance;
                    }
                }
            }

            if (plane1Normal == null) throw new Exception("Failed to find Plane1Normal!");
        }
        private void FindPlane2(List<Plane> candidatePlanes, List<Vect3> points)
        {
            Vect3 plane1OrthogonalFaceNormal;

            double greatestNegativeDistance;
            double greatestPositiveDistance;

            if (plane2Normal != null)
            {
                Plane plane = new Plane(points[0], plane2Normal);

                FindPlaneDistances(out greatestPositiveDistance, out greatestNegativeDistance,
                                    points, plane.normal, plane.offset);

                plane2Offset = plane.offset + plane.normal*greatestPositiveDistance;
                antiPlane2Offset = plane.offset + plane.normal*greatestNegativeDistance;
            }
            else
            {
                double smallestTotalDistanceMeasured = Double.PositiveInfinity;
                double currentSmallestDistance = Double.PositiveInfinity;

                foreach (Plane plane in candidatePlanes)
                {
                    plane1OrthogonalFaceNormal = plane.normal - plane.normal.ProjectOnVector(plane1Normal);

                    if (plane1OrthogonalFaceNormal == Vect3.Zero) continue;

                    plane1OrthogonalFaceNormal = plane1OrthogonalFaceNormal.GetNormalized();

                    FindPlaneDistances(out greatestPositiveDistance, out greatestNegativeDistance,
                                    points, plane1OrthogonalFaceNormal, plane.offset);

                    currentSmallestDistance = (Math.Abs(greatestPositiveDistance) + Math.Abs(greatestNegativeDistance));

                    if (smallestTotalDistanceMeasured > currentSmallestDistance)
                    {
                        plane2Normal = plane1OrthogonalFaceNormal;
                        plane2Offset = plane.offset + greatestPositiveDistance*plane1OrthogonalFaceNormal;
                        antiPlane2Offset = plane.offset + greatestNegativeDistance*plane1OrthogonalFaceNormal;
                        smallestTotalDistanceMeasured = currentSmallestDistance;
                    }
                }
            }
        }

        private void FindPlane3(List<Plane> candidatePlanes, List<Vect3> points)
        {
            double greatestNegativeDistance;
            double greatestPositiveDistance;

            Vect3 planeOffset = candidatePlanes[0].offset;

            if (plane2Normal == null)
            {
                // All points share a plane
                Tuple<Vect3, Vect3> p2p3 = plane1Normal.GetNormalPlane();

                plane2Normal = p2p3.Item1;
                plane3Normal = p2p3.Item2;

                FindPlaneDistances(out greatestPositiveDistance, out greatestNegativeDistance, points, plane2Normal, planeOffset);
                plane2Offset = planeOffset + greatestPositiveDistance*plane2Normal;
                antiPlane2Offset = planeOffset + greatestNegativeDistance*plane2Normal;

                FindPlaneDistances(out greatestPositiveDistance, out greatestNegativeDistance, points, plane3Normal, planeOffset);
                plane3Offset = planeOffset + greatestPositiveDistance*plane3Normal;
                antiPlane3Offset = planeOffset + greatestNegativeDistance*plane3Normal;
                return;
            }

            if (plane3Normal == null) plane3Normal = Vect3.Cross(plane1Normal, plane2Normal);

            FindPlaneDistances(out greatestPositiveDistance, out greatestNegativeDistance,
                               points, plane3Normal, planeOffset);

            plane3Offset = planeOffset + greatestPositiveDistance*plane3Normal;
            antiPlane3Offset = planeOffset + greatestNegativeDistance*plane3Normal;
        }


        public void FindPlaneDistances(out double greatestPositiveDistance,
                                       out double greatestNegativeDistance,
                                       List<Vect3> points,
                                       Vect3 planeNormal,
                                       Vect3 planeOffset)
        {
            greatestPositiveDistance = Double.NegativeInfinity;
            greatestNegativeDistance = Double.PositiveInfinity;
            double planeDistanceSign;

            for (int i = 0; i < points.Count; i++)
            {
                planeDistanceSign = Vect3.Dot(points[i], planeNormal) - Vect3.Dot(planeNormal, planeOffset);

                if (planeDistanceSign > greatestPositiveDistance)
                {
                    greatestPositiveDistance = planeDistanceSign;
                }
                if (planeDistanceSign < greatestNegativeDistance)
                {
                    greatestNegativeDistance = planeDistanceSign;
                }
            }
        }
    }
}