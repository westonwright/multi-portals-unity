using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal
{
    public Transform portalTransform;
    public Material portalMaterial;
    public Plane portalPlane;
    public Bounds portalBounds;
    public Vector3 portalDimensions;
    public Vector3[] portalVertices;
    public Portal otherPortal;
    public float portalScaleFactor;
    public Quaternion portalRotationFactor;
}