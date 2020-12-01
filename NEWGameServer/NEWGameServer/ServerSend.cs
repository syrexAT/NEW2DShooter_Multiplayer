using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace NEWGameServer
{
    /// <summary>
    /// In this class we define the methods to create the packets to send over the network!
    /// </summary>
    class ServerSend
    {
        //this method is in charge of preparing the packet to be sent
        private static void SendTCPData(int toClient, Packet packet)
        {
            packet.WriteLength(); //this will take the length of the bytelist that we want to send, and insert that at the beginning of the packet!
            Server.clients[toClient].tcp.SendData(packet);
        }

        private static void SendUDPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].udp.SendData(packet);
        }

        private static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }

        //sending TCP Data to all expect to the client we received the data from
        private static void SendTCPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != exceptClient)
                {
                    Server.clients[i].tcp.SendData(packet);
                }
            }
        }

        private static void SendUDPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }

        private static void SendUDPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != exceptClient)
                {
                    Server.clients[i].udp.SendData(packet);
                }
            }
        }

        //packet to send to clients
        public static void Welcome(int toClient, string msg)
        {
            //since our packet class inherits from IDisposable, we need to make sure to dispose it when we are done with it
            //we could either call the packet dispose method at the end
            //or define the packet instance inside a using block! this will automatically dispose it for us and it is much cleaner
            using (Packet packet = new Packet((int)ServerPackets.welcome)) //when creating a packet, which we wnat so send, we need to pass it an ID --> Serverpackets.welcome
            {
                packet.Write(msg); //writing message to the packet
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }

        public static void SpawnPlayer(int toClient, Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                packet.Write(player.id);
                packet.Write(player.username);

                switch (player.id)
                {
                    case 1:
                        packet.Write(new Vector2(player.position.X - 9, player.position.Y - 9));
                        break;
                    case 2:
                        packet.Write(new Vector2(player.position.X + 9, player.position.Y + 9));
                        break;
                    case 3:
                        packet.Write(new Vector2(player.position.X + 9, player.position.Y - 9));
                        break;
                    case 4:
                        packet.Write(new Vector2(player.position.X - 9, player.position.Y + 9));
                        break;
                    default:
                        Console.WriteLine("Spawned Player exceeds Playerlimit!");
                        break;
                }

                packet.Write(player.rotation);

                SendTCPData(toClient, packet);
            }
        }

        public static void PlayerPosition(Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerPosition))
            {
                packet.Write(player.id);
                packet.Write(player.position);
                SendUDPDataToAll(player.id, packet);
            }
        }

        public static void PlayerRotation(Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerRotation))
            {
                packet.Write(player.id);
                packet.Write(player.rotation);

                SendUDPDataToAll(player.id, packet);
            }
        }

        public static void SpawnProjectile(int toClient, Projectile projectile)
        {
            using (Packet packet = new Packet((int)ServerPackets.spawnProjectile))
            {
                packet.Write(projectile.player.id);
                packet.Write(projectile.position);

                SendTCPData(toClient, packet);
            }
        }

        public static void PlayerDisconnected(int playerID)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerDisconnected))
            {
                packet.Write(playerID);

                SendTCPDataToAll(packet);
            }
        }
    }
}
