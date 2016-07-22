using System.Collections.Generic;

namespace RailwayWars.ContestRunner
{
    public interface IPlayer
    {
        string Id { get; }
        void NewTurn(GameState gameState);
        IEnumerable<IPlayerCommand> GetCommands();
    }
}
