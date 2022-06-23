using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Victory : MonoBehaviour
{
    public GameObject thing;
    public GameObject otherThing;
    public int gems = 0;
    void Start()
    {
        otherThing.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(gems == 3)
        {
            otherThing.SetActive(true);
            thing.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            GetComponent<PlayerMovement>().enabled = false;
            GetComponentInChildren<MouseLook>().enabled = false;
        }
    }
}
