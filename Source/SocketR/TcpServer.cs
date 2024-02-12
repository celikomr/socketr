using System.Net.Sockets;

namespace SocketR;

public class TcpServer : ITcpServer
{
    public void Dispose()
    {
        throw new NotImplementedException();
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
}
