using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml;


namespace TankWars
{
    /// <summary>
    /// This class is to store all the constants used in the game
    /// </summary>
    public static class Constant
    {
        public const int MaxHp = 3;
        public const int ClientSize = 800;
        public const int PortNum = 11000;
        public const int WallSize = 50;
        public const int ProjSize = 30;
        public const int TankSize = 60;
        public const int TurretSize = 50;
        public const int ViewSize = 800;
        public const int MaxPowerupDelay = 1650;
        public const double TankSpeed = 2.9;
        public const double ProjSpeed = 25.0;
        public const int WebPortNum = 80;



        public static int MsPerFrame = 0;
        public static int FramePerShot = 0;
        public static int RespawnRate = 0;



        public const string ImgDirPath = "..\\..\\..\\Resource\\img\\";
        public const string BgFile = "..\\..\\..\\Resource\\img\\Background.png";
        public const string WallFile = "..\\..\\..\\Resource\\img\\WallSprite.png";

       

    }
}
