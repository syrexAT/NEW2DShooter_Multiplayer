using System;
using System.Threading;

namespace NEWGameServer
{
    class Program
    {
        private static bool isRunning = false;

        static void Main(string[] args)
        {
            Console.Title = "Game Server";
            isRunning = true;

            //create new therad to run our game loop
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(4, 26950);


        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
            //creating new local datetime object, storing the exact time when the next server tick shoudl be executed
            DateTime nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (nextLoop < DateTime.Now)
                {
                    GameLogic.Update();

                    //update the time of when the enxt tick should happen
                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if (nextLoop > DateTime.Now) //check if its in the future
                    {
                        Thread.Sleep(nextLoop - DateTime.Now); //setting it to sleep until its time to execute the next tick
                    }
                }
            }
        }
    }
}
