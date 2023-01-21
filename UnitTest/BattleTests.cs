using MCTG_Brian.Battle;
using MCTG_Brian.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.BATTLE
{
    [TestFixture]
    public class BattleTests
    {
        [Test]
        public void TestCatchRndCard_DeckContainsCards_ReturnsRandomCard()
        {
            //Arrange
            List<Card> deck = new List<Card>();
            deck.Add(new Card("Card1") { Id = Guid.NewGuid(), Damage = 10 });
            deck.Add(new Card("Card2") { Id = Guid.NewGuid(), Damage = 15 });
            deck.Add(new Card("Card3") { Id = Guid.NewGuid(), Damage = 20 });

            Battle battle = new Battle();

            //Act
            var result = battle.catchedRndCard(deck);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(deck.Contains(result));
        }
    }
}
