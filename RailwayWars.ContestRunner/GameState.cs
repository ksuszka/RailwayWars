using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RailwayWars.ContestRunner
{
    public class GameState
    {
        public int MaxInactivityTurns;
        public int TurnsTillEnd;
        public ISet<Cell> Cities { get; private set; }
        public IDictionary<Cell, FreeCell> FreeCells { get; private set; }
        public IDictionary<string, Railway> Railways { get; private set; }
        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }
        public int Turn { get; private set; }
        public TimeSpan TurnTime { get; set; }
        public int TurnProfit { get; set; }
        public IDictionary<string, int> Scores => Railways.Values.ToDictionary(r => r.Id, r => r.Score);
        public IDictionary<Tuple<Cell, Cell>, int> _shortestPaths;

        class CellComparer : IComparer<Cell>
        {
            public int Compare(Cell a, Cell b)
            {
                return a.X == b.X ? a.Y - b.Y : a.X - b.X;
            }

            public static readonly CellComparer Instance = new CellComparer();
        }

        private static Dictionary<Tuple<Cell, Cell>, int> FindShortestCityConnections(IEnumerable<Cell> cities, IEnumerable<Cell> freeCells)
        {
            var allCells = new HashSet<Cell>(freeCells.Union(cities));
            var shortestPaths = new Dictionary<Tuple<Cell, Cell>, int>();

            // By ordering origins collection we make sure that items in tuples in shortestPath collection are ordered
            var origins = cities.OrderByDescending(c => c, CellComparer.Instance).ToList();
            while (origins.Any())
            {
                var origin = origins.Last();
                origins.RemoveAt(origins.Count - 1);

                // BFS
                var fringe = new Queue<Tuple<Cell, int>>();
                var enqueued = new HashSet<Cell>();
                var citiesLeft = new HashSet<Cell>(origins);
                fringe.Enqueue(Tuple.Create(origin, 0));
                enqueued.Add(origin);

                while (fringe.Any())
                {
                    var current = fringe.Dequeue();
                    var cell = current.Item1;
                    var distance = current.Item2;
                    if (citiesLeft.Contains(cell))
                    {
                        citiesLeft.Remove(cell);
                        shortestPaths.Add(Tuple.Create(origin, cell), distance);
                        if (!citiesLeft.Any()) break;
                    }

                    cell.GetNeighbours()
                        .Where(n => !enqueued.Contains(n))
                        .Where(n => allCells.Contains(n))
                        .ToList()
                        .ForEach(n =>
                        {
                            fringe.Enqueue(Tuple.Create(n, distance + 1));
                            enqueued.Add(n);
                        });
                }
            }

            return shortestPaths;
        }

        public static GameState FromBoardDefinition(BoardDefinition board, IEnumerable<IPlayer> players)
        {
            var st = new Stopwatch();
            st.Start();
            var shortestPaths = FindShortestCityConnections(board.Cities, board.FreeCells.Select(fc => fc.Cell));
            Console.WriteLine($"Found {shortestPaths.Count} shortest paths for {board.Cities.Count} cities in {st.ElapsedMilliseconds}ms");

            return new GameState
            {
                _shortestPaths = shortestPaths,
                TurnTime = board.TurnTime,
                Cities = new HashSet<Cell>(board.Cities),
                FreeCells = board.FreeCells.ToDictionary(c => new Cell(c), c => c),
                Railways = players.ToDictionary(p => p.Id, p => new Railway() { Id = p.Id, Money = 0, Score = 0, OwnedCells = new HashSet<Cell>() }),
                MinPrice = board.FreeCells.Select(c => c.Price).Min(),
                MaxPrice = board.FreeCells.Select(c => c.Price).Max(),
                MaxInactivityTurns = board.MaxInactivityTurns,
                TurnsTillEnd = board.MaxInactivityTurns,
                TurnProfit = board.TurnProfit
            };
        }

        public void ApplyCommands(IEnumerable<IPlayerCommand> commands)
        {
            Turn++;
            TurnsTillEnd--;

            // Perform auctions
            // Group by cells, sort by max offer (filter real offers first)
            bool auctionOccured = false;
            Func<PlayerOffer, PlayerOffer> fixOffer = po => new PlayerOffer(po.PlayerId, po.Cell, Math.Min(po.Price, Railways[po.PlayerId].Money));
            var offers = commands.OfType<PlayerOffer>().ToList();

            // remove duplicated offers, leave only those with highest bid
            offers = offers.GroupBy(o => Tuple.Create(o.PlayerId, o.Cell), (key, pos) => pos.OrderByDescending(po => po.Price).First()).ToList();

            while (offers.Any())
            {
                // Leave only valid free cells
                offers = offers.Where(po => FreeCells.ContainsKey(po.Cell)).ToList();

                // Group by cells
                var auctions = offers
                    .GroupBy(po => po.Cell, (cell, pos) =>
                    {
                        var minPrice = FreeCells[cell].Price;
                        // Fix offered prices and remove offers which are too low
                        return pos.Select(fixOffer).Where(po => po.Price >= minPrice);
                    })
                    // Leave only auctions with valid offers
                    .Where(pos => pos.Any())
                    // Sort desceding by highest valid offer
                    .OrderByDescending(pos => pos.Select(po => po.Price).Max());

                offers = auctions.Skip(1).SelectMany(pos => pos).ToList();

                if (auctions.Any())
                {
                    var bids = auctions.First();
                    var maxPrice = bids.Select(po => po.Price).Max();
                    var bestOffers = bids.Where(po => po.Price == maxPrice);
                    // If there are multiple best offers then auction is cancelled
                    if (bestOffers.Count() == 1)
                    {
                        auctionOccured = true;
                        var bestOffer = bestOffers.First();
                        // There is a winner.

                        FreeCells.Remove(bestOffer.Cell);
                        var railway = Railways[bestOffer.PlayerId];
                        railway.Money -= bestOffer.Price;
                        railway.OwnedCells.Add(bestOffer.Cell);
                    }
                }
            }

            if (auctionOccured)
            {
                TurnsTillEnd = MaxInactivityTurns;
            }

            // Add profit for players
            Railways.Values.ToList().ForEach(r => r.Money += TurnProfit);

            // Calculate scores
            Railways.Values.ToList().ForEach(r =>
            {
                // One point for every owned cell
                r.Score = r.OwnedCells.Count();

                var paths = FindShortestCityConnections(Cities, r.OwnedCells);
                r.ConnectedCitiesCount = paths.Count;

                r.Score += paths.Select(p =>
                {
                    var shortest = _shortestPaths[p.Key];
                    return shortest * shortest * shortest / p.Value;
                }).Sum();
            });
        }

        public bool Finished => !FreeCells.Any() || TurnsTillEnd <= 0;
    }
}
