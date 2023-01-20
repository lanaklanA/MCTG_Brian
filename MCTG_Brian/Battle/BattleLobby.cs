using MCTG_Brian.Database;
using MCTG_Brian.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MCTG_Brian.Battle
{
    public class BattleLobby
    {
        public DeckRepository deckRepo = new();

        public Battle battle = new Battle();
        public BattleLogger log = new BattleLogger();
        public Queue<User> users = new Queue<User>();
        
        private object syncPrimitive = new object();

        public User p1, p2;
        public List<Card> incomingDeck1 = new List<Card>();
        public List<Card> incomingDeck2 = new List<Card>();

        public void startLobby(User player)
        {
            lock (syncPrimitive)
            {
                users.Enqueue(player);

                if(users.Count > 1)
                {
                    p1 = users.Dequeue();
                    p2 = users.Dequeue();

                    incomingDeck1 = deckRepo.GetAll(p1.Id);
                    incomingDeck2 = deckRepo.GetAll(p2.Id);

                    Tuple<User, List<Card>> Deck1 = new Tuple<User, List<Card>>(p1, incomingDeck1);
                    Tuple<User, List<Card>> Deck2 = new Tuple<User, List<Card>>(p2, incomingDeck2);

                    log = battle.startBattle(Deck1, Deck2);
             
                    Monitor.Pulse(syncPrimitive);
                }
                else
                {
                    Monitor.Wait(syncPrimitive);
                }
            }

        }
    }
}
