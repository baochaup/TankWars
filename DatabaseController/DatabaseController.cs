using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankWars
{
    /// <summary>
    /// This class represents the controller that helps to save the game data
    /// the database and retrieve the data upon requests
    /// </summary>
    public class DatabaseController
    {
        public const string connectionString = "server=atr.eng.utah.edu;" +
            "database=cs3500_u1271272;" +
            "uid=cs3500_u1271272;" +
            "password=Password";
        public Dictionary<uint, GameModel> games;
        public List<SessionModel> gameSessions;

        public DatabaseController()
        {
            games = new Dictionary<uint, GameModel>();
            gameSessions = new List<SessionModel>();
        }

        /// <summary>
        /// This method saves the game data to the database
        /// </summary>
        /// <param name="server"></param>
        public void SaveGameData(ServerController server)
        {
            int gID = 0;
            List<int> pIDs = new List<int>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Open a connection
                    conn.Open();
                    long duration = server.GameDuration / 1000;
                    // Create a command
                    MySqlCommand command = conn.CreateCommand();
                    // insert data to the games table
                    command.CommandText = "insert into Games (Duration) values (" + duration + ")";
                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    { }

                    // insert data to the players table
                    foreach (string name in server.PlayerNames.Values)
                    {
                        command.CommandText = "insert into Players (Name) values ('" + name + "')";
                        using (MySqlDataReader reader = command.ExecuteReader())
                        { }
                    }

                    // retrieve the current game ID
                    command.CommandText = "select Max(gID) from Games";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            gID = reader.GetInt32(0);
                        }
                    }

                    // add the disconnected tanks to the current tank list
                    foreach (Tank tank in server.disconnectedTanks.Values)
                    {
                        server.theServerWorld.Tanks.Add(tank.ID, tank);
                    }

                    // retrieve the player ids of the current players 
                    command.CommandText = "select pID from Players order by pID desc limit " + server.theServerWorld.Tanks.Count;
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pIDs.Add(reader.GetInt32(0));
                        }
                    }

                    // compute the accuracy of the tanks
                    foreach (Tank tank in server.theServerWorld.Tanks.Values)
                    {
                        if (tank.projNum != 0) tank.accurancy = ((double)tank.projHits / tank.projNum) * 100;
                        else tank.accurancy = 0;
                    }

                    // insert the data to the gamesplayed table
                    int index = server.theServerWorld.Tanks.Count - 1;
                    foreach (int pID in pIDs)
                    {
                        command.CommandText = "insert into GamesPlayed (gID, pID, Score, Accuracy) values (" + gID + "," + pID + "," +
                                            server.theServerWorld.Tanks[index].score + "," + server.theServerWorld.Tanks[index].accurancy + ")";
                        using (MySqlDataReader reader = command.ExecuteReader())
                        { }
                        index--;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// This method retrieve data for the /games request
        /// </summary>
        public void GetGames()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand command = conn.CreateCommand();

                    command.CommandText = "select GamesPlayed.gID, Duration, Name, Score, Accuracy from Players, GamesPlayed, " +
                                       "Games Where Players.pID = GamesPlayed.pID and Games.gID = GamesPlayed.gID";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            uint gID = reader.GetUInt32(0);
                            uint duration = reader.GetUInt32(1);
                            string name = reader.GetString(2);
                            uint score = (uint)reader.GetInt32(3);
                            uint accuracy = (uint)reader.GetInt32(4);
                            if (!games.ContainsKey(gID))
                                games[gID] = new GameModel(gID, duration);
                            games[gID].AddPlayer(name, score, accuracy);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// This method retrieves data for for the /games?player=... request
        /// </summary>
        /// <param name="name"></param>
        public void GetSpecificGame(string name )
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand command = conn.CreateCommand();

                    command.CommandText = "select GamesPlayed.gID, Duration, Score, Accuracy from Players, GamesPlayed," +
                                                "Games Where Players.pID = GamesPlayed.pID and Games.gID = GamesPlayed.gID and Name = '" + name + "'";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            uint gID = reader.GetUInt32(0);
                            uint duration = reader.GetUInt32(1);
                            uint score = (uint)reader.GetInt32(2);
                            uint accuracy = (uint)reader.GetInt32(3);
                            gameSessions.Add(new SessionModel(gID, duration, score, accuracy));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}

