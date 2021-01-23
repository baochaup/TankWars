using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TankWars
{
    /// <summary>
    /// This class is to draw all the components for the game
    /// </summary>
    public class DrawingPanel : Panel
    {
        private World theWorld;
        private Image backgroundImg;
        private Image wallImg;
        private Dictionary<string, Image> tankImgs; // store all the tank images
        private Dictionary<string, Image> turretImgs; // store all the turret images
        private Dictionary<string, Image> projImgs; // store all the projectile images
        private SolidBrush[] powerColors; // store all the colors for powers
        private string[] colors; // string array to rotate colors
        private int colorIndex;
        private SolidBrush paintColorBrush; // color for HP bar
        private Font drawFont; // font for player name
        private string playerString; // string for player name
        private Image explosionImg1; // store the image of an explosion
        private Image explosionImg2;
        private List<Explosion> explosions; // store all the explosions in the world
        private Dictionary<int, int> tankColorIndexes;
        private int tankCounter;

        /// <summary>
        /// Initializes all the fields
        /// </summary>
        /// <param name="w"></param>
        public DrawingPanel(World w)
        {
            DoubleBuffered = true;
            theWorld = w;
            tankImgs = new Dictionary<string, Image>();
            turretImgs = new Dictionary<string, Image>();
            projImgs = new Dictionary<string, Image>();
            colorIndex = 0;
            colors = new string[] {"Blue", "Dark", "Green", "LightGreen",
                "Orange", "Purple", "Red", "Yellow"};
            powerColors = new SolidBrush[]
            {
                new SolidBrush(Color.Red),
                new SolidBrush(Color.Yellow),
                new SolidBrush(Color.Black),
                new SolidBrush(Color.Green),
                new SolidBrush(Color.Orange),
                new SolidBrush(Color.Violet),
                new SolidBrush(Color.Blue),
            };
            paintColorBrush = new SolidBrush(Color.White);
            drawFont = new Font("Arial", 12);
            explosionImg1 = Image.FromFile(Constant.ImgDirPath + "Explosion1.png");
            explosionImg2 = Image.FromFile(Constant.ImgDirPath + "Explosion2.png");
            explosions = new List<Explosion>();
            tankColorIndexes = new Dictionary<int, int>();
            tankCounter = 0;

        }

        /// <summary>
        /// Helper method for DrawObjectWithTransform
        /// </summary>
        /// <param name="size">The world (and image) size</param>
        /// <param name="w">The worldspace coordinate</param>
        /// <returns></returns>
        private static int WorldSpaceToImageSpace(int size, double w)
        {
            return (int)w + size / 2;
        }

        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);


        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name = "e" > PaintEventArgs to access the graphics(for drawing)</param>
        /// <param name = "o" > The object to draw</param>
        /// <param name = "worldSize" > The size of one edge of the world(assuming the world is square)</param>
        /// <param name = "worldX" > The X coordinate of the object in world space</param>
        /// <param name = "worldY" > The Y coordinate of the object in world space</param>
        /// <param name = "angle" > The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name = "drawer" > The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, int worldSize, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            int x = WorldSpaceToImageSpace(worldSize, worldX);
            int y = WorldSpaceToImageSpace(worldSize, worldY);
            e.Graphics.TranslateTransform(x, y);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }

        /// <summary>
        /// Method for drawing walls
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            // checks if the image of the wall is already loaded
            // the purpose is to load the image only in 1 frame, not every frame, so improve the performance
            if (wallImg == null) wallImg = Image.FromFile(Constant.WallFile);
            Wall w = o as Wall;
            int location = -(Constant.WallSize / 2);
            Vector2D head = w.endPoint1;
            Vector2D tail = w.endPoint2;

            // check which endpoint is the head
            if (!w.IsEndpoint1Head)
            {
                head = w.endPoint2;
                tail = w.endPoint1;
            }

            // if horizontal wall, increase X by 50 to draw the next wall
            // if vertical wall, increase Y
            if (w.IsHorizontal)
            {
                for (double i = head.GetX(); i <= tail.GetX(); i += 50.0F)
                {
                    e.Graphics.DrawImage(wallImg, location, -(Constant.WallSize / 2), Constant.WallSize, Constant.WallSize);
                    location += 50;
                }
            }
            else
            {
                for (double i = head.GetY(); i <= tail.GetY(); i += 50.0F)
                {
                    e.Graphics.DrawImage(wallImg, -(Constant.WallSize / 2), location, Constant.WallSize, Constant.WallSize);
                    location += 50;
                }
            }
        }

        /// <summary>
        /// Method for drawing tanks
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            Tank tank = o as Tank;
            // because tankCounter is unique to each tank ID, modulus it by 8, will return an index,
            // and we use this index to pick color from the color array
            // so up to 8 tanks will have different colors then they repeat
            if (!tankColorIndexes.ContainsKey(tank.ID))
            {
                tankColorIndexes.Add(tank.ID, tankCounter);
                tankCounter = (tankCounter > 7) ? 0 : ++tankCounter; // If tankCounter is greater than 7 drop to 0, else increment

            }

            colorIndex = tankColorIndexes[tank.ID] % 8;

            // check if the tank image is already in the image dictionary
            // if yes, load the image
            // if no, reuse the image in the dictionary
            // this is to make sure images are loaded only in 1 frame, not every frame
            if (!tankImgs.ContainsKey(colors[colorIndex]))
            {
                Image imgBody = Image.FromFile(Constant.ImgDirPath + colors[colorIndex] + "Tank.png");
                tankImgs.Add(colors[colorIndex], imgBody);
            }

            e.Graphics.DrawImage(tankImgs[colors[colorIndex]], -(Constant.TankSize / 2), -(Constant.TankSize / 2), Constant.TankSize, Constant.TankSize);
            
        }

        /// <summary>
        /// Method for drawing turrets
        /// Turret's location is the same as the tank's
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            if (!turretImgs.ContainsKey(colors[colorIndex]))
            {
                Image imgTurret = Image.FromFile(Constant.ImgDirPath + colors[colorIndex] + "Turret.png");
                turretImgs.Add(colors[colorIndex], imgTurret);
            }

            e.Graphics.DrawImage(turretImgs[colors[colorIndex]], -(Constant.TurretSize / 2), -(Constant.TurretSize / 2), Constant.TurretSize, Constant.TurretSize);
        }

        /// <summary>
        /// Method for drawing projectiles
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ProjDrawer(object o, PaintEventArgs e)
        {
            Projectile proj = o as Projectile;
            colorIndex = proj.owner % 8;
            if (!projImgs.ContainsKey(colors[colorIndex]))
            {
                Image imgProj = Image.FromFile(Constant.ImgDirPath + colors[colorIndex] + "Proj.png");
                projImgs.Add(colors[colorIndex], imgProj);
            }

            e.Graphics.DrawImage(projImgs[colors[colorIndex]], -(Constant.ProjSize / 2), -(Constant.ProjSize / 2), Constant.ProjSize, Constant.ProjSize);
        }

        /// <summary>
        /// Method for drawing powerups
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void PowerDrawer(object o, PaintEventArgs e)
        {
            Powerup power = o as Powerup;
            int index = power.ID % 7; // rotate using 7 colors
            int width = 10;
            int height = 10;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Circles are drawn starting from the top-left corner.
            // So if we want the circle centered on the powerup's location, we have to offset it
            // by half its size to the left (-width/2) and up (-height/2)
            // r1 is the outer circle
            // r2 is the inner circle
            Rectangle r1 = new Rectangle(-(width / 2), -(height / 2), width + 10, height + 10);
            e.Graphics.FillEllipse(powerColors[index], r1);

            Rectangle r2 = new Rectangle(-(width / 2) + 5, -(height / 2) + 5, width, height);
            if (index > 5) index = 0;
            e.Graphics.FillEllipse(powerColors[++index], r2);
        }

        /// <summary>
        /// Method for drawing beams
        /// beam has 2 layers, the outer layer's color is white
        /// the inner layer's color rotate 7 colors to make some visual effect
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void BeamDrawer(object o, PaintEventArgs e)
        {
            Beam beam = o as Beam;

            int index = beam.frameCount % 7; // use the beam's frameCount to rotate colors
            int width = 10;
            int height = 1200; // the beam's length

            // Rectangles are drawn starting from the top-left corner.
            // So if we want the rectangle centered on the player's location, we have to offset it
            // by half its size to the left (-width/2) and up (-height/2)
            using (SolidBrush whiteBrush = new SolidBrush(Color.White))
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Rectangle r1 = new Rectangle(-(width / 2), -height - (Constant.TurretSize / 2), width, height);
                e.Graphics.FillRectangle(whiteBrush, r1);
            }

            // inner layer
            Rectangle r2 = new Rectangle(-(width / 2) + 3, -height - (Constant.TurretSize / 2), width - 6, height);
            e.Graphics.FillRectangle(powerColors[index], r2);
        }

        /// <summary>
        /// Method for drawing HP bar
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void HPDrawer(object o, PaintEventArgs e)
        {
            Tank tank = o as Tank;
            int width = 60;
            int height = 8;

            using (System.Drawing.SolidBrush redBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
            using (System.Drawing.SolidBrush yellowBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow))
            using (System.Drawing.SolidBrush greenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green))
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Rectangles are drawn starting from the top-left corner.
                // So if we want the rectangle centered on the player's location, we have to offset it
                // by half its size to the left (-width/2) and up (-height/2)

                if (tank.hitpoints == 3) paintColorBrush = greenBrush;
                else if (tank.hitpoints == 2)
                {
                    paintColorBrush = yellowBrush;
                    width = (width / 2);
                }
                else if (tank.hitpoints == 1)
                {
                    paintColorBrush = redBrush;
                    width = (width / 4);
                }

                Rectangle r = new Rectangle(-(width / 2), -height - (Constant.TankSize / 2) - 10, width, height);
                e.Graphics.FillRectangle(paintColorBrush, r);
            }
        }

        /// <summary>
        /// Method for drawing player name and score
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void NameDrawer(object o, PaintEventArgs e)
        {
            Tank tank = o as Tank;
            playerString = (tank.name + ": " + tank.score.ToString());

            using (System.Drawing.SolidBrush whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
            {
                e.Graphics.DrawString(playerString, drawFont, whiteBrush, -(Constant.TankSize / 2), (-(Constant.TankSize / 2)) + 60);
            }
        }

        /// <summary>
        /// Method for drawing explosions
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ExplosionDrawer(object o, PaintEventArgs e)
        {
            Explosion exp = o as Explosion;
            e.Graphics.DrawImage(exp.img, -(Constant.TankSize / 2), -(Constant.TankSize / 2), Constant.TankSize, Constant.TankSize);
        }


        // This method is invoked when the DrawingPanel needs to be re-drawn
        protected override void OnPaint(PaintEventArgs e)
        {
            // the world should be drawn only when connecting to server is successful
            // and the client receives player's ID and the world size
            if (theWorld.Tanks.ContainsKey(theWorld.playerID))
            {
                if (backgroundImg == null) backgroundImg = Image.FromFile(Constant.BgFile); // load bg image only in 1 frame
                lock (theWorld)
                {
                    // center the player's view before drawing anything
                    CenterPlayerView(e);

                    // draw the background
                    e.Graphics.DrawImage(backgroundImg, 0.0F, 0.0F, theWorld.worldSize, theWorld.worldSize);

                    // Draw the walls
                    foreach (Wall wall in theWorld.Walls.Values)
                    {
                        // if endpoint1 is the head, use endponint1's location
                        // if not, use endpoint2's
                        if (wall.IsEndpoint1Head)
                            DrawObjectWithTransform(e, wall, theWorld.worldSize, wall.endPoint1.GetX(), wall.endPoint1.GetY(), 0, WallDrawer);
                        else
                            DrawObjectWithTransform(e, wall, theWorld.worldSize, wall.endPoint2.GetX(), wall.endPoint2.GetY(), 0, WallDrawer);
                    }

                    // Draw the tanks
                    foreach (Tank tank in theWorld.Tanks.Values)
                    {
                        if (tank.hitpoints > 0)
                        {
                            // tank
                            DrawObjectWithTransform(e, tank, theWorld.worldSize, tank.location.GetX(), tank.location.GetY(), tank.orientation.ToAngle(), TankDrawer);
                            // turret
                            DrawObjectWithTransform(e, tank, theWorld.worldSize, tank.location.GetX(), tank.location.GetY(), tank.aiming.ToAngle(), TurretDrawer);
                            // HP bar
                            DrawObjectWithTransform(e, tank, theWorld.worldSize, tank.location.GetX(), tank.location.GetY(), 0, HPDrawer);
                            // player name
                            DrawObjectWithTransform(e, tank, theWorld.worldSize, tank.location.GetX(), tank.location.GetY(), 0, NameDrawer);
                        }
                        if (tank.died)
                        {
                            explosions.Add(new Explosion(explosionImg1, tank.location));
                        }

                     
                        
                    }

                    // Draw the explosions
                    // rotate 2 explosion images to create some visual effect
                    if (explosions.Count > 0)
                    {
                        // because of removing explosion after drawing it, need to use ToList()
                        foreach (Explosion exp in explosions.ToList())
                        {
                            DrawObjectWithTransform(e, exp, theWorld.worldSize, exp.location.GetX(), exp.location.GetY(), 0, ExplosionDrawer);
                            // each image lasts for 10 frames
                            if ((exp.frameCount++ % 10) == 0)
                                exp.img = explosionImg2;
                            else if ((exp.frameCount % 20) == 0) // remove after 20 frames
                                explosions.Remove(exp);
                        }
                    }

                    // Draw the projectiles
                    foreach (Projectile proj in theWorld.Projectiles.Values)
                    {
                        if (!proj.died)
                            DrawObjectWithTransform(e, proj, theWorld.worldSize, proj.location.GetX(), proj.location.GetY(), proj.orientation.ToAngle(), ProjDrawer);
                    }

                    // Draw the powerups
                    foreach (Powerup power in theWorld.Powerups.Values)
                    {
                        DrawObjectWithTransform(e, power, theWorld.worldSize, power.location.GetX(), power.location.GetY(), 0, PowerDrawer);
                    }

                    // Draw the beams
                    foreach (Beam beam in theWorld.Beams.Values)
                    {
                        DrawObjectWithTransform(e, beam, theWorld.worldSize, beam.origin.GetX(), beam.origin.GetY(), beam.direction.ToAngle(), BeamDrawer);

                    }
                    foreach (Beam beam in theWorld.Beams.Values.ToList())
                    {
                        if ((beam.frameCount++ % 20) == 0)
                            theWorld.Beams.Remove(beam.ID); //remove beam after 20 frames
                    }

                    // Do anything that Panel (from which we inherit) needs to do
                    base.OnPaint(e);
                }
            }
        }

        /// <summary>
        /// Helper method for centering player's view
        /// </summary>
        /// <param name="e"></param>
        private void CenterPlayerView(PaintEventArgs e)
        {
            double playerX = theWorld.Tanks[theWorld.playerID].location.GetX();
            double playerY = theWorld.Tanks[theWorld.playerID].location.GetY();

            // calculate view/world size ratio
            double ratio = (double)this.Size.Width / (double)theWorld.worldSize;
            int halfSizeScaled = (int)(theWorld.worldSize / 2.0 * ratio);

            double inverseTranslateX = -WorldSpaceToImageSpace(theWorld.worldSize, playerX) + halfSizeScaled;
            double inverseTranslateY = -WorldSpaceToImageSpace(theWorld.worldSize, playerY) + halfSizeScaled;

            e.Graphics.TranslateTransform((float)inverseTranslateX, (float)inverseTranslateY);
        }

        /// <summary>
        /// This class is for drawing explosions
        /// </summary>
        private class Explosion
        {
            public Image img;
            public int frameCount; // use to keep track duration of each explosion
            public Vector2D location;

            public Explosion(Image image, Vector2D loc)
            {
                img = image;
                frameCount = 1;
                location = loc;
            }
        }
    }
}

