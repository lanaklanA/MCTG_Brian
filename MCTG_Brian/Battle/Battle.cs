using MCTG_Brian.Database.Models;

namespace MCTG_Brian.Battle
{
    public class Battle
    {
        public BattleLogger log = new BattleLogger();

        public Card catchedRndCard(List<Card> deck)
        {
            Random rnd = new Random();
            int randomIndex = rnd.Next(deck.Count);
            return deck[randomIndex];
        }

        public BattleLogger startBattle(User player1, User player2)
        {
            log.addToProtocol($"Fight between {player1.Username} against {player2.Username}");

            for (int rounds = 0; rounds < 100; rounds++)
            {
                Card player1Card = catchedRndCard(player1.Deck);
                Card player2Card = catchedRndCard(player2.Deck);

                log.addToProtocol($"\n\n######Round {rounds+1}/100");

                log.addToProtocol($"Player ({player1.Username}) -> Card {player1Card.Name} (ATK: {player1Card.Damage})");
                log.addToProtocol($"Player ({player2.Username}) -> Card {player2Card.Name} (ATK: {player2Card.Damage})\n");
                
                Card loserCard = BattleLogic.Calculate(player1Card, player2Card, true);

                if (loserCard == null)
                {
                    log.addToProtocol($"[{player1.Username}] {player1Card.Type}|{player1Card.Monster}|{player1Card.Element} draws against [{player2.Username}] {player2Card.Type}|{player2Card.Monster}|{player2Card.Element}\n");
                }
                else if (loserCard == player1Card)
                {
                    log.addToProtocol($"[{player1.Username}] {player1Card.Type}|{player1Card.Monster}|{player1Card.Element} wins against [{player2.Username}] {player2Card.Type}|{player2Card.Monster}|{player2Card.Element}\n");
                    player1.Deck.Remove(loserCard);
                    player2.Deck.Add(loserCard);
                }
                else if (loserCard == player2Card)
                {
                    log.addToProtocol($"[{player2.Username}] {player2Card.Type}|{player2Card.Monster}|{player2Card.Element} wins against [{player1.Username}] {player1Card.Type}|{player1Card.Monster}|{player1Card.Element}\n");
                    player2.Deck.Remove(loserCard);
                    player1.Deck.Add(loserCard);
                }

                if (((player1.Deck.Count() + player2.Deck.Count()) < 1) || rounds >= 99)
                {
                    log.addToProtocol($"\n\nPlayer {player1.Username} drawed against the Player {player2.Username}");
                    log.isDraw = true;
                    break;
                }
                else if (player1.Deck.Count() < 1)
                {
                    log.addToProtocol($"{player2.Username} won against the Player {player1.Username}");
                    log.classifyUsers(player2, player1);
                    break;
                }
                else if (player2.Deck.Count() < 1)
                {
                    log.addToProtocol($"{player1.Username} won against the Player {player2.Username}");
                    log.classifyUsers(player1, player2);
                    break;
                }
            }

            log.addToProtocol($"\nBattle ended");
            
            return log;  
        }
    }
}

