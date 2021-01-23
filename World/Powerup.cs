using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace TankWars
{
    /// <summary>
    /// This class is to create powerups for the world
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Powerup
    {
        [JsonProperty(PropertyName = "power")]
        public int ID;
        [JsonProperty(PropertyName = "loc")]
        public Vector2D location;
        [JsonProperty(PropertyName = "died")]
        public bool died;

        public static int powerUpIdCounter;
        public static int frameCounter;
        public static int randomFrame;

        public Powerup()
        {
        }


        public Powerup(int powerID, Vector2D loc)
        {
            ID = powerID;
            location = loc;
            died = false;
            
        }
    }



}
