using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PortalCollectionMethods
{
    /// <summary>
    /// Raycasts through multiple portals, able to form raycast connections between them.
    /// </summary>
    /// <param name="portals"></param>
    /// <param name="ray"></param>
    /// <param name="maxDist"></param>
    /// <param name="layerMask">Used to detects obstructing a portal</param>
    /// <param name="portalLink"></param>
    /// <param name="hit"></param>
    /// <returns></returns>
    public static bool RaycastAll(IEnumerable<IPortal> portals, Ray ray, float maxDist, LayerMask layerMask, out PortalLink portalLink, out RaycastHit hit)
    {
        //Debug.DrawRay(ray.origin, ray.direction * maxDist, Color.red);
        hit = new RaycastHit();
        portalLink = null;

        PortalHit savedPortalaHit = new PortalHit();
        foreach (IPortal portal in portals)
        {
            if (portal.Raycast(ray, maxDist, out PortalHit portalHit))
                if (portalHit.distance < savedPortalaHit.distance)
                    savedPortalaHit = portalHit;
        }

        RaycastHit objHit;
        // check if you collided with an object before the portal
        if (Physics.Raycast(ray, out objHit, layerMask))
        {
            if (objHit.distance <= maxDist)
            {
                if (objHit.distance < savedPortalaHit.distance)
                {
                    hit = objHit;
                    return true;
                }
            }
        }

        // if we hit a portal and nothing else, call the function recursively
        if (savedPortalaHit.portal != null)
        {
            // check next portal
            PortalLink childLink = null;
            RaycastHit childHit;
            /*
            Debug.DrawRay(savedPortalaHit.portal.TeleportPosition(savedPortalaHit.position),
                savedPortalaHit.portal.TeleportDirection(ray.direction) * savedPortalaHit.portal.TeleportScale(
                    maxDist *
                    (1 -
                    (savedPortalaHit.distance / maxDist)
                    )
                ),
                Color.blue);;
            */

            bool rayCast = RaycastAll(
                portals,
                new Ray(
                savedPortalaHit.portal.TeleportPosition(savedPortalaHit.position),
                savedPortalaHit.portal.TeleportDirection(ray.direction)
                ),
                savedPortalaHit.portal.TeleportScale(
                    maxDist *
                    (1 -
                    (savedPortalaHit.distance / maxDist)
                    )
                ),
                layerMask,
                out childLink,
                out childHit
                );

            portalLink = new PortalLink(
                savedPortalaHit.portal,
                childLink // this can be null
                );
            hit = childHit;
            return rayCast;
        }

        // nothing was hit and no portals were passed
        return false;
    }

}
