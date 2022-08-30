using System;
using System.Collections.Generic;

namespace Rummy
{
    public class Deck                       //an object that generates random cards the same way you pull cards from a deck
    {
        private int suits = 4;             //Number of suits            (ex.: Spades, Hearts, Diamonds, Spears)
        private int values = 14;            //Number of different values  (ex.: A, 2, 3, 4, 5, 6, 7, 8, 9, 10, J, Q, K, Joker)

        public int[,] Cards;
        public int CardsLeft => Enumerate();

        public Deck()
        {
            Cards = new int[suits, values];
            Populate();
        }

        public void Populate()
        {
            for (int y = 0; y < suits; y++)
            {
                for (int x = 0; x < values; x++)
                {
                    Cards[y, x] = 2;
                    if (x == 0) { Cards[y, x] -= 1; }
                }
            }
        }
        public void Repopulate(List<Card> newCards)            //Function to essentially re-shuffle the throw deck, for an extended play (consumes the contents of the list)
        {
            while (newCards.Count > 0)
            {
                Cards[(int)newCards[0].Suit, newCards[0].Value]++;
                newCards.RemoveAt(0); 
            }
        }
        private int Enumerate()
        {
            int output = 0;
            for (int y = 0; y < suits; y++)
            {
                for (int x = 0; x < values; x++)
                {
                    output += Cards[y, x];
                }
            }
            return output;
        }
        public Card Draw(Random r)
        {
            if (CardsLeft <= 0) { return null;}

            Card output;
            
            int suit = r.Next(suits);
            int value = r.Next(values);

            while (Cards[suit, value] <= 0)
            {
                suit = r.Next(suits);
                value = r.Next(values);
            }

            Cards[suit, value]--;
            output = new Card { Suit = (Suit)suit, Value = value };

            return output;
        }

        public Card[] Deal(Random r, int amount = 14)
        {
            Card[] output = new Card[amount];
            for (int i = 0; i < amount; i++)
            {
                output[i] = Draw(r);
            }

            return output;
        }
    }
}