using MCTG_Brian.Database.Models;

namespace MCTG_Brian.Battle
{
    public static class BattleLogic
    {
         public static Card Calculate(Card card1, Card card2)
            {
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

 
        private static bool IsMonsterAfraid(Card.MonsterType monster1, Card.MonsterType monster2)
        {
            if (monster1 == Card.MonsterType.Goblin && monster2 == Card.MonsterType.Dragon)
            {
                return true;
            }
            return false;
        }

        private static bool IsMonsterControlled(Card.MonsterType monster1, Card.MonsterType monster2)
        {
            if (monster1 == Card.MonsterType.Wizard && monster2 == Card.MonsterType.Orks)
            {
                return true;
            }
            return false;
        }

        private static bool IsMonsterImmune(Card.MonsterType monster1, Card.CardType cardType)
        {
            if (monster1 == Card.MonsterType.Kraken && cardType == Card.CardType.Spell)
            {
                return true;
            }
            return false;
        }

        private static bool IsMonsterEvading(Card.MonsterType monster1, Card.MonsterType monster2)
        {
            if (monster1 == Card.MonsterType.FireElf && monster2 == Card.MonsterType.Dragon)
            {
                return true;
            }
            return false;
        }

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

//public static Card Calculate(Card card1, Card card2)
//{

//    if (card1.Damage < card2.Damage)
//    {
//        return card1;
//    }
//    else if (card1.Damage > card2.Damage)
//    {
//        return card2;
//    }
//    return null;
//}