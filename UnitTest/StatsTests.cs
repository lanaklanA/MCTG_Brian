using MCTG_Brian.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.MODEL
{
    [TestFixture]
    public class StatsTests
    {
        [Test]
        public void TestUpdateWinStats_Adds10ToEloAndIncrementsWins()
        {
            // Arrange
            Stats stats = new Stats();

     
            // Act
            stats.updateWinStats();

            // Assert
            Assert.That(stats.Elo, Is.EqualTo(110));
            Assert.That(stats.Wins, Is.EqualTo(1));
        }

        [Test]
        public void TestUpdateLoseStats_Subtracts10FromEloAndIncrementsLoses()
        {
            // Arrange
            Stats stats = new Stats();

            // Act
            stats.updateLoseStats();

            // Assert
            Assert.AreEqual(90, stats.Elo);
            Assert.AreEqual(1, stats.Loses);
        }

        [Test]
        public void TestUpdateDrawStats_IncrementsDraws()
        {
            // Arrange
            Stats stats = new Stats();

            // Act
            stats.updateDrawStats();

            // Assert
            Assert.AreEqual(1, stats.Draws);
        }
    }
}
