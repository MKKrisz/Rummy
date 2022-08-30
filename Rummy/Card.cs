namespace Rummy
{
    public enum Suit{Spades = 0, Hearts = 1, Diamonds = 3, Clubs = 2}
    public class Card
    {
        public Suit Suit;
        public int Value;

        public int PointValue => GetValue();

        private int GetValue()
        {
            if (Value >  10) { return 10; }
            if (Value == 1 ) { return 10; }

            return Value;
        }
    }
}