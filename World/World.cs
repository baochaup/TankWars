using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace TankWars
{
    /// <summary>
    /// This class is to create the world
    /// </summary>
    public class World
    {

        public int playerID = -1;
        public int worldSize;
        public Dictionary<int, Tank> Tanks;
        public Dictionary<int, Powerup> Powerups;
        public Dictionary<int, Wall> Walls;
        public Dictionary<int, Beam> Beams;
        public Dictionary<int, Projectile> Projectiles;
        public Control Control;

        public World()
        {
            Tanks = new Dictionary<int, Tank>();
            Powerups = new Dictionary<int, Powerup>();
            Walls = new Dictionary<int, Wall>();
            Beams = new Dictionary<int, Beam>();
            Projectiles = new Dictionary<int, Projectile>();
            Control = new Control();
            worldSize = 0;
        }
    }
}
