using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace GuessMyNumberServer
{
    internal class Server
    {
        public Server(string serverAddress, int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            players = new List<TcpClient>();
            waitingPlayers = new List<TcpClient>();
            runServer = false;
            games = new List<GuessMyNumber>();
            currentGame = new GuessMyNumber();
            taskGames = new List<Task>();
        }

        public void Run()
        {
            try
            {
                Console.WriteLine("Server starting");
                runServer = true;
                listener.Start();
                Console.WriteLine("Server started");
                while (runServer)
                {
                    if (listener.Pending())
                        HandleNewConnection();
                    StartNewGames();
                    HandleDisconnectedPlayersInGame();
                    HandleDisconnectedWaitingPlayers();
                }
                Console.WriteLine("Server ending");
                CleanClientsList(players);
                CleanClientsList(waitingPlayers);
                games.Clear();
                taskGames.Clear();
                Console.WriteLine("Server stopped");
            }
            catch(SocketException e)
            {
                Console.WriteLine("Error! Server stopped!");
            }
        }


        public void Shutdown()
        {
            runServer = false;
        }

        private void StartNewGames()
        {
            foreach(TcpClient client in waitingPlayers.ToArray())
            {
                Console.WriteLine($"{client.Client.RemoteEndPoint} is in the waiting list");
                if(currentGame.AddPlayer(client))
                {
                    waitingPlayers.Remove(client);
                    games.Add(currentGame);
                    Task.Run(currentGame.Run);
                    currentGame = new GuessMyNumber();
                    Console.WriteLine($"Added {client.Client.RemoteEndPoint} to a game");
                }
            }
        }

        private void HandleNewConnection()
        {
            Console.WriteLine("Adding new Player");
            TcpClient client = listener.AcceptTcpClientAsync().GetAwaiter().GetResult();
            players.Add(client);
            waitingPlayers.Add(client);
            Console.WriteLine($"Added {client.Client.RemoteEndPoint} to the waiting list");
        }

        private void HandleDisconnectedPlayersInGame()
        {
            foreach(GuessMyNumber game in games.ToArray())
                if(game.IsPlayerDisconnected())
                {
                    games.Remove(game);
                    if(game.Player is not null)
                    {
                        Console.WriteLine($"Removing {game.Player.Client.RemoteEndPoint}");
                        players.Remove(game.Player);
                    }
                }
        }

        private void HandleDisconnectedWaitingPlayers()
        {
            foreach(TcpClient player in waitingPlayers.ToArray())
                if(IsDisconnected(player))
                {
                    Console.WriteLine($"Removing waiting: {player.Client.RemoteEndPoint}");
                    waitingPlayers.Remove(player);
                }
        }

        private void CleanUp(TcpClient client)
        {
            if(client.Connected)
            {
                client.GetStream().Close();
                client.Close();
            }
        }

        private void CleanClientsList(List<TcpClient> clients)
        {
            foreach (TcpClient client in clients)
                CleanUp(client);
            clients.Clear();
        }

        private static bool IsDisconnected(TcpClient client)
        {
            try
            {
                Socket s = client.Client;
                return s.Poll(10 * 1000, SelectMode.SelectRead) && (s.Available == 0);
            }
            catch (SocketException)
            {
                return true;
            }
        }

        private TcpListener listener;
        private List<TcpClient> players;
        private List<TcpClient> waitingPlayers;
        private List<GuessMyNumber> games;
        private List<Task> taskGames;
        private GuessMyNumber currentGame;
        private bool runServer;
    }
}
