using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameObject[] collectables;
    //an array that tracks if a collectable has been collected or not
    public static bool[] collected;
    //an int that describes where in the array of collectables the most recently collected object was
    public static int collectableCollected;
    //an int that counts how many collectables the player has gotten
    public static int collectableCount = 0;
    //a bool that tracks when the player has actually made contact with a collectable
    public static bool collectableHit;

    public static bool restart;

    public static bool startup;

    void Start()
    {
        Debug.Log("loaded");
        //DontDestroyOnLoad(gameObject);
        restart = PlayerPrefsX.GetBool("Restart");
        Debug.Log("startup " + PlayerPrefsX.GetBool("Startup"));
        startup = PlayerPrefsX.GetBool("Startup");
        PlayerPrefsX.GetBool("Startup", false);
    }

    void Update()
    {
        if (startup == true && SceneManager.GetActiveScene().buildIndex == 1 && PlayerPrefsX.GetBoolArray("Collectable") != null)
        {
            collectables = GameObject.FindGameObjectsWithTag("Collectable");
            collectableCount = 0;
            collectables = GameObject.FindGameObjectsWithTag("Collectable");
            collected = new bool[collectables.Length];
            if(PlayerPrefsX.GetBoolArray("Collected") != null)
            {
                if (PlayerPrefsX.GetBoolArray("Collected").Length == collected.Length)
                {
                    Debug.Log("Correct Length");
                    collected = PlayerPrefsX.GetBoolArray("Collected");
                }
            }
            for (int i = 0; i < collectables.Length; i++)
            {
                //runs if the saved data says a specific object was collected
                if (collected[i] == true)
                {
                    //counts up the collectables
                    collectableCount++;
                    //deactivates collectables that were already collected based on save data
                    collectables[i].SetActive(false);

                }
            }
            startup = false;
        }

        if (restart == true && SceneManager.GetActiveScene().buildIndex == 1)
        {
            collected = new bool[collectables.Length];
            restart = false;
            PlayerPrefsX.SetBool("Restart", false);
        }

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            //bit of a placehoder. Would prever to have a victory screen showing how long it took to beat
        }
            if (collectableHit == true)
        {
            //deactivates the collectable that was just hit
            collectables[collectableCollected].SetActive(false);
            //records that something was collected and where it was in the array for the save game
            collected[collectableCollected] = true;
            //turns off collectable hit bool so loop runs once
            collectableHit = false;
        }
        if (Input.GetKeyUp(KeyCode.U))
        {
            Debug.Log(PlayerPrefsX.GetBoolArray("Collected").Length);
            bool[] testArray = PlayerPrefsX.GetBoolArray("Collected");
            Debug.Log(testArray[0]);
            Debug.Log(testArray[1]);
            Debug.Log(testArray[2]);
            Debug.Log(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
