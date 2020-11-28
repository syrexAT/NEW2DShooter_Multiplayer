using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>(); //this stores all players on the client side

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    public GameObject projectilePrefab;

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

    public void SpawnPlayer(int id, string username, Vector2 position, Quaternion rotation)
    {
        GameObject player;
        if (id == Client.instance.myID) //if the player we spawning is the local player
        {
            player = Instantiate(localPlayerPrefab, position, rotation);
        }
        else
        {
            player = Instantiate(playerPrefab, position, rotation);
        }

        player.GetComponent<PlayerManager>().id = id;
        player.GetComponent<PlayerManager>().username = username;
        players.Add(id, player.GetComponent<PlayerManager>());
    }

    public void SpawnProjectile(int id, Vector2 position)
    {
        GameObject projectile = Instantiate(projectilePrefab, position, players[id].transform.rotation);
        //players[id].projectiles.Add(projectileID, projectile.GetComponent<Projectile>());
    }

    public void RespawnPlayer(int id)
    {
        switch (id)
        {
            case 1:
                players[id].transform.position = new Vector2(-9, -9);
                break;
            case 2:
                players[id].transform.position = new Vector2(9, 9);
                break;
            case 3:
                players[id].transform.position = new Vector2(9, -9);
                break;
            case 4:
                players[id].transform.position = new Vector2(-9, +9);
                break;
            default:
                Debug.Log("Spawned Player exceeds Playerlimit!");
                break;
        }
    }
}
