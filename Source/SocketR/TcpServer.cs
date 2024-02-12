using System.Net.Sockets;

namespace SocketR;

public class TcpServer : ITcpServer
{
    private TcpListener? _listener;
    private CancellationTokenSource _cancellationTokenSource;
    private Action<TcpClient> _clientConnectedAction;
    private Func<ArraySegment<byte>, Task> _dataReceivedFunc;
    private Action<Exception> _errorAction;

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
        throw new NotImplementedException();
    }

    public ITcpServer OnDataReceived(Func<ArraySegment<byte>, Task> dataReceivedFunc)
    {
        throw new NotImplementedException();
    }

    public ITcpServer OnError(Action<Exception> errorAction)
    {
        throw new NotImplementedException();
    }

    public Task StartAsync(string ipAddress, int port, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync()
    {
        throw new NotImplementedException();
    }

    private async Task AcceptClientsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(cancellationToken);
            _clientConnectedAction?.Invoke(client);

            _ = Task.Run(async () => await ReceiveDataAsync(client, cancellationToken), cancellationToken);
        }
    }

    private async Task ReceiveDataAsync(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            var stream = client.GetStream();
            var buffer = new byte[1024];

            while (!cancellationToken.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
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
            client.Close();
        }
    }
}
