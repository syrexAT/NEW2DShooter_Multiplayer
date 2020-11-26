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
        //public Quaternion rotation;
        public Quaternion rotation; //because 2d 

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
            //ServerSend.PlayerRotation(this);
        }

        private void Move(Vector2 _position)
        {

        }

        public void SetPosition(Vector2 _position, Quaternion _rotation)
        {
            position += _position * moveSpeed;
            rotation = _rotation;
        }
    }
}
