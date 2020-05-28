using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Nodegraph_Generator
{
    /**
     * Creates an XML representation of a node graph based on a 3D-model.
     */
    public class GraphGenerator
    {

        static void Main(string[] args)
        {
            FilePaths filePaths = ArgParser.Parse(args);

            AssimpInterpreter interpreter = new AssimpInterpreter();

            Structure structure = interpreter.Interpret(filePaths.inFile);
            
            NodeGraph nodeGraph = null;

            // Voxel solution.
            if (filePaths.voxelSolution) {
                nodeGraph = VoxelGridGraphGenerator.GenerateNodeGraph(structure);
            }
            // Mesh solution.
            else {
                List<NodeGraph> nodeGraphs = new List<NodeGraph>();
                for(int i = 0; i < structure.components.Count; i++) {
                    NodeGraph n = ComponentGraphGenerator.GenerateComponentNodeGraph(structure.components[i]);
                    nodeGraphs.Add(n);
                }
                nodeGraph = GraphUnifier.UnifyGraphs(nodeGraphs);

            }

            XMLCreator.writeXML(nodeGraph, filePaths.outFile);

        }

        /*
         * Generates a nodeGraph using the componentNodeGraph + UnifyNodeGraphs strategy.
         */
        static NodeGraph GenerateNodeGraph(Structure structure)
        {
            List<NodeGraph> nodeGraphs = new List<NodeGraph>();

            foreach (Component component in structure.components)
            {
                nodeGraphs.Add(ComponentGraphGenerator.GenerateComponentNodeGraph(component));
            }

            return GraphUnifier.UnifyGraphs(nodeGraphs);
        }
    }
}

