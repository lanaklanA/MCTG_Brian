using MCTG_Brian.Battle;
using MCTG_Brian.Database.Models;

namespace UnitTest.BATTLE
{
        [TestFixture]
        public class BattleLoggerTests
        {
            private BattleLogger _logger;

            [SetUp]
            public void SetUp()
            {
                _logger = new BattleLogger();
            }

            [Test]
            public void TestAddToProtocol_AddsMessageToProtocol()
            {
                //Arrange
                string message = "Player 1 played card: Fire Spell";

                //Act
                _logger.addToProtocol(message);

                //Assert
                Assert.AreEqual(message, _logger.protocol[0]);
            }

            [Test]
            public void TestClassifyUsers_SetsWinnerAndLoserProperties()
            {
                //Arrange
                User winner = new User() { Username = "winner" };
                User loser = new User() { Username = "loser" };

                //Act
                _logger.classifyUsers(winner, loser);

                //Assert
                Assert.AreEqual(winner, _logger.winner);
                Assert.AreEqual(loser, _logger.loser);
            }

           
        }
}
