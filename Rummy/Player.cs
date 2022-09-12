using System;
using System.Linq;
using System.Collections.Generic;

namespace Rummy
{
    public class Player
    {
        public Hand Hand;
        public Shell SH;
        public int ID;
        public int Score => GetScore(Program.Game.Melds);

        public Player(){}
        public Player(Random r, Deck deck, int id)
        {
            ID = id;
            Hand = new Hand(r, deck, id == 0, id);
            SH = new Shell(this);
        }

        public int GetScore(List<Meld> Melds)
        {
            int output = 0;
            for (int i = 0; i < Melds.Count; i++)
            {
                if (Melds[i].PlayerID == ID)
                {
                    for (int n = 0; n < Melds[i].Cards.Count; n++)
                    {
                        if (Melds[i].Cards[n].Value != (int)Value.Joker) { output+=Melds[i].Cards[n].Value; }
                        else
                        {
                            int value = 0;
                            if (n > 0) value = Melds[i].Cards[n - 1].Value + 1;
                            else value = Melds[i].Cards[n + 1].Value - 1;
                            output += value;
                        }
                    }
                }
            }
            return output;
        }
    }
}