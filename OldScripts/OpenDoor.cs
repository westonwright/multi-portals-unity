using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public Animator questAnim;
    public Animator warning;
    Animator doorOpen { get { return GetComponentInParent<Animator>(); } }
    PickUp pickUp;
    void Start()
    {
        pickUp = GameObject.FindGameObjectWithTag("Player").GetComponent<PickUp>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Selected")
        {
            questAnim.SetTrigger("Close");
            doorOpen.SetTrigger("Open");
            //pickUp.drop = true;
            Destroy(other.gameObject);

        }
    }
}
