using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalHolder : MonoBehaviour
{
    [SerializeField]
    private Transform parentTransform;
    [SerializeField]
    private PortalFrame portalFrame;
    public PortalFrame PortalFrame { get => portalFrame; }

    [SerializeField]
    private PortalPlane portalPlane;
    public PortalPlane PortalPlane { get => portalPlane; }

    // because this object acts as the pivot for the portal, you can offset it if you want
    public void SetPortalPivot()
    {

    }

    private void SetParentTransform(Transform parent)
    {
        parentTransform = parent;
        transform.parent = parentTransform;
    }

    public void Initialize(PortalFrame portalFrame, Transform parent)
    {
        this.portalFrame = portalFrame;
        portalPlane = portalFrame.PortalPlane;

        transform.position = portalPlane.transform.position;
        transform.rotation = portalPlane.transform.rotation;
        transform.localScale = Vector3.one;
        portalPlane.transform.parent = transform;
        portalFrame.transform.parent = transform;
        SetParentTransform(parent);
        // move to center of portal
        // match rotation to portal plane's rotation
        // set frame and plane to be children
    }

    public void DeInitialize()
    {        
        // de-parent, and delete

        if (portalFrame != null)
        {
            portalFrame.DeInitialize();
#if UNITY_EDITOR
            DestroyImmediate(portalFrame);
#else
            Destroy(portalFrame);
#endif

        }
        if (portalPlane != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(portalPlane);
#else
            Destroy(portalPlane);
#endif
        }

    }

    private void OnTransformParentChanged()
    {
        if (transform.parent != parentTransform)
            if (parentTransform != null)
            {
                Debug.LogWarning("Don't change Portal Holder's parent!", this);
                transform.parent = parentTransform;
                Debug.Log("Portal Holder returned to parent.", this);

            }
            else
                DeInitialize(); // this should destroy the holder and the frame/plane?
    }
}
