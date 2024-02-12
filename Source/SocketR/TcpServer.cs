using System.Net;
using System.Net.Sockets;

namespace SocketR;

public class TcpServer : ITcpServer
{
    private TcpListener? _listener;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly List<TcpClient> _connectedClients = new List<TcpClient>();

    private Action<TcpClient> _clientConnectedAction = client =>
    {
        IPEndPoint endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
        Console.WriteLine($"Client connected: {endPoint?.Address}:{endPoint?.Port}");
    };

    private Func<string, Task<string>> _dataReceivedFunc = data =>
    {
        // Default function: Print the incoming string to the screen and return a success message
        Console.WriteLine($"Data received: {data}");
        return Task.FromResult("Server received your message.");
    };

    private Action<Exception> _errorAction = error => Console.WriteLine($"Error: {error.Message}");

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
        StopAsync().Wait();
    }

    public ITcpServer OnClientConnected(Action<TcpClient> clientConnectedAction)
    {
        _clientConnectedAction = clientConnectedAction;
        return this;
    }

    public ITcpServer OnDataReceived(Func<string, Task<string>> dataReceivedFunc)
    {
        _dataReceivedFunc = dataReceivedFunc;
        return this;
    }

    public ITcpServer OnError(Action<Exception> errorAction)
    {
        _errorAction = errorAction;
        return this;
    }

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
            _ = Task.Run(async () => await ReceiveDataAsync(client), cancellationToken);
        }
    }

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

    private async Task ReceiveDataAsync(TcpClient client)
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
