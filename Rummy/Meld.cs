using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Rummy.TextColor;

namespace Rummy
{
    public abstract class Meld
    {
        public int PlayerID;
        public int Value;
        public List<Card> Cards;

        public abstract bool Validate(Card c);
        public static Meld Melder(List<Card> Cards, int playerID)   //The cards should be in increasing order
        {
            Meld Output;
            if(Cards.Count<3){throw new Exception("Not enough cards to form a meld");}
            
            if(Cards[0].Value == Cards[1].Value){Output = new SetMeld();}
            else{Output = new RunMeld();}

            if (Output is SetMeld sm)
            {
                if (Cards.Count > Program.Suit.Length) {throw new Exception($"Can't have more than {Program.Suit.Length} cards in a set meld");}
                Cards = Card.Sort(Cards.ToArray(), Hand.SortType.Suit).ToList();
                
                int n = 0;
                //gets the first non-joker card, sets its suit as the MeldSuit 
                while (Cards[n].Value == (int)Rummy.Value.Joker) { n++; }
                sm.MeldValue = Cards[n].Value;

                for (int i = 0; i < Cards.Count; i++)
                {
                    if (Cards[i].Value != (int)Rummy.Value.Joker && Cards[i].Value != sm.MeldValue) {
                        throw new Exception("Can't have two different values in a set meld");}
                    for (int j = 0; j < Cards.Count; j++) { 
                        if (i != j && 
                            Cards[i].Value != (int)Rummy.Value.Joker &&
                            Cards[j].Value != (int)Rummy.Value.Joker && 
                            Cards[i].Suit == Cards[j].Suit) { 
                            throw new Exception("Can't have two cards with the same suit in a set meld");
                        }
                    }
                }

                sm.Cards = Cards;
            }
            if (Output is RunMeld rm)
            {
                Cards = Card.Sort(Cards.ToArray(), Hand.SortType.Value).ToList();

                int n = 0;
                //gets the first non-joker card, sets its suit as the MeldSuit
                while (Cards[n].Value == (int)Rummy.Value.Joker) { n++; }
                rm.MeldSuit = Cards[n].Suit;
                for(int i = 0; i<Cards.Count; i++)
                {
                    if (Cards[i].Value != (int)Rummy.Value.Joker && Cards[i].Suit != rm.MeldSuit){throw new Exception("Can't have different suits in a run meld"); }

                    if (Cards[i].Value != (int)Rummy.Value.Joker && i > 0 && Cards[i - 1].Value != Cards[i].Value - 1)
                    {
                        if (Cards[0].Value == (int)Rummy.Value.Joker)
                        {
                            Card Joker = Cards[0];
                            Cards.RemoveAt(0);
                            Cards.Insert(i-1, Joker);
                        }
                        else{throw new Exception("Meld is not continuos");}
                    }

                    rm.Cards = Cards;
                }
            }

            Output.PlayerID = playerID;
            return Output;
        }
    }

    public class SetMeld : Meld
    {
        public int MeldValue;
        public override bool Validate(Card c) 
        {
            if(c.Value == (int)Rummy.Value.Joker){return true;}
            if(c.Value != MeldValue){return false;}
            for (int i = 0; i < Cards.Count; i++) { if(Cards[i].Value != (int)Rummy.Value.Joker && c.Suit == Cards[i].Suit){return false;} }

            return true;
        }
    }

    public class RunMeld : Meld
    {
        public Suit MeldSuit;

        public override bool Validate(Card c)
        {
            if(c.Suit      != MeldSuit)       {return false;}
            if(c.Value + 1 == Cards[0].Value) {return true;}
            if(c.Value - 1 == Cards[Cards.Count-1].Value){return true;}
            for(int i = 1; i<Cards.Count-1; i++){
                if (Cards[i].Value == (int)Rummy.Value.Joker)
                {
                    if (i > 0             && Cards[i - 1].Value == c.Value - 1) { return true; }

                    if (i < Cards.Count-1 && Cards[i + 1].Value == c.Value + 1) { return true; }
                }
            }

            return false;
        }
    }
}