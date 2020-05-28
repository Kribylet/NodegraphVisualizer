using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nodegraph_Generator;

public static class Utilities
{
    public static Vector3 GetVector3FromVect3(Vect3 vect3)
    {
        // -X because Unity has X defined to the left.
        return new Vector3(-(float)vect3.x, (float)vect3.y, (float)vect3.z);
    }

    public static GameObject DrawObject(GameObject o, Vect3 pos, GameObject parent = null)
    {
        GameObject visualObject;
        if (parent)
        {
            visualObject = Object.Instantiate(o, parent.transform);

        }
        else
        {
            visualObject = Object.Instantiate(o);

        }
        visualObject.transform.position = GetVector3FromVect3(pos);
        return visualObject;
    }

    public static GameObject DrawLine(Vect3 startPos, Vect3 endPos, GameObject parent = null)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (parent)
        {
            cylinder.transform.parent = parent.transform;
        }

        Vect3 localVect = startPos - endPos;
        cylinder.transform.position = GetVector3FromVect3(Vect3.MiddlePoint(startPos, endPos));
        cylinder.transform.localScale = new Vector3(0.05f, (float)localVect.Length()/2, 0.05f);
        Vector3 crossedVector = Vector3.Cross(GetVector3FromVect3(localVect), cylinder.transform.right);
        if (crossedVector == Vector3.zero) crossedVector = Vector3.Cross(GetVector3FromVect3(localVect), cylinder.transform.forward);
        cylinder.transform.rotation = Quaternion.LookRotation(crossedVector,
                                                                GetVector3FromVect3(localVect));
        return cylinder;
    }

    public static GameObject DrawTunnelLine(Vect3 startPos, Vect3 endPos, float scalingFactor, float width, float height, GameObject parent = null)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (parent)
        {
            cylinder.transform.parent = parent.transform;
        }

        startPos.y += height/(2f*scalingFactor);
        endPos.y += height/(2f*scalingFactor);

        Vect3 localVect = startPos - endPos;
        cylinder.transform.position = GetVector3FromVect3(Vect3.MiddlePoint(startPos, endPos));
        cylinder.transform.localScale = new Vector3(width/scalingFactor, (float)localVect.Length()/2, height/scalingFactor);
        Vector3 crossedVector = Vector3.Cross(GetVector3FromVect3(localVect), cylinder.transform.right);
        if (crossedVector == Vector3.zero) crossedVector = Vector3.Cross(GetVector3FromVect3(localVect), cylinder.transform.forward);
        cylinder.transform.rotation = Quaternion.LookRotation(crossedVector,
                                                                GetVector3FromVect3(localVect));
        return cylinder;
    }

    public static void ColorObject(GameObject o, Color c)
    {
        var renderer = o.GetComponent<Renderer>();
        renderer.material.SetColor("_Color", c);
    }

    public static Vect3 GetStructureMiddlePoint(Structure structure)
    {
        Vect3 totVect3 = new Vect3();
        int vectCount = 0;
        foreach (Nodegraph_Generator.Component component in structure.components)
        {
            foreach (Vertex vertex in component.vertices)
            {
                totVect3 += vertex.coordinate;
                vectCount++;
            }
        }
        return totVect3 / vectCount;
    }

    public static Vect3 GetScaledTransletedVect3(Vect3 v, Vect3 middlePoint, double divScaler)
    {
        return (v - middlePoint) / divScaler;
    }

}
