using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalRenderCamera : MonoBehaviour
{
    public int renderDepth = 1;
    public float qualityDropoff = 1;
    private List<RenderTexture> camRenderTextures = new List<RenderTexture>();
    private List<RenderTexture> baseRenderTextures = new List<RenderTexture>();
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    public void RenderCamera(PortalRenderingComponents portalRenderingComponents)
    {
        cam.transform.position = portalRenderingComponents.cameraPosition;
        cam.transform.rotation = portalRenderingComponents.cameraRotation;

        Color rayColor = Color.red;
        rayColor.r = 1 / portalRenderingComponents.depth;
        Debug.DrawRay(cam.transform.position, cam.transform.rotation * (Vector3.forward * 5), rayColor);

        Vector4 clippingPlaneWorldSpace = new Vector4(portalRenderingComponents.renderingPortal.otherPortal.portalPlane.normal.x, portalRenderingComponents.renderingPortal.otherPortal.portalPlane.normal.y, portalRenderingComponents.renderingPortal.otherPortal.portalPlane.normal.z, portalRenderingComponents.renderingPortal.otherPortal.portalPlane.distance);
        Vector4 clippingPlaneCameraSpace = Matrix4x4.Transpose(cam.cameraToWorldMatrix) * clippingPlaneWorldSpace;
        cam.projectionMatrix = cam.CalculateObliqueMatrix(clippingPlaneCameraSpace);

        float quality = 1 * Mathf.Pow(qualityDropoff, portalRenderingComponents.depth - 1);
        camRenderTextures.Add(RenderTexture.GetTemporary(Mathf.RoundToInt(Screen.width * quality), Mathf.RoundToInt(Screen.height * quality), 24));
        if(portalRenderingComponents.depth == 1)
        {
            baseRenderTextures.Add(camRenderTextures[camRenderTextures.Count - 1]);
        }
        if(portalRenderingComponents.depth == renderDepth)
        {
            cam.cullingMask = (1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("PortalFade") | 1 << LayerMask.NameToLayer("Portal"));
        }
        else
        {
            cam.cullingMask = (1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Portal"));
        }
        cam.targetTexture = camRenderTextures[camRenderTextures.Count - 1];
        cam.Render();
        portalRenderingComponents.renderingPortal.portalMaterial.mainTexture = camRenderTextures[camRenderTextures.Count - 1];
        //should probably find a better solution than setting this stuff every frame multiple times
        portalRenderingComponents.renderingPortal.portalMaterial.renderQueue = 3000;
        
        cam.ResetProjectionMatrix();
    }
    public void SetMaterials(List<PortalRenderingComponents> portalRenderingComponents)
    {
        foreach(PortalRenderingComponents portalRendComps in portalRenderingComponents)
        {
            if(portalRendComps.depth == 1)
            {
                portalRendComps.renderingPortal.portalMaterial.mainTexture = baseRenderTextures[0];
                //should probably find a better solution than setting this stuff every frame multiple times
                portalRendComps.renderingPortal.portalMaterial.renderQueue = 3000;
                baseRenderTextures.RemoveAt(0);
            }
        }
    }
    public void ReleaseTextures()
    {
        cam.targetTexture = null;
        foreach (RenderTexture renderTexture in camRenderTextures)
        {
            RenderTexture.ReleaseTemporary(renderTexture);
        }
        camRenderTextures = new List<RenderTexture>();
    }
    /*
    public void Delete()
    {
        Destroy(camRender);
        Destroy(targetPortal);
        Destroy(gameObject);
    }
    */
}
