using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RailwayWars.ContestRunner
{
    public class RemotePlayer : IPlayer
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private DateTime _lastCommandTime = DateTime.MinValue;
        private DateTime _lastTurnStartTime = DateTime.UtcNow;
        private BufferBlock<string> commandQueue = new BufferBlock<string>();
        public string Name { get; set; }
        public int? Rating { get; set; }
        public string LoginId { get; set; }
        public event Action<string> GameStateUpdated;
        public string Id { get; set; }

        public int? LastCommandTimeMilliseconds
        {
            get
            {
                var commandTime = _lastCommandTime;
                var turnStartTime = _lastTurnStartTime;
                Thread.MemoryBarrier();
                if (turnStartTime > commandTime) return null;
                return (commandTime - turnStartTime).Milliseconds;
            }
        }

        public void NewTurn(GameState gameState)
        {
            _lastTurnStartTime = DateTime.UtcNow;
            GameStateUpdated?.Invoke(JsonConvert.SerializeObject(Mapping.CreateGameStateDTO(gameState),
                jsonSerializerSettings));
        }

        private Regex buyCommand = new Regex(@"^\s*BUY\s+(?<x>\d+)\s+(?<y>\d+)\s+(?<price>\d+)\s*$", RegexOptions.IgnoreCase);

        public IEnumerable<IPlayerCommand> GetCommands()
        {
            IList<string> commands;
            if (!commandQueue.TryReceiveAll(out commands))
            {
                commands = new List<string>();
            }
            return commands
                .Select(c => buyCommand.Match(c))
                .Where(m => m.Success)
                .Select(m => (IPlayerCommand)new PlayerOffer(Id, Convert.ToInt32(m.Groups["x"].Value), Convert.ToInt32(m.Groups["y"].Value), Convert.ToInt32(m.Groups["price"].Value)));
        }

        public void AddCommand(string command)
        {
            // Called from various threads by PlayersConnector.
            commandQueue.Post(command);
            _lastCommandTime = DateTime.UtcNow;
        }
    }
}
