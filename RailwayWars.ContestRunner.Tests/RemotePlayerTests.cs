using NUnit.Framework;
using System.Linq;

namespace RailwayWars.ContestRunner.Tests
{
    [TestFixture()]
    public class RemotePlayerTests
    {
        [Test]
        public void ShouldParseBuyCommand()
        {
            var rp = new RemotePlayer();
            rp.Id = "A";
            rp.AddCommand("BUY 12 13 14");
            rp.AddCommand("something else ");
            rp.AddCommand(" BUY 15 16 17 ");
            rp.AddCommand("BUY 20 21 22.2");

            var commands = rp.GetCommands();
            Assert.AreEqual(2, commands.Count());
            CollectionAssert.Contains(commands, new PlayerOffer("A", 12, 13, 14));
            CollectionAssert.Contains(commands, new PlayerOffer("A", 15, 16, 17));
        }
    }
}
