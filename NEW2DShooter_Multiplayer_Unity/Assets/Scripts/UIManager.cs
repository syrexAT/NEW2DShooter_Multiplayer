using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject startMenu;
    public TMP_InputField userNameField;

    public Camera cam;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(this);
        }
    }

    public void ConnectToServer() //gets called when connect button is pressed
    {
        //Destroy(cam.gameObject);
        startMenu.SetActive(false);
        userNameField.interactable = false;
        Client.instance.ConnectToServer(); //calling the clients ConnectToServer method!
    }
}
