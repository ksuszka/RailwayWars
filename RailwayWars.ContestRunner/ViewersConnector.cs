using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RailwayWars.ContestRunner
{
    internal class ViewersConnector
    {
        private readonly TextTcpListener _listener;
        public event Action<string> StateUpdated;
        public void UpdateState(string state) => StateUpdated?.Invoke(state);

        public ViewersConnector(int listenerPort)
        {
            _listener = new TextTcpListener(listenerPort, ConnectionHandler);
        }

        public Task Start(CancellationToken cancellationToken) => _listener.Start(cancellationToken);

        private async Task ConnectionHandler(Func<Task<string>> readLine, Func<string, Task> writeLine, CancellationTokenSource cts)
        {
            await HandleOutgoingData(writeLine, cts.Token);
        }

        private async Task HandleOutgoingData(Func<string, Task> writeLine,
            CancellationToken cancellationToken)
        {
            var dataToSend = new BufferBlock<string>();
            var statusUpdater = new Action<string>(state => dataToSend.Post(state));
            try
            {
                StateUpdated += statusUpdater;

                while (!cancellationToken.IsCancellationRequested)
                {
                    var data = await dataToSend.ReceiveAsync(cancellationToken);
                    await writeLine(data);
                }
            }
            finally
            {
                StateUpdated -= statusUpdater;
            }
        }
    }
}
