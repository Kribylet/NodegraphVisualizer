using Assimp;
using System.Collections.Generic;
using System.IO;
using Assimp.Configs;

namespace Nodegraph_Generator
{
    /*
    * AssimpInterpreter is an implementation of the DataInterpreter interface which uses assimp to read the files. It translates the assimp datastructure
    * of the file into a equivalent Structure object.
    */
    public class AssimpInterpreter : DataInterpreter
    {
        private Structure _structure;

        Structure DataInterpreter.structure => _structure;

        public AssimpInterpreter(){
            _structure = new Structure();
        }

        /*
        * Interpretes file at 'filepath' using Assimp and converts it to a equivalent Structure class.
        */
        public Structure Interpret(string filepath){
            var context = new AssimpContext();
            Scene scene;
            try{
                NormalSmoothingAngleConfig smoothingConfig = new NormalSmoothingAngleConfig(0.0f);
                SortByPrimitiveTypeConfig removeConfig = new SortByPrimitiveTypeConfig(PrimitiveType.Point | PrimitiveType.Line);
                context.SetConfig(smoothingConfig);
                context.SetConfig(removeConfig);
                scene = context.ImportFile(filepath,
                                           PostProcessSteps.Triangulate |
                                           PostProcessSteps.JoinIdenticalVertices |
                                           PostProcessSteps.CalculateTangentSpace |
                                           PostProcessSteps.GenerateNormals |
                                           PostProcessSteps.FindDegenerates |
                                           PostProcessSteps.SortByPrimitiveType |
                                           PostProcessSteps.FixInFacingNormals);

            }catch(FileNotFoundException e){
                throw new FileNotFoundException("Unexpected error: Filepath into DataInterpreter not valid. {" + filepath + "}", e);
            }catch{
                throw new IOException("Error when importing file: " + filepath.ToString());
            }

            // For every mesh create a equivalent component.
            foreach(Mesh mesh in scene.Meshes){
                int compIndex = _structure.addComponent(new Component());

                Dictionary<(double, double, double), int> dict = PreprocessVertices(compIndex, mesh.Vertices);

                // For every face, lookup each of the three vertex indices using dict and their coordinates. Then create the face in component with
                // the indices we just got.
                foreach(var face in mesh.Faces) {
                    int[] vertices = new int[3];
                    vertices[0] = dict[(mesh.Vertices[face.Indices[0]].X, mesh.Vertices[face.Indices[0]].Y, mesh.Vertices[face.Indices[0]].Z)];
                    vertices[1] = dict[(mesh.Vertices[face.Indices[1]].X, mesh.Vertices[face.Indices[1]].Y, mesh.Vertices[face.Indices[1]].Z)];
                    vertices[2] = dict[(mesh.Vertices[face.Indices[2]].X, mesh.Vertices[face.Indices[2]].Y, mesh.Vertices[face.Indices[2]].Z)];
                    Vect3 normal = new Vect3(mesh.Normals[face.Indices[0]].X, mesh.Normals[face.Indices[0]].Y, mesh.Normals[face.Indices[0]].Z);
                    _structure.components[compIndex].CreateFace(normal ,vertices);
                }
            }
            return _structure;
        }

        /*
        * Remove duplicate vertices and add every unique vertex to component given by _structure.components[component]. Return value is a dictionary where the key
        * is a tuple of coordinates and the value is the index in component.vertices to the vertex at that coordinate.
        */
        private Dictionary<(double, double, double), int> PreprocessVertices(int component, List<Vector3D> vertices){
            Dictionary<(double, double, double), int> dict = new Dictionary<(double, double, double), int>();
            foreach(Vector3D vect in vertices){
                if (!dict.ContainsKey((vect.X, vect.Y, vect.Z))){
                    int index = _structure.components[component].CreateVertex(vect.X, vect.Y, vect.Z);
                    dict.Add((vect.X, vect.Y, vect.Z), index);
                }
            }
            return dict;
        }

    }
}
