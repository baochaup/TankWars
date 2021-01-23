using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetworkUtil;


namespace TankWars
{
    /// <summary>
    /// 
    /// Authors: Bao Chau Pham & Antonio Arceo
    /// 
    /// Assigment: PS8 Tankwars Fall 2019
    /// 
    /// This is the view client for the tankwars game
    /// 
    /// </summary>
    public partial class Form1 : Form
    {
        // The controller handles updates from the "server"
        // and notifies us via an event
        private GameController theController;

        // World is a simple container for Players and Powerups
        // The controller owns the world, but we have a reference to it
        private World theWorld;

        //The panel we for drawing graphics for the game
        DrawingPanel drawingPanel;
       
        public Form1(GameController ctl)
        {
            InitializeComponent();
            theController = ctl;
            theWorld = theController.GetWorld();
            theController.RegisterServerUpdateHandler(OnFrame); //Register OnFrame as the controller event handler
            theController.RegisterConnectionErrorHandler(UnlockMenuBar); //Register UnlockMenuBar as the controller event handler

            ClientSize = new Size(Constant.ClientSize, Constant.ClientSize + 35);
            drawingPanel = new DrawingPanel(theWorld);
            drawingPanel.Location = new Point(0, 35);
            drawingPanel.Size = new Size(Constant.ViewSize, Constant.ViewSize);
            
            drawingPanel.BackColor = Color.Black;
            this.Controls.Add(drawingPanel);

            // Set KeyPreview object to true to allow the form to process 
            // the key before the control with focus processes it.
            this.KeyPreview = true;

            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            this.KeyUp += new KeyEventHandler(Form1_KeyUp);
            drawingPanel.MouseMove += new MouseEventHandler(Form1_MouseMove);
            drawingPanel.MouseDown += new MouseEventHandler(Form1_MouseDown);
            drawingPanel.MouseUp += new MouseEventHandler(Form1_MouseUp);
            FormClosed += OnExit;
        }

        /// <summary>
        /// Refresh the whole form every time receiving updates from server
        /// </summary>
        private void OnFrame()
        {
            // Don't try to redraw if the window doesn't exist yet.
            // This might happen if the controller sends an update
            // before the Form has started.
            if (!IsHandleCreated)
                return;

            // use methodinvoker to put Invalidate in the form thread
            // Invalidate refreshes the form
            // drawingPanel's OnPaint is called when it is refreshed
            MethodInvoker m = new MethodInvoker(() => this.Invalidate(true));

            // catch errors sometimes caused by closing the form
            try
            {
                this.Invoke(m);
            }
            catch (ObjectDisposedException)
            { }
        }

        /// <summary>
        /// Method for handling when closing the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExit(object sender, FormClosedEventArgs e)
        {
            theController.OnExit();
        }

        /// <summary>
        /// Method for handling when clicking Connect button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectBtn_Click(object sender, EventArgs e)
        {
            // if the ipaddress is empty, ask users to enter it
            if (ipAddrText.Text == "")
            {
                MessageBox.Show("Please enter a server address");
                return;
            }
            if (playerNameBox.Text == "")
            {
                MessageBox.Show("Please enter your name");
                return;
            }
            // disable the textboxes and the button after connected
            ipAddrText.Enabled = false;
            playerNameBox.Enabled = false;
            connectBtn.Enabled = false;
            drawingPanel.Focus();

            // get controller connect to the server
            theController.Connect(ipAddrText.Text, playerNameBox.Text);
        }

        /// <summary>
        /// Method for handling pressing control keys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            theController.SendKeyDown(e);
        }

        /// <summary>
        /// Method for handling releasing control keys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            theController.SendKeyUp(e);
        }

        /// <summary>
        /// Method for handling mouse move
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            theController.MouseAim(e);
        }

        /// <summary>
        /// Method for handling mouse presses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            theController.MouseDown(e);
        }

        /// <summary>
        /// Method for releasing mouse presses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            theController.MouseUp();
        }

        /// <summary>
        /// Method for unclocking the textboxes and button when errors occur
        /// </summary>
        private void UnlockMenuBar()
        {
            MethodInvoker m = new MethodInvoker(() =>
            {
                ipAddrText.Enabled = true;
                playerNameBox.Enabled = true;
                connectBtn.Enabled = true;
                ipAddrText.Focus();
            });

            try
            {
                this.Invoke(m);
            }
            catch (ObjectDisposedException)
            { } 
        }

        /// <summary>
        /// Method for handling when clicking Controls menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlHelpBtn(object sender, EventArgs e)
        {
            theController.ControlsInfo();
        }

        /// <summary>
        /// Method for handling when clicking About menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theController.AboutGame();
        }
    }
}
