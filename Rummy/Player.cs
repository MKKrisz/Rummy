using System;
using System.Linq;

namespace Rummy
{
    public class Player
    {
        public Hand Hand;
        public Shell SH;
        public int ID;

        public Player(Random r, Deck deck, int id)
        {
            ID = id;
            Hand = new Hand(r, deck, id == 0, id);
            SH = new Shell(this);
        }
    }
}