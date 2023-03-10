using MCTG_Brian.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MCTG_Brian.Database.Models.Card;

namespace UnitTest.Model
{
    [TestFixture]
    public class CardTests
    {
        [Test]
        public void TestDetermineCardType_NameContainsSpell_ReturnsSpell()
        {
            // Arrange
            var card = new Card("FireSpell");

            // Act
            var result = card.DetermineCardType(card.Name);

            // Assert
            Assert.That(result, Is.EqualTo(Card.CardType.Spell));
        }


        [Test]
        public void TestDetermineCardType_NameDoesNotContainSpell_ReturnsMonster()
        {
            //Arrange
            Card card = new Card("WaterGoblin");
           

            //Act
            var result = card.DetermineCardType(card.Name);

            //Assert
            Assert.That(result, Is.EqualTo(Card.CardType.Monster));
        }

        [Test]
        public void TestDetermineElementType_NameDoesNotContainWaterOrFire_ReturnsNormal()
        {
            //Arrange
            Card card = new Card("Goblin");

            //Act
            var result = card.DetermineElementType(card.Name);

            //Assert
            Assert.That(result, Is.EqualTo(Card.ElementType.Normal));
        }

        [Test]
        public void TestDetermineElementType_NameContainsWater_ReturnsWater()
        {
            // Arrange
            var card = new Card("WaterGoblin");

   
           // Act
            var result = card.DetermineElementType(card.Name);

            // Assert
            Assert.That(result, Is.EqualTo(ElementType.Water));
        }

        [Test]
        public void TestDetermineElementType_NameContainsFire_ReturnsFire()
        {
            // Arrange
            var card = new Card("FireDragon");

            // Act
            var result = card.DetermineElementType(card.Name);

            // Assert
            Assert.That(result, Is.EqualTo(ElementType.Fire));
        }

        [Test]
        public void TestDetermineElementType_NameContainsNormal_ReturnsNormal()
        {
            // Arrange
            var card = new Card("NormalCard");

            // Act
            var result = card.DetermineElementType(card.Name);

            // Assert
            Assert.That(result, Is.EqualTo(ElementType.Normal));
        }

        [Test]
        public void TestDetermineElementType_NameDoesNotContainWaterFireNormal_ReturnsNormal()
        {
            // Arrange
            var card = new Card("Card");

            // Act
            var result = card.DetermineElementType(card.Name);

            // Assert
            Assert.That(result, Is.EqualTo(ElementType.Normal));
        }

    }
}