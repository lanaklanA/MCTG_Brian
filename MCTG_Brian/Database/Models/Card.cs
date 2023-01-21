using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;

namespace MCTG_Brian.Database.Models
{
    public class Card
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Damage { get; set; }
        public CardType Type { get; set; }
        public ElementType Element { get; set; }
        public MonsterType Monster { get; set; }

        public enum Depot { Deck, Stack }
        public enum CardType
        {
            Monster,
            Spell,
            Normal
        }
        public enum ElementType
        {
            Fire,
            Water,
            Normal,
        }
        public enum MonsterType
        {
            Goblin,
            Dragon,
            Wizard,
            Orks,
            Knight,
            Kraken,
            FireElf,
            Normal,
        }

        public Card(string name)
        {
            Name = name;
            Type = DetermineCardType(name);
            Element = DetermineElementType(name);
            Monster = DetermineMonsterType(name);
        }

        public CardType DetermineCardType(string name)
        {
            if (name.Contains("Spell"))
                return CardType.Spell;

            return CardType.Monster;
        }

        public ElementType DetermineElementType(string name)
        {
            if (name.Contains("Water"))
                return ElementType.Water;
            if (name.Contains("Fire"))
                return ElementType.Fire;
            if (name.Contains("Normal"))
                return ElementType.Normal;

            return ElementType.Normal;
        }
        
        public MonsterType DetermineMonsterType(string name)
        {
            if (name.Contains("Dragon"))
                return MonsterType.Dragon;
            if (name.Contains("Goblin"))
                return MonsterType.Goblin;
            if (name.Contains("Wizard"))
                return MonsterType.Wizard;
            if (name.Contains("Knight"))
                return MonsterType.Knight;
            if (name.Contains("Kraken"))
                return MonsterType.Kraken;
            if (name.Contains("FireElf"))
                return MonsterType.FireElf;

            return MonsterType.Normal;
        }
    }
}