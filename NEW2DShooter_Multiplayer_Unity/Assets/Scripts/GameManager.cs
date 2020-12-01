using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class takes care of spawning in the correct player models and projectile prefabs
/// Also takes care of respawning a player and holds a dictionary containing all players
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance; //creating singelton instance
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
        //get the average player position
        Vector2 averagePlayerPosition = new Vector2(0, 0);
        foreach (PlayerManager player in players.Values)
        {
            averagePlayerPosition += (Vector2)player.transform.position;
        }

        averagePlayerPosition /= players.Count;

        //create a list of all spawnpoint Vector2s
        List<Vector2> spawnPositions = new List<Vector2>();
        spawnPositions.Add(new Vector2(-9, -9));
        spawnPositions.Add(new Vector2(9, 9));
        spawnPositions.Add(new Vector2(9, -9));
        spawnPositions.Add(new Vector2(-9, 9));

        Dictionary<float, Vector2> distDic = new Dictionary<float, Vector2>();

        //calculate distance between the averageplayerposition and the spawnposition index
        foreach (Vector2 spawnPosition in spawnPositions)
        {
            float dist = Vector2.Distance(averagePlayerPosition, spawnPosition);
            distDic.Add(dist, spawnPosition);
        }

        List<float> distances = distDic.Keys.ToList();

        //sort it so the lowest one is at the beginning
        distances.Sort();

        Vector2 furthestVector = distDic[distances[distances.Count - 1]];
        Debug.Log("FURTHEST VECTOR" + furthestVector);

        players[id].transform.position = furthestVector;
    }
}
