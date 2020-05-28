using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Nodegraph_Generator;
using UnityEditor;
using System;

[Serializable]
public struct Files
{
    public UnityEngine.Object modelFile;
    public UnityEngine.Object XMLFile;
}

[Serializable]
public struct DrawStructure
{
    public bool drawVertices;
    public bool drawFaces;
    public bool colorFloor;
}

[Serializable]
public struct GraphStrategy
{
    public Strategy strategy;
    public bool writeXML;
}

public enum Strategy
{
    Voxel,
    Mesh,
    XML
}

[Serializable]
public struct DrawNodeGraph
{
    public bool drawNodes;
    public bool drawEdges;
    public bool drawAsTunnels;
}

[Serializable]
public struct DrawVoxels
{
    public bool drawVoxels;
    public bool fillInternalVolume;
    public bool skeletalize;
}

[Serializable]
public struct DrawOBBs
{
    public OBBType oBBType;
    public OBBDrawType oBBDrawType;
}

public enum OBBType
{
    None,
    components,
    voxelGrid
}

public enum OBBDrawType
{
    lines,
    points
}

[Serializable]
public struct MeshStrategy
{
    public bool unifyGraph;
    public bool drawCollisions;
}

[Serializable]
public struct VoxelStrategy
{
    public VoxelScope voxelScope;
    public double ComponentResDivider;
    public double GraphVoxelDeviationThreshold;
    public double mergeThreshold;
    public double peelFactor;
}

public enum VoxelScope
{
    structure,
    component
}

public class Main : MonoBehaviour
{
    public Files files;
    public GraphStrategy graphStrategy;
    public DrawStructure drawStructure;
    public DrawNodeGraph drawNodeGraph;
    public DrawVoxels drawVoxels;
    public DrawOBBs drawOBBs;
    public MeshStrategy meshStrategy;
    public VoxelStrategy voxelStrategy;
    public Drawer drawer;

    private static readonly Stopwatch gridTimer = new Stopwatch();
    private static readonly Stopwatch gridProcessingTimer = new Stopwatch();
    private static readonly Stopwatch graphGenerationTimer = new Stopwatch();
    private static readonly Stopwatch drawTimer = new Stopwatch();

    private Structure structure;
    private Vect3 structureMiddlePoint;
    // Start is called before the first frame update
    void Start()
    {
        // Build Up Structure
        AssimpInterpreter interpreter = new AssimpInterpreter();
        structure = interpreter.Interpret(AssetDatabase.GetAssetPath(files.modelFile));

        VoxelGridGraphGenerator.outerLayerLimit = voxelStrategy.peelFactor;
        VoxelGridGraphGenerator.VoxelDistanceThreshold = voxelStrategy.GraphVoxelDeviationThreshold;
        VoxelGridGraphGenerator.mergeThreshold = voxelStrategy.mergeThreshold;

        VoxelGrid.DefaultComponentWiseResolutionDivider = voxelStrategy.ComponentResDivider;
        structureMiddlePoint = Utilities.GetStructureMiddlePoint(structure);

        bool draw = drawNodeGraph.drawNodes || drawNodeGraph.drawEdges;

        List<NodeGraph> nodeGraphs = new List<NodeGraph>();

        List<VoxelGrid> voxelGrids = new List<VoxelGrid>();
        List<(DistanceGrid, VoxelGrid)> skeletonGrids = new List<(DistanceGrid, VoxelGrid)>();

        if (graphStrategy.strategy == Strategy.Voxel)
        {
            if (voxelStrategy.voxelScope == VoxelScope.component)
            {
                foreach (var component in structure.components)
                {
                   gridTimer.Start();
                   VoxelGrid voxelGrid = new VoxelGrid(component);
                   gridTimer.Stop();
                   gridProcessingTimer.Start();
                   if (draw || drawVoxels.fillInternalVolume || drawVoxels.skeletalize)
                   {
                       voxelGrid.FillInternalVolume(component);
                   }
                   gridProcessingTimer.Stop();
                   if (draw)
                   {
                       graphGenerationTimer.Start();
                       nodeGraphs.Add(VoxelGridGraphGenerator.GenerateNodeGraph(component));
                       graphGenerationTimer.Stop();
                   }

                   if (drawVoxels.drawVoxels)
                   {
                       gridProcessingTimer.Start();
                       if (drawVoxels.skeletalize)
                       {
                           skeletonGrids.Add((VoxelGridGraphGenerator.GenerateSkeletalGrid(voxelGrid), voxelGrid));
                       }
                       gridProcessingTimer.Stop();
                   }
                   voxelGrids.Add(voxelGrid);
                }
            }
            else if (voxelStrategy.voxelScope == VoxelScope.structure)
            {
                gridTimer.Start();
                VoxelGrid voxelGrid = new VoxelGrid(structure, plane1Normal: Vect3.Right, plane2Normal: Vect3.Up, plane3Normal: Vect3.Forward);
                gridTimer.Stop();
                gridProcessingTimer.Start();
                if (draw || drawVoxels.fillInternalVolume || drawVoxels.skeletalize)
                {
                    voxelGrid.FillInternalVolume(structure);
                }
                gridProcessingTimer.Stop();
                if (draw)
                {
                    graphGenerationTimer.Start();
                    nodeGraphs.Add(VoxelGridGraphGenerator.GenerateNodeGraph(structure));
                    graphGenerationTimer.Stop();
                }

                if (drawVoxels.drawVoxels)
                {
                    gridProcessingTimer.Start();
                    if (drawVoxels.skeletalize)
                    {
                        skeletonGrids.Add((VoxelGridGraphGenerator.GenerateSkeletalGrid(voxelGrid), voxelGrid));
                    }
                    gridProcessingTimer.Stop();
                }
                voxelGrids.Add(voxelGrid);
            }
        }
        else if (graphStrategy.strategy == Strategy.Mesh)
        {
            foreach (Nodegraph_Generator.Component component in structure.components)
            {
                graphGenerationTimer.Start();
                nodeGraphs.Add(ComponentGraphGenerator.GenerateComponentNodeGraph(component));
                graphGenerationTimer.Stop();
            }

            if (meshStrategy.drawCollisions)
            {
                drawTimer.Start();
                drawer.DrawCollisions(nodeGraphs, structureMiddlePoint);
                drawTimer.Stop();
            }

            if (meshStrategy.unifyGraph)
            {
                graphGenerationTimer.Start();
                NodeGraph nodeGraph = GraphUnifier.UnifyGraphs(nodeGraphs);
                graphGenerationTimer.Stop();
                nodeGraphs.Clear();
                nodeGraphs.Add(nodeGraph);
            }
        }
        else if (graphStrategy.strategy == Strategy.XML)
        {
            graphGenerationTimer.Start();
            nodeGraphs.Add(XMLCreator.readXML<NodeGraph>(AssetDatabase.GetAssetPath(files.XMLFile)));
            graphGenerationTimer.Stop();
        }

        drawTimer.Start();
        if (drawStructure.drawFaces || drawStructure.drawVertices)
        {
            drawer.DrawStructure(drawStructure, structure, structureMiddlePoint);
        }

        if (drawOBBs.oBBType == OBBType.components)
        {
            foreach (var voxelGrid in voxelGrids)
            {
                foreach (var oBB in voxelGrid.componentOBBs.Values)
                {
                    drawer.DrawOBB(oBB, drawOBBs, structureMiddlePoint);
                }
            }
        }
        else if (drawOBBs.oBBType == OBBType.voxelGrid)
        {
            foreach (var voxelGrid in voxelGrids)
            {
                drawer.DrawOBB(voxelGrid.orientedBbox, drawOBBs, structureMiddlePoint);
            }
        }

        if (drawVoxels.drawVoxels)
        {
            if (drawVoxels.skeletalize)
            {
                foreach (var skeletonGrid in skeletonGrids)
                {
                    drawer.DrawSkeletonGrid(skeletonGrid.Item1, skeletonGrid.Item2, structureMiddlePoint);
                }
            }
            else
            {
                foreach (var voxelGrid in voxelGrids)
                {
                    drawer.DrawVoxelGrid(voxelGrid, structureMiddlePoint);
                }
            }
        }

        if (drawNodeGraph.drawNodes || drawNodeGraph.drawEdges)
        {
            foreach (NodeGraph nodeGraph in nodeGraphs)
            {
                drawer.DrawNodeGraph(drawNodeGraph, nodeGraph, structureMiddlePoint);
            }
        }
        drawTimer.Stop();

        if (graphStrategy.writeXML && !(graphStrategy.strategy == Strategy.XML))
        {
            if (nodeGraphs.Count == 1)
            {
                string filename = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(files.modelFile));
                filename += ".xml";
                XMLCreator.writeXML(nodeGraphs[0], System.IO.Path.Combine("Assets", "XML", filename));
            }
            else
            {
                string filename = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(files.modelFile));
                int counter = 1;
                foreach (NodeGraph nodeGraph in nodeGraphs)
                {
                    string subfilename = filename + (counter++) + ".xml";
                    XMLCreator.writeXML(nodeGraphs[0], System.IO.Path.Combine("Assets", "XML", subfilename));
                }
            }
        }

        ReportProcessTimes();
    }

    private void ReportProcessTimes()
    {
        UnityEngine.Debug.Log("Voxelgrids: " + gridTimer.Elapsed.TotalMilliseconds.ToString("0.00") + " ms.\n" +
                              "Processing grids: " + gridProcessingTimer.Elapsed.TotalMilliseconds.ToString("0.00") + " ms.\n" +
                              "Graph generation: " + graphGenerationTimer.Elapsed.TotalMilliseconds.ToString("0.00") + " ms.\n" +
                              "Rendering: " + drawTimer.Elapsed.TotalMilliseconds.ToString("0.00") + " ms.");
    }
}
