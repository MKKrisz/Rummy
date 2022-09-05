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
        public static void Sort(Hand H, SortType T = SortType.Value)
        {
            Card[] buffer = H.Cards.ToArray();
            if (T == SortType.Suit || T == SortType.Both)
            {
                Array.Sort(buffer, new CardSuitComparer());
            }
            if (T == SortType.Value || T == SortType.Both)
            {
                Array.Sort(buffer, new CardValueComparer());
            }

            H.Cards = buffer.ToList();
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

        class CardSuitComparer : IComparer<Card>
        {
            public int Compare(Card x, Card y)
            {
                return (int)x.Suit - (int)y.Suit;
            }
        }

        [PlayerInvokable(Name = "Switch", Description = "Switches two cards in the hand (useful for manual sorting)")]
        public static void Switch(Hand H, int a, int b)
        {
            if (a > H.Cards.Count || b > H.Cards.Count)
            {
                Console.Write("Error: one index is out of range");
                return;
            }
            if (a == b)
            {
                Console.Write("Error: the ID-s are the same, switching is not required");
                return;
            }

            Card buffer = H.Cards[a].Copy();
            H.Cards[a] = H.Cards[b];
            H.Cards[b] = buffer;
        }
        
        [PlayerInvokable(Name = "Discard", Description = "Discards a card, and thus ends the turn")]
        [TurnEnder]
        public static void Discard(Hand H, List<Card> DiscardPile, int id)
        {
            DiscardPile.Add(H.Cards[id]);
            H.Cards.RemoveAt(id);
        }

        [PlayerInvokable(Name = "List", Description = "Lists the player's cards")]
        public static void List(Hand H)
        {
            for (int i = 0; i < H.Cards.Count; i++)
            {
                Console.WriteLine($"{i}:\t{Program.Suit[(int)H.Cards[i].Suit]}{Program.Value[H.Cards[i].Value]}");
            }
        }
    }

    
}
