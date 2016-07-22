using NUnit.Framework;

namespace RailwayWars.ContestRunner.Tests
{
    [TestFixture()]
    public class LocationTests
    {
        class Location : ILocation
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        //     4 4 4 4 4        -> 4 4 4 4 4
        //    4 3 3 3 3 4       -> 4 3 3 3 3 4
        //   4 3 2 2 2 3 4      -> 4 3 2 2 2 3 4
        //  4 3 2 1 1 2 3 4     -> 4 3 2 1 1 2 3 4
        // 4 3 2 1 0 1 2 3 4    -> 4 3 2 1 0 1 2 3 4
        //  4 3 2 1 1 2 3 4     ->   4 3 2 1 1 2 3 4
        //   4 3 2 2 2 3 4      ->     4 3 2 2 2 3 4
        //    4 3 3 3 3 4       ->       4 3 3 3 3 4
        //     4 4 4 4 4        ->         4 4 4 4 4
        [TestCase(10, 10, 0)]
        [TestCase(11, 10, 1)]
        [TestCase(09, 10, 1)]
        [TestCase(10, 11, 1)]
        [TestCase(10, 09, 1)]
        [TestCase(11, 11, 1)]
        [TestCase(09, 09, 1)]
        [TestCase(08, 08, 2)]
        [TestCase(09, 08, 2)]
        [TestCase(10, 08, 2)]
        [TestCase(08, 09, 2)]
        [TestCase(11, 09, 2)]
        [TestCase(08, 10, 2)]
        [TestCase(12, 10, 2)]
        [TestCase(09, 11, 2)]
        [TestCase(12, 11, 2)]
        [TestCase(10, 12, 2)]
        [TestCase(11, 12, 2)]
        [TestCase(12, 12, 2)]
        public void ShouldCalculateHexManhattanDistance(int x, int y, int distance)
        {
            var p1 = new Location { X = 10, Y = 10 };
            var p2 = new Location { X = x, Y = y };
            var d = p1.HexManhattanDistance(p2);
            Assert.AreEqual(distance, d);
        }
    }
}
