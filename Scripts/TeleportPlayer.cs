using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayer : MonoBehaviour, ITeleportable
{
    public event EventHandler<TeleportEventArgs> OnTeleported;

    private PortalManagerSO portalManager;
    private CharacterController playerController;

    private Vector3 prevPostion = Vector3.one;

    void Start()
    {
        portalManager = PortalStaticMemebers.portalManagerSO;
        playerController = GetComponent<CharacterController>();
    }

    void Update()
    {
        //teleport is set to false by the pick up script instead to make sure it registered it.
        //teleported = false;
        foreach(IPortal portal in portalManager)
        {
            //Debug.DrawRay(portal.TeleportPosition(transform.position), Vector3.up * 3, Color.magenta);
            //this is where the player is actually teleported
            //localPosition = portalManager.CalculateLocalPosition(portalManager.portals[i], playerController.transform.position);
            if (portal.ShouldTeleport(prevPostion, transform.position))
            {
                Teleport(portal);
                break;
            }
        }

        prevPostion = transform.position;
    }

    private void Teleport(IPortal portal)
    {
        playerController.enabled = false;
        transform.position = portal.TeleportPosition(transform.position);
        transform.rotation = portal.TeleportRotation(transform.rotation);
        transform.localScale = portal.TeleportScale(transform.localScale);

        playerController.enabled = true;

        if(OnTeleported != null)
            OnTeleported.Invoke(this, new TeleportEventArgs(portal));
    }
}
