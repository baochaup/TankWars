using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace TankWars
{
    /// <summary>
    /// This class is to create walls
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        [JsonProperty(PropertyName = "wall")]
        public int ID;
        [JsonProperty(PropertyName = "p1")]
        public Vector2D endPoint1;
        [JsonProperty(PropertyName = "p2")]
        public Vector2D endPoint2;

        public double x1 = 0;
        public double x2 = 0;
        public double y1 = 0;
        public double y2 = 0;
        public double halfWallSize = Constant.WallSize / 2;

        /// <summary>
        /// Check if the block of walls is horizontal
        /// </summary>
        public bool IsHorizontal
        {
            get
            {
                if (endPoint2.GetX() == endPoint1.GetX())
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Check if endpoint1 is the head of the block
        /// </summary>
        public bool IsEndpoint1Head
        {
            get
            {
                if ((IsHorizontal && endPoint2.GetX() < endPoint1.GetX())
                    || (!IsHorizontal && endPoint2.GetY() < endPoint1.GetY()))
                    return false;
                return true;
            }
        }

        public Wall()
        {
           

        }

    }
}
