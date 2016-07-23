using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RailwayWars.ContestRunner.Rating;

namespace RailwayWars.ContestRunner
{
    internal class Tournament
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static readonly string PlayersDefaultFileName = "players.json";
        private static readonly string PlayersTournamentFileName = "players_tournament.json";
        private readonly List<RemotePlayer> _players;
        private readonly Func<int, BoardDefinition> _boardGenerator;
        private readonly IRatingStrategy<string> _ratingStrategy;
        private int _gameNumber;

        public IEnumerable<RemotePlayer> Players => _players;
        public TimeSpan GameBreakTime { get; set; } = TimeSpan.FromSeconds(1);
        public event Action<string> StateUpdated;

        public Tournament(Func<int, BoardDefinition> boardGenerator, IRatingStrategy<string> ratingStrategy)
        {
            _boardGenerator = boardGenerator;
            _ratingStrategy = ratingStrategy;
            _gameNumber = 0;
            _players = LoadPlayers();
            _players.ForEach(player => player.Rating = player.Rating ?? _ratingStrategy.InitialRating);
        }

        private void ReportGameState(GameState gameState)
        {
            var data = Mapping.CreateTournamentStateDTO(_gameNumber, gameState, _players);
            var serializedData = JsonConvert.SerializeObject(data, jsonSerializerSettings);
            StateUpdated?.Invoke(serializedData);
        }

        public void Run(Func<bool> stopPredicate)
        {
            Console.WriteLine("Starting tournament...");
            while (!stopPredicate())
            {
                _gameNumber++;
                Console.WriteLine($"Game {_gameNumber}");
                var board = _boardGenerator(_players.Count);
                Console.WriteLine($"Board {board}");
                var game = new Game(board, _players, ReportGameState);
                game.Run();
                UpdatePlayersScores(game.Scores);
                Thread.Sleep(GameBreakTime);
            }
            Console.WriteLine("Tournament stopped.");
        }

        private void UpdatePlayersScores(IDictionary<string, int> scores)
        {
            var playerMap = _players.ToDictionary(p => p.Id);
            var playerRatingsAndScores = scores.Select(s => new PlayerRatingScore<string>(s.Key, playerMap[s.Key].Rating ?? _ratingStrategy.InitialRating, s.Value));
            var newPlayerRatings = _ratingStrategy.Calculate(playerRatingsAndScores);
            newPlayerRatings.ToList().ForEach(pr => playerMap[pr.Id].Rating = pr.Rating);
            SavePlayers(_players);
        }

        private static void SavePlayers(IEnumerable<RemotePlayer> players)
        {
            var serializedData = JsonConvert.SerializeObject(players, Formatting.Indented, jsonSerializerSettings);
            File.WriteAllText(PlayersTournamentFileName, serializedData);
        }

        private static List<RemotePlayer> LoadPlayers()
        {
            var playersFileName = File.Exists(PlayersTournamentFileName)
                ? PlayersTournamentFileName
                : PlayersDefaultFileName;
            Console.WriteLine($"Loading players list from {playersFileName}...");
            var players = JsonConvert.DeserializeObject<List<RemotePlayer>>(File.ReadAllText(playersFileName));
            var index = 0;
            var idGenerator = new Func<int, string>(i =>
            {
                if (i < 'Z' - 'A') return $"{(char)('A' + i)}";
                return $"{i + 1}";
            });
            players.ForEach(player => player.Id = idGenerator(index++));
            Console.WriteLine($"Loaded {players.Count} players: {string.Join(", ", players)}");
            return players;
        }
    }
}
