using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RailwayWars.ContestRunner
{
    internal class PlayersConnector
    {
        private readonly TextTcpListener _listener;
        private readonly IDictionary<string, RemotePlayer> _players;

        public PlayersConnector(int listenerPort, IEnumerable<RemotePlayer> players)
        {
            _listener = new TextTcpListener(listenerPort, ConnectionHandler);
            _players = players.ToDictionary(p => p.LoginId);
        }

        public Task Start(CancellationToken cancellationToken) => _listener.Start(cancellationToken);

        private async Task ConnectionHandler(Func<Task<string>> readLine, Func<string, Task> writeLine, CancellationTokenSource cts)
        {
            await writeLine("ID");
            var loginId = await readLine();
            RemotePlayer player;
            if (!_players.TryGetValue(loginId.Trim(), out player))
            {
                await writeLine("Invalid login id!");
            }
            else
            {
                await writeLine(player.Id);
                var tasks = new[]
                {
                                HandleIncomingData(player, readLine, cts.Token),
                                HandleOutgoingData(player, writeLine, cts.Token)
                            };
                await Task.WhenAny(tasks);
                // If reading or writing fails abort second process
                cts.Cancel();
                await Task.WhenAll(tasks);
            }

        }

        private async Task HandleIncomingData(RemotePlayer player, Func<Task<string>> readLine,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var line = (await readLine())?.Trim();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    player.AddCommand(line);
                }
            }
        }

        private async Task HandleOutgoingData(RemotePlayer player, Func<string, Task> writeLine,
            CancellationToken cancellationToken)
        {
            var dataToSend = new BufferBlock<string>();
            var statusUpdater = new Action<string>(state => dataToSend.Post(state));
            try
            {
                player.GameStateUpdated += statusUpdater;

                while (!cancellationToken.IsCancellationRequested)
                {
                    var data = await dataToSend.ReceiveAsync(cancellationToken);
                    await writeLine(data);
                }
            }
            finally
            {
                player.GameStateUpdated -= statusUpdater;
            }
        }
    }
}
