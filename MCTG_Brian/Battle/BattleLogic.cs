using MCTG_Brian.Database.Models;

namespace MCTG_Brian.Battle
{
    public static class BattleLogic
    {
       
        public static Card Calculate(Card card1, Card card2)
        {
            

            if (card1.Damage > card2.Damage)
            {
                return card1;
            }
            else if (card1.Damage < card2.Damage)
            {
                return card2;
            }
            return null;
        }
    }
}
