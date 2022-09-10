using System;
using System.Collections.Generic;

namespace Rummy
{
    /// Holds cards.
    public class Deck : IEnumerable<Card>
    {
        const int SUIT_COUNT = 4;
        private int values = 14;            //Number of different values  (ex.: A, 2, 3, 4, 5, 6, 7, 8, 9, 10, J, Q, K, Joker)

        //private int[,] Cards; // Kept commented here as a source of shame
        private readonly List<Card> cards = new List<Card>();
        public int CardsLeft => cards.Count;

        public Card this[int index]
        {
            get => cards[index];
        }

        public IEnumerator<Card> GetEnumerator() => cards.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator(); // [Dit05] This might look arcane, but it's only because it IS arcane.

        private static readonly Random shuffler = Program.r;

        public Deck()
        {
            Populate();
            Shuffle();
        }


        public void Clear() => cards.Clear();

        /// Adds a card onto the top of the stack, so that it will be the next card drawn.
        public void PushCard(Card card) => cards.Add(card);
        /// Inserts a card so that it occupies the specified index. Invalid index values are clamped.
        public void InsertCard(int index, Card card)
        {
            if(index < 0) index = 0;
            else if(index >= CardsLeft) index = CardsLeft;
            cards.Insert(index, card);
        }

        /// Adds several cards onto the top of the stack, so that the last card added will be the first one drawn.
        public void PushCards(IEnumerable<Card> collection)
        {
            foreach(Card card in collection) PushCard(card);
        }

        /// Adds a card onto the bottom of the stack, so that it will be the last card drawn.
        public void AddCard(Card card) => InsertCard(0, card);

        /// Adds several cards onto the bottom of the stack, so that the last card added will be the last one drawn.
        public void AddCards(IEnumerable<Card> collection)
        {
            foreach(Card card in collection) AddCard(card);
        }

        public Card PopCard() => CardsLeft > 0 ? RemoveCard(CardsLeft - 1) : null;
        public Card RemoveCard(int index)
        {
            if(CardsLeft == 0) return null;
            Card card = cards[index];
            cards.RemoveAt(index);
            return card;
            }

        public void Populate()
        {
            for (int s = 0; s < SUIT_COUNT; s++)
            {
                for (int v = 0; v < values; v++)
                {
                    Card card = new Card((Suit)s, v);
                    // Regular cards added twice; Joker only once.
                    PushCard(card);
                    if (v != (int)Value.Joker) { PushCard(card/*.Copy()*/); } // [Dit05] WHY was this being copied? It's literally a local variable being created JUST above this line!
                }
            }
        }

        [Obsolete("Use AddRange or PushRange instead.", error: !true)]
        public void Repopulate(List<Card> newCards)            //Function to essentially re-shuffle the throw deck, for an extended play (consumes the contents of the list)
        {
            while (newCards.Count > 0)
            {
                cards.Add(newCards[0].Copy());
                //Cards[(int)newCards[0].Suit, newCards[0].Value]++;
                newCards.RemoveAt(0); 
            }
        }

        /// Draws a card from the deck, removing it. If a Random is not provided, the topmost (last) card will be drawn. Essentially a fusion of RemoveCard and PopCard.
        public Card Draw(Random r = null)
        {
            if(CardsLeft == 0) return null;

            return (r == null) ? PopCard() : RemoveCard(r.Next(CardsLeft));
        }

        /// Draws at most <param name="amount"/> cards. If there aren't enough, the returned array will be shorter.
        public Card[] Deal(Random r, int amount = 14)
        {
            amount = Math.Min(amount, CardsLeft);
            Card[] output = new Card[amount];
            for (int i = 0; i < amount; i++)
            {
                output[i] = Draw(r);
            }

            return output;
        }

        /// Randomizes the order of cards within this deck.
        public void Shuffle(Random rand = null)
        {
            if(rand == null)
                rand = shuffler;

            void Swap(int i, int j) {
                Card temp = cards[i]/*.Copy()*/; // [Dit05] Please show me how you make copies of cards when shuffling. I knew they're pretty old, but I didn't know they were magical also.
                cards[i] = cards[j];
                cards[j] = temp;
            }

            for(int i = 0; i < cards.Count - 1; i++)
            {
                Swap(i, rand.Next(i + 1, cards.Count));
            }
        }
    }
}
