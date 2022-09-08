using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PortalStaticMemebers
{
    private static readonly string packageName = "com.weston-wright.multi-portals";
    private static readonly string scriptableObjectsPath = "Scriptable Objects/";
    private static readonly string prefabsPath = "Prefabs/";
    private static readonly string materialsPath = "Materials/";

    private static readonly string portalManagerSOName = "PortalManagerSO";
    public static PortalManagerSO portalManagerSO;

    private static readonly string portalCameraPrefabName = "PortalCamera";
    public static PortalCamera portalCameraPrefab;
    
    private static readonly string defaultPortalMaterialName = "DefaultPortalMaterial";
    public static Material defaultPortalMaterial;
    

    static PortalStaticMemebers()
    {
        portalManagerSO = Resources.Load<PortalManagerSO>(scriptableObjectsPath + portalManagerSOName);
        portalCameraPrefab = Resources.Load<PortalCamera>(prefabsPath + portalCameraPrefabName);
        defaultPortalMaterial = Resources.Load<Material>(materialsPath + defaultPortalMaterialName);
    }
}
