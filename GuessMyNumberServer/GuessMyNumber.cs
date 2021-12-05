using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace GuessMyNumberServer
{
    internal class GuessMyNumber
    {
        public GuessMyNumber()
        {
            Player = null;
            runGame = false;
            Random random = new Random();
            numberToGuess = random.Next(0, 500);
            isItConnected = false;
        }

        public void Run()
        {
            try
            {
                if (Player is not null)
                {
                    Console.WriteLine("The game is about to run");
                    Packet? packet = new Packet("output", "Welcome to Guess My Number game");
                    SendPacket(Player, packet).GetAwaiter().GetResult();
                    runGame = true;
                    Console.WriteLine($"Game with {Player.Client.RemoteEndPoint} is starting");
                    while (runGame)
                    {
                        packet = new Packet("input", "Enter the number to guess:\nEnter 'exit' to stop playing");
                        SendPacket(Player, packet).GetAwaiter().GetResult();
                        packet = ReceivePacket(Player);
                        Console.WriteLine($"Packet received: {packet}");
                        if(packet is not null && packet.Command == "input")
                            HandleInputCommand(packet);                                
                    }
                }
            }
            catch(SocketException)
            {
                if(Player is not null)
                    CleanUp(Player);
                isItConnected = false;
                Console.WriteLine($"{Player?.Client.RemoteEndPoint} has left the game");
            }
            catch(IOException)
            {
                if (Player is not null)
                    CleanUp(Player);
                isItConnected = false;
                Console.WriteLine($"{Player?.Client.RemoteEndPoint} has left the game");
            }
        }

        public bool AddPlayer(TcpClient player)
        {
            if (Player is null)
            {
                Player = player;
                isItConnected = true;
                return true;
            }
            return false;
        }

        public bool IsPlayerDisconnected()
        {
            if (Player is not null && isItConnected)
                return false;
            return true;
        }

        private void HandleInputCommand(Packet packet)
        {
            if(int.TryParse(packet.Message, out int numberInput))
            {
                if (numberInput > numberToGuess)
                    packet = new Packet("output", "The number you entered is too large");

                else if (numberInput < numberToGuess)
                    packet = new Packet("output", "The number you entered is too small");
                else
                {
                    packet = new Packet("disconnect", "Really god you guessed the number!");
                    runGame = false;
                    isItConnected = false;
                }
                if(Player is not null)
                    SendPacket(Player, packet).GetAwaiter().GetResult();
            }
            else if(packet is not null && packet.Message == "exit")
            {
                packet = new Packet("disconnect", $"Game quit! The number to guess was: {numberToGuess}");
                runGame = false;
                isItConnected = false;
                if (Player is not null)
                    SendPacket(Player, packet).GetAwaiter().GetResult();
            }
            else
            {
                packet = new Packet("output", "Inserted invalid input, please try again");
                if (Player is not null)
                    SendPacket(Player, packet).GetAwaiter().GetResult();
            }
        }

        private async Task SendPacket(TcpClient client, Packet packet)
        {
            string jsonPacket = packet.ToJson();
            ushort lengthPacket = Convert.ToUInt16(jsonPacket.Length);
            byte[] rawLength = BitConverter.GetBytes(lengthPacket);
            byte[] rawJson = Encoding.UTF8.GetBytes(jsonPacket);
            byte[] rawData = new byte[rawJson.Length + rawLength.Length];
            rawLength.CopyTo(rawData, 0);
            rawJson.CopyTo(rawData, rawLength.Length);
            await client.GetStream().WriteAsync(rawData, 0, rawData.Length);
        }

        private Packet ReceivePacket(TcpClient client)
        {
            byte[] rawLength = new byte[2];
            client.GetStream().Read(rawLength, 0, rawLength.Length);
            ushort length = BitConverter.ToUInt16(rawLength);
            byte[] rawData = new byte[length];
            client.GetStream().Read(rawData, 0, rawData.Length);
            string data = Encoding.UTF8.GetString(rawData);
            return Packet.FromJson(data);
        }

        private void CleanUp(TcpClient client)
        {
            if (client.Connected)
            {
                client.GetStream().Close();
                client.Close();
            }
        }

        public TcpClient? Player { get; private set; }
        private bool runGame;
        private int numberToGuess;
        private bool isItConnected;
    }
}
