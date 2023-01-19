using MCTG_Brian.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG_Brian.Battle
{
    public class BattleLogger
    {
        public List<string> protocol = new();
        public User winner { get; set; }
        public User loser { get; set; }
        public bool isDraw = false;
        public Tuple<User, List<Card>> Deck1, Deck2;

        public void saveNewDecks(Tuple<User, List<Card>> d1, Tuple<User, List<Card>> d2)
        {
            this.Deck1 = d1;
            this.Deck2 = d2;
        }
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
