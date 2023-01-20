namespace MCTG_Brian.Database.Models
{
    public class Stats
    {
        public Guid Id { get; set; }
        public int Elo { get; set; }
        public int Wins { get; set; }
        public int Loses { get; set; }
        public int Draws { get; set; }

        public Stats()
        {
            Id = Guid.NewGuid();
            Elo = 100;
            Wins = 0;
            Loses = 0;
            Draws = 0;
        }
       
        public void updateWinStats()
        {
            Elo += 10;
            Wins += 1;
        }

        public void updateLoseStats()
        {
            Elo -= 10;
            Loses += 1;
        }

        public void updateDrawStats()
        {
            Draws += 1;
        }
    }
}
