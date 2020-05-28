using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Xml;
using System.Numerics;

namespace Nodegraph_Generator
{
    /*
    * Component is a representation of a mesh for the Nodegraph project. It contains a list of vertices and a list of faces that use the vertices.
    * When working with a component object the indices for vertices and faces is used. Therefor any add and create functions return the index of the new vertex/face
    * and help functions take in the index of a vertex/face instead of the actual object.
    */
    [Serializable()]
    public class Component
    {
        private List<Vertex> _vertices;
        private List<Face> _faces;
        public int index = -1;

        private double _lowestXValue = Double.PositiveInfinity;
        private double _highestXValue = Double.NegativeInfinity;
        private double _lowestYValue = Double.PositiveInfinity;
        private double _highestYValue = Double.NegativeInfinity;
        private double _lowestZValue = Double.PositiveInfinity;
        private double _highestZValue = Double.NegativeInfinity;

        public double lowestXValue {get {return _lowestXValue;} private set{}}
        public double highestXValue {get {return _highestXValue;} private set{}}
        public double lowestYValue {get {return _lowestYValue;} private set{}}
        public double highestYValue {get {return _highestYValue;} private set{}}
        public double lowestZValue {get {return _lowestZValue;} private set{}}
        public double highestZValue {get {return _highestZValue;} private set{}}

        public Component(){
            _vertices = new List<Vertex>();
            _faces = new List<Face>();
            vertices = _vertices.AsReadOnly();
            faces = _faces.AsReadOnly();
        }

        public ReadOnlyCollection<Vertex> vertices{
            get; private set;
        }

        public ReadOnlyCollection<Face> faces{
            get; private set;
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
                Component other = (Component)obj;

                return System.Linq.Enumerable.SequenceEqual(_faces, other._faces) &&
                       System.Linq.Enumerable.SequenceEqual(_vertices, other._vertices);
            }
        }

        public override int GetHashCode()
        {
            return this._faces.GetHashCode() ^ this._vertices.GetHashCode();
        }

        /*
         * Creates a deep copy of this component and returns it.
         */
        public Component DeepCopy()
        {
            using (MemoryStream stream = new MemoryStream())
            {

                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(stream, this);

                stream.Position = 0;

                return (Component) formatter.Deserialize(stream);

            }
        }

        public Component Transform(Matrix4x4 transform) {
            Component componentCopy = this.DeepCopy();

            Vect3 firstCoordinate = componentCopy.vertices[0].coordinate * transform;

            componentCopy._lowestXValue = firstCoordinate.x;
            componentCopy._highestXValue = firstCoordinate.x;
            componentCopy._lowestYValue = firstCoordinate.y;
            componentCopy._highestYValue = firstCoordinate.y;
            componentCopy._lowestZValue = firstCoordinate.z;
            componentCopy._highestZValue = firstCoordinate.z;


            foreach (Vertex vertex in componentCopy.vertices)
            {
                vertex.coordinate = vertex.coordinate * transform;
                componentCopy.UpdateBoxConstraints(vertex.coordinate);
            }

            Matrix4x4 tiTransform = new Matrix4x4();

            if (Matrix4x4.Invert(transform, out tiTransform))
            {
                tiTransform = Matrix4x4.Transpose(tiTransform);
            }
            else
            {
                throw new InvalidTransformException("Invalid transformation matrix.");
            }

            foreach (Face face in componentCopy.faces)
            {
                face.normal = face.normal * tiTransform;
            }

            return componentCopy;
        }

        /*
        * Returns a List<int> of indicies of all faces that share a side with face f. I.E. they
        share two indices.
        */
        public List<int> GetNeighbouringFacesViaEdges(int faceIndex){
            List<int> neighbouringFaces = new List<int>();

            // Find any common faces between vertex index pairs (0,1), (0,2), (1,2).
            // If the vertices share a face that face shares a 'side' with param face f

            // Check (0,1), (0,2) pairs
            foreach(int face1Index in GetVertex(GetFace(faceIndex).vertexIndices[0]).faceIndices){
                foreach(int face2Index in GetVertex(GetFace(faceIndex).vertexIndices[1]).faceIndices){
                    if(face1Index == face2Index && face1Index != faceIndex){
                        neighbouringFaces.Add(face1Index);
                    }
                }
                foreach(int face2Index in GetVertex(GetFace(faceIndex).vertexIndices[2]).faceIndices){
                    if(face1Index == face2Index && face1Index != faceIndex){
                        neighbouringFaces.Add(face1Index);
                    }
                }
            }

            // Check (1,2) pair
            foreach(int face1Index in GetVertex(GetFace(faceIndex).vertexIndices[1]).faceIndices){
                foreach(int face2Index in GetVertex(GetFace(faceIndex).vertexIndices[2]).faceIndices){
                    if(face1Index == face2Index && face1Index != faceIndex){
                        neighbouringFaces.Add(face1Index);
                    }
                }
            }
            return neighbouringFaces;
        }

        /*
        * Returns a List<int> of indices of all faces that share a vertex with face f.
        */
        public List<int> GetNeighbouringFacesViaVertices(int faceIndex){
            HashSet<int> neighbouringFaces = new HashSet<int>();
            foreach(int i in GetFace(faceIndex).vertexIndices){
                foreach(int face in GetVertex(i).faceIndices){
                    neighbouringFaces.Add(face);
                }
            }
            neighbouringFaces.Remove(faceIndex);
            return neighbouringFaces.ToList();
        }

        /*
        * Create a new face from it's normal and 3 vertex indices and add it to faces list. Also adds the face in the vertices list of faces containing it.
        * Returns the index of the new face.
        */
        public int CreateFace(Vect3 normal,int[] vertices) {
            if (vertices.Length != 3) throw new InvalidFaceException("Face takes exactly three vertices.");
            Vertex vertex1 = GetVertex(vertices[0]);
            Vertex vertex2 = GetVertex(vertices[1]);
            Vertex vertex3 = GetVertex(vertices[2]);
            Face face = new Face(normal, vertices);
            _faces.Add(face);

            vertex1.AddFaceIndex(_faces.Count - 1);
            vertex2.AddFaceIndex(_faces.Count - 1);
            vertex3.AddFaceIndex(_faces.Count - 1);

            // Return the index of new face.
            return _faces.Count - 1;
        }

        /*
        * Create a Vertex with coordinates and adds it to vertices list.
        */
        public int CreateVertex(double x, double y, double z){
            Vertex v = new Vertex(x,y,z);
            _vertices.Add(v);
            UpdateBoxConstraints(v.coordinate);
            return vertices.Count - 1;
        }

        /* Help functions for faces */

        /*
        * Create a new face from list of 3 vertices index and add it to faces list.
        */
        public int CreateFace(Vect3 normal, List<int> vertices){
            return CreateFace(normal, vertices.ToArray());
        }

        /*
        * Add a face to face list if the face is correct.
        */
        public int AddFace(Face face){
            int [] vertices = face.vertexIndices;
            Vertex vertex1 = GetVertex(vertices[0]);
            Vertex vertex2 = GetVertex(vertices[1]);
            Vertex vertex3 = GetVertex(vertices[2]);
            Vect3 normal = Vect3.Cross(vertex1.coordinate - vertex2.coordinate,
                                        vertex1.coordinate - vertex3.coordinate).GetNormalized();

            if (normal != face.normal.GetNormalized()){
                throw new InvalidFaceException("Normal does not match vertices of face.");
            }

            _faces.Add(face);

            vertex1.AddFaceIndex(_faces.Count - 1);
            vertex2.AddFaceIndex(_faces.Count - 1);
            vertex3.AddFaceIndex(_faces.Count - 1);
            return _faces.Count - 1;
        }

        public List<int> GetVertexIndicesFromFace(int face){
            return new List<int>(GetFace(face).vertexIndices);
        }

        public List<(Vect3, Vect3)> GetFaceSides(Face face)
        {
            var Edges = new List<(Vect3, Vect3)>();

            Edges.Add((vertices[face.vertexIndices[0]].coordinate, vertices[face.vertexIndices[1]].coordinate));
            Edges.Add((vertices[face.vertexIndices[0]].coordinate, vertices[face.vertexIndices[2]].coordinate));
            Edges.Add((vertices[face.vertexIndices[1]].coordinate, vertices[face.vertexIndices[2]].coordinate));

            return Edges;
        }

        public Vect3 GetNormal(int face){
            return GetFace(face).normal;
        }

        public Face GetFace(int faceIndex){
            return _faces[faceIndex];
        }

        /* Help functions for vertex */

        public int AddVertex(Vertex vertex){
            _vertices.Add(vertex);

            UpdateBoxConstraints(vertex.coordinate);

            return _vertices.Count - 1;
        }

        private void UpdateBoxConstraints(Vect3 coordinate)
        {
            _lowestXValue = Math.Min(coordinate.x, _lowestXValue);
            _lowestYValue = Math.Min(coordinate.y, _lowestYValue);
            _lowestZValue = Math.Min(coordinate.z, _lowestZValue);
            _highestXValue = Math.Max(coordinate.x, _highestXValue);
            _highestYValue = Math.Max(coordinate.y, _highestYValue);
            _highestZValue = Math.Max(coordinate.z, _highestZValue);
        }

        public Vertex GetVertex(int index){
            return _vertices[index];
        }

        public ReadOnlyCollection<int> GetFaceIndicesFromVertex(int vertex){
            return GetVertex(vertex).faceIndices;
        }

        public Vect3 GetCoordinate(int vertex){
            return GetVertex(vertex).coordinate;
        }
    }
}
