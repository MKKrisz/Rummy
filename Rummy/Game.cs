using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rummy
{
    public class Game 
    {
        public Player[] Players;
        public Deck Deck;
        public Card TrumpCard;
        public Deck DiscardPile = new Deck(false);
        public List<Meld> Melds = new List<Meld>();

        public int Round;
        public int CurrentPlayerId = 0;
        public bool Run = true;
        
        public Game(){}

        public Game(int PlayerAmount, int seed = -1) {
            if (seed == -1) {
                Constants.Random = new Random();
            }
            else {
                Constants.Random = new Random(seed);
            }
            Players = new Player[PlayerAmount];
            Deck = new Deck(true);
            TrumpCard = Deck.Draw(Constants.Random);
            TrumpCard.MustBeUsed = true;
            
            for (int i = 0; i < PlayerAmount; i++)
            {
                Players[i] = new Player(Constants.Random, Deck, i);
            }
            Players[0].First = true;
        }

        public Card GetLastDiscard() => DiscardPile.PopCard();

        public void Loop()
        {
            while (Run)
            {
                if (CurrentPlayerId == Players.Length) {CurrentPlayerId = 0; Round++;}

                Player CP = Players[CurrentPlayerId];
                CP.StartTurn();
                if (Deck.CardsLeft == 1) {
                    Deck.AddCards(DiscardPile);
                    DiscardPile.Clear();
                }
                if (CP.Cards.Count == 0)
                {
                    Run = false;
                    Console.WriteLine($"Congratulations! Player {CurrentPlayerId} Wins!!");
                    Console.ReadKey(true);
                }


                CurrentPlayerId++;
            }
        }
    }
}
