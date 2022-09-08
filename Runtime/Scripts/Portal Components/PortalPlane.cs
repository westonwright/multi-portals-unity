using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class PortalPlane : MonoBehaviour
{
    private MeshFilter meshFilter = null;
    public MeshFilter MeshFilter
    {
        get
        {
            if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
            return meshFilter;
        }
    }
    private MeshRenderer meshRenderer = null;
    public MeshRenderer MeshRenderer
    {
        get
        {
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
            return meshRenderer;
        }
    }

    private Vector3[] worldSpaceVertices;
    public Vector3[] WorldSpaceVertices 
    { get
        {
            if(worldSpaceVertices == null)
            {
                Mesh portalMesh = MeshFilter.sharedMesh;

                Vector3[] vertices = new Vector3[8];
                Vector3 min = portalMesh.bounds.min;
                Vector3 max = portalMesh.bounds.max;
                vertices[0] = new Vector3(min.x, min.y, min.z);
                vertices[1] = new Vector3(min.x, min.y, max.z);
                vertices[2] = new Vector3(min.x, max.y, min.z);
                vertices[3] = new Vector3(min.x, max.y, max.z);
                vertices[4] = new Vector3(max.x, min.y, min.z);
                vertices[5] = new Vector3(max.x, min.y, max.z);
                vertices[6] = new Vector3(max.x, max.y, min.z);
                vertices[7] = new Vector3(max.x, max.y, max.z);
                Vector3[] worldVertices = new Vector3[8];
                for (int j = 0; j < vertices.Length; j++)
                {
                    worldVertices[j] = transform.position + transform.rotation *
                        new Vector3(
                            vertices[j].x * transform.lossyScale.x,
                            vertices[j].y * transform.lossyScale.y,
                            vertices[j].z * transform.lossyScale.z);

                }
                worldSpaceVertices = worldVertices;
            }
            return worldSpaceVertices;
        }
    }
    
    public Plane Plane { get => new Plane(transform.forward, transform.position); }

    private Vector3 dimensions;
    public Vector3 Dimensions
    {
        get
        {
            if(dimensions == Vector3.zero)
            {
                Mesh portalMesh = MeshFilter.sharedMesh;

                dimensions = new Vector3(
                    portalMesh.bounds.size.x * transform.lossyScale.x,
                    portalMesh.bounds.size.y * transform.lossyScale.y,
                    portalMesh.bounds.size.z * transform.lossyScale.z
                    );
            }
            return dimensions;
        }
    }
    // add parent removal protection like on other objects
}
