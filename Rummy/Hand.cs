using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rummy
{
    public class Hand
    {
        public List<Card> Cards;
        public int PlayerID;

        public Hand(Random r, Deck deck, bool firstPlayer, int playerId)
        {
            PlayerID = playerId;
            if (firstPlayer) { Cards = deck.Deal(r, 15).ToList(); }
            else             { Cards = deck.Deal(r).ToList();}
        }
        
        public enum SortType{Suit = 0, Value = 1, Both = 2}
        [PlayerInvokable(Name = "Sort", Description = "Sorts the hand based on input (0/Suit, 1/Value, 2/Both)")]
        public void Sort(SortType T = SortType.Value)
        {
            Card[] buffer = Cards.ToArray();
            if (T == SortType.Suit || T == SortType.Both)
            {
                Array.Sort(buffer, new CardSuitComparer());
            }
            if (T == SortType.Value || T == SortType.Both)
            {
                Array.Sort(buffer, new CardValueComparer());
            }

            Cards = buffer.ToList();
        }
        public class CardValueComparer : IComparer<Card>
        {
            public int Compare(Card x, Card y)
            {
                if (x == null || y == null) { return 0;}
                if (x.Value <  y.Value) {return -1;}
                if (x.Value == y.Value) {return  0;}
                return  1;
            }
        }

        public class CardSuitComparer : IComparer<Card>
        {
            public int Compare(Card x, Card y)
            {
                return (int)x.Suit - (int)y.Suit;
            }
        }

        [PlayerInvokable(Name = "Switch", Description = "Switches two cards in the hand (useful for manual sorting)")]
        public void Switch(int a, int b)
        {
            if (a > Cards.Count || b > Cards.Count)
            {
                Console.Write("Error: one index is out of range");
                return;
            }
            if (a == b)
            {
                Console.Write("Error: the ID-s are the same, switching is not required");
                return;
            }

            Card buffer = Cards[a].Copy();
            Cards[a] = Cards[b];
            Cards[b] = buffer;
        }
        
        [PlayerInvokable(Name = "Discard", Description = "Discards a card, and thus ends the turn")]
        [TurnEnder]
        public void Discard(List<Card> DiscardPile, int id)
        {
            DiscardPile.Add(Cards[id]);
            Cards.RemoveAt(id);
        }

        [PlayerInvokable(Name = "List", Description = "Lists the player's cards")]
        public void List(bool horizontal = true)
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                if(!horizontal) Console.WriteLine($"{i}:\t{Program.Suit[(int)Cards[i].Suit]}{Program.Value[Cards[i].Value]}");
                if (horizontal)
                {
                    Console.Write($"{Program.Suit[(int)Cards[i].Suit]}{Program.Value[Cards[i].Value]} ");
                }
            }
            if(horizontal){Console.Write("\n");}
        }
    }

    
}
