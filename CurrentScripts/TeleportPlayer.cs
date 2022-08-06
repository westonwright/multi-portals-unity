using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
    private PortalManager portalManager;
    private CharacterController playerController;

    private Vector3 localPosition = Vector3.one;
    private float[] lastDistances;

    public Portal mostRecentPortal = null;
    public bool teleported = false;
    void Start()
    {
        portalManager = FindObjectOfType<PortalManager>();
        playerController = GetComponent<CharacterController>();

        lastDistances = new float[portalManager.portals.Count];
        for(int i = 0; i < lastDistances.Length; i++)
        {
            lastDistances[i] = -1;
        }
    }

    void Update()
    {
        //teleport is set to false by the pick up script instead to make sure it registered it.
        //teleported = false;
        for (int i = 0; i < portalManager.portals.Count; i++)
        {
            //this is where the player is actually teleported
            localPosition = portalManager.CalculateLocalPosition(portalManager.portals[i], playerController.transform.position);
            if (portalManager.PortalTeleported(portalManager.portals[i], localPosition, lastDistances[i]))
            {
                playerController.enabled = false;
                transform.position = portalManager.UpdatePosition(portalManager.portals[i], localPosition);
                transform.rotation = portalManager.UpdateRotation(portalManager.portals[i], transform.rotation);
                transform.localScale = portalManager.UpdateScale(portalManager.portals[i], transform.localScale);
                playerController.enabled = true;

                for(int j = 0; j < lastDistances.Length; j++)
                {
                    lastDistances[j] = -1;
                }

                mostRecentPortal = portalManager.portals[i];
                teleported = true;

                break;
            }
            lastDistances[i] = localPosition.z;
        }
    }
}
