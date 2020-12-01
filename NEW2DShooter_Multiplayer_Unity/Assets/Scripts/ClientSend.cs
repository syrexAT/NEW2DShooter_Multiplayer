using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class takes care of all data that gets sent from the client to the server
/// We create a new packet in a block, which disposes it at the end as it inherits from IDisposable
/// In the created packet, we use overload methods from Packet.cs to convert data into bytes and sending it over to the server
/// This works in TCP as well as in UDP.
/// </summary>
public class ClientSend : MonoBehaviour
{
    //just like on the server
    private static void SendTCPData(Packet packet)
    {
        packet.WriteLength(); //to insert packet length at the start
        Client.instance.tcp.SendData(packet);
    }

    private static void SendUDPData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.udp.SendData(packet);
    }

    //send back the ID and the username
    public static void WelcomeReceived()
    {
        using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            packet.Write(Client.instance.myID);
            packet.Write(UIManager.instance.userNameField.text);

            SendTCPData(packet);
        }
    }

    //sending the player position to the server
    public static void PlayerPosition(Vector2 movement)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerPosition))
        {
            packet.Write(movement);

            packet.Write(GameManager.players[Client.instance.myID].transform.rotation);

            SendUDPData(packet);
        }

    }

    //when a projectile gets spawned we send this information to the server
    public static void SpawnProjectile(Projectile projectile, int _shotByPlayer)
    {
        using (Packet packet = new Packet((int)ClientPackets.newProjectile))
        {
            packet.Write(projectile.transform.position);
            packet.Write(_shotByPlayer);

            SendTCPData(packet);
        }
    }

    //if a projectile gets destroyed by colliding with something, we also send this over to the server
    public static void RemoveProjectile(Projectile projectile)
    {
        using (Packet packet = new Packet((int)ClientPackets.newProjectile))
        {
            packet.Write(projectile.transform.position);

            SendTCPData(packet);
        }
    }
}
