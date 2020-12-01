using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the camera, to follow the player, pretty simple
/// </summary>
public class CameraController : MonoBehaviour
{
    private GameObject player;

    private void Start()
    {

    }

    private void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("LocalPlayer");
        }
        if (player != null)
        {
            transform.position = player.transform.position;
        }
    }
}
