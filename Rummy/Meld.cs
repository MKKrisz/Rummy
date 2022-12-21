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
        public int Value => Evaluate();
        public List<Card> Cards;
        public bool CanBeAddedTo = false;

        public abstract bool Validate(Card c);

        public void UpdateAddableStatus(int PlayerScore) {
            if (PlayerScore >= Constants.MinMeldScore) {
                CanBeAddedTo = true;
            }
        }

        public static void UpdateAllMeldStatus(Player P){
            for (int i = 0; i < Program.Game.Melds.Count; i++) {
                Meld Current = Program.Game.Melds[i];
                if (Current.PlayerID == P.ID) { Current.UpdateAddableStatus(P.Score);}
            }
        }
        public abstract int Evaluate();
        public static Meld Melder(List<Card> Cards, int playerID){
            Meld Output;
            if(Cards.Count<3){throw new Exception(Constants.Translator.Translate("Not enough cards to form a meld"));}
            
            if(Cards[0].Value == Cards[1].Value){Output = new SetMeld();}
            else{Output = new RunMeld();}

            if (Output is SetMeld sm) {
                if (Cards.Count > Constants.Suit.Length) {throw new Exception($"{Constants.Translator.Translate("Can't have more than")} {Constants.Suit.Length} {Constants.Translator.Translate("cards in a set meld")}");}
                Cards = Card.Sort(Cards.ToArray(), Hand.SortType.Suit).ToList();
                
                int firstNonJoker = 0;
                //gets the first non-joker card, sets its suit as the MeldSuit 
                while (Cards[firstNonJoker].IsJoker) { firstNonJoker++; }
                sm.MeldValue = Cards[firstNonJoker].Value;
                
                int JokerCount = 0;
                for (int i = 0; i < Cards.Count; i++) {
                    if(Cards[i].IsJoker){ JokerCount++; } 
                    if(JokerCount>= Cards.Count/2.0){throw new Exception(Constants.Translator.Translate("Too many Jokers!"));}

                    if (!Cards[i].IsJoker && Cards[i].Value != sm.MeldValue) { throw new Exception(Constants.Translator.Translate("Can't have two different values in a set meld"));}
                    for (int j = i+1; j < Cards.Count; j++) { 
                        if (!Cards[i].IsJoker && !Cards[j].IsJoker && Cards[i].Suit == Cards[j].Suit) { 
                            throw new Exception(Constants.Translator.Translate("Can't have two cards with the same suit in a set meld"));
                        }
                    }
                }
                sm.Cards = Cards;
            }
            if (Output is RunMeld rm) {
                Cards = Card.Sort(Cards.ToArray(), Hand.SortType.Value).ToList();
                if (Cards[0].Value == (int)Rummy.Value.Ace) {
                    if (Cards[1].Value != (int)Rummy.Value.N2) {
                        Cards.Add(Cards[0]);
                        Cards.RemoveAt(0);
                    }
                }

                int firstNonJoker = 0;
                //gets the first non-joker card, sets its suit as the MeldSuit
                while (Cards[firstNonJoker].IsJoker) { firstNonJoker++; }
                rm.MeldSuit = Cards[firstNonJoker].Suit;
                
                int JokerCount = 0;
                for (int i = 0; i < Cards.Count; i++) {
                    if(Cards[i].IsJoker){ JokerCount++; } 
                    if(JokerCount>= Cards.Count/2.0){throw new Exception(Constants.Translator.Translate("Too many Jokers!"));}
                
                    if (!Cards[i].IsJoker && Cards[i].Suit != rm.MeldSuit){throw new Exception(Constants.Translator.Translate("Can't have different suits in a run meld")); }

                    if (i > 0 && !Cards[i].IsJoker && !Cards[i - 1].IsJoker && Cards[i - 1].Value != (Cards[i].Value == (int)Rummy.Value.Ace ? (int)Rummy.Value.King : Cards[i].Value - 1)) {
                        if (Cards[0].IsJoker) {
                            Card Joker = Cards[0];
                            Cards.RemoveAt(0);
                            Cards.Insert(i-1, Joker);
                        }
                        else{throw new Exception(Constants.Translator.Translate("Meld is not continuous"));}
                    }
                }
                bool choice = true;
                while(Cards[0].IsJoker && choice) {
                    Console.Write($"{Colors.Important.AnsiFGCode}{Constants.Translator.Translate("[IMPORTANT]: Unused Joker found. Shall it be inserted at the end of the meld? [y/N]")}{Color.Reset} ");
                    char input = Console.ReadKey(false).KeyChar;
                    Console.Write("\n");
                    if(input == 'y'){Cards.Add(Cards[0]); Cards.RemoveAt(0);}
                    else{choice = false;}
                }
                rm.Cards = Cards;
            }
            Output.PlayerID = playerID;
            return Output;
        }
    }

    public class SetMeld : Meld{
    public int MeldValue;
        public override int Evaluate() => Cards[0].PointValue * Cards.Count;
        public override bool Validate(Card c) {
            if (c.IsJoker) {
                int JokerCount = 1;
                for (int i = 0; i < Cards.Count; i++) {
                    if(Cards[i].IsJoker){ JokerCount++; }
                } 
                if(JokerCount >= Cards.Count/2.0){return false;}
                return true;
            }
            if(c.Value != MeldValue){return false;}

            for (int i = 0; i < Cards.Count; i++) {
                if(!Cards[i].IsJoker && c.Suit == Cards[i].Suit){return false;}
            }
            return true;
        }
    }

    public class RunMeld : Meld {
        public Suit MeldSuit;
        
        public override int Evaluate() {
            int Score = 0;
            for (int i = 0; i < Cards.Count; i++) {
                if (Cards[i].IsJoker) {
                    if(i<Cards.Count-1){Score += new Card(MeldSuit, Cards[i+1].Value -1).PointValue;}
                    else if(i>0){Score += new Card(MeldSuit, Cards[i-1].Value + 1).PointValue;}
                }
                else if (Cards[i].Value == (int)Rummy.Value.Ace) {
                    if(i == 0){Score += 1;}
                    else Score += Cards[i].PointValue;
                }
                else{Score += Cards[i].PointValue;}
            }
            return Score;
        }
        public override bool Validate(Card c) {
            if (c.IsJoker) {
                int JokerCount = 1;
                for (int i = 0; i < Cards.Count; i++) {
                    if(Cards[i].Value == (int)Rummy.Value.Joker){ JokerCount++; }
                }
                if(JokerCount >= Cards.Count/2.0){return false;}
                return true;
            }
            if(c.Suit      != MeldSuit)       {return false;}
            if(c.Value + 1 == Cards[0].Value) {return true;}
            if(c.Value - 1 == Cards[^1].Value){return true;}

            if (c.Value == (int)Rummy.Value.Ace) {
                if (Cards[^1].Value == (int)Rummy.Value.King) {
                    return true;
                }
                if (Cards[0].Value == (int)Rummy.Value.N2) {
                    return true;
                }
            }

            if (Cards[0].IsJoker) {
                Card JokerSubstitute = new Card(MeldSuit, Cards[1].Value - 1);
                if(c.Value + 1 == JokerSubstitute.Value){return true;}
                if (c.Value == (int)Rummy.Value.Ace) {
                    if (JokerSubstitute.Value == (int)Rummy.Value.King) {
                        return true;
                    }
                    if (JokerSubstitute.Value == (int)Rummy.Value.N2) {
                        return true;
                    }
                }
            }
            
            if (Cards[^1].Value == (int)Rummy.Value.Joker) {
                Card JokerSubstitute = new Card(MeldSuit, Cards[^2].Value + 1);
                if(c.Value - 1 == JokerSubstitute.Value){return true;}
                if (c.Value == (int)Rummy.Value.Ace) {
                    if (JokerSubstitute.Value == (int)Rummy.Value.King) {
                        return true;
                    }
                    if (JokerSubstitute.Value == (int)Rummy.Value.N2) {
                        return true;
                    }
                }
            }
            for(int i = 0; i<Cards.Count; i++){
                if (Cards[i].Value == (int)Rummy.Value.Joker) {
                    if (i > 0             && Cards[i - 1].Value == c.Value - 1) { return true; }

                    if (i < Cards.Count-1 && Cards[i + 1].Value == c.Value + 1) { return true; }
                }
            }

            return false;
        }
    }
}