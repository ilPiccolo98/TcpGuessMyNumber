using GuessMyNumberClient;

Client client = new Client("localhost", 5000);
client.Connect();
client.Run();
Console.ReadKey();