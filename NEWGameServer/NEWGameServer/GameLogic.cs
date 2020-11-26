using System;
using System.Collections.Generic;
using System.Text;

namespace NEWGameServer
{
    class GameLogic
    {
        //to run the updatemain method here 
        public static void Update()
        {
            //looping through all playerse to update position
            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    client.player.Update();
                }
            }

            ThreadManager.UpdateMain();
        }
    }
}
