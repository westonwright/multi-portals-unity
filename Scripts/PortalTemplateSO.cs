using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// doesnt have any use yet
// could be used to quickly create portals following a template?
[CreateAssetMenu(fileName = "Portal Template", menuName = "Portals")]
public class PortalTemplateSO : ScriptableObject
{
    [SerializeField]
    private Mesh mesh;
    [SerializeField]
    private Vector3 ratio;
}
