using System;

namespace Rummy
{
    public enum Suit{Spades = 0, Hearts = 1, Diamonds = 3, Clubs = 2}
    public enum Value{Joker, Ace, N2, N3, N4, N5, N6, N7, N8, N9, N10, Jack, Queen, King}
    
    public class Card
    {
        public int UID;
        public Suit Suit;   
        public int Value;
        public bool MustBeUsed = false;
        public string name => $"{Program.Suit[(int)Suit]}{Program.Value[Value]}";

        public int PointValue => GetValue();
        
        public Card(Suit suit, int val)
        {
            if(val < 0 || val > (int)Rummy.Value.King)
                throw new ArgumentOutOfRangeException(nameof(val));
            Suit = suit;
            Value = val;
            UID = Program.r.Next();
        }

        public Card Copy()
        {
            Card cp = (Card)this.MemberwiseClone();
            return cp;
        }
        private int GetValue()
        {
            if (Value >  10 || Value == 1) { return 10; }

            return Value;
        }

        public static Card[] Sort(Card[] input, Hand.SortType T)
        {
            if (T == Hand.SortType.Suit || T == Hand.SortType.Both)
            {
                Array.Sort(input, new Hand.CardSuitComparer());
            }
            if (T == Hand.SortType.Value || T == Hand.SortType.Both)
            {
                Array.Sort(input, new Hand.CardValueComparer());
            }
            return input;
        }
    }
}
