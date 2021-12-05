using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace GuessMyNumberClient
{
    internal class Client
    {
        public Client(string serverAddress, int port)
        {
            this.serverAddress = serverAddress;
            this.port = port;
            server = new TcpClient();
            runGame = false;
        }

        public void Connect()
        {
            try
            {
                server.Connect(serverAddress, port);
            }
            catch(SocketException e)
            {
                Console.WriteLine("Impossible to connect to the server");
            }
        }
        public void Run()
        {
            try
            {
                runGame = true;
                while (runGame && server.Connected)
                {
                    Packet packet = ReceivePacket(server);
                    if (packet is not null)
                    {
                        switch (packet.Command)
                        {
                            case "output":
                                HandleOutput(packet);
                                break;
                            case "input":
                                HandleInput(packet);
                                break;
                            case "disconnect":
                                HandleDisconnect(packet);
                                break;
                        }
                    }
                }
                CleanUp(server);
            }
            catch(SocketException)
            {
                Console.WriteLine("Lost server connection");
                CleanUp(server);
            }
            catch(IOException)
            {
                Console.WriteLine("Lost server connection");
                CleanUp(server);
            }
        }

        private void HandleOutput(Packet packet)
        {
            Console.WriteLine(packet.Message);
        }

        private void HandleInput(Packet packet)
        {
            Console.WriteLine(packet.Message);
            string? input = Console.ReadLine();
            Packet inputPacket = new Packet("input", (input is not null) ? input : "");
            SendPacket(server, inputPacket).GetAwaiter().GetResult();
        }

        private void HandleDisconnect(Packet packet)
        {
            Console.WriteLine(packet.Message);
            runGame = false;
        }

        private void CleanUp(TcpClient client)
        {
            if (client.Connected)
            {
                client.GetStream().Close();
                client.Close();
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

        private string serverAddress;
        private int port;
        private TcpClient server;
        private bool runGame;
    }
}
