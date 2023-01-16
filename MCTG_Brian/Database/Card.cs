using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG_Brian.Database
{
    public class Card
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Damage { get; set; }

        public Card(JObject json)
        {
            Id = (Guid)json["Id"];
            Name = (string)json["Name"];
            Damage = (double)json["Damage"];
        }
    }
}