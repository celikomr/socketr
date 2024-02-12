using SocketR;
using System.Net;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Sunucu için IP adresi ve port numarası
            string ipAddress = "127.0.0.1"; // Localhost
            int port = 12345;

            // TCP sunucusunu oluştur
            TcpServer tcpServer = new ();

            // Client bağlantısı kurulduğunda gerçekleştirilecek işlem
            //tcpServer.OnClientConnected(client =>
            //{
            //    Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
            //});

            //// Veri alındığında gerçekleştirilecek işlem
            //tcpServer.OnDataReceived(async data =>
            //{
            //    string receivedData = System.Text.Encoding.UTF8.GetString(data.Array, data.Offset, data.Count);
            //    Console.WriteLine($"Data received: {receivedData}");

            //    // Gelen veriyi işle ve bir yanıt oluştur
            //    string response = $"Echo: {receivedData}";

            //    // Yanıtı byte dizisine çevir
            //    byte[] responseData = System.Text.Encoding.UTF8.GetBytes(response);

            //    // İstemciye yanıtı gönder
            //    await client.GetStream().WriteAsync(responseData, 0, responseData.Length);
            //    Console.WriteLine($"Response sent: {response}");
            //});

            // Sunucuyu başlat
            await tcpServer.StartAsync(ipAddress, port);

            // Sunucuyu durdur
            await tcpServer.StopAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
