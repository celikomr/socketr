namespace SocketR;

public interface ITcpServer : IDisposable
{
    Task StartAsync(string ipAddress, int port, CancellationToken cancellationToken = default);
    Task StopAsync();
}
