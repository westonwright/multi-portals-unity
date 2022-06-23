using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class revealPortal2 : MonoBehaviour
{
    public GameObject portal;
    private void Start()
    {
        portal.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Selected")
        {
            portal.SetActive(true);
        }
    }
}
