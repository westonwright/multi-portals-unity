using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClone : MonoBehaviour
{
    private PortalManager portalManager;
    private Collider objectCol;
    public Transform parentObject;
    public Portal parentPortal;
    void Start()
    {
        portalManager = FindObjectOfType<PortalManager>();
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
