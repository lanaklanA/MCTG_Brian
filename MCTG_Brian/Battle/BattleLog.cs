using MCTG_Brian.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG_Brian.Battle
{
    public class BattleLog
    {
        public List<string> protocol = new();
        public User winner { get; set; }
        public User loser { get; set; }
        public bool isDraw = false;

        public void addToProtocol(string message)
        {
            protocol.Add(message);
        }


        
        public void printProtocol()
        {
            foreach (string line in protocol.ToList())
            {
                Console.WriteLine(line);
            }
           
        }

        public void classifyUsers(User winner, User loser)
        {
            this.winner = winner;
            this.loser = loser;
        }
        


    }

}
