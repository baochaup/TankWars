using Newtonsoft.Json;
using System;
using TankWars;

namespace TankWars
{
    /// <summary>
    /// This class is to create tanks
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        [JsonProperty(PropertyName = "tank")]
        public int ID;

        [JsonProperty(PropertyName = "name")]
        public string name;

        [JsonProperty(PropertyName = "loc")]
        public Vector2D location;

        [JsonProperty(PropertyName = "bdir")]
        public Vector2D orientation;

        [JsonProperty(PropertyName = "tdir")]
        public Vector2D aiming;

        [JsonProperty(PropertyName = "score")]
        public int score;

        [JsonProperty(PropertyName = "hp")]
        public int hitpoints;

        [JsonProperty(PropertyName = "died")]
        public bool died;

        [JsonProperty(PropertyName = "dc")]
        public bool disconnected;

        [JsonProperty(PropertyName = "join")]
        public bool joined;

        public int projFrameCounter;
        public int respawnDelayCounter;
        public int powerUpCount;
        public double accurancy;
        public int projHits;
        public int projNum;


        public Tank()
        {

        }

        public Tank(int playerID, string playerName)
        {
            ID = playerID;
            name = playerName;
            location = new Vector2D(0, 0);
            orientation = new Vector2D(0, -1);
            aiming = new Vector2D(0, -1);
            score = 0;
            hitpoints = Constant.MaxHp;
            died = false;
            disconnected = false;
            joined = false;
            projFrameCounter = Constant.FramePerShot;
            respawnDelayCounter = -1;
            powerUpCount = 0;
            accurancy = 0;
            projHits = 0;
            projNum = 0;


        }


}
}
