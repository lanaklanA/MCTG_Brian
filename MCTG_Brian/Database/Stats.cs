namespace MCTG_Brian.Database
{
    public class Stats
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public int Elo { get; set; }
        public int Wins { get; set; }
        public int Loses { get; set; }
        public int Draws { get; set; }

        public Stats()
        {
            Id = Guid.NewGuid();
            UserId = Guid.NewGuid();
            Username = "";
            Elo = 100;
            Wins = 0;
            Loses = 0;
            Draws = 0;
        }
        public Stats(Guid userId)
        {
            UserId = userId;
            Username = "";
            Elo = 100;
            Wins = 0;
            Loses = 0;
            Draws = 0;
        }

        public void updateWinStats()
        {
            this.Elo += 10;
            this.Wins += 1;
        }

        public void updateLoseStats()
        {
            this.Elo -= 10;
            this.Loses += 1;
        }

        public void updateDrawStats()
        {
            this.Draws += 1;
        }
    }
}
