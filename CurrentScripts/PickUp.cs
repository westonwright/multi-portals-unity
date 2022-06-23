using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    private PortalManager portalManager;
    private TeleportPlayer teleportPlayer;
    private TeleportObject teleportObject;

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

    private List<PickUpSegment> pickUpSegments = new List<PickUpSegment>();
    private Vector3 holderPosition = Vector3.zero;//should be set to position of player camera when being reset
    private Vector3 holderDirection = Vector3.zero;//should be set to forward vector of player camera when being reset
    private float holderScale = 1;
    private float holderSpeed = 10;
    private Transform playerTransform { get { return FindObjectOfType<CharacterController>().transform; } }
    private float startingScale;
    void Start()
    {
        portalManager = FindObjectOfType<PortalManager>();
        teleportPlayer = FindObjectOfType<TeleportPlayer>();

        startingScale = playerTransform.transform.lossyScale.x;
        playerCamera = Camera.main.transform;
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
                return;
            }
            else
            {
                if (teleportObject.teleported)
                {
                    teleportObject.teleported = false;
                    PickUpSegment found = pickUpSegments.FindLast(x => x.currentPortal.otherPortal.portalTransform == teleportObject.mostRecentPortal.portalTransform);
                    if (found != null)
                    {
                        pickUpSegments.Remove(found);
                    }
                    else
                    {
                        Portal mostRecentPortal = teleportObject.mostRecentPortal;
                        Transform currentPortal = mostRecentPortal.portalTransform;
                        Transform otherPortal = mostRecentPortal.otherPortal.portalTransform;
                        float scaleFactor = otherPortal.lossyScale.x / currentPortal.lossyScale.x;
                        Vector3 holderLocalDirection = (holderPosition - currentPortal.position).normalized;
                        float holderLocalDistance = Vector3.Distance(holderPosition, currentPortal.position);
                        Quaternion rotationFactor = Quaternion.AngleAxis(180, otherPortal.up) * (otherPortal.rotation * Quaternion.Inverse(currentPortal.rotation));
                        pickUpSegments.Add(new PickUpSegment()
                        {
                            currentPortal = mostRecentPortal,
                            distance = (otherPortal.position + (rotationFactor * holderLocalDirection * holderLocalDistance * scaleFactor)) - holderPosition,
                            rotation = rotationFactor,
                            scale = scaleFactor,
                        });
                    }
                }
                if (teleportPlayer.teleported)
                {
                    teleportPlayer.teleported = false;
                    PickUpSegment foundSameRecent = pickUpSegments.Find(x => x.currentPortal == teleportPlayer.mostRecentPortal);
                    if (foundSameRecent != null)
                    {
                        pickUpSegments.Remove(foundSameRecent);
                    }
                    else
                    {
                        Portal mostRecentPortal = teleportPlayer.mostRecentPortal;
                        Transform currentPortal = mostRecentPortal.otherPortal.portalTransform;
                        Transform otherPortal = mostRecentPortal.portalTransform;

                        float scaleFactor = otherPortal.lossyScale.x / currentPortal.lossyScale.x;
                        Vector3 holderLocalDirection = (holderPosition - currentPortal.position).normalized;
                        float holderLocalDistance = Vector3.Distance(holderPosition, currentPortal.position);
                        Quaternion rotationFactor = Quaternion.AngleAxis(180, otherPortal.up) * (otherPortal.rotation * Quaternion.Inverse(currentPortal.rotation));

                        pickUpSegments.Insert(0, new PickUpSegment()
                        {
                            currentPortal = mostRecentPortal.otherPortal,
                            distance = (otherPortal.position + (rotationFactor * holderLocalDirection * holderLocalDistance * scaleFactor)) - holderPosition,
                            rotation = rotationFactor,
                            scale = scaleFactor,
                        });

                    }
                }
                holderScale = (playerTransform.lossyScale.x / startingScale);
                holderPosition = playerCamera.position;
                holderDirection = playerCamera.forward;

                int counter = 0;
                foreach (PickUpSegment pickUpSegment in pickUpSegments)
                {
                    counter++;
                    Transform currentPortal = pickUpSegment.currentPortal.portalTransform;
                    Transform otherPortal = pickUpSegment.currentPortal.otherPortal.portalTransform;
                    Vector3 holderLocalDirection = (holderPosition - currentPortal.position).normalized;
                    float holderLocalDistance = Vector3.Distance(holderPosition, currentPortal.position);

                    pickUpSegment.distance = (otherPortal.position + (pickUpSegment.rotation * holderLocalDirection * holderLocalDistance * pickUpSegment.scale)) - holderPosition;
                    //Debug.DrawRay(holderPosition, holderDirection * (holderScale * trueHoldDistance), Color.green);
                    holderPosition += pickUpSegment.distance;
                    holderDirection = pickUpSegment.rotation * holderDirection;
                    holderScale *= pickUpSegment.scale;
                    //Debug.DrawRay(holderPosition, holderDirection * (holderScale * trueHoldDistance), Color.green);
                }
                //Debug.DrawRay(holderPosition, holderDirection * (holderScale * trueHoldDistance), Color.green);

                Vector3 objectTargetDirection = (holderPosition + (holderDirection * (holderScale * trueHoldDistance))) - objectRigidbody.position;
                objectRigidbody.velocity = objectTargetDirection * holderSpeed;
            }
        }

        else
        {
            if (teleportPlayer.teleported)
            {
                teleportPlayer.teleported = false;
            }
            if (teleportObject != null && teleportObject.teleported)
            {
                teleportObject.teleported = false;
            }
            if (selectedObject != null)
            {
                var selectionRenderer = selectedObject.GetComponent<Renderer>();
                selectionRenderer.material = defaultMaterial;
                selectedObject = null;
            }


            float rayDistance = 0;
            float maxRayDistance = trueGrabDistance;
            Vector3 rayStartPoint = playerCamera.position;
            Vector3 rayDircetion = playerCamera.forward;
            float rayScale = 1;
            int loops = 0;

            pickUpSegments = new List<PickUpSegment>();
            holderScale = (playerTransform.lossyScale.x / startingScale);
            holderPosition = playerCamera.position;
            holderDirection = playerCamera.forward;

            while (true)
            {
                Color debugColor = new Color(loops * .3f, 0, 0);
                Ray ray = new Ray(rayStartPoint, rayDircetion);
                RaycastHit hit;

                //might want to make is so that raycasts only collide with portals that are facing the right way.
                if (Physics.Raycast(ray, out hit, LayerMask.NameToLayer("ObjectClone")))
                {

                    //Debug.Log("holder position " + holderPosition + " position offset " + holderPositionOffset);
                    //Debug.DrawRay(rayStartPoint, rayDircetion * ((maxRayDistance - rayDistance) * rayScale), debugColor);
                    //Debug.DrawRay(holderPosition, holderDirection * (trueHoldDistance * holderScale), Color.green);
                    Transform selection = hit.transform;
                    float hitDistance = hit.distance / rayScale;
                    //Debug.Log("hit dist " + hitDistance + " ray scale " + rayScale +     " loop: " + loops + " hit obj " + hit.transform.name);
                    if (selection.CompareTag("Interactable"))
                    {
                        if(rayDistance + hitDistance < maxRayDistance)
                        {
                            selectedObject = selection;
                            break;
                        }
                        else
                        {
                            break;
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
                    else if (selection.CompareTag("Portal"))
                    {
                        if(rayDistance + hitDistance < maxRayDistance)
                        {
                            Portal prtl = portalManager.portals.Find(x => x.portalTransform == hit.transform);
                            Transform hitCurrentPortal = prtl.portalTransform;
                            Transform hitOtherPortal = prtl.otherPortal.portalTransform;
                            //Transform hitCurrentPortal = hit.transform;
                            //Transform hitOtherPortal = portalManager.portals.Find(x => x.otherPortal.portalTransform == hit.transform).portalTransform;
                            float scaleFactor = hitOtherPortal.lossyScale.x / hitCurrentPortal.lossyScale.x;
                            Vector3 hitLocalDirection = (hit.point - hitCurrentPortal.position).normalized;
                            float hitLocalDistance = Vector3.Distance(hit.point, hitCurrentPortal.position);
                            Quaternion newRayRotation = Quaternion.AngleAxis(180, hitOtherPortal.up) * (hitOtherPortal.rotation * Quaternion.Inverse(hitCurrentPortal.rotation));
                            Vector3 newRayLocalDirection = newRayRotation * hitLocalDirection;
                            float newRayLocalDistance = hitLocalDistance * scaleFactor;
                            rayStartPoint = hitOtherPortal.position + (newRayLocalDirection * newRayLocalDistance);
                            rayDircetion = newRayRotation * rayDircetion;
                            rayScale *= scaleFactor;
                            rayDistance += hitDistance;

                            PickUpSegment found = pickUpSegments.Find(x => x.currentPortal.otherPortal.portalTransform == hitCurrentPortal);
                            if(found != null)
                            {
                                pickUpSegments.Remove(found);
                            }
                            else
                            {
                                Vector3 holderLocalDirection = (holderPosition - hitCurrentPortal.position).normalized;
                                float holderLocalDistance = Vector3.Distance(holderPosition, hitCurrentPortal.position);
                                Quaternion rotationFactor = Quaternion.AngleAxis(180, hitOtherPortal.up) * (hitOtherPortal.rotation * Quaternion.Inverse(hitCurrentPortal.rotation));
                                pickUpSegments.Add(new PickUpSegment()
                                {
                                    currentPortal = portalManager.portals.Find(x => x.portalTransform == hitCurrentPortal),
                                    distance = (hitOtherPortal.position + (rotationFactor * holderLocalDirection * holderLocalDistance * scaleFactor)) - holderPosition,
                                    rotation = rotationFactor,
                                    scale = scaleFactor,
                                });
                            }
                            holderScale = (playerTransform.lossyScale.x / startingScale);
                            holderPosition = playerCamera.position;
                            holderDirection = playerCamera.forward;
                            foreach(PickUpSegment pickUpSegment in pickUpSegments)
                            {
                                holderPosition += pickUpSegment.distance;
                                holderDirection = pickUpSegment.rotation * holderDirection;
                                holderScale *= pickUpSegment.scale;
                            }

                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    //Debug.DrawRay(rayStartPoint, rayDircetion * ((maxRayDistance - rayDistance) * rayScale), debugColor);
                    //Debug.DrawRay(holderPosition, holderDirection * ((trueHoldDistance) * holderScale), Color.green);
                    break;
                }
                loops++;
            }

            if (selectedObject != null)
            {
                if (Input.GetKeyUp(KeyCode.E))
                {
                    teleportObject = selectedObject.GetComponent<TeleportObject>();
                    objectRigidbody = selectedObject.GetComponent<Rigidbody>();

                    //rb.tag = "Selected";

                    selected = true;
                    objectRigidbody.useGravity = false;
                    selectedObject.GetComponent<Renderer>().material = defaultMaterial;
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
}
public class PickUpSegment 
{
    public Portal currentPortal;
    public Vector3 distance;
    public Quaternion rotation;
    public float scale;
}

