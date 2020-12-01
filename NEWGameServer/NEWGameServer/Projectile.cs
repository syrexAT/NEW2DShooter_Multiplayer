﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace NEWGameServer
{
    class Projectile
    {
        public int id;
        public Vector2 position;
        public Player player;

        public Projectile(Vector2 Position, Player Player)
        {
            position = Position;
            player = Player;
        }
    }
}
