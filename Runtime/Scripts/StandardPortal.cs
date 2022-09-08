using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardPortal : IPortal
{
    private Transform transform;
    public Transform Transform { get => transform; }
    private Material material;
    public Material Material { get => material; }
    private Plane plane;
    public Plane Plane { get => plane; }
    private Bounds bounds;
    public Bounds Bounds { get => bounds; }
    private Vector3 dimensions;
    public Vector3 Dimensions { get => dimensions; }
    private Vector3[] vertices;
    public Vector3[] Vertices { get => vertices; }

    private IPortal otherPortal = null;
    public IPortal OtherPortal { get => otherPortal; }

    private Vector3 translationFactor;
    public Vector3 TranslationFactor { get => translationFactor; }

    private Quaternion rotationFactor;
    public Quaternion RotationFactor { get => rotationFactor; }

    private float scaleFactor;
    public float ScaleFactor { get => scaleFactor; }

    private Matrix4x4 thisToOtherMatrix;
    public Matrix4x4 ThisToOtherMatrix { get => thisToOtherMatrix; }

    public StandardPortal(
    Transform transform,
    Material material,
    Plane plane,
    Bounds bounds,
    Vector3 dimensions,
    Vector3[] vertices
    )
    {
        this.transform = transform;
        this.material = material;
        this.plane = plane;
        this.bounds = bounds;
        this.dimensions = dimensions;
        this.vertices = vertices;
    }

    public StandardPortal(
        PortalPlane portalPlane
    )
    {
        this.transform = portalPlane.transform;
        this.material = portalPlane.MeshRenderer.material;
        this.plane = portalPlane.Plane;
        this.bounds = portalPlane.MeshRenderer.bounds;
        this.dimensions = portalPlane.Dimensions;
        this.vertices = portalPlane.WorldSpaceVertices;
    }

    public void SetOtherPortal(IPortal otherPortal)
    {
        if (this.otherPortal == null && otherPortal != null)
        {
            this.otherPortal = otherPortal;

            translationFactor = otherPortal.Transform.position - transform.position;

            rotationFactor =
                Quaternion.AngleAxis(
                    180,
                    otherPortal.Transform.up
                )
                *
                (
                    otherPortal.Transform.rotation *
                    Quaternion.Inverse(transform.rotation)
                );
            /*
            scaleFactor = new Vector3(
                otherPortal.transform.lossyScale.x / transform.lossyScale.x,
                otherPortal.transform.lossyScale.y / transform.lossyScale.y,
                otherPortal.transform.lossyScale.z / transform.lossyScale.z
                );
            */
            scaleFactor = otherPortal.Transform.lossyScale.x / transform.lossyScale.x;

            thisToOtherMatrix = transform.worldToLocalMatrix;
            thisToOtherMatrix = Matrix4x4.Scale(new Vector3(-1, 1, -1)) * thisToOtherMatrix;
            thisToOtherMatrix = otherPortal.Transform.localToWorldMatrix * thisToOtherMatrix;
        }
    }
}
