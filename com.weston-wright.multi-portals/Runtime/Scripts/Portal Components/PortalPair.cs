using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalPair : MonoBehaviour
{
    [SerializeField]
    private bool visualizeConnection;
    [SerializeField]
    private PortalHolder leftHolder = null;
    public PortalHolder LeftHolder { get => leftHolder; }
    [SerializeField]
    private PortalHolder rightHolder = null;
    public PortalHolder RightHolder { get => rightHolder; }

    private void OnDrawGizmos()
    {
        if (visualizeConnection)
        {
            if(leftHolder != null && rightHolder != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(leftHolder.PortalPlane.transform.position, rightHolder.PortalPlane.transform.position);
                Gizmos.color = Color.white;
            }
        }
    }

    public void Initialize(PortalHolder leftHolder, PortalHolder rightHolder)
    {
        this.leftHolder = leftHolder;
        this.rightHolder = rightHolder;
    }

    private void OnEnable()
    {
        PortalStaticMemebers.portalManagerSO.AddPortalPair(this);
    }
    private void OnDisable()
    {
        PortalStaticMemebers.portalManagerSO.RemovePortalPair(this);
    }
}
