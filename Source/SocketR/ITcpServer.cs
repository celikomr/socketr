using System.Net.Sockets;

namespace SocketR;

/// <summary>
/// Represents a TCP server.
/// </summary>
public interface ITcpServer : IDisposable
{
    /// <summary>
    /// Starts the TCP server asynchronously.
    /// </summary>
    /// <param name="ipAddress">The IP address to bind the server to.</param>
    /// <param name="port">The port number to listen on.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartAsync(string ipAddress, int port, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the TCP server asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StopAsync();

    /// <summary>
    /// Registers an action to be executed when a client connects to the server.
    /// </summary>
    /// <param name="clientConnectedAction">The action to execute when a client connects.</param>
    /// <returns>The current instance of the ITcpServer interface.</returns>
    ITcpServer OnClientConnected(Action<TcpClient> clientConnectedAction);

    /// <summary>
    /// Registers a function to handle received data asynchronously.
    /// </summary>
    /// <param name="dataReceivedFunc">The function to handle received data.</param>
    /// <returns>The current instance of the ITcpServer interface.</returns>
    ITcpServer OnDataReceived(Func<string, Task<string>> dataReceivedFunc);

    /// <summary>
    /// Registers an action to be executed when an error occurs.
    /// </summary>
    /// <param name="errorAction">The action to execute when an error occurs.</param>
    /// <returns>The current instance of the ITcpServer interface.</returns>
    ITcpServer OnError(Action<Exception> errorAction);
}
