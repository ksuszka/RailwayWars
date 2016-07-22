using System;
using System.Collections.Generic;
using System.Linq;

namespace RailwayWars.ContestRunner
{
    public static class BoardFactory
    {
        private static readonly Random _random = new Random();

        private static Dictionary<Cell, int> Blur(this Dictionary<Cell, int> cells)
        {
            return cells.ToDictionary(kv => kv.Key, kv =>
            {
                var values = (new[] { kv.Value }).Concat(kv.Key.GetNeighbours().Where(cells.ContainsKey).Select(n => cells[n]));
                return (int)values.Average();
            });
        }

        private static Dictionary<Cell, int> RescaleFreeCells(this Dictionary<Cell, int> cells, int minPrice, int maxPrice)
        {
            var min = cells.Values.Min();
            var max = Math.Max(cells.Values.Max(), min + 1);
            return cells.ToDictionary(kv => kv.Key, kv => minPrice + (maxPrice - minPrice) * (kv.Value - min) / (max - min));
        }

        private static IEnumerable<Cell> GenerateGrid(int size)
        {
            return Enumerable.Range(0, size).SelectMany(x => Enumerable.Range(0, size).Select(y => new Cell(x + y / 2, y)));
        }

        private static Dictionary<Cell, int> GenerateRandomGrid(int size)
        {
            return GenerateGrid(size).ToDictionary(c => c, c => _random.Next(0, 1000));
        }

        private static Dictionary<Cell, int> GenerateEmptyGrid(int size)
        {
            return GenerateGrid(size).ToDictionary(c => c, c => 0);
        }

        private static ISet<Cell> GenerateCities(this Dictionary<Cell, int> cells, int minDistance)
        {
            var cities = new HashSet<Cell>();
            var cellsLeft = cells.Keys.ToList();
            while (cellsLeft.Any())
            {
                var city = cellsLeft[_random.Next(0, cellsLeft.Count - 1)];
                cities.Add(city);
                cellsLeft = cellsLeft.Where(c => c.HexManhattanDistance(city) > minDistance).ToList();
            }

            return cities;
        }

        private static int CalculateTurnProfit(IEnumerable<FreeCell> cells, int playersCount, int turnTimeMilliseconds, int expectedMatchDurationSeconds)
        {
            // Calculates money amount added for each player every turn so there is enough money to finish game in more-or-less fixed number of turns.
            var expectedTurnCount = expectedMatchDurationSeconds * 1000 / turnTimeMilliseconds;
            return cells.Select(c => c.Price).Sum() / expectedTurnCount / playersCount;
        }

        private static int CalculateMaxInactivityTurns(IEnumerable<FreeCell> cells, int turnProfit)
        {
            var maxPrice = cells.Select(c => c.Price).Max();
            return Math.Max(10, 10 * maxPrice / turnProfit);
        }

        public static BoardDefinition RandomSquareBoard(int size, int waterPercentage, int turnTimeMilliseconds, int expectedMatchDurationSeconds, int playersCount)
        {
            if (size <= 10) throw new ArgumentException($"Board side length must be greater than 10.");

            var turnTime = TimeSpan.FromMilliseconds(turnTimeMilliseconds);
            var cells = GenerateRandomGrid(size).Blur().Blur();

            var threshold = cells.Values.OrderByDescending(i => i).Skip(cells.Count * waterPercentage / 100).First();
            cells = cells.Where(kv => kv.Value <= threshold).ToDictionary(kv => kv.Key, kv => kv.Value);

            cells = cells.RescaleFreeCells(100, 500);

            var cities = cells.GenerateCities(5);

            var freeCells = cells.Where(kv => !cities.Contains(kv.Key)).Select(kv => new FreeCell(kv.Key.X, kv.Key.Y, kv.Value));
            var turnProfit = CalculateTurnProfit(freeCells, playersCount, turnTimeMilliseconds, expectedMatchDurationSeconds);
            var maxInactivityTurns = CalculateMaxInactivityTurns(freeCells, turnProfit);
            return new BoardDefinition(cities.ToList(), freeCells.ToList(), turnTime, turnProfit, maxInactivityTurns);
        }

        public static BoardDefinition MountainBoard(int size, int turnTimeMilliseconds, int expectedMatchDurationSeconds, int playersCount)
        {
            if (size <= 10) throw new ArgumentException($"Board side length must be greater than 10.");

            var turnTime = TimeSpan.FromMilliseconds(turnTimeMilliseconds);

            var cells = GenerateEmptyGrid(size);
            cells[new Cell(size / 2 + size / 2 / 2, size / 2)] = 10000000;
            cells = Enumerable.Range(0, size).Aggregate(cells, (c, _) => c.Blur());

            var threshold = cells.Values.OrderByDescending(i => i).Skip(cells.Count * 65 / 100).First();
            cells = cells.Where(kv => kv.Value >= threshold).ToDictionary(kv => kv.Key, kv => kv.Value);

            cells = cells.RescaleFreeCells(100, 1000);

            var cities = cells.GenerateCities(5);

            var freeCells = cells.Where(kv => !cities.Contains(kv.Key)).Select(kv => new FreeCell(kv.Key.X, kv.Key.Y, kv.Value));
            var turnProfit = CalculateTurnProfit(freeCells, playersCount, turnTimeMilliseconds, expectedMatchDurationSeconds);
            var maxInactivityTurns = CalculateMaxInactivityTurns(freeCells, turnProfit);
            return new BoardDefinition(cities.ToList(), freeCells.ToList(), turnTime, turnProfit, maxInactivityTurns);
        }

        public static Func<int, BoardDefinition> CreateRandomSquareBoardLoopGenerator(int size, int maxWaterPercentage, int turnTimeMilliseconds, int expectedMatchDurationSeconds)
        {
            var currentWaterPercentage = 0;
            return playersCount =>
            {
                var board = RandomSquareBoard(size, currentWaterPercentage, turnTimeMilliseconds, expectedMatchDurationSeconds, playersCount);

                currentWaterPercentage += 5;
                if (currentWaterPercentage > maxWaterPercentage)
                {
                    currentWaterPercentage = 0;
                }

                return board;
            };
        }
    }
}
