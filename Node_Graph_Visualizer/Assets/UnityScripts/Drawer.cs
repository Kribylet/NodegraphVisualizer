using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Nodegraph_Generator;

[CreateAssetMenu(fileName = "Drawer", menuName = "ScriptableObject/Drawer")]
public class Drawer : Editor
{

    private static int nodeCounter = 0;
    public GameObject vertexPrefab;
    public GameObject nodePrefab;
    public GameObject faceMiddlePointPrefab;
    public GameObject cubePrefab;
    public GameObject collisionPrefab;
    public GameObject bBoxPrefab;

    public int scalingFactor = 1000;

    public void DrawStructure(DrawStructure choices, Structure structure, Vect3 structureMiddlePoint)
    {
        GameObject structureObject = new GameObject("Structure");
        for (int componentIndex = 0; componentIndex< structure.components.Count; componentIndex++)
        {
            GameObject componentObject = new GameObject(string.Format("Component {0}", componentIndex));
            componentObject.transform.parent = structureObject.transform;
            Nodegraph_Generator.Component component = structure.components[componentIndex];

            // To prepare color
            List<int> floorFaces = ComponentGraphGenerator.GetFloorFaces(component);
            LinkedList<int> linkedList = ComponentGraphGenerator.GetShellVertices(component, floorFaces);
            (LinkedList<int>, LinkedList<int>) sidePair = ComponentGraphGenerator.SplitLinkedList(component, linkedList);

            if (choices.drawVertices)
            {
                GameObject verticesObject = new GameObject(string.Format("Component {0}, Vertices", componentIndex));
                verticesObject.transform.parent = componentObject.transform;
                for (int vertexIndex = 0; vertexIndex < component.vertices.Count; vertexIndex++)
                {
                    Vertex vertex = component.GetVertex(vertexIndex);
                    Vect3 vertexPosition = Utilities.GetScaledTransletedVect3(vertex.coordinate, structureMiddlePoint, scalingFactor);
                    GameObject vertexObject = Utilities.DrawObject(vertexPrefab, vertexPosition, verticesObject);
                    vertexObject.name = string.Format("Vertex {0}", vertexIndex);
                    if (choices.colorFloor)
                    {
                        if (sidePair.Item1.Contains(vertexIndex))
                        {
                            Utilities.ColorObject(vertexObject, Color.blue);
                        }
                        else if (sidePair.Item2.Contains(vertexIndex))
                        {
                            Utilities.ColorObject(vertexObject, Color.red);
                        }
                    }
                }
            }

            if (choices.drawFaces)
            {
                var drawnVertexPairs = new List<(int, int)>();
                var drawnFaceSides = new List<GameObject>();
                GameObject faces = new GameObject(string.Format("Component {0}, Faces", componentIndex));
                faces.transform.parent = componentObject.transform;
                for (int faceIndex = 0; faceIndex < component.faces.Count; faceIndex++)
                {
                    Face face = component.GetFace(faceIndex);
                    var faceMiddlepoint = new Vect3();
                    for (int i = 0; i < face.vertexIndices.Length; i++)
                    {
                        int firstVertexIndex = face.vertexIndices[i];
                        Vertex firstVertex = component.GetVertex(firstVertexIndex);
                        faceMiddlepoint += firstVertex.coordinate;
                        for (int j = i+1; j < face.vertexIndices.Length; j++)
                        {
                            int secondVertexIndex = face.vertexIndices[j];
                            if(!(drawnVertexPairs.Contains((firstVertexIndex, secondVertexIndex)) ||
                                 drawnVertexPairs.Contains((secondVertexIndex, firstVertexIndex))))
                            {
                                Vertex secondVertex = component.GetVertex(secondVertexIndex);
                                Vect3 startPos = Utilities.GetScaledTransletedVect3(firstVertex.coordinate,
                                                                            structureMiddlePoint, scalingFactor);
                                Vect3 endPos = Utilities.GetScaledTransletedVect3(secondVertex.coordinate,
                                                                            structureMiddlePoint, scalingFactor);
                                GameObject faceSideObject = Utilities.DrawLine(startPos, endPos, faces);
                                faceSideObject.name = string.Format("Faceside to Face {0}", faceIndex);
                                Utilities.ColorObject(faceSideObject,
                                    (choices.colorFloor && floorFaces.Contains(faceIndex)) ? Color.green : Color.black);
                                drawnVertexPairs.Add((secondVertexIndex, firstVertexIndex));
                                drawnFaceSides.Add(faceSideObject);
                            }
                            else
                            {
                                GameObject faceSideObject = drawnFaceSides[drawnVertexPairs.FindIndex(a =>
                                (a.Item1 == firstVertexIndex && a.Item2 == secondVertexIndex) || (a.Item1 == secondVertexIndex && a.Item2 == firstVertexIndex))];
                                if(choices.colorFloor && floorFaces.Contains(faceIndex))
                                {
                                    Utilities.ColorObject(faceSideObject, Color.green);
                                }
                                faceSideObject.name += ", " + faceIndex;
                            }
                        }
                    }
                    faceMiddlepoint = Utilities.GetScaledTransletedVect3(faceMiddlepoint/3, structureMiddlePoint, scalingFactor);
                    GameObject faceMiddepointObject = Utilities.DrawObject(faceMiddlePointPrefab, faceMiddlepoint, faces);
                    faceMiddepointObject.name = string.Format("Face {0} MiddlePoint", faceIndex);
                    if (choices.colorFloor && floorFaces.Contains(faceIndex))
                    {
                        Utilities.ColorObject(faceMiddepointObject, Color.green);
                    }
                }
            }
        }
    }

    public int Count(VoxelGrid voxelGrid)
    {
        int counter = 0;
        for (int x = 0; x < voxelGrid.xBound; x++)
        {
            for (int y = 0; y < voxelGrid.yBound; y++)
            {
                for (int z = 0; z < voxelGrid.zBound; z++)
                {
                    if (voxelGrid.coordinateGrid[x][y][z]) counter++;
                }
            }
        }
        return counter;
    }


    public void DrawVoxelGrid(VoxelGrid voxelGrid, Vect3 structureMiddlePoint)
    {
        GameObject voxelGridObject = new GameObject(string.Format("VoxelGrid {0}, Vertices", Count(voxelGrid)));

        for (int x = 0; x < voxelGrid.xBound; x++)
        {
            for (int y = 0; y < voxelGrid.yBound; y++)
            {
                for (int z = 0; z < voxelGrid.zBound; z++)
                {
                    if (!voxelGrid.coordinateGrid[x][y][z]) continue;


                    Vect3 cubePos = voxelGrid.voxelStartCoordinate +
                                voxelGrid.orientedBbox.GetGlobalCoordinate(new Vect3(x*voxelGrid.resolution, y*voxelGrid.resolution, z*voxelGrid.resolution));

                    cubePos = Utilities.GetScaledTransletedVect3(cubePos, structureMiddlePoint, scalingFactor);
                    GameObject cubeObject = Utilities.DrawObject(cubePrefab, cubePos, voxelGridObject);
                    Vector3 scale = cubeObject.transform.localScale;

                    scale.x = scale.x * ((float) voxelGrid.resolution / scalingFactor);
                    scale.y = scale.y * ((float) voxelGrid.resolution / scalingFactor);
                    scale.z = scale.z * ((float) voxelGrid.resolution / scalingFactor);

                    cubeObject.transform.localScale = scale;

                    var rotation = Quaternion.LookRotation(new Vector3((float)-voxelGrid.orientedBbox.localZ.x,
                                                                        (float)voxelGrid.orientedBbox.localZ.y,
                                                                        (float)voxelGrid.orientedBbox.localZ.z),
                                                            new Vector3((float)-voxelGrid.orientedBbox.localY.x,
                                                                        (float)voxelGrid.orientedBbox.localY.y,
                                                                        (float)voxelGrid.orientedBbox.localY.z));
                    cubeObject.transform.rotation = rotation;
                }
            }
        }

        DrawPoint(voxelGrid.orientedBbox.localOrigin, structureMiddlePoint);
        DrawPoint(voxelGrid.orientedBbox.localMaxX, structureMiddlePoint);
        DrawPoint(voxelGrid.orientedBbox.localMaxY, structureMiddlePoint);
        DrawPoint(voxelGrid.orientedBbox.localMaxZ, structureMiddlePoint);
        DrawPoint(voxelGrid.orientedBbox.localMaxXZ, structureMiddlePoint);
        DrawPoint(voxelGrid.orientedBbox.localMaxXY, structureMiddlePoint);
        DrawPoint(voxelGrid.orientedBbox.localMaxYZ, structureMiddlePoint);
        DrawPoint(voxelGrid.orientedBbox.localMaxXYZ, structureMiddlePoint);
        DrawPoint(voxelGrid.voxelStartCoordinate, structureMiddlePoint);
    }

    public void DrawSkeletonGrid(DistanceGrid skeletonGrid, VoxelGrid voxelGrid, Vect3 structureMiddlePoint)
    {
        GameObject voxelGridObject = new GameObject(string.Format("VoxelGrid {0}, Vertices", 0));

        for (int x = 0; x < skeletonGrid.xBound; x++)
        {
            for (int y = 0; y < skeletonGrid.yBound; y++)
            {
                for (int z = 0; z < skeletonGrid.zBound; z++)
                {
                    if (skeletonGrid.grid[x][y][z] == 0) continue;
                    Vect3 cubePos = voxelGrid.voxelStartCoordinate + voxelGrid.orientedBbox.GetGlobalCoordinate(new Vect3(x*voxelGrid.resolution, y*voxelGrid.resolution, z*voxelGrid.resolution));

                    cubePos = Utilities.GetScaledTransletedVect3(cubePos, structureMiddlePoint, scalingFactor);
                    GameObject cubeObject = Utilities.DrawObject(cubePrefab, cubePos, voxelGridObject);
                    Vector3 scale = cubeObject.transform.localScale;

                    scale.x = scale.x * ((float) voxelGrid.resolution / scalingFactor);
                    scale.y = scale.y * ((float) voxelGrid.resolution / scalingFactor);
                    scale.z = scale.z * ((float) voxelGrid.resolution / scalingFactor);

                    cubeObject.name = string.Format("Cube {0}", "{" + x + ", " + y + ", " + z + "}");

                    cubeObject.transform.localScale = scale;

                    var rotation = Quaternion.LookRotation(new Vector3((float)-voxelGrid.orientedBbox.localZ.x,
                                                                        (float)voxelGrid.orientedBbox.localZ.y,
                                                                        (float)voxelGrid.orientedBbox.localZ.z),
                                                            new Vector3((float)-voxelGrid.orientedBbox.localY.x,
                                                                        (float)voxelGrid.orientedBbox.localY.y,
                                                                        (float)voxelGrid.orientedBbox.localY.z));
                    cubeObject.transform.rotation = rotation;
                }
            }
        }
    }

    public void DrawOBB(OrientedBBox oBB, DrawOBBs drawOBBs, Vect3 structureMiddlePoint)
    {
        if (drawOBBs.oBBDrawType == OBBDrawType.lines)
        {
            DrawLine(oBB.localOrigin, oBB.localMaxX, structureMiddlePoint);
            DrawLine(oBB.localOrigin, oBB.localMaxY, structureMiddlePoint);
            DrawLine(oBB.localOrigin, oBB.localMaxZ, structureMiddlePoint);

            DrawLine(oBB.localMaxYZ, oBB.localMaxY, structureMiddlePoint);
            DrawLine(oBB.localMaxYZ, oBB.localMaxZ, structureMiddlePoint);
            DrawLine(oBB.localMaxYZ, oBB.localMaxXYZ, structureMiddlePoint);

            DrawLine(oBB.localMaxXY, oBB.localMaxX, structureMiddlePoint);
            DrawLine(oBB.localMaxXY, oBB.localMaxY, structureMiddlePoint);
            DrawLine(oBB.localMaxXY, oBB.localMaxXYZ, structureMiddlePoint);

            DrawLine(oBB.localMaxXZ, oBB.localMaxX, structureMiddlePoint);
            DrawLine(oBB.localMaxXZ, oBB.localMaxZ, structureMiddlePoint);
            DrawLine(oBB.localMaxXZ, oBB.localMaxXYZ, structureMiddlePoint);
        }
        else
        {
            foreach (var point in oBB.allPoints)
            {
                DrawPoint(point, structureMiddlePoint);
            }
        }
    }

    public void DrawUnscaledVoxelGrid(VoxelGrid voxelGrid)
    {
        GameObject voxelGridObject = new GameObject(string.Format("VoxelGrid {0}, Vertices", 0));

        for (int x = 0; x < voxelGrid.coordinateGrid.Length; x++)
        {
            for (int y = 0; y < voxelGrid.coordinateGrid[x].Length; y++)
            {
                for (int z = 0; z < voxelGrid.coordinateGrid[x][y].Length; z++)
                {
                    if (!voxelGrid.coordinateGrid[x][y][z]) continue;
                    Vect3 cubePos = voxelGrid.voxelStartCoordinate + new Vect3(x*voxelGrid.resolution, y*voxelGrid.resolution, z*voxelGrid.resolution);

                    GameObject vertexObject = Utilities.DrawObject(cubePrefab, cubePos, voxelGridObject);
                    Vector3 scale = vertexObject.transform.localScale;

                    scale.x = scale.x * ((float) voxelGrid.resolution);
                    scale.y = scale.y * ((float) voxelGrid.resolution);
                    scale.z = scale.z * ((float) voxelGrid.resolution);

                    vertexObject.transform.localScale = scale;
                }
            }
        }
    }

    public void DrawNodeGraph(DrawNodeGraph choices, NodeGraph nodeGraph, Vect3 structureMiddlePoint)
    {
        GameObject nodeGraphObject = new GameObject("NodeGraph" + (nodeCounter++).ToString());

        if (choices.drawNodes)
        {
            GameObject nodesObject = new GameObject("Nodes");
            nodesObject.transform.parent = nodeGraphObject.transform;
            for (int nodeIndex = 0; nodeIndex < nodeGraph.Nodes.Count; nodeIndex++)
            {
                Node node = nodeGraph.GetNode(nodeIndex);
                Vect3 nodePosition = Utilities.GetScaledTransletedVect3(node.coordinate, structureMiddlePoint, scalingFactor);
                GameObject nodeObject = Utilities.DrawObject(nodePrefab, nodePosition, nodesObject);
                nodeObject.name = string.Format("Node {0}", nodeIndex);
            }
        }

        if (choices.drawEdges)
        {
            GameObject edgesObject = new GameObject("Edges");
            edgesObject.transform.parent = nodeGraphObject.transform;
            for (int edgeIndex = 0; edgeIndex < nodeGraph.Edges.Count; edgeIndex++)
            {
                Edge edge = nodeGraph.GetEdge(edgeIndex);
                Node startNode = nodeGraph.GetNode(edge.nodeIndex1);
                Node endNode = nodeGraph.GetNode(edge.nodeIndex2);
                Vect3 edgeStart = Utilities.GetScaledTransletedVect3(startNode.coordinate, structureMiddlePoint, scalingFactor);
                Vect3 edgeEnd = Utilities.GetScaledTransletedVect3(endNode.coordinate, structureMiddlePoint, scalingFactor);
                GameObject edgeObject;

                if (choices.drawAsTunnels)
                {
                    edgeObject = Utilities.DrawTunnelLine(edgeStart, edgeEnd, scalingFactor, (float) edge.width, (float) edge.height, edgesObject);
                }
                else
                {
                    edgeObject = Utilities.DrawLine(edgeStart, edgeEnd, edgesObject);
                }
                edgeObject.name = string.Format("Edge {0}", edgeIndex);
                Utilities.ColorObject(edgeObject, Color.cyan);
            }
        }
    }

    public void DrawLine(Vect3 startPoint, Vect3 endPoint, Vect3 structureMiddlePoint)
    {
        GameObject linesObject = new GameObject("LineParent");

        Vect3 lineStart = Utilities.GetScaledTransletedVect3(startPoint, structureMiddlePoint, scalingFactor);
        Vect3 lineEnd = Utilities.GetScaledTransletedVect3(endPoint, structureMiddlePoint, scalingFactor);
        GameObject edgeObject = Utilities.DrawLine(lineStart, lineEnd, linesObject);
        edgeObject.name = string.Format("Line {0}", 0);
        Utilities.ColorObject(edgeObject, Color.red);
    }

    public void DrawPoint(Vect3 point, Vect3 structureMiddlePoint)
    {
        GameObject pointsObject = new GameObject("Points");

        Vect3 pointPosition = Utilities.GetScaledTransletedVect3(point, structureMiddlePoint, scalingFactor);
        GameObject pointObject = Utilities.DrawObject(bBoxPrefab, pointPosition, pointsObject);
        pointObject.name = string.Format("Node");
    }

    public void DrawCollisions(List<NodeGraph> componentGraphs, Vect3 structureMiddlePoint)
    {
        for (int i = 0; i < componentGraphs.Count; i++)
        {
            NodeGraph outerGraph = componentGraphs[i];
            for (int j = i + 1; j < componentGraphs.Count; j++)
            {
                NodeGraph innerGraph = componentGraphs[j];
                foreach (Edge outerEdge in outerGraph.Edges)
                {
                    foreach (Edge innerEdge in innerGraph.Edges)
                    {
                        if (GraphUnifier.EdgesCollide(outerEdge, innerEdge, outerGraph, innerGraph))
                        {
                            Vect3 collsionPoint = GraphUnifier.GetEdgesCollidingPoint(outerEdge, innerEdge, outerGraph, innerGraph);
                            if (collsionPoint != null)
                            {
                                GameObject o = Utilities.DrawObject(collisionPrefab,
                                Utilities.GetScaledTransletedVect3(collsionPoint,
                                structureMiddlePoint, scalingFactor));
                                o.name = "Graph " + i + ": Edge " + outerEdge.index + ", Graph " + j + ": Edge " + innerEdge.index;
                            }
                        }
                    }
                }
            }
        }
    }
}
