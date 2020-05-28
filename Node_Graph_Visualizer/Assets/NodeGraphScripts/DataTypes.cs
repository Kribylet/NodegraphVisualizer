 ﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

namespace Nodegraph_Generator
{
    /*
     * Contains ArgParser return values.
     */
    public class FilePaths
    {
        public string inFile = "";
        public string outFile = "";

        public bool voxelSolution;

        /* Default Constructor*/
        public FilePaths() {}

        /* Initializer */
        public FilePaths(string _inFile = "", string _outFile = "")
        {
            inFile = _inFile;
            outFile = _outFile;
        }
    }

    public enum Axes
    {
        PositiveX,
        PositiveY,
        PositiveZ,
        NegativeX,
        NegativeY,
        NegativeZ
    }

    /*
     * Represents a 3D position and contains references
     * to the faces that position is associated with.
     */

    [Serializable()]
    public class Vertex
    {
        public Vect3 coordinate;
        private readonly List<int> _faceIndices;

        /* Default Constructor*/
        public Vertex() {}

        /* Coordinate initializer */
        public Vertex(Vect3 _coordinate) {
            this.coordinate = _coordinate;
            this._faceIndices = new List<int>();
            this.faceIndices = _faceIndices.AsReadOnly();
        }

        /* Double initializer */
        public Vertex(double x, double y, double z) {
            coordinate = new Vect3(x,y,z);
            _faceIndices = new List<int>();
            this.faceIndices = _faceIndices.AsReadOnly();
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
                Vertex other = (Vertex)obj;

                return coordinate.Equals(other.coordinate) &&
                       System.Linq.Enumerable.SequenceEqual(_faceIndices, other._faceIndices);
            }
        }

        public override int GetHashCode()
        {
            return this.coordinate.GetHashCode() ^ this._faceIndices.GetHashCode();
        }

        public override string ToString() {

            string returnString = "[Vertex: " + coordinate.ToString() + "] | ";

            foreach (int faceIndex in _faceIndices) {
                returnString += faceIndex + " ";
            }

            return returnString;
        }

        public ReadOnlyCollection<int> faceIndices{
            get; private set;
        }

        /* Wrapper to add valid face indices to face index list. */
        public void AddFaceIndex(int faceIndex)
        {
            if (faceIndex < 0)
            {
                throw new NegativeIndexException("Tried to insert a negative value as face index.");
            }
            else
            {
                _faceIndices.Add(faceIndex);
            }
        }

        /* Wrapper to add multiple valid face indices to face index list. */
        public void AddFaceIndices(List<int> faceIndices)
        {
            foreach(int index in faceIndices){
            if (index < 0)
            {
                throw new NegativeIndexException("Tried to insert a negative value as face index.");
            }
            else
            {
                _faceIndices.Add(index);
            }
            }
        }

        /*
        * Removes the face with index faceIndex from faces. Returns true if removed, false if not found.
        */
        public bool RemoveFace(int faceIndex){
            return _faceIndices.Remove(faceIndex);
        }
    }

    /*
     * Represents a face and has indices for the vertices contained.
     */
    [Serializable()]
    public class Face
    {
        public Vect3 normal;
        private int[] _vertexIndices;

        /* Default Constructor*/
        public Face() {}

        /* Coordinate initializer */
        public Face(Vect3 _normal, int[] _vertexIndices) {
            if (_vertexIndices.Length != 3) throw new InvalidFaceException("Face takes exactly three vertices.");
            if (_vertexIndices[0] < 0 || _vertexIndices[1] < 0 || _vertexIndices[2] < 0) throw new NegativeIndexException("Vertex indices can not be negative.");
            if (_vertexIndices[0] == _vertexIndices[1] || _vertexIndices[0] == _vertexIndices[2] ||
                _vertexIndices[1] == _vertexIndices[2])throw new InvalidFaceException("Vertex indices must be unique");
            this.normal = _normal;
            this._vertexIndices = _vertexIndices;
        }

        /* Double initializer */
        public Face(double x, double y, double z, int[] _vertexIndices) : this(new Vect3(x,y,z), _vertexIndices) {}

        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Face other = (Face)obj;

                return normal.Equals(other.normal) &&
                       System.Linq.Enumerable.SequenceEqual(_vertexIndices, other._vertexIndices);
            }
        }

        public override int GetHashCode()
        {
            return this.normal.GetHashCode() ^ this._vertexIndices.GetHashCode();
        }

        public override String ToString() {
            return "[" + vertexIndices[0] + vertexIndices[1] + vertexIndices[2] +"] N=" + normal.ToString();
        }

        public int[] vertexIndices{
            get => _vertexIndices;
        }
    }

    /*
     * Represents a link to another node and the edge that leads to it.
     */
    [Serializable()]
    [DataContract()]
    public class NodeEdgePair
    {
        [DataMember(Order = 0)]
        public int nodeIndex
        {
            get
            {
                if (_nodeIndex < 0) {
                    throw new NegativeIndexException("Tried to read uninitialized neighborNodeIndex");
                } else {
                    return _nodeIndex;
                }
            }
            set
            {
                if (value < 0)
                {
                    throw new NegativeIndexException("Tried to set neighborNodeIndex to a negative value");
                }
                else
                {
                    _nodeIndex = value;
                }
            }
        }
        [DataMember(Order = 1)]
        public int edgeIndex
        {
            get
            {
                if (_edgeIndex < 0) {
                    throw new NegativeIndexException("Tried to read uninitialized edgeIndex");
                } else {
                    return _edgeIndex;
                }
            }
            set
            {
                if (value < 0)
                {
                    throw new NegativeIndexException("Tried to set edgeIndex to a negative value");
                }
                else
                {
                    _edgeIndex = value;
                }
            }
        }

        private int _nodeIndex = -1;
        private int _edgeIndex = -1;

        /* Default Constructor*/
        public NodeEdgePair() {}

        /* Initializer */
        public NodeEdgePair(int _neighborNodeIndex, int _edgeIndex)
        {
            nodeIndex = _neighborNodeIndex;
            edgeIndex = _edgeIndex;
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
                NodeEdgePair other = (NodeEdgePair)obj;

                return nodeIndex == other.nodeIndex &&
                       edgeIndex == other.edgeIndex;
            }
        }

        public override int GetHashCode()
        {
            return this._edgeIndex.GetHashCode() ^ this._nodeIndex.GetHashCode();
        }
    }

    /* Represents a Node in the node graph. */
    [Serializable()]
    [DataContract()]
    public class Node
    {
        [DataMember(Order = 0)]
        public int index = -1;
        [DataMember(Order = 1)]
        public Vect3 coordinate = new Vect3();
        [DataMember(Order = 2)]
        public List<NodeEdgePair> neighbors = new List<NodeEdgePair>();

        /* Default Constructor*/
        public Node() {
        }

        /* Coordinate initializer */
        public Node(Vect3 _coordinate) {
            coordinate = _coordinate;
            neighbors = new List<NodeEdgePair>();
        }

        /* Double initializer */
        public Node(double x = 0, double y = 0, double z = 0)
        {
            coordinate = new Vect3(x,y,z);
            neighbors = new List<NodeEdgePair>();
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
                Node other = (Node)obj;

                return System.Linq.Enumerable.SequenceEqual(neighbors, other.neighbors) &&
                       (coordinate.Equals(other.coordinate) && index.Equals(other.index));
            }
        }

        public override int GetHashCode()
        {
            return this.coordinate.GetHashCode() ^ this.neighbors.GetHashCode();
        }

        public Node DeepCopy()
        {
            using (MemoryStream stream = new MemoryStream())
            {

                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(stream, this);

                stream.Position = 0;

                return (Node) formatter.Deserialize(stream);

            }
        }
    }

    /* Represents an Edge in the node graph. */
    [Serializable()]
    [DataContract()]
    public class Edge
    {
        [DataMember(Order = 0)]
        public int index = -1;
        [DataMember(Order = 1)]
        public int nodeIndex1
        {
            get
            {
                if (_nodeIndex1 < 0) {
                    throw new NegativeIndexException("Tried to read uninitialized nodeIndex1");
                } else {
                    return _nodeIndex1;
                }
            }
            set
            {
                if (value < 0)
                {
                    throw new NegativeIndexException("Tried to set node index 1 to a negative value.");
                }
                else
                {
                    _nodeIndex1 = value;
                }
            }
        }
        [DataMember(Order = 2)]
        public int nodeIndex2
        {
            get
            {
                if (_nodeIndex2 < 0) {
                    throw new NegativeIndexException("Tried to read uninitialized nodeIndex2");
                } else {
                    return _nodeIndex2;
                }
            }
            set
            {
                if (value < 0)
                {
                    throw new NegativeIndexException("Tried to set node index 2 to a negative value.");
                }
                else
                {
                    _nodeIndex2 = value;
                }
            }
        }

        [DataMember(Order = 3, Name = "minimumWidth")]
        public double width = -1;

        [DataMember(Order = 4, Name = "minimumHeight")]
        public double height = -1;
        
        private int _nodeIndex1 = -1;
        private int _nodeIndex2 = -1;

        /* Default Constructor*/
        public Edge() {}

        /* Initializer */
        public Edge(int nIndex1, int nIndex2)
        {
            nodeIndex1 = nIndex1;
            nodeIndex2 = nIndex2;
        }

        public Edge(int nIndex1, int nIndex2, int index, double width, double height){
            this.index = index;
            nodeIndex1 = nIndex1;
            nodeIndex2 = nIndex2;
            this.width = width;
            this.height = height;
        }

        public override String ToString() {
            return "Edge[" + index + "]{nodeIndex1: " + nodeIndex1 + ", nodeIndex2: " + nodeIndex2 + "}";
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
                Edge other = (Edge)obj;

                return nodeIndex1 == other.nodeIndex1 &&
                       nodeIndex2 == other.nodeIndex2 &&
                       index == other.index &&
                       width == other.width;
            }
        }

        public override int GetHashCode()
        {
            return this._nodeIndex1.GetHashCode() ^ this._nodeIndex2.GetHashCode();
        }

        public Edge DeepCopy()
        {
            using (MemoryStream stream = new MemoryStream())
            {

                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(stream, this);

                stream.Position = 0;

                return (Edge) formatter.Deserialize(stream);

            }
        }
    }
    public struct Point3
    {
        public int x;
        public int y;
        public int z;
        public Point3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Point3 operator+(Point3 p1, Point3 p2)
        {
            return new Point3(p1.x+p2.x, p1.y+p2.y, p1.z+p2.z);
        }

        static public bool operator==(Point3 p1, Point3 p2)
        {
            return p1.Equals(p2);
        }

        static public bool operator!=(Point3 p1, Point3 p2)
        {
            return !(p1 == p2);
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
                Point3 other = (Point3)obj;

                return x == other.x &&
                       y == other.y &&
                       z == other.z;
            }
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << this.z^2;
        }
    }
}
