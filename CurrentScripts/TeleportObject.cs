using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportObject : MonoBehaviour
{
    private PortalManager portalManager;
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

    private Vector3 velocity = Vector3.zero;
    private Vector3 localPosition = Vector3.one;
    private float[] lastDistances;

    public Portal mostRecentPortal = null;
    public bool teleported = false;
    void Start()
    {
        portalManager = FindObjectOfType<PortalManager>();
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

        objectClones = new GameObject[portalManager.portals.Count];

        lastDistances = new float[portalManager.portals.Count];
        for (int i = 0; i < lastDistances.Length; i++)
        {
            lastDistances[i] = -1;
        }
    }

    void FixedUpdate()
    {
        //jitter caused by teleport is probably caused by fixed update and interpolation on the physics applied to the object.
        //teleportation of object might be better in update?



        //teleport is set to false by the pick up script instead to make sure it registered it.
        //teleported = false;
        for (int i = 0; i < portalManager.portals.Count; i++)
        {
            localPosition = portalManager.CalculateLocalPosition(portalManager.portals[i], transform.position);
            if (portalManager.PortalTeleported(portalManager.portals[i], localPosition, lastDistances[i]))
            {
                for (int j = 0; j < objectClones.Length; j++)
                {
                    if(objectClones[j] != null)
                    {
                        DestroyClone(j);
                    }
                }

                velocity = objectRB.velocity;
                //objectRB.isKinematic = true;
                transform.position = portalManager.UpdatePosition(portalManager.portals[i], localPosition);
                transform.rotation = portalManager.UpdateRotation(portalManager.portals[i], transform.rotation);
                transform.localScale = portalManager.UpdateScale(portalManager.portals[i], transform.localScale);
                //objectRB.isKinematic = false;
                objectRB.velocity = portalManager.portals[i].portalRotationFactor * velocity * portalManager.portals[i].portalScaleFactor;

                for (int j = 0; j < lastDistances.Length; j++)
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
    private void Update()
    {
        for (int i = 0; i < meshVertices.Length; i++)
        {
            adjustedVertices[i] = transform.position + transform.rotation * new Vector3(meshVertices[i].x * transform.lossyScale.x, meshVertices[i].y * transform.lossyScale.y, meshVertices[i].z * transform.lossyScale.z);
        }

        for (int i = 0; i < portalManager.portals.Count; i++)
        {
            localPosition = portalManager.CalculateLocalPosition(portalManager.portals[i], transform.position);

            oneInBounds = false;
            oneIn = false;
            oneOut = false;
            for (int j = 0; j < vertexLocalPositions.Length; j++)
            {
                vertexLocalPositions[j] = portalManager.CalculateLocalPosition(portalManager.portals[i], adjustedVertices[j]);
                oneIn = vertexLocalPositions[j].z <= 0 ? true : oneIn;
                oneOut = vertexLocalPositions[j].z > 0 ? true : oneOut;
                oneInBounds = portalManager.InPortalBounds(portalManager.portals[i], vertexLocalPositions[j]) ? true : oneInBounds;
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
                objectClones[i].transform.position = portalManager.UpdatePosition(portalManager.portals[i], localPosition);
                objectClones[i].transform.rotation = portalManager.UpdateRotation(portalManager.portals[i], transform.rotation);
                objectClones[i].transform.localScale = portalManager.UpdateScale(portalManager.portals[i], transform.localScale);
               
                objectClones[i].GetComponent<MeshRenderer>().material.SetInt("_PlaneCount", 1);
                objectClones[i].GetComponent<MeshRenderer>().material.SetVectorArray("_Planes", new Vector4[] { portalManager.PlaneRepresentation(portalManager.portals[i].otherPortal) });
            }
        }
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
        Destroy(objectClones[cloneValue].GetComponent<TeleportObject>());
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
