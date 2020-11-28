using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int id;
    public Rigidbody2D rigBody;
    //public int shotByPlayer;
    private float flyingSpeed = 7.5f;


    // Start is called before the first frame update
    void Start()
    {
        //id = nextProjectileID;
        //nextProjectileID++;
        //projectiles.Add(id, this);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //ClientSend.ProjectilePosition(this);
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
