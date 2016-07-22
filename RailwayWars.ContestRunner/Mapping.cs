using System.Collections.Generic;
using AutoMapper;
using RailwayWars.Contracts;

namespace RailwayWars.ContestRunner
{
    public static class Mapping
    {
        static Mapping()
        {
            Mapper.CreateMap<RemotePlayer, PlayerPublicInfoDTO>();
            Mapper.CreateMap<GameState, GameStateDTO>()
                  .ForMember(g => g.FreeCells, gs => gs.MapFrom(g => g.FreeCells.Values))
                  .ForMember(g => g.Railways, gs => gs.MapFrom(g => g.Railways.Values));
            Mapper.CreateMap<Cell, CellDTO>();
            Mapper.CreateMap<FreeCell, FreeCellDTO>();
            Mapper.CreateMap<Railway, RailwayDTO>();

            Mapper.AssertConfigurationIsValid();
        }

        public static TournamentStateDTO CreateTournamentStateDTO(int gameNumber, GameState gameState,
            IEnumerable<IPlayer> players)
        {
            return new TournamentStateDTO
            {
                GameState = Mapper.Map<GameStateDTO>(gameState),
                GameNumber = gameNumber,
                Players = Mapper.Map<IEnumerable<PlayerPublicInfoDTO>>(players)
            };
        }

        public static GameStateDTO CreateGameStateDTO(GameState gameState)
        {
            return Mapper.Map<GameStateDTO>(gameState);
        }
    }
}
