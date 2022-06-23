using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectablePickup : MonoBehaviour
{
    //Animator collectAnim {get { return GetComponent<Animator>(); } }

    void Start()
    {
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            //other.GetComponent<Victory>().gems++;
            GameManager.collectableCount++;
            //GameManager.collectableCollected = System.Array.IndexOf(GameManager.collectables, gameObject);
            GameManager.collectableHit = true;
            gameObject.SetActive(false);
        }
    }
}
