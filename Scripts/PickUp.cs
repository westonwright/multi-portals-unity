using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    private PortalManagerSO portalManager;
    private TeleportPlayer teleportPlayer;
    private PortalableRigidbody teleportObject;
    private TeleportConnection playerObjectConnection;

    private Transform playerCamera;
    private float trueHoldDistance = 5;
    private float trueGrabDistance = 8;
    private Rigidbody objectRigidbody;
    private bool selected = false;
    private bool drop = false;
    private Transform selectedObject;
    public bool teleported = true;
    public Material highlightMaterial;
    public Material defaultMaterial;

    //private List<PickUpSegment> pickUpSegments = new List<PickUpSegment>();
    private Vector3 holderPosition = Vector3.zero;//should be set to position of player camera when being reset
    private Vector3 holderDirection = Vector3.zero;//should be set to forward vector of player camera when being reset
    private float holderScale = 1;
    private float holderSpeed = 10;
    private Transform playerTransform { get { return FindObjectOfType<CharacterController>().transform; } }
    private float startingScale;
    void Start()
    {
        portalManager = PortalStaticMemebers.portalManagerSO;
        teleportPlayer = FindObjectOfType<TeleportPlayer>();

        startingScale = playerTransform.transform.lossyScale.x;
        playerCamera = Camera.main.transform;

        playerObjectConnection = new TeleportConnection(teleportPlayer);
    }

    void Update()
    {
        if (selected == true)
        {
            if (Input.GetKeyUp(KeyCode.E) || drop == true)
            {
                objectRigidbody.useGravity = true;
                selected = false;
                drop = false;
                RemoveTeleportObject();
                return;
            }
            else
            {
                /*
                if (playerObjectConnection != null)
                    Debug.Log(playerObjectConnection.PortalChain.Count);
                */

                holderPosition = playerObjectConnection.PortalChain.TransformFollowerPosition(playerCamera.position);
                holderDirection = playerObjectConnection.PortalChain.TransformFollowerDirection(playerCamera.forward);
                holderScale = playerObjectConnection.PortalChain.TransformFollowerScale(playerTransform.lossyScale.x / startingScale);

                //Debug.DrawRay(holderPosition, holderDirection * (holderScale * trueHoldDistance), Color.blue);

                //Debug.DrawRay(holderPosition, holderDirection * (holderScale * trueHoldDistance), Color.blue);
                //Debug.DrawRay(holderPosition, Vector3.down * 5, Color.blue);

                Vector3 objectTargetDirection = (holderPosition + (holderDirection * (holderScale * trueHoldDistance))) - objectRigidbody.position;
                objectRigidbody.velocity = objectTargetDirection * holderSpeed;
            }
        }
        else
        {
            if (selectedObject != null)
            {
                var selectionRenderer = selectedObject.GetComponent<Renderer>();
                selectionRenderer.material = defaultMaterial;
                selectedObject = null;
            }


            float rayDistance = 0;
            float maxRayDistance = trueGrabDistance * teleportPlayer.transform.localScale.x;
            Vector3 rayStartPoint = playerCamera.position;
            Vector3 rayDircetion = playerCamera.forward;
            float rayScale = 1;
            PortalLink portalLink = null;
            //int loops = 0;

            //pickUpSegments = new List<PickUpSegment>();
            holderScale = (playerTransform.lossyScale.x / startingScale);
            holderPosition = playerCamera.position;
            holderDirection = playerCamera.forward;

            if(PortalCollectionMethods.RaycastAll(
                portalManager,
                new Ray(rayStartPoint, rayDircetion),
                maxRayDistance,
                LayerMask.NameToLayer("ObjectClone") | LayerMask.NameToLayer("Portal"),
                out portalLink,
                out RaycastHit hit
                ))
            {
                Transform selection = hit.transform;
                float hitDistance = hit.distance / rayScale;

                if (selection.CompareTag("Interactable"))
                {
                    if (rayDistance + hitDistance < maxRayDistance)
                    {
                        selectedObject = selection;
                    }
                }
                /*
                else if (selection.CompareTag("ObjectClone"))
                {
                    if (rayDistance + hitDistance < maxRayDistance)
                    {
                        selectedObject = selection.GetComponent<ObjectClone>().originalObject;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                */
            }

            if (selectedObject != null)
            {
                if (Input.GetKeyUp(KeyCode.E))
                {
                    TryNewTeleportObject(selectedObject.gameObject, portalLink);
                }

                var selectionRenderer = selectedObject.GetComponent<Renderer>();
                if (selectionRenderer != null)
                {
                    defaultMaterial = selectionRenderer.material;
                    selectionRenderer.material = highlightMaterial;
                }
            }
        }
    }

    private void TryNewTeleportObject(GameObject newObject, PortalLink portalLink)
    {
        teleportObject = newObject.GetComponent<PortalableRigidbody>();
        if (teleportObject == null) return;

        playerObjectConnection.SetFollower(teleportObject, portalLink);

        objectRigidbody = newObject.GetComponent<Rigidbody>();
        if (objectRigidbody == null) return;
        //rb.tag = "Selected";
        selected = true;
        objectRigidbody.useGravity = false;
        selectedObject.GetComponent<Renderer>().material = defaultMaterial;
    }

    private void RemoveTeleportObject()
    {
        playerObjectConnection.DisconnectFollower();
    }
}

