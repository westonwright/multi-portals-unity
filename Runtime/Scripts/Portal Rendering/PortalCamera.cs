using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    [SerializeField]
    private Camera renderCamera;

    public RenderTexture targetTexture
    {
        get
        {
            if(renderCamera != null)
            {
                return renderCamera.targetTexture;
            }
            return null;
        }
        set 
        {
            if(renderCamera != null)
            {
                renderCamera.targetTexture = value;
            }
        } 
    }

    void Awake()
    {
        renderCamera.fieldOfView = Camera.main.fieldOfView;
    }

    public RenderTexture RenderPortalCameraToTexture(PortalRenderingComponents portalRenderingComponents, float quality)
    {
        /*
        Color rayColor = Color.red;
        Debug.Log(portalRenderingComponents.depth);
        rayColor.r = 1f / (portalRenderingComponents.depth + 1);
        Debug.DrawRay(renderCamera.transform.position, renderCamera.transform.rotation * (Vector3.forward * 5), rayColor);
        */

        renderCamera.transform.position = portalRenderingComponents.cameraPosition;
        renderCamera.transform.rotation = portalRenderingComponents.cameraRotation;
        

        // set near clippling plane parallel to and at location of portal
        Vector4 clippingPlaneWorldSpace = new Vector4(
            portalRenderingComponents.renderingPortal.OtherPortal.Plane.normal.x,
            portalRenderingComponents.renderingPortal.OtherPortal.Plane.normal.y,
            portalRenderingComponents.renderingPortal.OtherPortal.Plane.normal.z,
            portalRenderingComponents.renderingPortal.OtherPortal.Plane.distance
            );

        Vector4 clippingPlaneCameraSpace = Matrix4x4.Transpose(renderCamera.cameraToWorldMatrix) * clippingPlaneWorldSpace;
        renderCamera.projectionMatrix = renderCamera.CalculateObliqueMatrix(clippingPlaneCameraSpace);

        // reduce rendering quality
        RenderTexture rt = RenderTexture.GetTemporary(Mathf.RoundToInt(Screen.width * quality), Mathf.RoundToInt(Screen.height * quality), 24);

        renderCamera.targetTexture = rt;
        // TODO: Determine what causes errors here
        renderCamera.Render();
        
        renderCamera.ResetProjectionMatrix();

        return rt;
    }

    // detects if the main camera can see a portal. Used for initial pass
    public bool CheckParentPortalVisibility(IPortal checkPortal, float portalRenderDistance)
    {
        //Debug.DrawRay(currentCamera.transform.position, currentCamera.transform.forward * portalRenderDistance, Color.red);
        //could edit this to take scale into acount when determining if portal is too far away to render.
        if (!(Vector3.Distance(Camera.main.transform.position, checkPortal.Transform.position) < portalRenderDistance))
        {
            return false;
        }
        Vector3 playerLocalDirection = (Camera.main.transform.position - checkPortal.Transform.position).normalized;
        // change to dot product
        float playerLocalAngle = Vector3.Angle(checkPortal.Transform.forward, playerLocalDirection);
        //checks if portal is rotated the right direction to be seen
        if (!(playerLocalAngle < 90))
        {
            return false;
        }

        if (checkPortal.Transform.GetComponent<MeshRenderer>().IsVisibleFrom(Camera.main))
        {
            //Debug.Log(Vector3.Angle(-checkPortal.currentPortal.forward, renderCamera.transform.forward));
            if (Vector3.Angle(-checkPortal.Transform.forward, Camera.main.transform.forward) > 135)
            {
                return false;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Determines if 'check portal' is visible from the portal being rendered
    /// </summary>
    /// <param name="portalRenderingComponents"></param>
    /// <param name="checkPortal"></param>
    /// <param name="portalRenderDistance"></param>
    /// <returns></returns>
    public bool CheckChildPortalVisibility(PortalRenderingComponents portalRenderingComponents, IPortal checkPortal, float portalRenderDistance)
    {
        IPortal viewingPortal = portalRenderingComponents.renderingPortal;

        renderCamera.transform.position = portalRenderingComponents.cameraPosition;
        renderCamera.transform.rotation = portalRenderingComponents.cameraRotation;

        //Debug.DrawRay(currentCamera.transform.position, currentCamera.transform.forward * portalRenderDistance, Color.red);
        //could edit this to take scale into acount when determining if portal is too far away to render.
        if (!(Vector3.Distance(renderCamera.transform.position, checkPortal.Transform.position) < portalRenderDistance))
        {
            return false;
        }
        Vector3 playerLocalDirection = (renderCamera.transform.position - checkPortal.Transform.position).normalized;
        // change to dot product
        float playerLocalAngle = Vector3.Angle(checkPortal.Transform.forward, playerLocalDirection);
        //checks if portal is rotated the right direction to be seen
        if (!(playerLocalAngle < 90))
        {
            return false;
        }

        // This skips over the next portion if we are too close to the portal
        // because being too close can cause false negatives
        // TODO: Find a better solution for this
        //Debug.DrawRay(viewingPortal.OtherPortal.Transform.position, Vector3.up * 5, Color.green);
        //Debug.Log(viewingPortal.OtherPortal.Transform.InverseTransformPoint(renderCamera.transform.position).z);
        if (viewingPortal.OtherPortal.Transform.InverseTransformPoint(renderCamera.transform.position).z > -1) return true;

        //starts calculating if portal is visible within portal.
        float lowestX = Screen.width;
        float highestX = 0;
        float lowestY = Screen.height;
        float highestY = 0;

        float trueLowestX = Screen.width;
        float trueHighestX = 0;
        float trueLowestY = Screen.height;
        float trueHighestY = 0;

        float lastLowestX = trueLowestX;
        float lastHighestX = trueHighestX;
        float lastLowestY = trueLowestY;
        float lastHighestY = trueHighestY;

        float lowXZ = 0;
        float highXZ = 0;
        float lowYZ = 0;
        float highYZ = 0;

        foreach (Vector3 vertex in viewingPortal.OtherPortal.Vertices)
        {
            //might want to account for if player is inside of the vertex box due to portal being rotated.
            //Debug.DrawLine(vertex, vertex + Vector3.up, Color.yellow);
            Vector3 screenPos = renderCamera.WorldToScreenPoint(vertex);
            // if very close to the portal, some valid vertices might be behind the camera
            lowestX = screenPos.x < lowestX ? (screenPos.z < 0 ? lowestX : screenPos.x) : lowestX;
            highestX = screenPos.x > highestX ? (screenPos.z < 0 ? highestX : screenPos.x) : highestX;
            lowestY = screenPos.y < lowestY ? (screenPos.z < 0 ? lowestY : screenPos.y) : lowestY;
            highestY = screenPos.y > highestY ? (screenPos.z < 0 ? highestY : screenPos.y) : highestY;

            trueLowestX = screenPos.x < trueLowestX ? screenPos.x : trueLowestX;
            trueHighestX = screenPos.x > trueHighestX ? screenPos.x : trueHighestX;
            trueLowestY = screenPos.y < trueLowestY ? screenPos.y : trueLowestY;
            trueHighestY = screenPos.y > trueHighestY ? screenPos.y : trueHighestY;

            lowXZ = trueLowestX != lastLowestX || lowXZ == 0 ? screenPos.z : lowXZ;
            highXZ = trueHighestX != lastHighestX || highXZ == 0 ? screenPos.z : highXZ;
            lowYZ = trueLowestY != lastLowestY || lowYZ == 0 ? screenPos.z : lowYZ;
            highYZ = trueHighestY != lastHighestY || highYZ == 0 ? screenPos.z : highYZ;

            lastLowestX = trueLowestX;
            lastHighestX = trueHighestX;
            lastLowestY = trueLowestY;
            lastHighestY = trueHighestY;
        }
        if (lowXZ < 0)
        {
            highestX = Screen.width;
        }
        if (highXZ < 0)
        {
            lowestX = 0;
        }
        if (lowYZ < 0)
        {
            highestY = Screen.height;
        }
        if (highYZ < 0)
        {
            lowestY = 0;
        }
        lowestX = lowestX < 0 ? 0 : lowestX;
        lowestY = lowestY < 0 ? 0 : lowestY;
        highestX = highestX > Screen.width ? Screen.width : highestX;
        highestY = highestY > Screen.height ? Screen.height : highestY;
        if (lowestX == highestX || lowestY == highestY)
        {
            return false;
        }

        Plane[] planes = new Plane[6];
        planes[0] = new Plane(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(lowestX, highestY, portalRenderDistance)), renderCamera.ScreenToWorldPoint(new Vector3(lowestX, lowestY, portalRenderDistance)));
        planes[1] = new Plane(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(highestX, lowestY, portalRenderDistance)), renderCamera.ScreenToWorldPoint(new Vector3(highestX, highestY, portalRenderDistance)));
        planes[2] = new Plane(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(lowestX, lowestY, portalRenderDistance)), renderCamera.ScreenToWorldPoint(new Vector3(highestX, lowestY, portalRenderDistance)));
        planes[3] = new Plane(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(highestX, highestY, portalRenderDistance)), renderCamera.ScreenToWorldPoint(new Vector3(lowestX, highestY, portalRenderDistance)));
        planes[4] = new Plane(renderCamera.transform.forward, renderCamera.transform.position);
        planes[5] = new Plane(-renderCamera.transform.forward, 100);

        /*
        Debug.DrawLine(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(lowestX, lowestY, portalRenderDistance)));
        Debug.DrawLine(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(lowestX, highestY, portalRenderDistance)));
        Debug.DrawLine(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(highestX, lowestY, portalRenderDistance)));
        Debug.DrawLine(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(highestX, highestY, portalRenderDistance)));
        */
        if (GeometryUtility.TestPlanesAABB(planes, checkPortal.Bounds))
        {
            //Debug.Log("In Bounds");
            return true;
        }
        return false;
    }
}
