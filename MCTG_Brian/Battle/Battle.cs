using MCTG_Brian.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG_Brian.Battle
{
    public class Battle
    {
        public BattleLogger log = new BattleLogger();

        private Card catchedRndCard(List<Card> deck)
        {
            Random rnd = new Random();
            int randomIndex = rnd.Next(deck.Count);
            return deck[randomIndex];
        }

        public BattleLogger startBattle(Tuple<User, List<Card>> Deck1, Tuple<User, List<Card>> Deck2)
        {
            foreach (var x in Deck1.Item2)
            {
                x.Damage = 0;
            }

            log.addToProtocol($"Fight between {Deck1.Item1.Username} against {Deck2.Item1.Username}");

            for (int rounds = 0; rounds < 100; rounds++)
            {
                Card player1Card = catchedRndCard(Deck1.Item2);
                Card player2Card = catchedRndCard(Deck2.Item2);

                log.addToProtocol($"Card {player1Card.Name} with (ATK: {player1Card.Damage}) against Card{player2Card.Name} with (ATK: {player2Card.Damage})\n");

                Card winnerCard = BattleLogic.Calculate(player1Card, player2Card);


                if (winnerCard == null)
                {
                    log.addToProtocol($"Player {Deck2.Item1.Username} draws with Card {player2Card.Name} (ATK {player2Card.Damage})\n");
                }
                else if (winnerCard == player1Card)
                {
                    log.addToProtocol($"Player {Deck1.Item1.Username} wins with Card {player1Card.Name} (ATK {player1Card.Damage})\n");
                    Deck1.Item2.Add(player2Card);
                    Deck2.Item2.Remove(player2Card);
                }
                else if (winnerCard == player2Card)
                {
                    log.addToProtocol($"Player {Deck2.Item1.Username} wins with Card {player2Card.Name} (ATK {player2Card.Damage})\n");
                    Deck1.Item2.Remove(player1Card);
                    Deck2.Item2.Add(player1Card);
                }


                log.addToProtocol($"New Deck from {Deck1.Item1.Username}");

                foreach (var x in Deck1.Item2)
                {
                    log.addToProtocol($"{x.Id} {x.Name} {x.Damage}");
                }


                log.addToProtocol($"New Deck from {Deck2.Item1.Username}");

                foreach (var x in Deck2.Item2)
                {
                    log.addToProtocol($"{x.Id} {x.Name} {x.Damage}");
                }

                log.addToProtocol($"\n\n{Deck1.Item2.Count()} {Deck2.Item2.Count()}\n\n");

                if (Deck1.Item2.Count() < 1)
                {
                    log.addToProtocol($"{Deck2.Item1.Username} won against the other Player {Deck1.Item1.Username}");
                    log.classifyUsers(Deck2.Item1, Deck1.Item1);
                    break;
                }

                if (Deck2.Item2.Count() < 1)
                {
                    log.addToProtocol($"{Deck1.Item1.Username} won against the other Player {Deck2.Item1.Username}");
                    log.classifyUsers(Deck1.Item1, Deck2.Item1);

                    break;
                }

                if ((Deck1.Item2.Count() < 1 && Deck2.Item2.Count() < 1) || rounds >= 99)
                {
                    log.addToProtocol($"{Deck1.Item1.Username} drawed against the other Player {Deck2.Item1.Username}");
                    log.isDraw = true;
                }
            }


            log.addToProtocol($"Battle ended");
            log.saveNewDecks(Deck1, Deck2);

            return log;  
        }
    }
}

