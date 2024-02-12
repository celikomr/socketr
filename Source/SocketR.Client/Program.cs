using System.Net.Sockets;

/// <summary>
/// Represents a TCP client application that sends messages to a server and receives responses.
/// </summary>
class Program
{
    /// <summary>
    /// Represents the entry point of the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    static async Task Main(string[] args)
    {
        try
        {
            const string ipAddress = "127.0.0.1"; // Server IP address
            const int port = 12345; // Server port

            // Establish TCP connection with the server
            using TcpClient tcpClient = new(ipAddress, port);
            using NetworkStream networkStream = tcpClient.GetStream();
            using StreamReader streamReader = new(networkStream);
            using StreamWriter streamWriter = new(networkStream) { AutoFlush = true };

            int messageCounter = 1; // Message counter

            while (true)
            {
                try
                {
                    // Construct the message to be sent to the server (numbered)
                    string message = $"Message {messageCounter++}";

                    // Send the message to the server
                    await streamWriter.WriteLineAsync(message);

                    // Read and print the response from the server
                    string? response = await streamReader.ReadLineAsync();
                    Console.WriteLine($"Server response: {response}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

                // Wait for 3 seconds
                await Task.Delay(3000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
