using System;
using System.Threading;

namespace TankWars
{

    /// <summary>
    /// The view for the server application
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            ServerController server = new ServerController();
            DatabaseController databaseController = new DatabaseController();
            WebServerController webServer = new WebServerController(databaseController);
            
            server.StartServer();
            webServer.StartServer();

            // start a new thread to run updateworld()
            Thread updateThread = new Thread(server.UpdateWorld);
            updateThread.Start();

            // Sleep to prevent the program from closing,
            // since all the real work is done in separate threads.
            // StartServer is non-blocking.
            string completeGame;
            completeGame = Console.ReadLine();
            // A game is considered completed when the user types 
            // anything into the console in which the game server is running 
            // and enter is pressed
            if (completeGame != "")
            {
                databaseController.SaveGameData(server);
            }

        }


    }
}
