using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rummy
{
    [Serializable]
    public class Game 
    {
        public Player[] Players;
        public Deck Deck;
        public Card TrumpCard;
        public Deck DiscardPile = new Deck(false);
        public List<Meld> Melds = new List<Meld>();

        public int Round;
        public int CurrentPlayer = 0;
        public bool Run = true;
        
        public Game(){}

        public Game(int PlayerAmount)
        {
            Players = new Player[PlayerAmount];
            Deck = new Deck(true);
            TrumpCard = Deck.Draw(Program.r);
            TrumpCard.MustBeUsed = true;
            
            for (int i = 0; i < PlayerAmount; i++)
            {
                Players[i] = new Player(Program.r, Deck, i);
            }
            Players[0].First = true;
        }

        public Card GetLastDiscard() => DiscardPile.PopCard();

        public void Loop()
        {
            while (Run)
            {
                if (CurrentPlayer == Players.Length) {CurrentPlayer = 0; Round++;}
                Players[CurrentPlayer].SH.StartTurn();
                if (Deck.CardsLeft == 1) {
                    Deck.AddCards(DiscardPile);
                    DiscardPile.Clear();
                }
                if (Players[CurrentPlayer].Hand.Cards.Count == 0)
                {
                    Run = false;
                    Console.WriteLine($"Congratulations! Player {CurrentPlayer} Wins!!");
                    Console.ReadKey(true);
                }


                CurrentPlayer++;
            }
        }
    }
}
