using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject startMenu;
    public GameObject deathCountGO;
    public TMP_InputField userNameField;
    public TMP_Text deathCountText;
    public float deathCount;

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
        deathCountGO.SetActive(true);
        Client.instance.ConnectToServer(); //calling the clients ConnectToServer method!
    }

    public void IncreaseDeathCount()
    {
        deathCount++; 
        deathCountText.text = $"Deaths: {deathCount}";
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
