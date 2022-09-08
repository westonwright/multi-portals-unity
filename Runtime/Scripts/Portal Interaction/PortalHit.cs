using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PortalHit
{
    IPortal portal_m;
    public IPortal portal { get => portal_m; }

    float distance_m;
    public float distance { get => distance_m; }

    Vector3 position_m;
    public Vector3 position { get => position_m; }
    public PortalHit(IPortal portal, float distance, Vector3 position)
    {
        portal_m = portal;
        distance_m = distance;
        position_m = position;
    }
    public PortalHit()
    {
        portal_m = null;
        distance_m = Mathf.Infinity;
        position_m = Vector3.zero;
    }
}