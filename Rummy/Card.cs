namespace Rummy
{
    public enum Suit{Spades = 0, Hearts = 1, Diamonds = 3, Clubs = 2}
    public enum Value{Joker, Ace, N2, N3, N4, N5, N6, N7, N8, N9, N10, Jack, Queen, King}
    
    public class Card
    {
        public Suit Suit;   
        public int Value;

        public int PointValue => GetValue();

        public Card Copy()
        {
            Card cp = (Card)this.MemberwiseClone();
            return cp;
        }
        private int GetValue()
        {
            if (Value >  10) { return 10; }
            if (Value == 1 ) { return 10; }

            return Value;
        }
    }
}