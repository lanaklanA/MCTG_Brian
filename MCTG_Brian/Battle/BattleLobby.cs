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
        public Battle battle = new Battle();
        public BattleLogger log = new BattleLogger();
        public Queue<User> users = new Queue<User>();
        
        private object syncPrimitive = new object();

        public User p1, p2;

        public bool joinLobby(User player)
        {


            lock (syncPrimitive)
            {
                users.Enqueue(player);

                if(users.Count > 1)
                {
                    p1 = users.Dequeue();
                    p2 = users.Dequeue();

                    if (p1.Deck.Count() + p2.Deck.Count() != 8) 
                    {
                        log.addToProtocol($"Die Decks sind nicht breit! P1 ({p1.Deck.Count()}) P2 ({p2.Deck.Count()})");
                        return false;
                    }

                    log = battle.startBattle(p1, p2);

                    Monitor.Pulse(syncPrimitive);
                }
                else
                {
                    Monitor.Wait(syncPrimitive);
                }
            }
            return true;

        }
    }
}
