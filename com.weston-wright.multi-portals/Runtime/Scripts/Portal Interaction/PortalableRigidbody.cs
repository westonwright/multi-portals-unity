using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PortalableRigidbody : MonoBehaviour, ITeleportable
{
    private PortalManagerSO portalManager;
    private Rigidbody objectRB;
    private Collider objectCol;
    //private Mesh objectMesh;
    private Vector3[] meshVertices = new Vector3[8];
    private Vector3[] adjustedVertices = new Vector3[8];
    private Vector3[] vertexLocalPositions = new Vector3[8];
    private bool oneInBounds = false;
    private bool oneIn = false;
    private bool oneOut = false;
    public GameObject[] objectClones;

    private Vector3 prevPosition = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    public event EventHandler<TeleportEventArgs> OnTeleported;

    void Start()
    {
        portalManager = PortalStaticMemebers.portalManagerSO;
        objectRB = GetComponent<Rigidbody>();
        objectCol = GetComponent<Collider>();

        //objectMesh = GetComponent<MeshFilter>().mesh;
        Mesh objectMesh = GetComponent<MeshFilter>().mesh;
        Vector3 min = objectMesh.bounds.min;
        Vector3 max = objectMesh.bounds.max;
        meshVertices[0] = new Vector3(min.x, min.y, min.z);
        meshVertices[1] = new Vector3(min.x, min.y, max.z);
        meshVertices[2] = new Vector3(min.x, max.y, min.z);
        meshVertices[3] = new Vector3(min.x, max.y, max.z);
        meshVertices[4] = new Vector3(max.x, min.y, min.z);
        meshVertices[5] = new Vector3(max.x, min.y, max.z);
        meshVertices[6] = new Vector3(max.x, max.y, min.z);
        meshVertices[7] = new Vector3(max.x, max.y, max.z);

        objectClones = new GameObject[portalManager.Count];
    }

    void FixedUpdate()
    {
        //jitter caused by teleport is probably caused by fixed update and interpolation on the physics applied to the object.
        //teleportation of object might be better in update?

        //teleport is set to false by the pick up script instead to make sure it registered it.
        //teleported = false;
        foreach(IPortal portal in portalManager)
        {
            //localPosition = portalManager.CalculateLocalPosition(portalManager.portals[i], transform.position);
            if (portal.ShouldTeleport(prevPosition, transform.position))
            {
                Teleport(portal);

                break;
            }
        }
        // setting prev position is important!!
        prevPosition = transform.position;
    }

    private void Update()
    {
        for (int i = 0; i < meshVertices.Length; i++)
        {
            adjustedVertices[i] = 
                transform.position + 
                transform.rotation *
                new Vector3(
                    meshVertices[i].x * transform.lossyScale.x,
                    meshVertices[i].y * transform.lossyScale.y,
                    meshVertices[i].z * transform.lossyScale.z
                    );
        }

        for (int i = 0; i < portalManager.Count; i++)
        {
            //localPosition = portalManager.CalculateLocalPosition(portalManager.portals[i], transform.position);

            oneInBounds = false;
            oneIn = false;
            oneOut = false;
            for (int j = 0; j < adjustedVertices.Length; j++)
            {
                vertexLocalPositions[j] = portalManager[i].Transform.InverseTransformPoint(adjustedVertices[j]);
                oneIn = vertexLocalPositions[j].z <= 0 ? true : oneIn;
                oneOut = vertexLocalPositions[j].z > 0 ? true : oneOut;
                oneInBounds = portalManager[i].InBounds(adjustedVertices[j]) ? true : oneInBounds;
            }
            if (oneInBounds)
            {
                if (oneIn && oneOut)
                {
                    if (objectClones[i] == null)
                    {
                        SpawnClone(i);
                    }
                }
                else if (objectClones[i] != null)
                {
                    DestroyClone(i);
                }
            }
            else if (objectClones[i] != null)
            {
                DestroyClone(i);
            }
            if (objectClones[i] != null)
            {
                objectClones[i].transform.position = portalManager[i].TeleportPosition(transform.position);
                objectClones[i].transform.rotation = portalManager[i].TeleportRotation(transform.rotation);
                objectClones[i].transform.localScale = portalManager[i].TeleportScale(transform.localScale);
               
                // skip material stuff here because we arent including the "cut planes" material in the base version
                /*
                objectClones[i].GetComponent<MeshRenderer>().material.SetInt("_PlaneCount", 1);
                objectClones[i].GetComponent<MeshRenderer>().material.SetVectorArray("_Planes", new Vector4[] { portalManager[i].PlaneRepresentation() });
                */
            }
        }
    }

    private void Teleport(IPortal portal)
    {
        for (int j = 0; j < objectClones.Length; j++)
        {
            if (objectClones[j] != null)
            {
                DestroyClone(j);
            }
        }

        velocity = objectRB.velocity;
        //objectRB.isKinematic = true;
        transform.position = portal.TeleportPosition(transform.position);
        transform.rotation = portal.TeleportRotation(transform.rotation);
        transform.localScale = portal.TeleportScale(transform.localScale);

        //objectRB.isKinematic = false;
        objectRB.velocity = portal.TeleportVector(velocity);

        if (OnTeleported != null)
            OnTeleported.Invoke(this, new TeleportEventArgs(portal));
    }

    private void DestroyClone(int cloneValue)
    {
        Destroy(objectClones[cloneValue].gameObject);
        objectClones[cloneValue] = null;
    }

    void SpawnClone(int cloneValue)
    {
        //Debug.Log("spawned");
        if (objectClones[cloneValue] != null)
        {
            DestroyClone(cloneValue);
        }
        objectClones[cloneValue] = Instantiate(objectRB.gameObject);
        Destroy(objectClones[cloneValue].GetComponent<PortalableRigidbody>());
        Destroy(objectClones[cloneValue].GetComponent<Rigidbody>());
        objectClones[cloneValue].GetComponent<Collider>().isTrigger = false;
        objectClones[cloneValue].tag = "ObjectClone";
        objectClones[cloneValue].GetComponent<MeshRenderer>().material = objectRB.gameObject.GetComponent<MeshRenderer>().material;
        objectClones[cloneValue].AddComponent<ObjectClone>();
        objectClones[cloneValue].GetComponent<ObjectClone>().parentObject = transform;
        objectClones[cloneValue].GetComponent<MeshRenderer>().material = objectRB.gameObject.GetComponent<MeshRenderer>().material;
       
        Physics.IgnoreCollision(objectClones[cloneValue].GetComponent<Collider>(), objectCol);
    }
}
