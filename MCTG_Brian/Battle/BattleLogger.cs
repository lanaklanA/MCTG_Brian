using MCTG_Brian.Database.Models;

namespace MCTG_Brian.Battle
{
    public class BattleLogger
    {
        public List<string> protocol = new();
        public bool isDraw = false;
        public User winner { get; set; }
        public User loser { get; set; }

        public void addToProtocol(string message)
        {
            protocol.Add(message);
        }
        
        public void classifyUsers(User winner, User loser)
        {
            this.winner = winner;
            this.loser = loser;
        }
        public void printProtocol()
        {
            foreach (string line in protocol.ToList())
            {
                Console.WriteLine(line);
            }
           
        }
    }
}
