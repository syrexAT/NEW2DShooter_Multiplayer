using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
public class ClientHandle : MonoBehaviour
{
    //welcome packet erhalten, auslesen und welcomerecieved zurücksenden
    //noch die id die mir der server geschicckt hat auf meine id gleichweisen
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

    public static void SpawnPlayer(Packet packet)
    {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector2 position = packet.ReadVector2();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(id, username, position, rotation);
    }

    public static void PlayerPosition(Packet packet)
    {
        int id = packet.ReadInt();
        Vector2 position = packet.ReadVector2();
        Debug.Log(id);

        GameManager.players[id].transform.position = position;
    }

    public static void PlayerRotation(Packet packet)
    {
        int id = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.players[id].transform.rotation = rotation;
    }
}
