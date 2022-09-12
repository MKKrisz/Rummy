using System.Collections.Generic;
using System.Linq;

namespace Rummy
{
    public class Game
    {
        public Player[] Players;
        public Deck Deck;
        public Card TrumpCard;
        public Deck DiscardPile = new Deck();
        public List<Meld> Melds = new List<Meld>();

        public int Round;
        public bool Run = true;

        public Game(int PlayerAmount)
        {
            Players = new Player[PlayerAmount];
            Deck = new Deck();
            TrumpCard = Deck.Draw(Program.r);
            
            for (int i = 0; i < PlayerAmount; i++)
            {
                Players[i] = new Player(Program.r, Deck, i);
            }
        }

        public Card GetLastDiscard() => DiscardPile.PopCard();

        public void Loop()
        {
            int i = 0;
            while (Run)
            {
                if (i == Players.Length) {i = 0;Round++;}
                if ((i != 0 || Round != 0) && Deck.CardsLeft != 0) {Players[i].Hand.Cards.Add(Deck.Draw(Program.r));}
                else if(Deck.CardsLeft == 0 && TrumpCard != null){Players[i].Hand.Cards.Add(TrumpCard.Copy()); TrumpCard = null;}
                if (Deck.CardsLeft == 0) {
                    Deck.AddCards(DiscardPile);
                    DiscardPile.Clear();
                }
                Players[i].SH.StartTurn();
                if (Players[i].Hand.Cards.Count == 0)
                {
                    Run = false;
                }


                i++;
            }
        }
    }
}
