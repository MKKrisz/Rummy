using System.Collections.Generic;
using System.Linq;

namespace Rummy
{
    public class Game
    {
        public Player[] Players;
        public Deck Deck;
        public Card TrumpCard;
        public List<Card> DiscardPile = new List<Card>();
        public List<Meld> Melds = new List<Meld>();

        public int Round;

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
        public Card GetLastDiscard(){return DiscardPile[DiscardPile.Count()-1];}

        public void Loop()
        {
            Players[0].SH.StartTurn();
        }
    }
}