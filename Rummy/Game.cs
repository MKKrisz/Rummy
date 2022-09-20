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
        }

        public Card GetLastDiscard() => DiscardPile.PopCard();

        public void Loop()
        {
            while (Run)
            {
                if (CurrentPlayer == Players.Length) {CurrentPlayer = 0;Round++;}
                if ((CurrentPlayer != 0 || Round != 0) && Deck.CardsLeft != 0) {Players[CurrentPlayer].Hand.Cards.Add(Deck.Draw(Program.r));}
                else if(Deck.CardsLeft == 0 && TrumpCard != null){Players[CurrentPlayer].Hand.Cards.Add(TrumpCard.Copy()); TrumpCard = null;}
                if (Deck.CardsLeft == 0) {
                    Deck.AddCards(DiscardPile);
                    DiscardPile.Clear();
                }
                Players[CurrentPlayer].SH.StartTurn();
                if (Players[CurrentPlayer].Hand.Cards.Count == 0)
                {
                    Run = false;
                }


                CurrentPlayer++;
            }
        }
    }
}
