using System.Numerics;
using System;
using System.Collections.Generic;

namespace Nodegraph_Generator
{
    /*
     * Vect3 is a utility class for the Nodegraph project. It provides linear algebra tools for
     * working with vectors and to a limited extent, normals and planes in a 3D space.
     */
    [Serializable()]
    public class Vect3
    {
        // Epsilon used for triangle capture comparisons.
        private const double TRIANGLE_EPSILON = 0.5 * 1E-3;
        public double x;
        public double y;
        public double z;

        /* Declared static vectors for general use */
        static public readonly Vect3 Zero = new Vect3();
        static public readonly Vect3 One = new Vect3(1, 1, 1);
        static public readonly Vect3 Right = new Vect3(1, 0, 0);
        static public readonly Vect3 Left = new Vect3(-1, 0, 0);
        static public readonly Vect3 Up = new Vect3(0, 1, 0);
        static public readonly Vect3 Down = new Vect3(0, -1, 0);
        static public readonly Vect3 Forward = new Vect3(0, 0, 1);
        static public readonly Vect3 Backward = new Vect3(0, 0, -1);

        static public readonly List<Vect3> BaseVectors = new List<Vect3>(){Right, Up, Forward};


        /* Constructing an empty vector initializes it to the zero vector. */
        public Vect3()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        public Vect3(double _x, double _y, double _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public Vect3(Vector3 vector3)
        {
            x = vector3.X;
            y = vector3.Y;
            z = vector3.Z;
        }

        public Vect3(Vect3 other)
        {
            x = other.x;
            y = other.y;
            z = other.z;
        }

        public double this[int idx]
        {
            get
            {
                if (idx == 0) return x;
                if (idx == 1) return y;
                if (idx == 2) return z;
                throw new IndexOutOfRangeException();
            }
            set
            {
                if (idx == 0) x = value;
                if (idx == 1) y = value;
                if (idx == 2) z = value;

            }
        }

        /* Operators */
        public static bool operator ==(Vect3 v1, Vect3 v2)
        {
            if (object.ReferenceEquals(v1, null) || object.ReferenceEquals(v2, null))
            {
                return object.ReferenceEquals(v1, null) && object.ReferenceEquals(v2, null);
            }
            return Util.NearlyEqual(v1.x, v2.x) && Util.NearlyEqual(v1.y, v2.y) && Util.NearlyEqual(v1.z, v2.z);
        }

        public static bool operator !=(Vect3 v1, Vect3 v2)
        {
            return !(v1 == v2);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Vect3);
        }

        public bool Equals(Vect3 other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2;
        }

        public static Vect3 operator +(Vect3 v1, Vect3 v2)
            => new Vect3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);

        public static Vect3 operator -(Vect3 v1, Vect3 v2)
            => new Vect3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);

        public static Vect3 operator *(Vect3 v1, double constant)
            => new Vect3(v1.x * constant, v1.y * constant, v1.z * constant);

        public static Vect3 operator *(double constant, Vect3 v1)
            => v1 * constant;

        /*
         * Performs matrix multiplication. Float/double conversion potentially causes some loss of precision.
         */
        public static Vect3 operator *(Vect3 v, Matrix4x4 matrix) {

            Vector3 v3 = new Vector3((float)v.x, (float)v.y, (float)v.z);

            v3 = Vector3.Transform(v3, matrix);

            return new Vect3(v3.X, v3.Y, v3.Z);
        }

        public static Vect3 operator /(Vect3 v1, double constant)
        {
            if (constant == 0)
            {
                throw new DivideByZeroException("Can not divide a Vect3 with Zero");
            }
            else
            {
                return new Vect3(v1.x / constant, v1.y / constant, v1.z / constant);
            }
        }

        /* Functions */

        /* Formats vector as string */
        public override string ToString() {
            return "{" + x + ", " + y + ", " + z + "}";
        }

        /* Calculates the euclidean length of the vector. */
        public double Length()
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
        }

        /* Calculates the squared euclidean length of the vector. */
        public double LengthSquared()
        {
            return Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2);
        }

        /* Creates and returns a normalized version of this vector. */
        public Vect3 GetNormalized()
        {
            if (this == Zero)
            {
                throw new ZeroVectorException("Can not Normalize the zero Vect3");
            }
            else
            {
                return this / Length();
            }
        }

        /* Generates two vectors that the describe the plane normal to this vector. */
        public Tuple<Vect3, Vect3> GetNormalPlane()
        {
            if(this == Zero)
            {
                throw new InvalidPlaneException("Tried to get normal plane of the Zero vector");
            }
            Vect3 tempVect = this - ProjectOnVector(Forward);
            if (tempVect == Zero)
            {
                tempVect = this - ProjectOnVector(Up);
            }
            Vect3 baseVect1 = Cross(this, tempVect);
            Vect3 baseVect2 = Cross(this, baseVect1);
            return new Tuple<Vect3, Vect3>(baseVect1, baseVect2);
        }

        /* Projects this vector onto another vector. */
        public Vect3 ProjectOnVector(Vect3 other)
        {
            if (other == Zero)
            {
                return Zero;
            }
            else
            {
                double projection_factor = Dot(this, other) / Dot(other, other);
                return other * projection_factor;
            }
        }

        /* Projects this vector onto a plane defined by a normal. */
        public Vect3 ProjectOnPlane(Vect3 normal)
        {
            if (normal == Zero)
            {
                throw new InvalidPlaneException("Tried to project a vector on a plane with zero as normal vector.");
            }
            return this - ProjectOnVector(normal);
        }

        /* Projects this vector onto a plane defined by two base vectors. */
        public Vect3 ProjectOnPlane(Vect3 baseVector1, Vect3 baseVector2)
        {
            Vect3 v1 = Cross(baseVector1, baseVector2);
            if (v1 == Zero)
            {
                throw new InvalidPlaneException("Tried to project a vector on a plane with parallel base vectors.");
            }

            return ProjectOnPlane(v1);
        }


        /* Calculates the smallest angle in degrees between two lines defined by vectors. */
        public double AngleTo(Vect3 other)
        {
            if (this == Zero)
            {
                throw new ZeroVectorException("Tried to calculate the angle from the Zero Vector.");
            }
            if (other == Zero)
            {
                throw new ZeroVectorException("Tried to calculate the angle to the Zero Vector.");
            }
            if (this == other)
            {
                return 0;
            }
            Vect3 v1 = this.GetNormalized();
            Vect3 v2 = other.GetNormalized();

            double angle = Math.Acos(Dot(v1, v2)) * (180 / Math.PI);

            return angle;
        }

        /* Static Functions */

        /* Calculates dot product (scalar) of two vectors. */
        public static double Dot(Vect3 v1, Vect3 v2)
        {
            return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z);
        }

        /* Calculates the cross product of two vectors. */
        public static Vect3 Cross(Vect3 v1, Vect3 v2)
        {
            return new Vect3(
                (v1.y * v2.z) - (v1.z * v2.y),
                (v1.z * v2.x) - (v1.x * v2.z),
                (v1.x * v2.y) - (v1.y * v2.x));
        }

        /*
         * Finds the point on a line segment closest to a given point.
         *
         * Solves the equation system that minimizes the point-line distance.
         * https://math.stackexchange.com/questions/846054/closest-points-on-two-line-segments
         */
        public static Vect3 FindClosestLinePoint(Vect3 lineStart, Vect3 lineEnd, Vect3 point)
        {
            Vect3 lineVector = lineEnd - lineStart;

            double lineVectorSquared = Vect3.Dot(lineVector, lineVector);
            double pointToLineVectorDotProduct = Vect3.Dot((point - lineStart), lineVector);

            double vectorScalingFactor = pointToLineVectorDotProduct/lineVectorSquared;

            // Clamps the result to the line segment distance [0,1]
            // 0 is lineStart, 1 applies the entire lineVector from lineStart towards lineEnd.
            vectorScalingFactor = vectorScalingFactor < 0 ? 0 : vectorScalingFactor;
            vectorScalingFactor = vectorScalingFactor > 1 ? 1 : vectorScalingFactor;

            return lineStart + lineVector * vectorScalingFactor;
        }

        public static double FindShortestDistanceLinePoint(Vect3 lineStart, Vect3 lineEnd, Vect3 point)
        {
            Vect3 p = FindClosestLinePoint(lineStart, lineEnd, point);
            return (point - p).Length();
        }

        public static double FindShortestDistanceLinePoint(Vertex lineStart, Vertex lineEnd, Vertex point)
        {
            return FindShortestDistanceLinePoint(lineStart.coordinate, lineEnd.coordinate, point.coordinate);
        }

        /*
         * Finds the area of the triangle formed by 3 vectors.
         */
        public static double TriangleArea(Vect3 point1, Vect3 point2, Vect3 point3)
        {
            Vect3 triangleBase = (point1 - point2);
            Vect3 side = (point1 - point3);
            Vect3 height = side - side.ProjectOnVector(triangleBase);

            return (triangleBase.Length() * height.Length()) / 2;
        }

        /* Returns the middle point in between two vect3. */
        public static Vect3 MiddlePoint(Vect3 v1, Vect3 v2)
        {
            return (v1 + v2) / 2d;
        }

        public static class Transforms
        {
            public static Matrix4x4 Translation(Vect3 v)
            {
                return Translation(v.x, v.y, v.z);
            }

            public static Matrix4x4 Translation(double x, double y, double z)
            {
                return Matrix4x4.CreateTranslation((float)x, (float)y, (float)z);
            }

            public static Matrix4x4 Rotation(double x_Angle, double y_Angle, double z_Angle)
            {
                Matrix4x4 rotationTransform = Matrix4x4.CreateRotationX((float)x_Angle);
                rotationTransform *= Matrix4x4.CreateRotationY((float)y_Angle);
                rotationTransform *= Matrix4x4.CreateRotationZ((float)z_Angle);

                return rotationTransform;
            }
        }
    }
}
