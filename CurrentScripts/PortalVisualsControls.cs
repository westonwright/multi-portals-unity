using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System;

public class PortalVisualsControls : MonoBehaviour
{
    PortalManager portalManager;
    PortalRenderingComponents playerCameraRenderingComponents = new PortalRenderingComponents();
    Camera renderCamera;

    //0 means portals won't render anything through themselves
    //max depth has not been defined yet but will probably be around 5
    private int portalRenderDepth = 3;
    //1 means each layer of portals being rendered will use the same resolution
    private float portalQualityDropoff = 1;
    //how close a portal has to be for it to be rendered
    private float portalRenderDistance = 100;
    private List<PortalRenderingComponents> portalRenderingComponents = new List<PortalRenderingComponents>();


    void Start()
    {
        //could probably get rid of camera instantiation and just have a permanant one on the scene
        portalManager = FindObjectOfType<PortalManager>();
        portalRenderDepth = portalRenderDepth < 0 ? 0 : portalRenderDepth;
        GameObject newCameraObject = Instantiate(Resources.Load("PortalCamera"), transform) as GameObject;
        newCameraObject.GetComponent<PortalRenderCamera>().renderDepth = portalRenderDepth;
        newCameraObject.GetComponent<PortalRenderCamera>().qualityDropoff = portalQualityDropoff;
        renderCamera = newCameraObject.GetComponent<Camera>();
        //renderCamera.cullingMask = (1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer(i < portalRenderDepth ? "RenderDepth" + (i) : "RenderDepthEnd"));

        playerCameraRenderingComponents.depth = 0;
    }

    void Update()
    {
        portalRenderingComponents = new List<PortalRenderingComponents>();
        //don't include playerCameraRenderingComponents in portalRenderingComponents list because player camera is rendered seperately
        playerCameraRenderingComponents.cameraPosition = Camera.main.transform.position;
        playerCameraRenderingComponents.cameraRotation = Camera.main.transform.rotation;
        //portal pass calls itself to loop through all cameras that need to be rendered
        PortalPass(0, playerCameraRenderingComponents);


        //need to add section that makes the portals fade in from white correctly.



        //renders cameras in order from deepest camera to main camera
        //portalRenderingComponents.Sort((x, y) => y.depth.CompareTo(x.depth));
        Vector3 CurrentCameraPosition;
        Vector3 LastCameraPosition = Camera.main.transform.position;
        float pathStep = 1f / (portalRenderingComponents.Count > 1 ? portalRenderingComponents.Count - 1 : 1);
        float pathLevel = 0;
        Color pathColor = new Color(pathLevel, 1, pathLevel);
        int count = 0;
        foreach (PortalRenderingComponents portalRendComps in portalRenderingComponents)
        {
            //Debug.Log("Rendering");
            renderCamera.GetComponent<PortalRenderCamera>().RenderCamera(portalRendComps);
            CurrentCameraPosition = portalRendComps.cameraPosition;
            Debug.DrawLine(LastCameraPosition, CurrentCameraPosition + (Vector3.up * count), pathColor);
            LastCameraPosition = CurrentCameraPosition;
            pathLevel += pathStep;
            pathColor = new Color(pathLevel, 1, pathLevel);
            count++;
        }
        renderCamera.GetComponent<PortalRenderCamera>().SetMaterials(portalRenderingComponents);

        renderCamera.GetComponent<PortalRenderCamera>().ReleaseTextures();

        if (Input.GetKeyDown(KeyCode.T))
        {
            EditorApplication.isPaused = true;
        }
    }

    private void PortalPass(int renderPass, PortalRenderingComponents currentRenderingComponents)
    {
        if (renderPass < portalRenderDepth)
        {
            foreach (Portal portal in portalManager.portals)
            {
                renderCamera.transform.position = currentRenderingComponents.cameraPosition;
                renderCamera.transform.rotation = currentRenderingComponents.cameraRotation;
                if (CalculateVisibility(portal, currentRenderingComponents.renderingPortal, renderPass))
                {
                    PortalRenderingComponents newRenderingComponents = new PortalRenderingComponents();
                    newRenderingComponents.depth = renderPass + 1;
                    //newRenderingComponents.renderingPortal = CreateNewPortal(portal.currentPortal.gameObject);
                    newRenderingComponents.renderingPortal = portal;
                    newRenderingComponents.parentRenderingComponents = currentRenderingComponents;
                    newRenderingComponents = CalculateCameraTransform(newRenderingComponents);
                    portalRenderingComponents.Insert(0, newRenderingComponents);
                    //portalRenderingComponents.Add(newRenderingComponents);
                    PortalPass(renderPass + 1, newRenderingComponents);
                }
            }
        }
    }

    private bool CalculateVisibility(Portal checkPortal, Portal viewingPortal, int pass)
    {
        //Debug.DrawRay(currentCamera.transform.position, currentCamera.transform.forward * portalRenderDistance, Color.red);
        //could edit this to take scale into acount when determining if portal is too far away to render.
        if(!(Vector3.Distance(renderCamera.transform.position, checkPortal.portalTransform.position) < portalRenderDistance))
        {
            return false;
        }
        Vector3 playerLocalDirection = (renderCamera.transform.position - checkPortal.portalTransform.position).normalized;
        float playerLocalAngle = Vector3.Angle(checkPortal.portalTransform.forward, playerLocalDirection);
        //checks if portal is rotated the right direction to be seen
        if(!(playerLocalAngle < 90))
        {
            return false;
        }
        //starts calculating if portal is visible within portal.
        if(pass == 0)
        {
            if (checkPortal.portalTransform.GetComponent<MeshRenderer>().IsVisibleFrom(renderCamera))
            {
                //Debug.Log(Vector3.Angle(-checkPortal.currentPortal.forward, renderCamera.transform.forward));
                if(Vector3.Angle(-checkPortal.portalTransform.forward, renderCamera.transform.forward) > 135)
                {
                    return false;
                }
                return true;
            }
            return false;
        }
        else
        {
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

            foreach (Vector3 vertex in viewingPortal.otherPortal.portalVertices)
            {
                //might want to account for if player is inside of the vertex box due to portal being rotated.
                Debug.DrawLine(vertex, vertex + Vector3.up, Color.yellow);
                Vector3 screenPos = renderCamera.WorldToScreenPoint(vertex);
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
            if(highXZ < 0)
            {
                lowestX = 0;
            }
            if(lowYZ < 0)
            {
                highestY = Screen.height;
            }
            if(highYZ < 0)
            {
                lowestY = 0;
            }
            lowestX = lowestX < 0 ? 0 : lowestX;
            lowestY = lowestY < 0 ? 0 : lowestY;
            highestX = highestX > Screen.width ? Screen.width : highestX;
            highestY = highestY > Screen.height ? Screen.height : highestY;
            if (lowestX == highestX | lowestY == highestY)
            {
                //Debug.Log("they were the same");
                return false;
            }
            Plane[] planes = new Plane[6];
            planes[0] = new Plane(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(lowestX, highestY, portalRenderDistance)), renderCamera.ScreenToWorldPoint(new Vector3(lowestX, lowestY, portalRenderDistance)));
            planes[1] = new Plane(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(highestX, lowestY, portalRenderDistance)), renderCamera.ScreenToWorldPoint(new Vector3(highestX, highestY, portalRenderDistance)));
            planes[2] = new Plane(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(lowestX, lowestY, portalRenderDistance)), renderCamera.ScreenToWorldPoint(new Vector3(highestX, lowestY, portalRenderDistance)));
            planes[3] = new Plane(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(highestX, highestY, portalRenderDistance)), renderCamera.ScreenToWorldPoint(new Vector3(lowestX, highestY, portalRenderDistance)));
            planes[4] = new Plane(renderCamera.transform.forward, renderCamera.transform.position);
            planes[5] = new Plane(-renderCamera.transform.forward, 100);

            Debug.DrawLine(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(lowestX, lowestY, portalRenderDistance)));
            Debug.DrawLine(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(lowestX, highestY, portalRenderDistance)));
            Debug.DrawLine(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(highestX, lowestY, portalRenderDistance)));
            Debug.DrawLine(renderCamera.transform.position, renderCamera.ScreenToWorldPoint(new Vector3(highestX, highestY, portalRenderDistance)));
            
            if (GeometryUtility.TestPlanesAABB(planes, checkPortal.portalBounds))
            {
                //Debug.Log("In Bounds");
                return true;
            }
            //Debug.Log("Out Of Bounds");
            return false;
        }
    }

    //probably should find a way to change this so it doesn't appear seperately in so many scripts
    private PortalRenderingComponents CalculateCameraTransform(PortalRenderingComponents portalRendComps)
    {
        Transform currentPortal = portalRendComps.renderingPortal.portalTransform;
        Transform otherPortal = portalRendComps.renderingPortal.otherPortal.portalTransform;

        float scaleFactor = otherPortal.lossyScale.x / currentPortal.lossyScale.x;
        Vector3 playerLocalDirection = (portalRendComps.parentRenderingComponents.cameraPosition - currentPortal.position).normalized;
        float playerLocalDistance = Vector3.Distance(portalRendComps.parentRenderingComponents.cameraPosition, currentPortal.position);
        //this angle calculation might not be working properly because its using AngleAxis
        Quaternion camRotation = Quaternion.AngleAxis(180, otherPortal.up) * (otherPortal.rotation * Quaternion.Inverse(currentPortal.rotation));
        Vector3 camLocalDirection = camRotation * playerLocalDirection;
        float camLocalDistance = playerLocalDistance * scaleFactor;
        portalRendComps.cameraPosition = otherPortal.position + (camLocalDirection * camLocalDistance);
        portalRendComps.cameraRotation = camRotation * portalRendComps.parentRenderingComponents.cameraRotation;

        return portalRendComps;
    }
}
public class PortalRenderingComponents
{
    public int depth = 0;
    public PortalRenderingComponents parentRenderingComponents = null;
    public Vector3 cameraPosition = Vector3.zero;
    public Quaternion cameraRotation = Quaternion.identity;
    public Portal renderingPortal = null;
}
