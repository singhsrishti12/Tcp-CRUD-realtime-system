using System.Net.Sockets;
using System.Text;
class Program
{
    public static async Task Main()
    {
        TcpClient client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", 5000);
        Console.WriteLine("Connected to server.");

        using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        
        _ = Task.Run(async () =>
        {
            while (true)
            {
                var res = await reader.ReadLineAsync();
                Console.WriteLine("Server:"+ res);

            }
        });
        while(true)
        {
            Console.WriteLine("\nEnter command:");
            var command = Console.ReadLine();
            await writer.WriteLineAsync(command);
        }
    }
}