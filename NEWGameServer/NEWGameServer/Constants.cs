using System;
using System.Collections.Generic;
using System.Text;

namespace NEWGameServer
{
    /// <summary>
    /// This class hols constant variable such as Ticks per second and MS per tick
    /// </summary>
    class Constants
    {
        public const int TICKS_PER_SEC = 30; //server runs on 30 ticks per second, so we change the project settings in Unity accordingly, by setting fixed Timestep to 0.033333
        public const int MS_PER_TICK = 1000 / TICKS_PER_SEC;
    }
}
