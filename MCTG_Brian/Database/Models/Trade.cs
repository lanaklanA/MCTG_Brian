using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG_Brian.Database.Models
{
    public class Trade
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CardId { get; set; }
        public JObject Details { get; set; }

        public Trade()
        {
            Id = Guid.NewGuid();
            Details = new JObject();
        }
    }
}
