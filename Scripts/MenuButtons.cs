using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtons : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    public void Quit()
    {
        Application.Quit();
    }

    public void SaveGame()
    {

    }

    public void LoadGame()
    {
        PlayerPrefsX.SetBool("Startup", true);
        SceneManager.LoadScene(1);
    }

    public void NewGame()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(1);
        PlayerPrefsX.SetBool("Started", false);
        PlayerPrefsX.SetBool("Restart", true);
        PlayerPrefsX.SetBool("Startup", true);
    }

    /*public void SaveButton()
    {
        //PlayerPrefsX.SetBool("SaveData", true);
        //PlayerPrefs.SetFloat("TimePassed", GameManager.TimePassed);
        //PlayerPrefsX.SetBoolArray("Collected", GameManager.collected);
        //PlayerPrefsX.SetBoolArray("Destroyed", GameManager.destroyed);
        //PlayerPrefsX.SetVector3("PlayerPosition", GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>().position);
        //PlayerPrefsX.SetBoolArray("PlayerHealth", GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().lightOff);
        //alphaValue = 1;
        //saving = true;
    }*/
}
