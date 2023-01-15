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
        public bool isLoggedIn { get; set; }
              
        public User(JObject json)
        {
            Id = (Guid)json["Id"];
            Name = (string)json["Username"];
            Password = (string)json["Password"];
        }

   
    }

}
