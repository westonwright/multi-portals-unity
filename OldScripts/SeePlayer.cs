using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SeePlayer : MonoBehaviour
{
    public bool hallway = false;
    public bool corner = false;
    public bool other = false;

    Animator camAnim {get { return GetComponentInParent<Animator>(); } }

    bool caught = false;
    void Start()
    {
        if (hallway)
        {
            camAnim.SetBool("Hallway", true);

        }
        if (corner)
        {
            camAnim.SetBool("Corner", true);
        }
        if (other)
        {
            camAnim.SetBool("Other", true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(caught == true)
        {
            if (Input.GetKey(KeyCode.R))
            {
                SceneManager.LoadScene(1);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            PlayerPrefsX.SetBool("Startup", true);
            SceneManager.LoadScene(1);
            //other.GetComponent<PlayerMovement>().enabled = false;
            //add something for UI
            caught = true;
        }
    }
}
