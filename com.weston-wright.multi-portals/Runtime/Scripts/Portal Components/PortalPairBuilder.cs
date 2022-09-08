using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalPairBuilder : MonoBehaviour
{
    [SerializeField]
    private Mesh portalMesh;
    [SerializeField]
    private Vector3 stretch = Vector3.one;
    [SerializeField]
    private Material defaultMateiral;
    [SerializeField]
    private PortalHolder leftHolder = null;
    [SerializeField]
    private PortalHolder rightHolder = null;
    [SerializeField]
    private PortalFrame leftFrame = null;
    [SerializeField]
    private PortalFrame rightFrame = null;

    private void Reset()
    {
        
    }
    private void OnValidate()
    {
        stretch = new Vector3(
            Mathf.Max(.1f, stretch.x),
            Mathf.Max(.1f, stretch.y),
            Mathf.Max(.1f, stretch.z)
            );
    }
}
