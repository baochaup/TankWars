using System;
using NetworkUtil;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace TankWars
{
    /// <summary>
    /// This is the controller for the game
    /// </summary>
    public class GameController
    {
        public delegate void ServerUpdateHandler(); // delegate for handling updates from the server
        public delegate void ConnectionErrorHandler(); // delgate for handling when error occurred
        private event ServerUpdateHandler UpdateArrived;
        private event ConnectionErrorHandler ErrorHandle;
        private SocketState theServer;
        private World theWorld;
        private string userName = "";
        private string ipAddr = "";
        private Control control;
        private string[] moveArray; // array for storing pressed keys
        private int moveIndex; // to track the latest pressed key to send to the server

        public GameController()
        {
            theWorld = new World();
            control = new Control();
            moveArray = new string[5];
            // first el of moveArray is always none, if moveIndex points this
            // it means the tank is not moving, no keys are pressed
            moveArray[0] = "none"; 
            moveIndex = 0;
        }

        /// <summary>
        /// Method to get the world
        /// </summary>
        /// <returns></returns>
        public World GetWorld()
        {
            return theWorld;
        }

        /// <summary>
        /// Helper method for registering ServerUpdateHandler
        /// </summary>
        /// <param name="h"></param>
        public void RegisterServerUpdateHandler(ServerUpdateHandler h)
        {
            UpdateArrived += h; // Event handler (OnFrame)
        }

        /// <summary>
        /// Helper method for registering ConnectionErrorHandler
        /// </summary>
        /// <param name="m"></param>
        public void RegisterConnectionErrorHandler(ConnectionErrorHandler m)
        {
            ErrorHandle += m; // Event handler (UnlockMenubar)
        }

        /// <summary>
        /// Method for initializing the connection
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="playerName"></param>
        public void Connect(string ipAddress, string playerName)
        {
            ClearWorldObjects(); // Clears all the world objects from respective dictionaries 
            userName = playerName; // Saving the player name sent from the user
            ipAddr = ipAddress; // saving the ip address
            Networking.ConnectToServer(FirstContact, ipAddress, Constant.PortNum);

        }

        /// <summary>
        /// callback method for Connect, used to send username
        /// </summary>
        /// <param name="state"></param>
        private void FirstContact(SocketState state)
        {
            theServer = state; // saves the state for later uses
            HandleConnectionError(state); // handles connection errors if occurred
            if (state.ErrorOccured) return; // If an connection error occured do not allow the connection to continue;
            state.OnNetworkAction = ReceiveStartup;
            Networking.Send(state.TheSocket, userName);
            Networking.GetData(state);
        }

        /// <summary>
        /// callback method for Connect, used to receive the initial data like player's id and world size
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveStartup(SocketState state)
        {
            HandleConnectionError(state);
            if (state.ErrorOccured) return;
            // when receiving data from server, split it into parts by \n
            string[] lines = state.GetData().Split('\n');
            theWorld.playerID = int.Parse(lines[0]); // playerID is always sent first
            theWorld.worldSize = int.Parse(lines[1]); // followed by worldSize
            state.OnNetworkAction = ReceiveWorld;
            Networking.GetData(state);
        }

        /// <summary>
        /// callback method to continuously receive data from server and send data to server
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveWorld(SocketState state)
        {
            HandleConnectionError(state);
            if (state.ErrorOccured) return;
            string totalData = state.GetData();
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

                ProcessMessage(p);
                if (UpdateArrived != null) UpdateArrived();

                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }

            Networking.GetData(state); // Need to call this in order to get new string info into state
            control.Moving = moveArray[moveIndex]; // control's moving is set

            // Convert data need to send to server to JSON string
            string sendMessage = JsonConvert.SerializeObject(control) + "\n";
            Networking.Send(state.TheSocket, sendMessage); // send data to server

        }

        /// <summary>
        /// Process JSON string received from server and convert it to objects
        /// </summary>
        /// <param name="message"></param>
        private void ProcessMessage(string message)
        {
            string caseString = "";
            if (message.Contains("tank")) caseString = "tank";
            if (message.Contains("proj")) caseString = "proj";
            if (message.Contains("wall")) caseString = "wall";
            if (message.Contains("power")) caseString = "power";
            if (message.Contains("beam")) caseString = "beam";

            // add and remove the world, so need to lock
            lock (theWorld)
            {
                // capture objects sent by server and update the world accordingly
                switch (caseString)
                {
                    case "tank":
                        Tank rebuiltTank = JsonConvert.DeserializeObject<Tank>(message);
                        if (theWorld.Tanks.ContainsKey(rebuiltTank.ID))
                        {
                            // if the tank is disconnected, remove the tank from the world
                            if (rebuiltTank.disconnected)
                                theWorld.Tanks.Remove(rebuiltTank.ID);
                            else theWorld.Tanks[rebuiltTank.ID] = rebuiltTank;
                        }
                        else
                            theWorld.Tanks.Add(rebuiltTank.ID, rebuiltTank);
                        break;

                    case "proj":
                        Projectile rebuiltProj = JsonConvert.DeserializeObject<Projectile>(message);
                        if (!theWorld.Projectiles.ContainsKey(rebuiltProj.ID))
                            theWorld.Projectiles.Add(rebuiltProj.ID, rebuiltProj);
                        else
                        {
                            if (rebuiltProj.died) theWorld.Projectiles.Remove(rebuiltProj.ID);
                            else theWorld.Projectiles[rebuiltProj.ID] = rebuiltProj;
                        }
                        break;

                    case "wall":
                        Wall rebuiltWall = JsonConvert.DeserializeObject<Wall>(message);
                        if(!theWorld.Walls.ContainsKey(rebuiltWall.ID))
                             theWorld.Walls.Add(rebuiltWall.ID, rebuiltWall);
                        break;

                    case "power":
                        Powerup rebuiltPower = JsonConvert.DeserializeObject<Powerup>(message);
                        if (!theWorld.Powerups.ContainsKey(rebuiltPower.ID))
                            theWorld.Powerups.Add(rebuiltPower.ID, rebuiltPower);
                        else
                        {
                            if (rebuiltPower.died) theWorld.Powerups.Remove(rebuiltPower.ID);
                            else theWorld.Powerups[rebuiltPower.ID] = rebuiltPower;
                        }
                        break;

                    case "beam":
                        Beam rebuiltBeam = JsonConvert.DeserializeObject<Beam>(message);
                        control.Fire = "none"; // Reset firing to prevent multiple beams in one frame

                        if (!theWorld.Beams.ContainsKey(rebuiltBeam.ID))
                            theWorld.Beams.Add(rebuiltBeam.ID, rebuiltBeam);
                        else
                            theWorld.Beams[rebuiltBeam.ID] = rebuiltBeam;
                        break;

                        
                }
            }
            
        }

        /// Detect all numeric characters at the form level and consume up, 
        /// down, left, and right. Note that Form.KeyPreview must be set to true for this
        /// event handler to be called.
        public void SendKeyDown(KeyEventArgs e)
        {
            // detect which key is pressed and add to moveArray
            // don't add the same key as the last one
            switch ((int)e.KeyCode)
            {
                case 87: // W up
                    if (!(moveArray[moveIndex] == "up"))
                        moveArray[++moveIndex] = "up";
                    break;
                case 83: // S down
                    if (!(moveArray[moveIndex] == "down"))
                        moveArray[++moveIndex] = "down";
                    break;
                case 65: // A left
                    if (!(moveArray[moveIndex] == "left"))
                        moveArray[++moveIndex] = "left";
                    break;
                case 68: // D right
                    if (!(moveArray[moveIndex] == "right"))
                        moveArray[++moveIndex] = "right";
                    break;
            }

        }

        /// <summary>
        /// Method for dealing with when releasing control keys
        /// </summary>
        /// <param name="e"></param>
        public void SendKeyUp(KeyEventArgs e)
        {
            string key = "";
            switch ((int)e.KeyCode)
            {
                case 87: // W up
                    key = "up";
                    break;
                case 83: // S down
                    key = "down";
                    break;
                case 65: // A left
                    key = "left";
                    break;
                case 68: // D right
                    key = "right";
                    break;
            }

            // deals with the delay when pressing keys, without this, the client sends "none" moving
            // to the server sometimes although user is pressing a movement key, and causes delay
            // when user depresses a key, shift all the elements in moveArray to the left 1 position
            // and decrement moveIndex
            for (int i = 1; i < moveArray.Length; i++)
            {
                if (key == moveArray[i])
                { 
                    for (int j = i; j < moveArray.Length - 1; j++)
                        moveArray[j] = moveArray[j + 1];
                    moveIndex--;
                }
            }
        }

        /// <summary>
        /// Method for dealing with using mouse to aim
        /// </summary>
        /// <param name="e"></param>
        public void MouseAim(MouseEventArgs e)
        {
            // before sending aiming to server, substract aiming by the coordinates of the center of the image
            // and normalize it
            control.Aiming = new Vector2D(e.X - (Constant.ViewSize / 2), (e.Y - (Constant.ViewSize / 2)));
            control.Aiming.Normalize();
        }

        /// <summary>
        /// Method for dealing with using mouse to fire
        /// </summary>
        /// <param name="e"></param>
        public void MouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                control.Fire = "main";
            if (e.Button == MouseButtons.Right)
            {
                control.Fire = "alt";
                
            }
        }

        /// <summary>
        /// when users depress mouse left button, stop firing
        /// </summary>
        public void MouseUp()
        {
            control.Fire = "none";
        }

        /// <summary>
        /// when users close the game, shutdown the socket
        /// </summary>
        public void OnExit()
        {
            try
            {
                if (theServer != null)
                    theServer.TheSocket.Shutdown(SocketShutdown.Both);
            }
            catch { }
        }

        /// <summary>
        /// When errors occur, show a message box to ask users for reconnecting
        /// </summary>
        /// <param name="state"></param>
        private void HandleConnectionError(SocketState state)
        {
            if (state.ErrorOccured)
            {
                string message = "Error occured when trying to connect to server.\nWould you like to retry with the same IP?";
                string caption = "Connection Error";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);

                // if yes, reconnect, if no, call delegate in the view to unlock the menu bar
                if (result == DialogResult.Yes)
                {
                    Connect(ipAddr, userName);
                }
                else
                {

                    ErrorHandle(); // Call the delegate for the UnlockMenuBar method in Form1.cs
                }
            }
        }

        /// <summary>
        /// Show the guide for how to play the game
        /// </summary>
        public void ControlsInfo()
        {
            string controlMessage = "W: \t\t Move up\n" +
                                    "A: \t\t Move left\n" +
                                    "S: \t\t Move down\n" +
                                    "D: \t\t Move right\n" +
                                    "Mouse: \t\t Aim\n" +
                                    "Left Click: \t Fire projectile\n" +
                                    "Right Click: \t Fire beam";
            string caption = "Game Controls";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            MessageBox.Show(controlMessage, caption, buttons);
        }

        /// <summary>
        /// Show the info about the game
        /// </summary>
        public void AboutGame()
        {
            string controlMessage = "TankWars PS8 Solution\nArtwork by Jolie Uk and Alex Smith\nGame design by Daniel Kopta\n" +
                                       "Implementation by Bao Chau Pham and Antonio Arceo\nCS 3500 Fall 2019, University of Utah";
            string caption = "About Game";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            MessageBox.Show(controlMessage, caption, buttons);
        }

        /// <summary>
        /// Removes all the world objects held in each dictionary.
        /// This is used when the user wants to connect to a new game
        /// </summary>
        private void ClearWorldObjects()
        {
            theWorld.Tanks.Clear();
            theWorld.Walls.Clear();
            theWorld.Projectiles.Clear();
            theWorld.Beams.Clear();
            theWorld.Powerups.Clear();
        }

    }

}
