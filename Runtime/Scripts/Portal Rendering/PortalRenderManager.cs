using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PortalRenderManager : MonoBehaviour
{
    private PortalManagerSO portalManager;
    List<PortalRenderingComponents> baseRenderingComponents = new List<PortalRenderingComponents>();
    //private PortalRenderingComponents playerCameraRenderingComponents = new PortalRenderingComponents();
    
    private List<RenderTexture> portalRenderTextures = new List<RenderTexture>();
    //private List<(PortalRenderingComponents renderingComponents, int textureIndex)> finalPortalTextureIndices = new List<(PortalRenderingComponents, int)>();

    //private Camera renderCamera;
    private PortalCamera portalRenderCamera;

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
        portalManager = PortalStaticMemebers.portalManagerSO;
        // move to on verify or whatever
        portalRenderCamera = Instantiate(PortalStaticMemebers.portalCameraPrefab);
    }

    void Update()
    {
        // initially set each portal material to be opaque
        foreach (IPortal portal in portalManager)
        {
            SetPortalMaterialDefaults(portal);
        }
        // get all portals, and children currently visible
        AccumulatePortals();
        //don't include playerCameraRenderingComponents in portalRenderingComponents list because player camera is rendered seperately
        //portal pass calls itself to loop through all cameras that need to be rendered

        //renders cameras in order from deepest camera to main camera
        //Debug.Log(baseRenderingComponents.Count);
        //Debug.Log(baseRenderingComponents[0].childrenRenderingComponents.Count);
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
        portalRenderingComponents.renderingPortal.Material.SetFloat(
            "_ColorStrength",
            portalRenderingComponents.depth >= portalRenderDepth ? 1 : 0
        );
        // set texture on portal that was just rendered so it can appear in parent portal
        portalRenderingComponents.renderingPortal.Material.mainTexture = portalRenderTextures[portalRenderingComponents.textureIndex];

        //should probably find a better solution than setting this stuff every frame multiple times
        portalRenderingComponents.renderingPortal.Material.renderQueue = 3000;
    }
    
    private void SetPortalMaterialDefaults(IPortal portal)
    {
        portal.Material.SetFloat(
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
        foreach (IPortal portal in portalManager)
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
            foreach (IPortal portal in portalManager)
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
        portalRendComps.cameraPosition = portalRendComps.renderingPortal.TeleportPosition(cameraTransform.position);
        portalRendComps.cameraRotation = portalRendComps.renderingPortal.TeleportRotation(cameraTransform.rotation);
        return portalRendComps;
    }
    private PortalRenderingComponents CalculateCameraTransform(PortalRenderingComponents portalRendComps)
    {
        portalRendComps.cameraPosition = portalRendComps.renderingPortal.TeleportPosition(portalRendComps.parentRenderingComponents.cameraPosition);
        portalRendComps.cameraRotation = portalRendComps.renderingPortal.TeleportRotation(portalRendComps.parentRenderingComponents.cameraRotation);
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
    public IPortal renderingPortal = null;
}
