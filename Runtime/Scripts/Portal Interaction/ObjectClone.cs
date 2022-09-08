using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClone : MonoBehaviour
{
    private PortalManagerSO portalManager;
    private Collider objectCol;
    public Transform parentObject;
    public IPortal parentPortal;
    void Start()
    {
        portalManager = FindObjectOfType<PortalManagerSO>();
        objectCol = GetComponent<Collider>();
    }

    void Update()
    {

    }
    private void FixedUpdate()
    {
        objectCol.isTrigger = false;
    }
}
