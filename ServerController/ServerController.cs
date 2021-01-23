using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NetworkUtil;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Xml;

namespace TankWars
{
    /// <summary>
    /// This class is the server controller for controlling new connections and update the world
    /// </summary>
    public class ServerController
    {
        // A set of clients that are connected.
        public Dictionary<long, SocketState> clients;
        // a set of command requests sent by clients
        private Dictionary<long, Control> clientControlMessages;
        public World theServerWorld;
        private string playerName;
        private StringBuilder tankString; // save JSON string for the tank
        private StringBuilder projString;
        private StringBuilder beamString;
        private StringBuilder powerUpString;
        private StringBuilder totalDataString; // the string the whole world
        private Vector2D rightVector;
        private Vector2D leftVector;
        private Vector2D upVector;
        private Vector2D downVector;
        private HashSet<long> disconnectedClients; // a set to store the disconnected clients
        public Dictionary<int, Tank> disconnectedTanks; // aset to store the disconnected tanks


        public long GameDuration
        {
            get;
            private set;
        }
        public Dictionary<long, string> PlayerNames
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializing all the needed objects
        /// </summary>
        public ServerController()
        {
            clients = new Dictionary<long, SocketState>();
            clientControlMessages = new Dictionary<long, Control>();
            PlayerNames = new Dictionary<long, string>();
            theServerWorld = new World();
            theServerWorld.worldSize = 0;
            playerName = "";
            tankString = new StringBuilder();
            projString = new StringBuilder();
            beamString = new StringBuilder();
            powerUpString = new StringBuilder();
            totalDataString = new StringBuilder();
            Projectile.projIdCounter = 0;
            Powerup.powerUpIdCounter = 0;
            Powerup.frameCounter = 0;
            Powerup.randomFrame = 0;
            Beam.beamIdCounter = 0;
            rightVector = new Vector2D(1, 0);
            leftVector = new Vector2D(-1, 0);
            upVector = new Vector2D(0, -1);
            downVector = new Vector2D(0, 1);
            disconnectedClients = new HashSet<long>();
            GameDuration = 0;
            disconnectedTanks = new Dictionary<int, Tank>();
        }

        /// <summary>
        /// Start accepting Tcp sockets connections from clients
        /// </summary>
        public void StartServer()
        {
            // This begins an "event loop"
            Networking.StartServer(NewClientConnected, Constant.PortNum);
            string xmlFileString = "..\\..\\..\\Resource\\settings.xml";
            LoadWorldConfig(xmlFileString); // load config from xml file

            Console.WriteLine("GameServer is running");
        }

        private void NewClientConnected(SocketState state)
        {
            if (state.ErrorOccured)
                return;

            state.OnNetworkAction = ReceivePlayerName;
            Networking.GetData(state);
        }

        private void ReceivePlayerName(SocketState state)
        {
            lock (state)
            {
                if (state.ErrorOccured)
                {
                    RemoveClient(state.ID);
                    return;
                }
                string[] lines = state.GetData().Split('\n');
                playerName = lines[0]; // save player name
                PlayerNames.Add(state.ID, playerName);
                state.RemoveData(0, playerName.Length);
            }
            Console.WriteLine(playerName + " joined the game.");

            // send ID and world size to client
            string sendMessage = state.ID + "\n" + theServerWorld.worldSize + "\n";
            Networking.Send(state.TheSocket, sendMessage);

            // Save the client state
            // Need to lock here because clients can disconnect at any time
            lock (clients)
            {
                clients[state.ID] = state;
            }
            SendWalls(state); // send walls info to the client
            WallCollisionSize(); // extend the wall detection size
            Tank tank = new Tank((int)state.ID, playerName);
            RespawnTank(tank);
            theServerWorld.Tanks.Add((int)state.ID, tank);

            state.OnNetworkAction = ReceiveClientData;
            // Continue the event loop that receives messages from this client
            Networking.GetData(state);

        }

        /// <summary>
        /// Given the data that has arrived so far, 
        /// potentially from multiple receive operations, 
        /// determine if we have enough to make a complete message,
        /// and process it (print it and broadcast it to other clients).
        /// </summary>
        /// <param name="sender">The SocketState that represents the client</param>
        private void ReceiveClientData(SocketState state)
        {
            lock (state)
            {
                string totalData = state.GetData();
                //Console.WriteLine("Connected with stateID: " + state.ID);
                string[] parts = Regex.Split(totalData, @"(?<=[\n])");

                // Loop until we have processed all messages.
                // We may have received more than one.
                foreach (string p in parts)
                {
                    // Ignore empty strings added by the regex splitter
                    if (p.Length == 0)
                        continue;
                    // The regex splitter will include the last string even if it doesn't end with a '\n',
                    // So we need to ignore it if this happens. 
                    if (p[p.Length - 1] != '\n')
                        break;
                    // Process the message sent by client
                    ProcessMessage(p, state);
                    // Remove it from the SocketState's growable buffer
                    state.RemoveData(0, p.Length);
                }
                Networking.GetData(state); // continue to receive data
            }
        }

        /// <summary>
        /// This method is to process the message sent by client
        /// convert it to control object and update control in controlmessages
        /// </summary>
        /// <param name="message"></param>
        /// <param name="state"></param>
        private void ProcessMessage(string message, SocketState state)
        {
            Control rebuiltControl = JsonConvert.DeserializeObject<Control>(message);
            lock (clientControlMessages)
            {
                clientControlMessages[state.ID] = rebuiltControl; // Replace the control for specific client

            }
        }

        /// <summary>
        /// This method is to update the movement of the tank
        /// </summary>
        /// <param name="stateID"></param>
        /// <param name="control"></param>
        private void ControlUpdate(long stateID, Control control)
        {
            if (control != null)
            {
                UpdateMovement(theServerWorld.Tanks[(int)stateID], control.Moving, control.Aiming);
                FireUpdate(theServerWorld.Tanks[(int)stateID], control);
            }
        }

        /// <summary>
        /// This method checks if client fires a projectile or a beam
        /// </summary>
        /// <param name="tank"></param>
        /// <param name="control"></param>
        private void FireUpdate(Tank tank, Control control)
        {
            if (tank.hitpoints > 0)
            {
                switch (control.Fire)
                {
                    case "main":
                        ProjectileUpdate(tank, control.Aiming);
                        break;
                    case "alt":
                        BeamUpdate(tank, control.Aiming);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// This method computes the beam and update it accordingly
        /// </summary>
        /// <param name="tank"></param>
        /// <param name="aiming"></param>
        private void BeamUpdate(Tank tank, Vector2D aiming)
        {
            lock (theServerWorld)
            {
                if (tank.powerUpCount > 0)
                {
                    Beam newBeam = new Beam(Beam.beamIdCounter++, tank.location, aiming, tank.ID);
                    theServerWorld.Beams.Add(newBeam.ID, newBeam);
                    tank.powerUpCount--;
                    tank.projNum++;
                }
            }
        }

        /// <summary>
        /// This methods create the projectile when client hits fire
        /// </summary>
        /// <param name="tank"></param>
        /// <param name="aiming"></param>
        private void ProjectileUpdate(Tank tank, Vector2D aiming)
        {
            lock (theServerWorld)
            {
                if (tank.projFrameCounter == Constant.FramePerShot && tank.died == false)
                {
                    Projectile newProj = new Projectile(Projectile.projIdCounter++, tank.location, aiming, tank.ID);
                    theServerWorld.Projectiles.Add(newProj.ID, newProj);
                    tank.projFrameCounter = 0;
                    tank.projNum++;
                }
            }

        }

        /// <summary>
        /// This methods computes the movement of the tank
        /// </summary>
        /// <param name="tank"></param>
        /// <param name="direction"></param>
        /// <param name="MouseAiming"></param>
        private void UpdateMovement(Tank tank, string direction, Vector2D MouseAiming)
        {
            if (tank.hitpoints > 0)
            {
                bool move = true;
                lock (theServerWorld)
                {
                    tank.aiming = MouseAiming;
                    switch (direction)
                    {
                        case "up":
                            tank.orientation = upVector;
                            break;
                        case "left":
                            tank.orientation = leftVector;
                            break;
                        case "down":
                            tank.orientation = downVector;
                            break;
                        case "right":
                            tank.orientation = rightVector;
                            break;
                        default: // "None movement"
                            move = false;
                            break;
                    }
                    if (move)
                    {
                        // computes the velocity and updates the location of the tank
                        tank.location += (tank.orientation * Constant.TankSpeed);
                        // if tank hits the wall, it stays still
                        if (WallCollisionCheck(tank.location, tank) == true)
                            tank.location -= (tank.orientation * Constant.TankSpeed);
                    }
                }
            }
            // check if the tank reaches the bounds
            WrapAroundCheck(tank);
        }

        /// <summary>
        /// This method checks if the tank reaches the bounds
        /// if yes, wrap the tank to the other side
        /// </summary>
        /// <param name="tank"></param>
        private void WrapAroundCheck(Tank tank)
        {
            lock (theServerWorld)
            {
                int offSet = Constant.TankSize / 2;
                if (tank.location.GetX() < -(theServerWorld.worldSize / 2) + offSet)
                {
                    tank.location = new Vector2D((theServerWorld.worldSize / 2) - offSet, tank.location.GetY());
                }
                else if (tank.location.GetX() > (theServerWorld.worldSize / 2) - offSet)
                {
                    tank.location = new Vector2D(-(theServerWorld.worldSize / 2) + offSet, tank.location.GetY());
                }
                else if (tank.location.GetY() < -(theServerWorld.worldSize / 2) + offSet)
                {
                    tank.location = new Vector2D(tank.location.GetX(), (theServerWorld.worldSize / 2) - offSet);
                }
                else if (tank.location.GetY() > (theServerWorld.worldSize / 2) - offSet)
                {
                    tank.location = new Vector2D(tank.location.GetX(), -(theServerWorld.worldSize / 2) + offSet);
                }
            }
        }

        /// <summary>
        /// This method removes the disconnected client from the game
        /// and set the disconnected tank with the appropriate values
        /// </summary>
        /// <param name="id"></param>
        private void RemoveClient(long id)
        {
            Console.WriteLine("Client " + id + " disconnected");
            lock (clients)
            {
                clients.Remove(id);
                theServerWorld.Tanks[(int)id].disconnected = true;
                theServerWorld.Tanks[(int)id].hitpoints = 0;
                theServerWorld.Tanks[(int)id].died = true;
                lock (clientControlMessages)
                {
                    clientControlMessages.Remove(id);
                }
            }
        }

        /// <summary>
        /// This method is to respawn the tank when it dies or first joins
        /// </summary>
        /// <param name="tank"></param>
        private void RespawnTank(Tank tank)
        {
            // the tank respawn only after a delay
            if ((tank.respawnDelayCounter == Constant.RespawnRate) || tank.respawnDelayCounter == -1)
            {
                tank.location = RandomLoc(tank);
                tank.respawnDelayCounter = 0; // set the counter back to 0
                tank.hitpoints = 3;
            }
        }

        /// <summary>
        /// This method computes the random location for the tank and powerups to appear
        /// as they can't conflict with the walls
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Vector2D RandomLoc(object obj)
        {
            Random r = new Random();
            double x = r.Next(-(theServerWorld.worldSize / 2), (theServerWorld.worldSize / 2));
            double y = r.Next(-(theServerWorld.worldSize / 2), (theServerWorld.worldSize / 2));
            Vector2D vector = new Vector2D(x, y);

            // for tanks
            if (obj is Tank)
            {
                Tank tank = obj as Tank;
                while (WallCollisionCheck(vector, tank))
                {
                    x = r.Next(-(theServerWorld.worldSize / 2), (theServerWorld.worldSize / 2));
                    y = r.Next(-(theServerWorld.worldSize / 2), (theServerWorld.worldSize / 2));
                    vector = new Vector2D(x, y);
                }
            }

            // for powerups
            if (obj is Powerup)
            {
                Powerup powerUp = obj as Powerup;
                while (WallCollisionCheck(vector, powerUp))
                {
                    x = r.Next(-(theServerWorld.worldSize / 2), (theServerWorld.worldSize / 2));
                    y = r.Next(-(theServerWorld.worldSize / 2), (theServerWorld.worldSize / 2));
                    vector = new Vector2D(x, y);
                }
            }
            return vector;
        }

        /// <summary>
        /// This method starts the infinite loop and call the update method to update the world
        /// </summary>
        public void UpdateWorld()
        {

            Stopwatch watch = new Stopwatch();
            watch.Start();

            while (true)
            {
                // run update only after some time
                while (watch.ElapsedMilliseconds < Constant.MsPerFrame)
                { /* do nothing */ }
                Update();
                watch.Restart(); // Needs to be Restart not Stop
                GameDuration += Constant.MsPerFrame;
            }
        }

        /// <summary>
        /// This method computes the neccessary values, update the world
        /// and send the update to all the clients
        /// </summary>
        private void Update()
        {
            ClientControlUpdate();
            ProjectileTankCollision();
            WallProjCollisionCheck();
            TankPowerUpCollisionCheck();
            TankBeamCollisionCheck();

            AddProjs();
            AddPowerUps();
            AddBeams();

            // check and remove disconnected clients
            if (disconnectedClients.Count > 0)
            {
                foreach (long id in disconnectedClients)
                    RemoveClient(id);
                disconnectedClients.Clear();
            }
            AddTanks();

            lock (theServerWorld)
            {
                if (clients.Count > 0)
                {
                    string stringMessage = totalDataString.ToString();
                    lock (clients)
                    {
                        foreach (SocketState client in clients.Values)
                        {
                            // if can't send, that client is disconnected
                            if (!Networking.Send(client.TheSocket, stringMessage))
                                disconnectedClients.Add(client.ID);
                        }
                    }
                    // clear up the message string every frame
                    totalDataString.Clear();
                }
                // increment the powerup's frame counter to compute the delay of powerup's appearance
                if (Powerup.frameCounter < Powerup.randomFrame) Powerup.frameCounter++;

                // increment the projectiles' frame counter to compute the delay
                foreach (Tank tank in theServerWorld.Tanks.Values)
                {
                    if (tank.projFrameCounter < Constant.FramePerShot) tank.projFrameCounter++;
                }
            }
            ClearObjects(); // clear up the world
        }

        /// <summary>
        /// This method clears up the world so that disconnected tanks and
        /// died objects won't be sent to clients
        /// </summary>
        private void ClearObjects()
        {
            lock (theServerWorld)
            {
                foreach (Tank tank in theServerWorld.Tanks.Values.ToList())
                {
                    if (tank.disconnected == true)
                    {
                        if(!disconnectedTanks.ContainsKey(tank.ID)) disconnectedTanks.Add(tank.ID, tank);
                        theServerWorld.Tanks.Remove(tank.ID);
                    }
                }

                foreach (Projectile proj in theServerWorld.Projectiles.Values.ToList())
                {
                    if (proj.died == true)
                        theServerWorld.Projectiles.Remove(proj.ID);
                }
                foreach (Powerup powUp in theServerWorld.Powerups.Values.ToList())
                {
                    if (powUp.died == true)
                        theServerWorld.Powerups.Remove(powUp.ID);
                }
            }
        }

        /// <summary>
        /// This methods send the walls' coordinates information to the clients
        /// this will be sent only once when the client first joins
        /// </summary>
        /// <param name="state"></param>
        private void SendWalls(SocketState state)
        {
            lock (theServerWorld)
            {
                if (clients.Count > 0)
                {
                    StringBuilder wallString = new StringBuilder();
                    foreach (Wall w in theServerWorld.Walls.Values)
                    {
                        wallString.Append(JsonConvert.SerializeObject(w) + "\n");
                    }
                    Networking.Send(state.TheSocket, wallString.ToString());
                }
            }
        }

        /// <summary>
        /// This method converts tanks' objects to JSON string
        /// </summary>
        private void AddTanks()
        {
            lock (theServerWorld)
            {
                foreach (Tank tank in theServerWorld.Tanks.Values)
                {
                    tankString.Append(JsonConvert.SerializeObject(tank) + "\n");
                }
                totalDataString.Append(tankString);
                tankString.Clear();
            }
        }

        /// <summary>
        /// This method converts projectiles' objects to JSON string
        /// </summary>
        private void AddProjs()
        {
            lock (theServerWorld)
            {
                foreach (Projectile proj in theServerWorld.Projectiles.Values)
                {
                    // update the new location for projectiles
                    proj.location = Projectile.CalculateNewLocation(proj.location, proj.orientation);
                    // if projectiles pass the bounds, set died to true
                    if (OutsideOfBoundsDetection(proj.location))
                        proj.died = true;
                }

                foreach (Projectile proj in theServerWorld.Projectiles.Values)
                {
                    projString.Append(JsonConvert.SerializeObject(proj) + "\n");
                }
                totalDataString.Append(projString);
                projString.Clear();
            }
        }

        /// <summary>
        /// This method converts powerups' objects to JSON string
        /// </summary>
        private void AddPowerUps()
        {
            lock (theServerWorld)
            {
                // maximum number of powerups is 2
                if (theServerWorld.Powerups.Count < 2)
                {
                    // what is the maximum delay between spawning new powerups? 
                    // The default is 1650 frames. After spawning a powerup, 
                    // the server should pick a random number of frames less than
                    // this number before trying to spawn another.
                    if (Powerup.frameCounter == Powerup.randomFrame)
                    {
                        Powerup newPowerUp = new Powerup(Powerup.powerUpIdCounter++, null);
                        newPowerUp.location = RandomLoc(newPowerUp); // set a random location
                        theServerWorld.Powerups.Add(newPowerUp.ID, newPowerUp);
                        Random r = new Random();
                        Powerup.randomFrame = r.Next(0, Constant.MaxPowerupDelay); // random number of frames
                        Powerup.frameCounter = 0;
                    }
                }

                foreach (Powerup powUp in theServerWorld.Powerups.Values)
                {
                    powerUpString.Append(JsonConvert.SerializeObject(powUp) + "\n");
                }
                totalDataString.Append(powerUpString);
                powerUpString.Clear();
            }
        }

        /// <summary>
        /// This method converts beams' objects to JSON string
        /// </summary>
        private void AddBeams()
        {
            lock (theServerWorld)
            {
                foreach (Beam beam in theServerWorld.Beams.Values)
                {
                    beamString.Append(JsonConvert.SerializeObject(beam) + "\n");
                }
                totalDataString.Append(beamString);
                beamString.Clear();
                // send beams to clients only once and remove them
                theServerWorld.Beams.Clear();
            }
        }


        /// <summary>
        /// This method reads setting from an xml file 
        /// and set the settings for the game accordingly
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadWorldConfig(string fileName)
        {
            if (ReferenceEquals(fileName, null))
                throw new Exception("The XML setting file cannot be null");
            if (fileName.Equals(""))
                throw new Exception("The XML filename cannot be empty");

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            try
            {
                using (XmlReader reader = XmlReader.Create(fileName, settings))
                {
                    double x = 0;
                    double y = 0;
                    int wallID = 0;
                    bool point2Seen = false;
                    Wall newWall = new Wall();
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "GameSettings":
                                    break;
                                case "UniverseSize":
                                    reader.Read();
                                    theServerWorld.worldSize = int.Parse(reader.Value);
                                    break;
                                case "MSPerFrame":
                                    reader.Read();
                                    Constant.MsPerFrame = int.Parse(reader.Value);
                                    break;
                                case "FramesPerShot":
                                    reader.Read();
                                    Constant.FramePerShot = int.Parse(reader.Value);
                                    break;
                                case "RespawnRate":
                                    reader.Read();
                                    Constant.RespawnRate = int.Parse(reader.Value);
                                    break;
                                case "Wall":
                                    newWall = new Wall();
                                    break;
                                case "p1":
                                    point2Seen = false;
                                    break;
                                case "p2":
                                    point2Seen = true;
                                    break;
                                case "x":
                                    reader.Read();
                                    x = double.Parse(reader.Value);
                                    break;
                                case "y":
                                    reader.Read();
                                    y = double.Parse(reader.Value);

                                    if (point2Seen) newWall.endPoint2 = new Vector2D(x, y);
                                    else
                                        newWall.endPoint1 = new Vector2D(x, y);
                                    break;

                            } // End of Switch    
                        }
                        // when reaching the end tag of the wall, add the wall to the set
                        if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Wall")
                        {
                            newWall.ID = wallID;
                            theServerWorld.Walls.Add(wallID++, newWall);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Read setting issue");
            }
        }

        /// <summary>
        /// This method checks if tanks and projectiles collide
        /// </summary>
        private void ProjectileTankCollision()
        {
            lock (theServerWorld)
            {
                foreach (Tank tank in theServerWorld.Tanks.Values)
                {
                    if (tank.died == true) tank.died = false;

                    foreach (Projectile proj in theServerWorld.Projectiles.Values)
                    {
                        if (proj.owner != tank.ID)
                        {
                            Vector2D distanceVector = (proj.location - tank.location);
                            // the projectile hits the tank when the distance btw them is smaller than the tank's size
                            if (distanceVector.Length() < (Constant.TankSize / 2) && proj.died == false)
                            {
                                theServerWorld.Tanks[proj.owner].projHits++;
                                proj.died = true;
                                if (tank.hitpoints > 0) tank.hitpoints--;
                                if (tank.hitpoints == 0) theServerWorld.Tanks[proj.owner].score++;
                            }
                        }
                    }

                    // when hitpoint is 0, set died to true and start delay counter
                    if (tank.hitpoints == 0)
                    {
                        if (tank.respawnDelayCounter == 0 && tank.died == false) tank.died = true;
                            tank.respawnDelayCounter++;
                        RespawnTank(tank);
                    }
                }
            }
        }

        /// <summary>
        /// This method checks if beams and tanks collide
        /// </summary>
        private void TankBeamCollisionCheck()
        {
            lock (theServerWorld)
            {
                foreach (Tank tank in theServerWorld.Tanks.Values)
                {
                    foreach (Beam beam in theServerWorld.Beams.Values)
                    {
                        // Intersect method is provided by Kopta
                        // it checks if the beam crosses the tank
                        if (Beam.Intersects(beam.origin, beam.direction, tank.location, Constant.TankSize / 2))
                        {
                            tank.died = true;
                            tank.hitpoints = 0;
                            theServerWorld.Tanks[beam.owner].score++;
                            theServerWorld.Tanks[beam.owner].projHits++;
                        }
                    }
                    if (tank.hitpoints == 0)
                    {
                        if (tank.respawnDelayCounter == 0 && tank.died == false) tank.died = true;
                        tank.respawnDelayCounter++;
                        RespawnTank(tank);
                    }
                }
            }
        }

        /// <summary>
        /// This method checks if tanks collide powerups
        /// </summary>
        private void TankPowerUpCollisionCheck()
        {
            lock (theServerWorld)
            {
                foreach (Tank tank in theServerWorld.Tanks.Values)
                {
                    foreach (Powerup powerUp in theServerWorld.Powerups.Values)
                    {
                        Vector2D distanceVector = (powerUp.location - tank.location);
                        if (distanceVector.Length() < (Constant.TankSize / 2) && powerUp.died == false)
                        {
                            powerUp.died = true;
                            if (tank.powerUpCount < 3) tank.powerUpCount++;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// This method checks if projectiles and walls collide
        /// </summary>
        private void WallProjCollisionCheck()
        {
            lock (theServerWorld)
            {
                foreach (Wall wall in theServerWorld.Walls.Values)
                {
                    foreach (Projectile proj in theServerWorld.Projectiles.Values)
                    {
                        if (proj.location.GetX() > wall.x1 && proj.location.GetX() < wall.x2 
                            && proj.location.GetY() > wall.y1 && proj.location.GetY() < wall.y2)
                            proj.died = true;

                    }
                }
            }
        }

        /// <summary>
        /// This method checks if a vector collides with walls
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool WallCollisionCheck(Vector2D vector, object obj)
        {
            double halfObj = 0;
            if (obj is Tank) halfObj = Constant.TankSize / 2;
            if (obj is Powerup) halfObj = 10;

            lock (theServerWorld)
            {
                foreach (Wall wall in theServerWorld.Walls.Values)
                {
                    if (vector.GetX() > wall.x1 - halfObj && vector.GetX() < wall.x2 + halfObj
                       && vector.GetY() > wall.y1 - halfObj && vector.GetY() < wall.y2 + halfObj)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This method computes the edges of the walls
        /// </summary>
        private void WallCollisionSize()
        {
            foreach (Wall wall in theServerWorld.Walls.Values)
            {
                Vector2D head = wall.endPoint1;
                Vector2D tail = wall.endPoint2;
                // check which endpoint is the head
                if (!wall.IsEndpoint1Head)
                {
                    head = wall.endPoint2;
                    tail = wall.endPoint1;
                }

                if (wall.IsHorizontal)
                {
                    wall.x1 = head.GetX() - wall.halfWallSize;
                    wall.x2 = tail.GetX() + wall.halfWallSize;
                    wall.y1 = head.GetY() - wall.halfWallSize;
                    wall.y2 = head.GetY() + wall.halfWallSize;
                }
                else
                {
                    wall.y1 = head.GetY() - wall.halfWallSize;
                    wall.y2 = tail.GetY() + wall.halfWallSize;
                    wall.x1 = head.GetX() - wall.halfWallSize;
                    wall.x2 = head.GetX() + wall.halfWallSize;
                }
            }
        }

        /// <summary>
        /// This method checks if a vector reaches the bounds of the world
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        private bool OutsideOfBoundsDetection(Vector2D vector)
        {
            if (vector.GetX() < -theServerWorld.worldSize / 2 || vector.GetX() > theServerWorld.worldSize / 2
                      || vector.GetY() < -theServerWorld.worldSize / 2 || vector.GetY() > theServerWorld.worldSize / 2)
                return true;
            return false;
        }

        /// <summary>
        /// This method updates control objects
        /// </summary>
        private void ClientControlUpdate()
        {
            lock (clientControlMessages)
            {
                foreach (long stateID in clientControlMessages.Keys)
                {
                    ControlUpdate(stateID, clientControlMessages[stateID]);
                }
            }
        }
    }
}
