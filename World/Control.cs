using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace TankWars
{
    /// <summary>
    /// This class is to create control for the player
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Control
    {
        [JsonProperty(PropertyName = "moving")]
        public string Moving = "none";
        [JsonProperty(PropertyName = "fire")]
        public string Fire = "none";
        [JsonProperty(PropertyName = "tdir")]
        public Vector2D Aiming = new Vector2D(0, -1);
       
        public Control()
        {

        }

     
    }
}
