using System.Net;
using System.Net.Sockets;

namespace SocketR;

/// <summary>
/// Represents a TCP server that listens for incoming connections and processes data received from clients.
/// </summary>
public class TcpServer : ITcpServer
{
    /// <summary>
    /// Represents the TCP listener used to accept incoming client connections.
    /// </summary>
    private TcpListener? _listener;

    /// <summary>
    /// Represents the cancellation token source used to cancel server operations.
    /// </summary>
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    /// <summary>
    /// Represents a list of connected TCP clients.
    /// </summary>
    private readonly List<TcpClient> _connectedClients = [];

    /// <summary>
    /// Represents an action to perform when a client connects to the server.
    /// </summary>
    private Action<TcpClient> _clientConnectedAction = client =>
    {
        IPEndPoint? endPoint = (IPEndPoint?)client.Client.RemoteEndPoint;
        Console.WriteLine($"Client connected: {endPoint?.Address}:{endPoint?.Port}");
    };

    /// <summary>
    /// Represents a function to handle data received from clients.
    /// </summary>
    private Func<string, Task<string>> _dataReceivedFunc = data =>
    {
        // Default function: Print the incoming string to the screen and return a success message
        Console.WriteLine($"Data received: {data}");
        return Task.FromResult("Server received your message.");
    };

    /// <summary>
    /// Represents an action to perform when an error occurs during server operation.
    /// </summary>
    private Action<Exception> _errorAction = error => Console.WriteLine($"Error: {error.Message}");


    /// <summary>
    /// Releases all resources used by the <see cref="TcpServer"/> instance.
    /// </summary>
    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
        StopAsync().Wait();
    }

    /// <summary>
    /// Sets the action to perform when a client connects to the server.
    /// </summary>
    /// <param name="clientConnectedAction">The action to perform when a client connects.</param>
    /// <returns>The current instance of <see cref="ITcpServer"/>.</returns>
    public ITcpServer OnClientConnected(Action<TcpClient> clientConnectedAction)
    {
        _clientConnectedAction = clientConnectedAction;
        return this;
    }

    /// <summary>
    /// Sets the function to handle data received from clients.
    /// </summary>
    /// <param name="dataReceivedFunc">The function to handle received data.</param>
    /// <returns>The current instance of <see cref="ITcpServer"/>.</returns>
    public ITcpServer OnDataReceived(Func<string, Task<string>> dataReceivedFunc)
    {
        _dataReceivedFunc = dataReceivedFunc;
        return this;
    }

    /// <summary>
    /// Sets the action to perform when an error occurs during server operation.
    /// </summary>
    /// <param name="errorAction">The action to perform when an error occurs.</param>
    /// <returns>The current instance of <see cref="ITcpServer"/>.</returns>
    public ITcpServer OnError(Action<Exception> errorAction)
    {
        _errorAction = errorAction;
        return this;
    }

    /// <summary>
    /// Starts the TCP server and begins listening for incoming connections.
    /// </summary>
    /// <param name="ipAddress">The IP address to listen on.</param>
    /// <param name="port">The port number to listen on.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StartAsync(string ipAddress, int port, CancellationToken cancellationToken = default)
    {
        if (_listener != null)
            throw new InvalidOperationException("Server is already running.");

        _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        _listener.Start();

        Console.WriteLine($"Server started at {ipAddress}:{port}");

        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _clientConnectedAction?.Invoke(client);
            _connectedClients.Add(client);
            _ = Task.Run(async () => await HandleDataAsync(client), cancellationToken);
        }
    }

    /// <summary>
    /// Stops the TCP server and closes all active connections.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StopAsync()
    {
        _cancellationTokenSource.Cancel();

        if (_listener != null)
        {
            _listener.Stop();
            _listener = null;
        }

        foreach (var client in _connectedClients)
        {
            client.Close();
        }
        _connectedClients.Clear();

        Console.WriteLine("Server stopped.");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the data received from a connected client.
    /// </summary>
    /// <param name="client">The TCP client connected to the server.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleDataAsync(TcpClient client)
    {
        try
        {
            var stream = client.GetStream();
            var reader = new StreamReader(new BufferedStream(stream));
            var writer = new StreamWriter(new BufferedStream(stream)) { AutoFlush = true };

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                string? receivedData = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(receivedData))
                {
                    break;
                }

                string responseData = await _dataReceivedFunc.Invoke(receivedData);

                // Send response to client
                await writer.WriteLineAsync(responseData);
            }
        }
        catch (Exception ex)
        {
            _errorAction?.Invoke(ex);
        }
        finally
        {
            _connectedClients.Remove(client);
            client.Close();
        }
    }
}
