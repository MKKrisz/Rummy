using System;
using System.Linq;
using System.Collections.Generic;

namespace Rummy
{
    public class Player
    {
        public Hand Hand;
        public List<Card> Cards => Hand.Cards;
        public List<Meld> Melds = new List<Meld>();
        public Shell SH;
        
        public int ID;
        public int Score => GetScore();
        public bool First;

        public Player(){}
        public Player(Random r, Deck deck, int id)
        {
            ID = id;
            Hand = new Hand(r, deck, id == 0, id);
            SH = new Shell(this);
        }

        public void StartTurn() {
            SH.StartTurn();
        }

        public int GetScore() => Melds.Sum(x => x.Value);
        /*public int GetScore(List<Meld> Melds)
        {
            int output = 0;
            for (int i = 0; i < Melds.Count; i++)
            {
                if (Melds[i].PlayerID == ID) { output += Melds[i].Evaluate(); }
            }
            return output;
        }*/
    }
}