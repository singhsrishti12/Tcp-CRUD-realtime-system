using ExamServer.Data;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ExamServer.Services
{
    public class WebSocketManager
    {
        private readonly List<WebSocket> clients = new();
        private readonly IServiceScopeFactory scopeFactory;

        public WebSocketManager(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public async Task AddClient(WebSocket socket)
        {
            clients.Add(socket);
            Console.WriteLine("WebSocket client connected");

            // Detect disconnects in background
            _ = Task.Run(async () =>
            {
                var buffer = new byte[1024 * 4];
                try
                {
                    while (socket.State == WebSocketState.Open)
                    {
                        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Close)
                            break;
                    }
                }
                catch { /* ignored */ }
                finally
                {
                    clients.Remove(socket);
                    Console.WriteLine("WebSocket client disconnected");
                }
            });

            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Send initial Students
            var students = context.Students.ToList();
            await Send(socket, new
            {
                eventType = "READ",
                entity = "Student",
                data = students
            });

            // Send initial Courses
            var courses = context.Courses.ToList();
            await Send(socket, new
            {
                eventType = "READ",
                entity = "Course",
                data = courses
            });
        }

        public async Task BroadCast(string msg)
        {
            var buffer = Encoding.UTF8.GetBytes(msg);
            var disconnectedSockets = new List<WebSocket>();

            foreach (var socket in clients.ToList())
            {
                try
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        await socket.SendAsync(
                            new ArraySegment<byte>(buffer),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                    else
                    {
                        disconnectedSockets.Add(socket);
                    }
                }
                catch (WebSocketException)
                {
                    disconnectedSockets.Add(socket);
                }
            }

            // Remove dead sockets
            foreach (var dead in disconnectedSockets)
            {
                clients.Remove(dead);
            }

            Console.WriteLine("Broadcast sent: " + msg);
        }

        private async Task Send(WebSocket socket, object obj)
        {
            var json = JsonSerializer.Serialize(obj);
            var buffer = Encoding.UTF8.GetBytes(json);

            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(
                        new ArraySegment<byte>(buffer),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
            }
            catch (WebSocketException)
            {
                clients.Remove(socket);
                Console.WriteLine("Failed to send to a client, removed from list.");
            }
        }
    }
}

















