using System.Collections.Generic;
using AutoMapper;
using RailwayWars.Contracts;

namespace RailwayWars.ContestRunner
{
    public static class Mapping
    {
        private static readonly IMapper _mapper;

        static Mapping()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<RemotePlayer, PlayerPublicInfoDTO>();
                cfg.CreateMap<GameState, GameStateDTO>()
                      .ForMember(g => g.FreeCells, gs => gs.MapFrom(g => g.FreeCells.Values))
                      .ForMember(g => g.Railways, gs => gs.MapFrom(g => g.Railways.Values));
                cfg.CreateMap<Cell, CellDTO>();
                cfg.CreateMap<FreeCell, FreeCellDTO>();
                cfg.CreateMap<Railway, RailwayDTO>();
            });

            config.AssertConfigurationIsValid();

            _mapper = config.CreateMapper();
        }

        public static TournamentStateDTO CreateTournamentStateDTO(int gameNumber, GameState gameState,
            IEnumerable<IPlayer> players)
        {
            return new TournamentStateDTO
            {
                GameState = _mapper.Map<GameStateDTO>(gameState),
                GameNumber = gameNumber,
                Players = _mapper.Map<IEnumerable<PlayerPublicInfoDTO>>(players)
            };
        }

        public static GameStateDTO CreateGameStateDTO(GameState gameState)
        {
            return _mapper.Map<GameStateDTO>(gameState);
        }
    }
}
