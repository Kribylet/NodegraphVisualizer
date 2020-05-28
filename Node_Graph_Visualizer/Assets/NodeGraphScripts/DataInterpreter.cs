
namespace Nodegraph_Generator
{
    /*
    * DataInterpreter is a interface that takes in a filepath to a .fbx file and creates a Structure object from the model in the file.
    */
    public interface DataInterpreter
    {
        Structure structure { get; }

        Structure Interpret(string filepath);
    }
}
