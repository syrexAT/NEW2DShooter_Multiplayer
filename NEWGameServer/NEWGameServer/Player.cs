using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace NEWGameServer
{
    //all player related data and logic
    class Player
    {
        public int id;
        public string username;

        public Vector2 position;
        public Quaternion rotation;

        public List<Projectile> projectiles = new List<Projectile>(); //this stores all projectiles on the server of the player

        private float moveSpeed = 5f / Constants.TICKS_PER_SEC; //same as multiplying Time.DeltaTime

        public Player(int _id, string _username, Vector2 _spawnPosition)
        {
            id = _id;
            username = _username;
            position = _spawnPosition;
            rotation = Quaternion.Identity;
        }

        public void Update()
        {
            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);

            //foreach (Projectile projectile in projectiles.Values)
            //{
            //    projectile.Update();
            //}
        }

        public void SetPosition(Vector2 _position, Quaternion _rotation)
        {
            position = _position;
            rotation = _rotation;
        }

        public void CreateNewNewProjectile(Vector2 projectilePos, int fromClient)
        {
            projectiles.Add(new Projectile(projectilePos, this));

            //this will send the newly spawned projectile information to all other player
            foreach (Client client in Server.clients.Values)
            {
                //using this to send the information of all other players that are already connected to our new player
                if (client.player != null)
                {
                    if (client.id != fromClient)
                    {
                        ServerSend.SpawnProjectile(client.id, projectiles[projectiles.Count -1]);
                    }
                }
            }
        }
    }
}
