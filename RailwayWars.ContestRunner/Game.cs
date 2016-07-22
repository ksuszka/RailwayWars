using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RailwayWars.ContestRunner
{
    internal class Game
    {
        public delegate void GameStateViewHandler(GameState gameState);

        private readonly GameState _gameState;
        private readonly GameStateViewHandler _gameStateWatcher;
        private readonly IReadOnlyList<IPlayer> _players;

        public Game(BoardDefinition boardDefinition, IReadOnlyList<IPlayer> players,
            GameStateViewHandler gameStateWatcher)
        {
            _players = players;
            _gameStateWatcher = gameStateWatcher;
            _gameState = GameState.FromBoardDefinition(boardDefinition, players);
        }

        public IDictionary<string, int> Scores => _gameState.Scores;

        public void Run()
        {
            while (!_gameState.Finished)
            {
                Console.WriteLine($"Turn {_gameState.Turn}");

                // 1. report board state to players and viewers
                _gameStateWatcher(_gameState);
                _players.ToList().ForEach(player => player.NewTurn(_gameState));

                // 2. wait for commands request
                Thread.Sleep(_gameState.TurnTime);

                // 3. perform commands
                _gameState.ApplyCommands(_players.SelectMany(p => p.GetCommands()));
            }

            // Report state last time
            _gameStateWatcher(_gameState);
            _players.ToList().ForEach(player => player.NewTurn(_gameState));
        }
    }
}
