using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MCTG_Brian.Database.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int? Coins { get; set; }
        public string? Bio { get; set; }
        public string? Image { get; set; }
        public Stats? Stats { get; set; }
        public List<Card>? Stack { get; set; }
        public List<Card>? Deck { get; set; }



    }
}
