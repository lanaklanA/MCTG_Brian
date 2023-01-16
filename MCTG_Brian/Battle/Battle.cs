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
        public BattleLog log = new BattleLog();
        private Card catchedRndCard(List<Card> deck)
        {
            Random rnd = new Random();
            int randomIndex = rnd.Next(deck.Count);
            return deck[randomIndex];
        }

        public BattleLog startBattle(Tuple<User, List<Card>> Deck1, Tuple<User, List<Card>> Deck2)
        {
            foreach(var x in Deck1.Item2)
            {
                x.Damage = 0;
            }
   
            log.addToProtocol($"Fight between {Deck1.Item1.Name} against {Deck2.Item1.Name}");
            
            for(int i = 0; i < 100; i++)
            { 
                Card player1Card = catchedRndCard(Deck1.Item2);
                Card player2Card = catchedRndCard(Deck2.Item2);

                log.addToProtocol($"Card {player1Card.Name} with (ATK: {player1Card.Damage}) against Card{player2Card.Name} with (ATK: {player2Card.Damage})\n");
       
                if (player1Card.Damage > player2Card.Damage)
                {
                    log.addToProtocol($"Player {Deck1.Item1.Name} wins with Card {player1Card.Name} (ATK {player1Card.Damage})\n");
                    Deck1.Item2.Add(player2Card);
                    Deck2.Item2.Remove(player2Card);
                }
                else if (player1Card.Damage < player2Card.Damage)
                {
                    log.addToProtocol($"Player {Deck2.Item1.Name} wins with Card {player2Card.Name} (ATK {player2Card.Damage})\n");
                    Deck1.Item2.Remove(player1Card);
                    Deck2.Item2.Add(player1Card);
                }
                else if(player1Card.Damage == player2Card.Damage)
                {
                    log.addToProtocol($"Player {Deck2.Item1.Name} draws with Card {player2Card.Name} (ATK {player2Card.Damage})\n");

                }

                log.addToProtocol($"New Deck from {Deck1.Item1.Name}");

                foreach (var x in Deck1.Item2)
                {
                    log.addToProtocol($"{x.Id} {x.Name} {x.Damage}");
                }


                log.addToProtocol($"New Deck from {Deck2.Item1.Name}");

                foreach (var x in Deck2.Item2)
                {
                    log.addToProtocol($"{x.Id} {x.Name} {x.Damage}");
                }

                log.addToProtocol($"\n\n{ Deck1.Item2.Count() } { Deck2.Item2.Count()}\n\n");

                if (Deck1.Item2.Count() < 1)
                {
                    log.addToProtocol($"{Deck2.Item1.Name} won against the other Player {Deck1.Item1.Name}");
                    log.classifyUsers(Deck2.Item1, Deck1.Item1);
                    break;
                }

                if (Deck2.Item2.Count() < 1)
                {
                    log.addToProtocol($"{Deck1.Item1.Name} won against the other Player {Deck2.Item1.Name}");
                    log.classifyUsers(Deck1.Item1, Deck2.Item1);
                    
                    break;
                }
            }

            if (Deck1.Item2.Count() < 1 && Deck2.Item2.Count() < 1)
            {
                log.addToProtocol($"{Deck1.Item1.Name} drawed against the other Player {Deck2.Item1.Name}");
                log.isDraw = true;
            }

            log.addToProtocol($"Battle ended");
            return log;  
        }
    }
}

