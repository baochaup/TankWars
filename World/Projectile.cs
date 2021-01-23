using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace TankWars
{
    /// <summary>
    /// This class is to create projectiles for tanks
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        [JsonProperty (PropertyName = "proj")]
        public int ID;
        [JsonProperty(PropertyName = "loc")]
        public Vector2D location;
        [JsonProperty(PropertyName = "dir")]
        public Vector2D orientation;
        [JsonProperty(PropertyName = "died")]
        public bool died;
        [JsonProperty(PropertyName = "owner")]
        public int owner;

        public static int projIdCounter;


        public Projectile(int projId, Vector2D loc, Vector2D ori, int projOwner)
        {
            this.ID = projId;
            this.location = loc;
            this.orientation = ori;
            this.owner = projOwner;

        }

        public static Vector2D CalculateNewLocation(Vector2D loc, Vector2D ori)
        {
            ori *= Constant.ProjSpeed;
            return (loc += ori);

        }
    }
}
