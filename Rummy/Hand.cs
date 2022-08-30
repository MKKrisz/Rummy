using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rummy
{
    public class Hand
    {
        public List<Card> Cards = new List<Card>();
        public int PlayerID;

        public Hand(Random r, Deck deck, bool firstPlayer, int playerId)
        {
            PlayerID = PlayerID;
            if (firstPlayer) { Cards = deck.Deal(r, 15).ToList(); }
            else             { Cards = deck.Deal(r).ToList();}
        }
        
        public enum SortType{Suit = 0, Value = 1, Both = 2}    
        public void Sort(SortType T)
        {
            if (T == SortType.Suit || T == SortType.Both)
            {
                List<Card> buffer = new List<Card>();
                int[] lastOfSuits = new int[4];
                for (int i = 0; i < Cards.Count; i++)
                {
                    Card card = Cards[i];
                    int suit = (int)card.Suit;
                    buffer[lastOfSuits[suit]] = card;
                    for (int j = suit; j < 4; j++)
                    {
                        lastOfSuits[j]++;
                    }
                }
                Cards = buffer;
            }

            if (T == SortType.Value || T == SortType.Both)
            {
                Card[] buffer = Cards.ToArray();
                Array.Sort(buffer, new CardValueComparer());
            }
        }
        class CardValueComparer : IComparer<Card>
        {
            public int Compare(Card x, Card y)
            {
                if (x == null || y == null) { return 0;}
                if (x.Value <  y.Value) {return -1;}
                if (x.Value == y.Value) {return  0;}
                return  1;
            }
        }
        
        
    }

    
}