using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    //ID und username zurückschicken
    public static void WelcomeReceived()
    {
        using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            packet.Write(Client.instance.myID);
            packet.Write(UIManager.instance.userNameField.text);

            SendTCPData(packet);
        }
    }

    public static void PlayerPosition(Vector2 movement)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerPosition))
        {
            packet.Write(movement);

            packet.Write(GameManager.players[Client.instance.myID].transform.rotation);

            SendUDPData(packet);
        }

    }

    public static void SpawnProjectile(Projectile projectile, int _shotByPlayer)
    {
        using (Packet packet = new Packet((int)ClientPackets.newProjectile))
        {
            packet.Write(projectile.transform.position);
            packet.Write(_shotByPlayer);

            SendTCPData(packet);
        }
    }

    //public static void ProjectilePosition(Projectile projectile)
    //{
    //    using (Packet packet = new Packet((int)ClientPackets.projectilePosition))
    //    {
    //        packet.Write(projectile.id);
    //        packet.Write(projectile.transform.position);

    //        SendTCPData(packet);
    //    }
    //}



}
