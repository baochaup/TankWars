using NetworkUtil;
using System;

namespace TankWars
{
    /// <summary>
    /// This class is a controller for starting the web server
    /// and the event loop, accepting http requests, and return
    /// the according http responses
    /// </summary>
    public class WebServerController
    {
        private DatabaseController database;
        public WebServerController(DatabaseController db)
        {
            database = db;
        }

        /// <summary>
        /// This method starts the tcp connection
        /// </summary>
        public void StartServer()
        {
            // This begins an "event loop"
            Networking.StartServer(HandleHttpConnection, Constant.WebPortNum);
            Console.WriteLine("WebServer is running");
        }

        /// <summary>
        /// This method starts the loop to retrieve http requests
        /// </summary>
        /// <param name="state"></param>
        private void HandleHttpConnection(SocketState state)
        {
            if (state.ErrorOccured)
                return;

            state.OnNetworkAction = ServeHttpRequest;
            Networking.GetData(state);
        }

        /// <summary>
        /// This method calls the appropriate methods
        /// to retrieve data from the database and respond to users
        /// </summary>
        /// <param name="state"></param>
        private void ServeHttpRequest(SocketState state)
        {
            if (state.ErrorOccured)
                return;

            string request = state.GetData();

            // serve GET game?player request
            if (request.Contains("games?player="))
            {
                // calculate and get the player's name from the request string
                int firstIndex = request.IndexOf("=") + 1;
                int secondIndex = request.IndexOf("HTTP");
                int nameLength = (secondIndex - firstIndex) -1;
                string name = request.Substring(firstIndex , nameLength );

                database.GetSpecificGame(name);

                Networking.SendAndClose(state.TheSocket,
                    WebViews.GetPlayerGames(name, database.gameSessions));
            }

            // serve GET games request
            else if (request.Contains("games"))
            {
                database.GetGames();
                Networking.SendAndClose(state.TheSocket,
                    WebViews.GetAllGames(database.games));
            }
            // serve the default request which will show the homepage
            else if (request.Contains("GET"))
            {
                Networking.SendAndClose(state.TheSocket,
                     WebViews.GetHomePage(2));
            }
            else
            {
                Networking.SendAndClose(state.TheSocket,
                     WebViews.Get404());
            }
        }

    }
}
