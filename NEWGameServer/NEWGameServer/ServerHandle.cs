using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace NEWGameServer
{
    class ServerHandle
    {
        public static void WelcomeReceived(int fromClient, Packet packet)
        {
            //make sure to read the data in the same order that we wrote it in the packet!
            //first int, then an string
            int clientIDCheck = packet.ReadInt();
            string username = packet.ReadString();

            Console.WriteLine($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}.");
            //double check that the client has claimed the correct IP
            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player \"{username}\" (ID: {fromClient} has assumed the wrong client ID ({clientIDCheck}!");
            }

            Server.clients[fromClient].SendIntoGame(username);
        }

        public static void PlayerPosition(int fromClient, Packet packet)
        {
            Vector2 position = packet.ReadVector2();
            Quaternion rotation = packet.ReadQuaternion();

            Server.clients[fromClient].player.SetPosition(position, rotation);
        }

        public static void SpawnProjectile(int fromClient, Packet packet)
        {
            Vector2 projectilePos = packet.ReadVector2();
            int playerID = packet.ReadInt();

            Server.clients[fromClient].player.CreateNewNewProjectile(projectilePos, fromClient);
        }

        //public static void ProjectilePosition(int fromClient, Packet packet)
        //{
        //    int projectileID = packet.ReadInt();
        //    Vector2 position = packet.ReadVector2();

        //    Server.clients[fromClient].player.projectiles[projectileID].SetProjectilePosition(position);
        //}
    }
}
