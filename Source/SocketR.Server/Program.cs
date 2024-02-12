using SocketR;
using System.Net.Sockets;

/// <summary>
/// A program to demonstrate the usage of the TcpServer class for handling TCP connections.
/// </summary>
class Program
{
    /// <summary>
    /// The entry point of the program.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    static async Task Main(string[] args)
    {
        try
        {
            // IP address and port number for the server
            const string ipAddress = "127.0.0.1"; // Localhost
            const int port = 12345;

            // Create TCP server
            TcpServer tcpServer = new();

            // Chain the methods with the defined actions and functions
            await tcpServer.OnClientConnected(onClientConnected)
                           .OnDataReceived(onDataReceived)
                           .StartAsync(ipAddress, port);

            // Stop the server
            // await tcpServer.StopAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Defines the action to be performed when a client connection is established.
    /// </summary>
    private static readonly Action<TcpClient> onClientConnected = client => Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");

    /// <summary>
    /// Defines the function to handle received data.
    /// </summary>
    private static readonly Func<string, Task<string>> onDataReceived = async data =>
    {
        // Default function: Print the incoming string to the screen and return a success message
        Console.WriteLine($"Data received: {data}");
        return await Task.FromResult("Message received and processed successfully.");
    };
}
