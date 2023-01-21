using MCTG_Brian.Battle;
using MCTG_Brian.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.BATTLE
{
    internal class BattleLogicTests
    {
        [Test]
        public void TestCalculate_Card1MonsterCard2Monster_ReturnsCorrectWinner()
        {
            // Arrange
            Card card1 = new Card("FireGoblin") { Damage = 10.0 };
            Card card2 = new Card("WaterDragon") { Damage = 20.0 };
            

            // Act
            var result = BattleLogic.Calculate(card1, card2);

            // Assert
            Assert.AreEqual(card1, result);
        }

        [Test]
        public void TestCalculate_Card1SpellCard2Spell_ReturnsCorrectWinner()
        {
            // Arrange
            Card card1 = new Card("FireSpell") { Damage = 10.0 };
            Card card2 = new Card("WaterSpell") { Damage = 20.0 };

            // Act
            var result = BattleLogic.Calculate(card1, card2);

            // Assert
            Assert.AreEqual(card1, result);
        }

        [Test]
        public void TestIsMonsterAfraid_Card1GoblinCard2Dragon_ReturnsTrue()
        {
            // Arrange
            Card.MonsterType monster1 = Card.MonsterType.Goblin;
            Card.MonsterType monster2 = Card.MonsterType.Dragon;

            // Act
            var result = BattleLogic.IsMonsterAfraid(monster1, monster2);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void TestIsMonsterControlled_Card1WizardCard2Orks_ReturnsTrue()
        {
            // Arrange
            Card.MonsterType monster1 = Card.MonsterType.Wizard;
            Card.MonsterType monster2 = Card.MonsterType.Orks;

            // Act
            var result = BattleLogic.IsMonsterControlled(monster1, monster2);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void TestIsMonsterImmune_Card1KrakenCardTypeSpell_ReturnsTrue()
        {
            // Arrange
            Card.MonsterType monster = Card.MonsterType.Kraken;
            Card.CardType cardType = Card.CardType.Spell;

            // Act
            var result = BattleLogic.IsMonsterImmune(monster, cardType);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
