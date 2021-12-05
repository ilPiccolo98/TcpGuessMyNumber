using GuessMyNumberServer;

Server server = new Server("localhost", 5000);

void HandleInterrupt(object? sender, ConsoleCancelEventArgs e)
{
    e.Cancel = true;
    server.Shutdown();
}

Console.CancelKeyPress += HandleInterrupt;

server.Run();
Console.ReadKey();


