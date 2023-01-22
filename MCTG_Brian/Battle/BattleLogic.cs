using MCTG_Brian.Database.Models;
using System.Diagnostics;

namespace MCTG_Brian.Battle
{
    public static class BattleLogic
    {
        /// <summary>
        /// calculates the winner card of a battle
        /// </summary>
        /// <param name="card1"></param>
        /// <param name="card2"></param>
        /// <param name="testing"></param>
        /// <returns></returns>
        public static Card? Calculate(Card card1, Card card2, bool testing = false)
        {
            if(testing)
            {
                card1.Damage = 0;

                if (card1.Damage < card2.Damage)
                {
                    return card1;
                }
                else if (card1.Damage > card2.Damage)
                {
                    return card2;
                }
                return null;
            }

            if (card1.Type == Card.CardType.Monster && card2.Type == Card.CardType.Monster)
            {
                if (IsMonsterAfraid(card1.Monster, card2.Monster))
                {
                    return card1;
                }
                if (IsMonsterControlled(card1.Monster, card2.Monster))
                {
                    return card2;
                }
                if (IsMonsterImmune(card1.Monster, card2.Type))
                {
                    return card2;
                }
                if (IsMonsterEvading(card1.Monster, card2.Monster))
                {
                    return card2;
                }
                if (card1.Damage < card2.Damage)
                {
                    return card1;
                }
                if (card1.Damage > card2.Damage)
                {
                    return card2;
                }
                return null;
            }
            else if (card1.Type == Card.CardType.Spell && card2.Type == Card.CardType.Spell)
            {
                card1.Damage = GetSpellDamage(card1, card2);
                if (card1.Damage > card2.Damage)
                {
                    return card2;
                }
                if (card1.Damage < card2.Damage)
                {
                    return card1;
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// Checks if the monster is afraid of the other monster
        /// </summary>
        /// <param name="monster1"></param>
        /// <param name="monster2"></param>
        /// <returns></returns>
        public static bool IsMonsterAfraid(Card.MonsterType monster1, Card.MonsterType monster2)
        {
            if (monster1 == Card.MonsterType.Goblin && monster2 == Card.MonsterType.Dragon)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the monster is afraid of the other monster
        /// </summary>
        /// <param name="monster1"></param>
        /// <param name="monster2"></param>
        /// <returns></returns>
        public static bool IsMonsterControlled(Card.MonsterType monster1, Card.MonsterType monster2)
        {
            if (monster1 == Card.MonsterType.Wizard && monster2 == Card.MonsterType.Orks)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the monster is immune
        /// </summary>
        /// <param name="monster1"></param>
        /// <param name="cardType"></param>
        /// <returns></returns>
        public static bool IsMonsterImmune(Card.MonsterType monster1, Card.CardType cardType)
        {
            if (monster1 == Card.MonsterType.Kraken && cardType == Card.CardType.Spell)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if monster is evading
        /// </summary>
        /// <param name="monster1"></param>
        /// <param name="monster2"></param>
        /// <returns></returns>
        private static bool IsMonsterEvading(Card.MonsterType monster1, Card.MonsterType monster2)
        {
            if (monster1 == Card.MonsterType.FireElf && monster2 == Card.MonsterType.Dragon)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calculate the damage
        /// </summary>
        /// <param name="card1"></param>
        /// <param name="card2"></param>
        /// <returns></returns>
        private static double GetSpellDamage(Card card1, Card card2)
        {
            if (card1.Element == Card.ElementType.Water && card2.Element == Card.ElementType.Fire)
            {
                return card1.Damage * 2;
            }
            else if (card1.Element == Card.ElementType.Fire && card2.Element == Card.ElementType.Normal)
            {
                return card1.Damage * 2;
            }
            else if (card1.Element == Card.ElementType.Normal && card2.Element == Card.ElementType.Water)
            {
                return card1.Damage / 2;
            }
            else if (card1.Element == card2.Element)
            {
                return card1.Damage;
            }
            else
            {
                return 0;
            }
        }
    }
}

