using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MCTG_Brian.Database
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Coins { get; set; }
        public string? Bio { get; set; }
        public string? Image { get; set; }
        public Stats Stats { get; set; }
        public List<Card> Stack { get; set; }
        public List<Card> Deck { get; set; }

        
        public User() { }
        public User(string name, string password, Guid? id = null, string bio = null, string image = null)
        {
            Id = id ?? Guid.NewGuid();
            Username = name;
            Password = password;
            Coins = 20;
            Bio = bio;
            Image = image;
            Stats = new Stats();
        }

        


       

        public User(JObject json)
        {
            if (!json.TryGetValue("Id", out JToken id))
            {
                Id = Guid.NewGuid();
            }
            else
            {
                Id = (Guid)id;
            }


            Username = (string)json["Username"];
            Password = (string)json["Password"];
        }
    }
}
