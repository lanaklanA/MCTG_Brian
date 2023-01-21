using MCTG_Brian.Database.Models;
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

                log.addToProtocol($"\n\nRound {rounds}");

                log.addToProtocol($"Card {player1Card.Name} with (ATK: {player1Card.Damage}) against Card {player2Card.Name} with (ATK: {player2Card.Damage})");

                Card loserCard = BattleLogic.Calculate(player1Card, player2Card);

                if (loserCard == null)
                {
                    log.addToProtocol($"Player {player1.Username} draws with Card {player1Card.Name} (ATK {player1Card.Damage}) against Player {player2.Username} Card {player2Card.Name} with (ATK: {player2Card.Damage})\n");
                }
                else if (loserCard == player1Card)
                {
                    log.addToProtocol($"1Player {player2.Username} wins with Card {player2Card.Name} (ATK {player2Card.Damage}) against Player {player1.Username} wins with Card {player1Card.Name} (ATK {player1Card.Damage})\n");
                    player1.Deck.Remove(loserCard);
                    player2.Deck.Add(loserCard);
                }
                else if (loserCard == player2Card)
                {
                    log.addToProtocol($"2Player {player1.Username} wins with Card {player1Card.Name} (ATK {player1Card.Damage}) against Player {player2.Username} wins with Card {player2Card.Name} (ATK {player2Card.Damage})\n"); 
                    player2.Deck.Remove(loserCard);
                    player1.Deck.Add(loserCard);
                }

                log.addToProtocol($"Anzahl deck player1: {player1.Deck.Count()} Anzahl deck player2: {player2.Deck.Count()}");
                log.addToProtocol($"Deck from Player1 {player1}:");
                foreach(Card card in player1.Deck)
                {
                    log.addToProtocol($"{card.Id} {card.Name} {card.Damage} // {card.Type} {card.Monster} {card.Element}");
                }

                log.addToProtocol($"\nDeck from Player2 {player2}:");
                foreach (Card card in player2.Deck)
                {
                    log.addToProtocol($"{card.Id} {card.Name} {card.Damage} // {card.Type} {card.Monster} {card.Element}");
                }
                log.addToProtocol($"\n\n");


                if (((player1.Deck.Count() + player2.Deck.Count()) < 1) || rounds >= 99)
                {
                    log.addToProtocol($"{player1.Username} drawed against the other Player {player2.Username}");
                    log.isDraw = true;
                    break;
                }
                else if (player1.Deck.Count() < 1)
                {
                    log.addToProtocol($"{player2.Username} won against the other Player {player1.Username}");
                    log.classifyUsers(player2, player1);
                    break;
                }
                else if (player2.Deck.Count() < 1)
                {
                    log.addToProtocol($"{player1.Username} won against the other Player {player2.Username}");
                    log.classifyUsers(player1, player2);
                    break;
                }
            }


            log.addToProtocol($"Battle ended");
            
            return log;  
        }
    }
}

