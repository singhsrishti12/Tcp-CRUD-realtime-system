
using ExamServer.Data;
using ExamServer.Services;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Net;
//using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace ExamServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
            ));
            builder.Services.AddSingleton<Services.WebSocketManager>();
            builder.Services.AddScoped<TcpCommandProcessor>();
            builder.Services.AddSingleton<TcpServer>();

            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
            });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseCors("AllowAngular");
            app.UseWebSockets();
            app.Map("/ws", async context =>
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 400;
                    return;
                }

                var socket = await context.WebSockets.AcceptWebSocketAsync();

                var wsManager = context.RequestServices
                    .GetRequiredService<Services.WebSocketManager>();

                await wsManager.AddClient(socket);
            });

            var tcpServer = app.Services.GetRequiredService<TcpServer>();
            _ = Task.Run(() => tcpServer.Start());
            app.MapControllers();

            app.Run();
        }
    }
}
