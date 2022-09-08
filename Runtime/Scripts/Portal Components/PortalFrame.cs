using UnityEngine;

public class PortalFrame : MonoBehaviour
{
    [SerializeField]
    private bool previewBounds = true;
    [SerializeField]
    private PortalPlane portalPlane;
    public PortalPlane PortalPlane { get => portalPlane; }

    // these form a box
    // we will scale this box to find out
    // how much we need to scale the whole frame
    // the min point can also act as an anchor for the portal?
    [SerializeField]
    private Vector3 minPoint = -Vector3.one;
    [SerializeField]
    private Vector3 maxPoint = Vector3.one;

    private bool initialized = false;
    public bool Initialized { get => initialized; }

    private void OnDrawGizmos()
    {
        if (!previewBounds) return;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        // fix this so the spheres dont get distorted
        Gizmos.color = Color.red;
        Vector3 minSpherePos = new Vector3(
                minPoint.x * transform.lossyScale.x,
                minPoint.y * transform.lossyScale.y,
                minPoint.z * transform.lossyScale.z
            );
        Vector3 maxSpherePos = new Vector3(
                maxPoint.x * transform.lossyScale.x,
                maxPoint.y * transform.lossyScale.y,
                maxPoint.z * transform.lossyScale.z
            );
        Gizmos.DrawWireSphere(
            minSpherePos,
            .1f
            );
        Gizmos.DrawWireSphere(
            maxSpherePos,
            .1f);
        Gizmos.DrawWireCube(
            Vector3.Lerp(minSpherePos, maxSpherePos, .5f),
            maxSpherePos - minSpherePos 
            );
    }

    public void Initialize(Mesh portalMesh, Vector3 stretch)
    {
        transform.localScale = Vector3.one;
        if (!initialized)
        {
            GameObject go = new GameObject("Portal Plane");
            go.AddComponent<MeshFilter>();
            go.GetComponent<MeshFilter>().sharedMesh = portalMesh;
            go.AddComponent<MeshRenderer>();
            portalPlane = go.AddComponent<PortalPlane>();
            portalPlane.MeshRenderer.sharedMaterial = PortalStaticMemebers.defaultPortalMaterial;
            // make sure to tell the plane to recalculate its vertices?
        }
        Vector3 trueMin = new Vector3(
            Mathf.Min(minPoint.x, maxPoint.x),
            Mathf.Min(minPoint.y, maxPoint.y),
            Mathf.Min(minPoint.z, maxPoint.z)
            );
        Vector3 trueMax = new Vector3(
            Mathf.Max(minPoint.x, maxPoint.x),
            Mathf.Max(minPoint.y, maxPoint.y),
            Mathf.Max(minPoint.z, maxPoint.z)
            );
        Bounds frameBounds = new Bounds();
        frameBounds.min = trueMin;
        frameBounds.max = trueMax;

        Vector3 stretchMin = new Vector3(
            portalMesh.bounds.min.x * stretch.x,
            portalMesh.bounds.min.y * stretch.y,
            portalMesh.bounds.min.z * stretch.z
            );
        Vector3 stretchMax = new Vector3(
            portalMesh.bounds.max.x * stretch.x,
            portalMesh.bounds.max.y * stretch.y,
            portalMesh.bounds.max.z * stretch.z
            );
        Bounds stretchBounds = new Bounds();
        stretchBounds.min = stretchMin;
        stretchBounds.max = stretchMax;

        Vector3 ratio = new Vector3(
            stretchBounds.size.x /frameBounds.size.x,
            stretchBounds.size.y / frameBounds.size.y,
            stretchBounds.size.z / frameBounds.size.z
            );

        transform.localScale = new Vector3(
            transform.localScale.x * ratio.x,
            transform.localScale.y * ratio.y,
            transform.localScale.z * ratio.z
            );

        Vector3 frameWorldMin = transform.TransformPoint(trueMin);
        portalPlane.transform.localScale = stretch;
        portalPlane.transform.position = frameWorldMin;
        portalPlane.transform.rotation = transform.rotation;
        portalPlane.transform.position -= transform.rotation * stretchMin;

        // moves the portal to the anchor point
        // then scales the frame to match the portal
        // then makes the portal the child of the frame
        initialized = true;
    }
    public void DeInitialize()
    {
        if(portalPlane != null)
        {
            // might also need to remove the portals form the manager?
            DestroyImmediate(portalPlane.gameObject);
            portalPlane = null;
            transform.localScale = Vector3.one;
        }
        initialized = false;
    }

    // add parent removal protection like on other objects
}
