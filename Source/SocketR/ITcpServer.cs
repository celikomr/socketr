using System.Net.Sockets;

namespace SocketR;

public interface ITcpServer : IDisposable
{
    Task StartAsync(string ipAddress, int port, CancellationToken cancellationToken = default);
    Task StopAsync();
    ITcpServer OnClientConnected(Action<TcpClient> clientConnectedAction);
    ITcpServer OnDataReceived(Func<ArraySegment<byte>, Task> dataReceivedFunc);
    ITcpServer OnError(Action<Exception> errorAction);
}
