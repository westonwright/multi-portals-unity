using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevealPortal : MonoBehaviour
{
    public GameObject portal;
    private void Start()
    {
        portal.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            portal.SetActive(true);
        }
    }
}
