using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ExamServer.Services
{
    public class TcpServer
    {
        private readonly IServiceProvider provider;

        public TcpServer(IServiceProvider provider)
        {
            this.provider = provider;
        }
        public async Task Start()
        {
            var listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();
            Console.WriteLine("Server started...");

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected!");
                _ = HandleClient(client);

            }
        }
        async Task HandleClient(TcpClient client)
        {

            using var scope = provider.CreateScope();

            var processor = scope.ServiceProvider.GetRequiredService<TcpCommandProcessor>();
            var stream = client.GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            while (true)
            {
                var command = await reader.ReadLineAsync();
                if (command == null)
                    break;
                var response = await processor.Process(command);

                await writer.WriteLineAsync(response);

            }
            client.Close();
            Console.WriteLine("Client disconnected");

        }
    }
}
