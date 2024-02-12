using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketR;

public class TcpServer : ITcpServer
{
    private TcpListener? _listener;
    private CancellationTokenSource _cancellationTokenSource;
    private List<TcpClient> _connectedClients = new List<TcpClient>();

    private Action<TcpClient> _clientConnectedAction = client =>
    {
        IPEndPoint endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
        Console.WriteLine($"Client connected: {endPoint?.Address}:{endPoint?.Port}");
    };

    private Func<ArraySegment<byte>, Task> _dataReceivedFunc = data =>
    {
        // Default function: Print the incoming byte array to the screen and return a success message
        string receivedData = Encoding.UTF8.GetString(data.Array, data.Offset, data.Count);
        Console.WriteLine($"Data received: {receivedData}");
        return Task.CompletedTask;
    };

    private Action<Exception> _errorAction = error => Console.WriteLine($"Error: {error.Message}");

    public TcpServer() => _cancellationTokenSource = new CancellationTokenSource();

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
        if (_listener != null)
        {
            _listener.Stop();
            _listener = null;
        }
    }

    public ITcpServer OnClientConnected(Action<TcpClient> clientConnectedAction)
    {
        _clientConnectedAction = clientConnectedAction;
        return this;
    }

    public ITcpServer OnDataReceived(Func<ArraySegment<byte>, Task> dataReceivedFunc)
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
            var client = await _listener.AcceptTcpClientAsync(cancellationToken);
            _clientConnectedAction?.Invoke(client);
            _connectedClients.Add(client);
            _ = Task.Run(async () => await ReceiveDataAsync(client, cancellationToken), cancellationToken);
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

        Console.WriteLine("Server stopped.");
        return Task.CompletedTask;
    }

    private async Task ReceiveDataAsync(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            var stream = client.GetStream();
            var buffer = new byte[1024];

            while (!cancellationToken.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                if (bytesRead == 0)
                {
                    break;
                }

                var data = new ArraySegment<byte>(buffer, 0, bytesRead);
                await _dataReceivedFunc.Invoke(data);
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
