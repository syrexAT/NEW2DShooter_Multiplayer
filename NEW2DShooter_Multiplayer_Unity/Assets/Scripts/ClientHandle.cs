using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

/// <summary>
/// This class handles incoming packets from the server by reading it and calling a associated method
/// </summary>
public class ClientHandle : MonoBehaviour
{
    //receive welcome packet, read it and send welcomereceived back
    //read the ID I got from the server and equate it on my ID
    public static void Welcome(Packet packet)
    {
        //it is important to read values from packet the same way as we wrote them! so string first, then int!
        string msg = packet.ReadString();
        int myID = packet.ReadInt();

        Debug.Log($"Message from server: {msg}");
        Client.instance.myID = myID;
        ClientSend.WelcomeReceived();

        //passing in the local port that the tcp connection is using
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    //reading the information we get sent from the server when spawning a player
    public static void SpawnPlayer(Packet packet)
    {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector2 position = packet.ReadVector2();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(id, username, position, rotation);
    }

    //reading the players transform position we get sent from the server 
    public static void PlayerPosition(Packet packet)
    {
        int id = packet.ReadInt();
        Vector2 position = packet.ReadVector2();
        Debug.Log(id);

        GameManager.players[id].transform.position = position;
    }

    //reading the players rotation
    public static void PlayerRotation(Packet packet)
    {
        int id = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.players[id].transform.rotation = rotation;
    }

    //reading the position and the ID we get sent from the server to spawn a projectile
    public static void SpawnProjectile(Packet packet)
    {
        int playerID = packet.ReadInt();
        Vector2 position = packet.ReadVector2();

        GameManager.instance.SpawnProjectile(playerID, position);
    }

    //reading what player disconnected and remove him from the game
    public static void PlayerDisconnected(Packet packet)
    {
        int id = packet.ReadInt();

        Destroy(GameManager.players[id].gameObject);
        GameManager.players.Remove(id);
    }
}
