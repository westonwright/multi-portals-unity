using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    public Animator questAnim;

    public bool startAnim = false;
    public bool closeAnim = false;
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
            if(questAnim != null)
            {
                if (startAnim)
                {
                    questAnim.SetTrigger("Start");
                }
                if (closeAnim)
                {
                    questAnim.SetTrigger("Close");
                }
            }
            PlayerPrefsX.SetBool("SaveData", true);
            PlayerPrefsX.SetVector3("SpawnPoint", transform.position);
            PlayerPrefsX.SetVector3("PlayerScale", other.transform.localScale);
            PlayerPrefsX.SetBoolArray("Collected", GameManager.collected);
            Debug.Log("Bool Array = " + string.Join("", new List<bool>(PlayerPrefsX.GetBoolArray("Collected")).ConvertAll(i => i.ToString()).ToArray()));
            PlayerPrefs.Save();
            //Application.Quit();

            GetComponent<BoxCollider>().enabled = false;
        }
    }
}
