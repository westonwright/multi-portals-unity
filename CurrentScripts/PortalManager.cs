using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public Material portalFadeMat;
    public GameObject[] portalObjects;
    public List<Portal> portals = new List<Portal>();
    //public PortalFade[] portalFades;

    void Awake()
    {
        portalObjects = GameObject.FindGameObjectsWithTag("Portal");
        for(int i = 0; i < portalObjects.Length; i++)
        {
            portals.Add(new Portal());
            portals[i] = new Portal();
            portals[i].portalTransform = portalObjects[i].transform;
            portals[i].portalMaterial = portalObjects[i].GetComponent<MeshRenderer>().material;
            portals[i].portalPlane = new Plane(portals[i].portalTransform.forward, portals[i].portalTransform.position);

            portals[i].portalBounds = portalObjects[i].GetComponent<Renderer>().bounds;

            Mesh portalMesh = portalObjects[i].GetComponent<MeshFilter>().mesh;
            portals[i].portalDimensions = new Vector3(portalMesh.bounds.size.x * portals[i].portalTransform.lossyScale.x, portalMesh.bounds.size.y * portals[i].portalTransform.lossyScale.y, portalMesh.bounds.size.z * portals[i].portalTransform.lossyScale.z);

            Vector3[] vertices = new Vector3[8];
            Vector3 min = portalMesh.bounds.min;
            Vector3 max = portalMesh.bounds.max;
            vertices[0] = new Vector3(min.x, min.y, min.z);
            vertices[1] = new Vector3(min.x, min.y, max.z);
            vertices[2] = new Vector3(min.x, max.y, min.z);
            vertices[3] = new Vector3(min.x, max.y, max.z);
            vertices[4] = new Vector3(max.x, min.y, min.z);
            vertices[5] = new Vector3(max.x, min.y, max.z);
            vertices[6] = new Vector3(max.x, max.y, min.z);
            vertices[7] = new Vector3(max.x, max.y, max.z);
            Vector3[] adjustedVertices = new Vector3[8];
            for (int j = 0; j < vertices.Length; j++)
            {
                adjustedVertices[j] = portals[i].portalTransform.position + portals[i].portalTransform.rotation * new Vector3(vertices[j].x * portals[i].portalTransform.lossyScale.x, vertices[j].y * portals[i].portalTransform.lossyScale.y, vertices[j].z * portals[i].portalTransform.lossyScale.z);
            }
            portals[i].portalVertices = adjustedVertices;
        }

        foreach(Portal portal in portals)
        {
            portal.otherPortal = OtherPortal(portal);
            portal.portalScaleFactor = portal.otherPortal.portalTransform.lossyScale.x / portal.portalTransform.lossyScale.x;
            portal.portalRotationFactor = Quaternion.AngleAxis(180, portal.otherPortal.portalTransform.up) * (portal.otherPortal.portalTransform.rotation * Quaternion.Inverse(portal.portalTransform.rotation));
            //portal.portalRotationFactor = (portal.otherPortal.portalTransform.rotation * Quaternion.Inverse(portal.portalTransform.rotation));
        }
    }

    private Portal OtherPortal(Portal currentPortal)
    {
        Transform currentPortalTransform = currentPortal.portalTransform;
        Transform otherPortalTransform = null;
        Transform portalGroup = null;
        Transform[] currentPortalParents = currentPortalTransform.GetComponentsInParent<Transform>();
        foreach(Transform portalParent in currentPortalParents)
        {
            if(portalParent.tag == "PortalGroup")
            {
                portalGroup = portalParent;
            }
        }
        Transform[] portalGroupChildren = portalGroup.GetComponentsInChildren<Transform>();
        foreach (Transform groupChild in portalGroupChildren)
        {
            //if "portal" is not the portal this script is attached to, set otherPortal to that portal
            if (groupChild.tag == "Portal" && groupChild != currentPortalTransform)
            {
                otherPortalTransform = groupChild;
            }
        }
        foreach(Portal portal in portals)
        {
            if(portal.portalTransform == otherPortalTransform)
            {
                return portal;
            }
        }
        return null;
    }

    //use this if a portal moves or rotates to update its information
    //make sure this updates the current portal and its connected portal because scale and rotation factors could both change
    //might also think about update for portal material.
    public void UpdatePortal(Portal portal)
    {

    }

    public Vector3 CalculateLocalPosition(Portal portal, Vector3 objectPosition)
    {
        //Vector3 playerLocalDirection = (playerController.transform.position - portal.currentPortal.position).normalized;
        //float playerLocalDistance = Vector3.Distance(playerController.transform.position, portal.currentPortal.position);
        //Vector3 playerLocalPosition = Quaternion.Inverse(portal.currentPortal.rotation) * (playerLocalDirection * playerLocalDistance);

        return Quaternion.Inverse(portal.portalTransform.rotation) * (objectPosition - portal.portalTransform.position);
    }

    public bool InPortalBounds(Portal portal, Vector3 currentPosition)
    {
        return Mathf.Abs(currentPosition.x) <= (portal.portalDimensions.x / 2) && Mathf.Abs(currentPosition.y) <= (portal.portalDimensions.y / 2);
    }

    public bool PortalTeleported(Portal portal, Vector3 currentPosition, float lastDistance)
    {
        return currentPosition.z <= 0 && lastDistance > 0 && InPortalBounds(portal, currentPosition);
    }

    public Vector3 UpdatePosition(Portal portal, Vector3 localPosition)
    {
        return portal.otherPortal.portalTransform.position + (portal.portalRotationFactor * (portal.portalTransform.rotation * localPosition) * portal.portalScaleFactor);
    }
    public Quaternion UpdateRotation(Portal portal, Quaternion objectRotation)
    {
        return portal.portalRotationFactor * objectRotation;
    }
    public Vector3 UpdateScale(Portal portal, Vector3 objectScale)
    {
        return objectScale * portal.portalScaleFactor;
    }

    public Vector4 PlaneRepresentation(Portal portal)
    {
        return new Vector4(-portal.portalPlane.normal.x, -portal.portalPlane.normal.y, -portal.portalPlane.normal.z, -portal.portalPlane.distance);
    }

    /*
    public Transform UpdateTransform(Portal portal, Vector3 localPosition, Transform objectTransform)
    {
        //playerController.transform.localScale *= portalManager.portals[i].otherPortalScaleFactor;
        //playerController.transform.position = portalManager.portals[i].otherPortal.position + (portalManager.portals[i].otherPortalRotationFactor * playerLocalDirection * playerLocalDistance * portalManager.portals[i].otherPortalScaleFactor);
        //playerController.transform.rotation = portalManager.portals[i].otherPortalRotationFactor * playerController.transform.rotation;
        objectTransform.position = portal.otherPortal.portalTransform.position + (portal.portalRotationFactor * (portal.portalTransform.rotation * localPosition) * portal.portalScaleFactor);
        objectTransform.rotation = portal.portalRotationFactor * objectTransform.rotation;
        objectTransform.localScale *= portal.portalScaleFactor;
        return objectTransform;
    }
    */
    public void FadeInPortals()
    {
        /*
        foreach (PortalFade portalFade in portalFades)
        {
            StopAllCoroutines();
            StartCoroutine(FI());

            //if(portalFade.transform != otherPortal)
            //{
            //    portalFade.FadeIn();
            //}
        }
        */
    }

    IEnumerator FI()
    {
        float fadeProgress = 0;
        Color startColor = new Color(1, 1, 1, 0);
        Color endColor = new Color(1, 1, 1, 1);
        while (fadeProgress <= 1)
        {
            fadeProgress += Time.deltaTime / 1f;
            startColor = Color.Lerp(startColor, endColor, fadeProgress);
            portalFadeMat.color = startColor;
            yield return null;
        }
        yield break;
    }

    public void TurnOffPortals(Transform portalParent)
    {
        /*
        foreach (PortalFade portalFade in portalFades)
        {
            if (portalFade.transform.parent.transform == portalParent)
            {
                portalFade.TurnOff();
            }
        }
        */
    }

    public void TurnOnPortals(Transform poralParent)
    {
        /*
        foreach (PortalFade portalFade in portalFades)
        {
            if (portalFade.transform.parent.transform == poralParent)
            {
                portalFade.TurnOn();
            }
        }
        */
    }
}
