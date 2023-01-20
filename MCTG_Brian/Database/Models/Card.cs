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

        public Card() { }
        public Card(JObject json)
        {
            Id = (Guid)json["Id"];
            Name = (string)json["Name"];
            Damage = (double)json["Damage"];

            Type = DetermineCardType(Name);
            Element = DetermineElementType(Name);
            Monster = DetermineMonsterType(Name);
        }

        private CardType DetermineCardType(string name)
        {
            if (name.Contains("Monster"))
                return CardType.Monster;
            if (name.Contains("Spell"))
                return CardType.Spell;
            if (name.Contains("Normal"))
                return CardType.Normal;

            return CardType.Normal;
        }

        private ElementType DetermineElementType(string name)
        {
            if (name.Contains("Water"))
                return ElementType.Water;
            if (name.Contains("Fire"))
                return ElementType.Fire;
            if (name.Contains("Normal"))
                return ElementType.Normal;

            return ElementType.Normal;
        }

        private MonsterType DetermineMonsterType(string name)
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