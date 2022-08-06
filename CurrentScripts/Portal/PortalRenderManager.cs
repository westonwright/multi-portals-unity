using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PortalRenderManager : MonoBehaviour
{
    private PortalManager portalManager;
    List<PortalRenderingComponents> baseRenderingComponents = new List<PortalRenderingComponents>();
    //private PortalRenderingComponents playerCameraRenderingComponents = new PortalRenderingComponents();
    
    private List<RenderTexture> portalRenderTextures = new List<RenderTexture>();
    //private List<(PortalRenderingComponents renderingComponents, int textureIndex)> finalPortalTextureIndices = new List<(PortalRenderingComponents, int)>();

    //private Camera renderCamera;
    private PortalRenderCamera portalRenderCamera;

    //0 means portals won't render anything through themselves
    //max depth has not been defined yet but will probably be around 5
    [SerializeField]
    [Range(0, 6)]
    private int portalRenderDepth = 3;
    //1 means each layer of portals being rendered will use the same resolution
    [SerializeField]
    [Range(.1f, 1)]
    private float portalQualityDropoff = 1;
    //how close a portal has to be for it to be rendered
    [SerializeField]
    private float portalRenderDistance = 100;

    private void Reset()
    {
        portalRenderDepth = Mathf.Max(0, portalRenderDepth);
    }

    void Start()
    {
        //could probably get rid of camera instantiation and just have a permanant one on the scene
        portalManager = FindObjectOfType<PortalManager>();
        // move to on verify or whatever
        GameObject newCameraObject = Instantiate(Resources.Load("PortalCamera")) as GameObject;
        portalRenderCamera = newCameraObject.GetComponent<PortalRenderCamera>();
    }

    void Update()
    {
        // initially set each portal material to be opaque
        foreach (Portal portal in portalManager.portals)
        {
            SetPortalMaterialDefaults(portal);
        }
        // get all portals, and children currently visible
        AccumulatePortals();
        //don't include playerCameraRenderingComponents in portalRenderingComponents list because player camera is rendered seperately
        //portal pass calls itself to loop through all cameras that need to be rendered

        //renders cameras in order from deepest camera to main camera
        foreach (PortalRenderingComponents renderingComponents in baseRenderingComponents)
        {
            RenderPortalsSorted(renderingComponents);
        }

        // set material values before the player camera renders the final image
        // these are of all the grandparent portals
        foreach(PortalRenderingComponents renderingComponents in baseRenderingComponents)
        {
            SetPortalMaterial(renderingComponents);
        }

        ReleaseRenderTextures();

        // debugging stuff
        /*
        Vector3 CurrentCameraPosition;
        Vector3 LastCameraPosition = Camera.main.transform.position;
        float pathStep = 1f / (portalRenderingComponents.Count > 1 ? portalRenderingComponents.Count - 1 : 1);
        float pathLevel = 0;
        Color pathColor = new Color(0, pathLevel, 0);
        int count = 0;
        foreach (PortalRenderingComponents portalRendComps in portalRenderingComponents)
        {
            //Debug.Log("Rendering: " + portalRendComps.depth);
            renderCamera.GetComponent<PortalRenderCamera>().RenderCamera(portalRendComps);
            CurrentCameraPosition = portalRendComps.cameraPosition;
            Debug.DrawLine(LastCameraPosition, CurrentCameraPosition + (Vector3.up), pathColor);
            LastCameraPosition = CurrentCameraPosition;
            pathLevel += pathStep;
            pathColor = new Color(0, pathLevel, 0);
            count++;
        }
        */
    }

    /// <summary>
    /// Renders portals in order of their depth and parents
    /// deepest children are rendered first, then parents
    /// Runs recursively to render all children before parents
    /// </summary>
    /// <param name="currentRenderingComponents"></param>
    private void RenderPortalsSorted(PortalRenderingComponents currentRenderingComponents)
    {
        // recursively renders deepest children first
        foreach (PortalRenderingComponents components in currentRenderingComponents.childrenRenderingComponents)
        {
            RenderPortalsSorted(components);
        }

        // sets material values of children to prepare for rendering
        foreach (PortalRenderingComponents components in currentRenderingComponents.childrenRenderingComponents)
        {
            SetPortalMaterial(components);
        }

        // actually renders the portal
        // might want to find a way to reuse textures here
        // instead of creating a new one for each portal?
        portalRenderTextures.Add(
            portalRenderCamera.RenderPortalCameraToTexture(
            currentRenderingComponents,
            1 * Mathf.Pow(portalQualityDropoff, currentRenderingComponents.depth + 1))
        );
        currentRenderingComponents.textureIndex = portalRenderTextures.Count - 1;
    }

    private void SetPortalMaterial(PortalRenderingComponents portalRenderingComponents)
    {
        portalRenderingComponents.renderingPortal.portalMaterial.SetFloat(
            "_ColorStrength",
            portalRenderingComponents.depth >= portalRenderDepth ? 1 : 0
        );
        // set texture on portal that was just rendered so it can appear in parent portal
        portalRenderingComponents.renderingPortal.portalMaterial.mainTexture = portalRenderTextures[portalRenderingComponents.textureIndex];

        //should probably find a better solution than setting this stuff every frame multiple times
        portalRenderingComponents.renderingPortal.portalMaterial.renderQueue = 3000;
    }
    
    private void SetPortalMaterialDefaults(Portal portal)
    {
        portal.portalMaterial.SetFloat(
            "_ColorStrength",
            1
        );
    }

    /// <summary>
    /// Used for Initialiaztion each frame.
    /// Starts by determining what portals can be seen by the player
    /// </summary>
    private void AccumulatePortals()
    {
        baseRenderingComponents.Clear();
        foreach (Portal portal in portalManager.portals)
        {
            if (
                portalRenderCamera.CheckParentPortalVisibility(
                portal,
                portalRenderDistance
                )
            )
            {
                PortalRenderingComponents newRenderingComponents = new PortalRenderingComponents();
                newRenderingComponents.depth = 0;
                newRenderingComponents.renderingPortal = portal;
                newRenderingComponents = CalculateCameraTransform(newRenderingComponents, Camera.main.transform);

                AccumulateChildren(newRenderingComponents);

                baseRenderingComponents.Add(newRenderingComponents);
            }
        }
    }
    
    /// <summary>
    /// Recursively finds all children and grandchildren of a portal
    /// </summary>
    /// <param name="currentRenderingComponents"></param>
    private void AccumulateChildren(PortalRenderingComponents currentRenderingComponents)
    {
        if (currentRenderingComponents.depth < portalRenderDepth - 1)
        {
            foreach (Portal portal in portalManager.portals)
            {
                if(
                    portalRenderCamera.CheckChildPortalVisibility(
                        currentRenderingComponents,
                        portal,
                        portalRenderDistance
                    )
                )
                {
                    PortalRenderingComponents newRenderingComponents = new PortalRenderingComponents();
                    newRenderingComponents.depth = currentRenderingComponents.depth + 1;
                    newRenderingComponents.renderingPortal = portal;
                    newRenderingComponents.parentRenderingComponents = currentRenderingComponents;
                    newRenderingComponents = CalculateCameraTransform(newRenderingComponents);

                    AccumulateChildren(newRenderingComponents);

                    currentRenderingComponents.childrenRenderingComponents.Add(newRenderingComponents);
                }
            }
        }
    }

    //probably should find a way to change this so it doesn't appear seperately in so many scripts
    private PortalRenderingComponents CalculateCameraTransform(PortalRenderingComponents portalRendComps, Transform cameraTransform)
    {
        Transform currentPortal = portalRendComps.renderingPortal.portalTransform;
        Transform otherPortal = portalRendComps.renderingPortal.otherPortal.portalTransform;

        float scaleFactor = otherPortal.lossyScale.x / currentPortal.lossyScale.x;
        Vector3 playerLocalDirection = (cameraTransform.position - currentPortal.position).normalized;
        float playerLocalDistance = Vector3.Distance(cameraTransform.position, currentPortal.position);
        //this angle calculation might not be working properly because its using AngleAxis
        Quaternion camRotation = Quaternion.AngleAxis(180, otherPortal.up) * (otherPortal.rotation * Quaternion.Inverse(currentPortal.rotation));
        Vector3 camLocalDirection = camRotation * playerLocalDirection;
        float camLocalDistance = playerLocalDistance * scaleFactor;
        portalRendComps.cameraPosition = otherPortal.position + (camLocalDirection * camLocalDistance);
        portalRendComps.cameraRotation = camRotation * cameraTransform.rotation;

        return portalRendComps;
    }
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


    public void ReleaseRenderTextures()
    {
        portalRenderCamera.targetTexture = null;
        foreach (RenderTexture renderTexture in portalRenderTextures)
        {
            RenderTexture.ReleaseTemporary(renderTexture);
        }
        portalRenderTextures.Clear();

        baseRenderingComponents.Clear();
    }
}

public class PortalRenderingComponents
{
    public int depth = -1;
    public int textureIndex = -1;
    public PortalRenderingComponents parentRenderingComponents = null;
    public List<PortalRenderingComponents> childrenRenderingComponents = new List<PortalRenderingComponents>(); 
    public Vector3 cameraPosition = Vector3.zero;
    public Quaternion cameraRotation = Quaternion.identity;
    public Portal renderingPortal = null;
}
