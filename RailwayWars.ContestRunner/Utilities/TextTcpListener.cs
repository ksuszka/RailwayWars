using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RailwayWars.ContestRunner
{
    public class TextTcpListener
    {
        private readonly int _listenerPort;
        public delegate Task NewConnectionHandler(Func<Task<string>> reader, Func<string, Task> writer, CancellationTokenSource cts);

        readonly NewConnectionHandler _newConnectionHandler;

        public TextTcpListener(int listenerPort, NewConnectionHandler newConnectionHandler)
        {
            _listenerPort = listenerPort;
            _newConnectionHandler = newConnectionHandler;
        }

        public Task Start(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Starting listener on port {_listenerPort}."); // TODO add listener name
            var listener = new TcpListener(IPAddress.Any, _listenerPort);
            listener.Start();

            return Task.Run(async () =>
            {
                var clients = new List<Task>();
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var tcpClient =
                            await listener.AcceptTcpClientAsync().ContinueWith(t => t.Result, cancellationToken);
                        clients = clients.Where(task => !task.IsCompleted).ToList();
                        // TODO: update list after any task ends
                        clients.Add(HandleSingleConnection(tcpClient, cancellationToken));
                    }
                }
                finally
                {
                    await Task.WhenAll(clients.ToArray());
                }
            }, cancellationToken);
        }

        private async Task HandleSingleConnection(TcpClient tcpClient, CancellationToken listenerCancellationToken)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(listenerCancellationToken);
            var remoteEndpoint = "Unknown";
            try
            {
                remoteEndpoint = tcpClient.Client.RemoteEndPoint.ToString();
                Console.WriteLine($"New client connected: {remoteEndpoint}");
                tcpClient.NoDelay = true;
                using (var writer = new StreamWriter(tcpClient.GetStream()))
                {
                    writer.AutoFlush = true;
                    using (var reader = new StreamReader(tcpClient.GetStream()))
                    {
                        Func<Task<string>> readLine = () => reader.ReadLineAsync(cts.Token);
                        Func<string, Task> writeLine = line => writer.WriteLineAsync(line, cts.Token);
                        await _newConnectionHandler(readLine, writeLine, cts);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Connection to {remoteEndpoint} aborted due to error: {ex.GetFlatMessage()}.");
            }
            finally
            {
                tcpClient.Close();
            }
        }
    }
}
