using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is on the projectile prefab, it controls it and calling RespawnPlayer methods if players get hit
/// </summary>
public class Projectile : MonoBehaviour
{
    public int id;
    public Rigidbody2D rigBody;
    private float flyingSpeed = 7.5f;

    void FixedUpdate()
    {
        transform.position += transform.up * flyingSpeed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //TODO: Respawn player
            GameManager.instance.RespawnPlayer(other.gameObject.GetComponent<PlayerManager>().id);
        }
        else if (other.gameObject.CompareTag("LocalPlayer"))
        {
            GameManager.instance.RespawnPlayer(other.gameObject.GetComponent<PlayerManager>().id);
            UIManager.instance.IncreaseDeathCount();
        }

        Destroy(this.gameObject);
    }


}
