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
        public string Name { get; set; }
        public string Password { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
  
              
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


            Name = (string)json["Username"];
            Password = (string)json["Password"];
        }
    }
}
