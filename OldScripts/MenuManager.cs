using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    public PlayerMovement playerMovement;
    public MouseLook mouseLook;

    public GameObject escapeMenu;
    bool toggleOn = false;
    GameObject player;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        escapeMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            toggleOn = !toggleOn;
            if (toggleOn)
            {
                ToggleOn();
            }
            else
            {
                ToggleOff();
            }
        }
    }

    public void ToggleOn()
    {
        escapeMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        playerMovement.enabled = false;
        mouseLook.enabled = false;
    }
    public void ToggleOff()
    {
        escapeMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        playerMovement.enabled = true;
        mouseLook.enabled = true;
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void Save()
    {
        PlayerPrefsX.SetBool("SaveData", true);
        PlayerPrefsX.SetVector3("SpawnPoint", player.transform.position);
        PlayerPrefsX.SetVector3("PlayerScale", player.transform.localScale);
        PlayerPrefsX.SetBoolArray("Collected", GameManager.collected);
        Debug.Log("Bool Array = " + string.Join("", new List<bool>(PlayerPrefsX.GetBoolArray("Collected")).ConvertAll(i => i.ToString()).ToArray()));
        PlayerPrefs.Save();
    }
}
