using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPortal
{
    public Transform Transform { get; }
    public Material Material { get; }
    public Plane Plane { get; }
    public Bounds Bounds { get; }
    public Vector3 Dimensions { get; }
    public Vector3[] Vertices { get; }
    public IPortal OtherPortal { get; }
    // these factors are what transformations should be applied to a point
    // to get the local position in the other portal
    public Vector3 TranslationFactor { get; }
    public Quaternion RotationFactor { get; }
    public float ScaleFactor { get; }
    public Matrix4x4 ThisToOtherMatrix { get; }

    public void SetOtherPortal(IPortal otherPortal) { }

    public Vector4 PlaneRepresentation()
    {
        return new Vector4(-Plane.normal.x, -Plane.normal.y, -Plane.normal.z, -Plane.distance);
    }

    /// <summary>
    /// What a position should be after being teleported
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector3 TeleportPosition(Vector3 position)
    {
        //return otherPortal.transform.position + (rotationFactor * (transform.rotation * position) * scaleFactor);
        return ThisToOtherMatrix.MultiplyPoint(position);
    }

    /// <summary>
    /// What the rotation should be after being teleported
    /// </summary>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public Quaternion TeleportRotation(Quaternion rotation)
    {
        return RotationFactor * rotation;
    }

    /// <summary>
    /// What the scale should be after being teleported
    /// </summary>
    /// <param name="scale"></param>
    /// <returns></returns>
    public Vector3 TeleportScale(Vector3 scale)
    {
        return scale * ScaleFactor;
    }
    public float TeleportScale(float scale)
    {
        return scale * ScaleFactor;
    }

    /// <summary>
    /// What a direction should be after being teleported
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public Vector3 TeleportDirection(Vector3 direction)
    {
        return (RotationFactor * direction).normalized;
    }

    /// <summary>
    /// What a vector should be after being teleported (doesnt account for translation)
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public Vector3 TeleportVector(Vector3 vector)
    {
        return RotationFactor * vector * ScaleFactor;
    }

    /// <summary>
    /// If the point is in the 2D projected bounds of the portal
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool InBounds(Vector3 position)
    {
        // local position doesnt incorporate scale because the bounds checks already do
        Vector3 localPosition = Quaternion.Inverse(Transform.rotation) * (position - Transform.position);

        return Mathf.Abs(localPosition.x) <= (Dimensions.x / 2) && Mathf.Abs(localPosition.y) <= (Dimensions.y / 2);
    }

    /// <summary>
    /// Determines if a ray would hit the portal's plane
    /// </summary>
    /// <param name="ray"></param>
    /// <param name="maxDistance"></param>
    /// <param name="portalHit"></param>
    /// <returns></returns>
    public bool Raycast(Ray ray, float maxDistance, out PortalHit portalHit)
    {
        portalHit = null;
        float hitDistance = 0f;
        Vector3 hitPosition = Vector3.zero;
        // only detects collision from one side of the portal
        // might need to flip which side matters
        if (Plane.GetSide(ray.origin))
        {
            if (Plane.Raycast(ray, out hitDistance))
            {
                // collision only counts if its within the distance range
                if(hitDistance > maxDistance)
                {
                    return false;
                }
                else
                {
                    // collision only counts if its is in the portal's bounds
                    hitPosition = ray.origin + (ray.direction * hitDistance);
                    if (InBounds(hitPosition))
                    {
                        portalHit = new PortalHit(this, hitDistance, hitPosition);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    public bool ShouldTeleport(Vector3 from, Vector3 to)
    {
        float maxDist = Vector3.Distance(from, to);
        Ray ray = new Ray(
            from,
            (to - from).normalized
            );

        // only teleport if on the correct side of the portal
        if (Plane.GetSide(ray.origin))
        {
            if (Plane.Raycast(ray, out float distance))
            {
                // collision only counts if its within the distance range
                if (distance <= maxDist)
                {
                    Vector3 hitPosition = ray.origin + (ray.direction * distance);
                    if (InBounds(hitPosition))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}

