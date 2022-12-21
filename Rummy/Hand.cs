using System;
using System.Collections.Generic;
using System.Linq;
using Rummy.TextColor;

namespace Rummy
{
    public class Hand
    {
        public readonly List<Card> Cards = new List<Card>();
        public List<int> Selection = new List<int>();
        public int PlayerID;
        public bool HasDrawn = false;
        public bool WasFirst = false;
        public bool CanEndTurn => !Cards.Any(x => x.MustBeUsed);

        public Hand(Random r, Deck deck, bool firstPlayer, int playerId) {
            PlayerID = playerId;
            
            if (firstPlayer) { Cards.AddRange(deck.Deal(r, 15)); }
            else             { Cards.AddRange(deck.Deal(r));}
        }

        public Hand(IEnumerable<Card> Cards, int PlayerID) {
            this.Cards.AddRange(Cards);
            this.PlayerID = PlayerID;
        }

        public void ResetUsingStatus() {
            for(int i = 0; i<Cards.Count; i++){Cards[i].MustBeUsed = false;}
        }
        
        public enum SortType{Suit = 0, Value = 1, Both = 2}
        [PlayerInvokable(Name = "Sort", Description = "Sorts the hand based on input (0/Suit, 1/Value, 2/Both)")]
        public void Sort(SortType T = SortType.Both) {
            if (T == SortType.Suit || T == SortType.Both) {
                Cards.Sort(new CardSuitComparer());
            }
            if (T == SortType.Value || T == SortType.Both){
                Cards.Sort(new CardValueComparer());
            }
        }
        public class CardValueComparer : IComparer<Card> {
            public int Compare(Card x, Card y) {
                if (x == null || y == null)     {return  0;}
                return  x.Value-y.Value;
            }
        }
        public class CardSuitComparer : IComparer<Card> {
            public int Compare(Card x, Card y) {
                return (int)x.Suit - (int)y.Suit;
            }
        }
        
        [PlayerInvokable(Name = "Swap", Description = "Switches two cards in the hand (useful for manual sorting)")]
        public void Swap(int a, int b) {
            if (a > Cards.Count || b > Cards.Count) {
                Console.WriteLine($"{Colors.Error.AnsiFGCode}{Constants.Translator.Translate("[ERROR]: one of the indexes is out of range")}{Color.Reset}");
                return;
            }
            if (a == b) {
                Console.WriteLine($"{Colors.Warning.AnsiFGCode}{Constants.Translator.Translate("[WARNING]: the ID-s are the same, switching is not required")}{Color.Reset}");
                return;
            }

            //TODO: Check if works
            Card buffer = Cards[a].Copy();
            Cards[a] = Cards[b];
            Cards[b] = buffer;
        }
        
        [TurnEnder]
        [PlayerInvokable(Name = "Discard", Description = "Discards a card, and thus ends the turn. Given \"n\" negative index, discards the last |n|-th card")]
        public bool Discard([AutoCompleteParameter]Deck DiscardPile, int id = -1) {
            if(id < 0){id = Cards.Count + id;}

            if (!HasDrawn && !WasFirst) {
                Console.WriteLine($"{Colors.Warning.AnsiFGCode}{Constants.Translator.Translate("[WARNING]: Player has not drawn")}{Colors.Reset}");
                return false;
            } 
            if(DiscardPile == null) throw new Exception(Constants.Translator.Translate("Discard pile is null"));
            while (id >= Cards.Count) {
                Console.Write($"{Colors.Warning.AnsiFGCode}{Constants.Translator.Translate("[WARNING]: Invalid index. Give a valid number to proceed, or \"cancel\" to cancel the action")}{Colors.Reset}\n{Constants.Translator.Translate("New Number")}> ");
                string s = Console.ReadLine();
                if(Int32.TryParse(s, out int b)) {id = b;}
                else if(s.ToLower() == "cancel" || Constants.Translator.Reverse(s.ToLower()) == "cancel"){return false;}
            }
            
            /*for (int i = 0; i < Cards.Count; i++) {
                if(i != id && Cards[i].MustBeUsed){Console.Write($"{Colors.Error.AnsiFGCode}[ERROR]: Action forbidden, there are cards required to be used!\n");
                    return false;
                }
            }*/
            if (!CanEndTurn) {
                Console.WriteLine($"{Colors.Error.AnsiFGCode}{Constants.Translator.Translate("[ERROR]: Action forbidden, there are cards required to be used!")}{Color.Reset}");
                return false;
            }

            Cards[id].MustBeUsed = true;                                  //Right here
            DiscardPile.PushCard(Cards[id]);                              //    ^
            Cards.RemoveAt(id);                                           //    |
            //DiscardPile[DiscardPile.CardsLeft - 1].MustBeUsed = true;     Moved it a bit up
            if(Constants.AutoSave){Save_Load.Save(Program.Game);}
            return true;
        }

        [PlayerInvokable(Name = "Ls", Description = "Alias for list")]
        public void Ls() => List(false);
        [PlayerInvokable(Name = "List", Description = "Lists the player's cards")]
        public void List(bool horizontal = true) {
            for (int i = 0; i < Cards.Count; i++) {
                string BG = Color.Reset;
                string FG = "";
                bool Selected = Selection.IndexOf(i) != -1; 
                if(Selected){BG = Colors.Selected.AnsiBGCode;}
                if(Cards[i].MustBeUsed){FG = Colors.Warning.AnsiFGCode;}
                
                if(!horizontal) Console.WriteLine($"{FG}{i}:\t{BG}{Constants.Suit[(int)Cards[i].Suit]}{BG}{FG}{Constants.Value[Cards[i].Value]}{Color.Reset}");
                if (horizontal) Console.Write($"{BG}{FG}{Constants.Suit[(int)Cards[i].Suit]}{BG}{FG}{Constants.Value[Cards[i].Value]}{Color.Reset} ");
            }
            if(horizontal){Console.Write("\n");}
        }

        [PlayerInvokable(Name = "Select", Description = "Selects the given cardIDs")]
        public void Select(params object[] newSelection) {  //all objects must be typeof(int)!
            if(newSelection.Length == 0){Selection.Clear();}
            for (int i = 0; i < newSelection.Length; i++) {
                //TODO: Refactor this try-catch
                try {
                    int s;
                    if((s = Convert.ToInt32(newSelection[i]))<Cards.Count && s>=0) Selection.Add(s);
                }
                //Tempoorary fix for exception crashing the program
                catch(Exception E){Console.WriteLine($"{Colors.Error.AnsiFGCode}{Constants.Translator.Translate("[ERROR]: ")}{E.Message}{Color.Reset}");}
            }
        }
        private void Select(IEnumerable<int> selection) => Selection.AddRange(selection);
        
        [PlayerInvokable(Name = "Meld",  Description = "Creates a meld made of the selection given")]
        public void Meld([AutoCompleteParameter]List<Meld> melds, [AutoCompleteParameter]Player P, params object[] selection) {
            if (selection.Length == 0) {
                if(Selection.Count == 0){Console.WriteLine($"{Colors.Error.AnsiFGCode}{Constants.Translator.Translate("[ERROR]: Nothing was selected, no melds were created")}{Color.Reset}");return;}
                if(Selection.Count  < 3){Console.WriteLine($"{Colors.Error.AnsiFGCode}{Constants.Translator.Translate("[ERROR]: Less than the required amount of cards were selected, no melds were created")}{Color.Reset}");return;}
            }
            else {
                for (int i = 0; i < selection.Length; i++) {
                    //TODO: Refactor this try-catch
                    try {
                        int s;
                        if((s = Convert.ToInt32(selection[i]))<Cards.Count && s>=0) Selection.Add(s);
                    }
                    //Tempoorary fix for exception crashing the program
                    catch(Exception E){Console.WriteLine($"{Colors.Error.AnsiFGCode}{Constants.Translator.Translate("[ERROR]: ")}{E.Message}{Color.Reset}");}
                }
            }

            List<Card> SelectedCards = new List<Card>();
            for(int i = 0; i<Selection.Count; i++){SelectedCards.Add(Cards[Selection[i]]);}
            Meld newMeld = null;
            bool success = true;
            try {
                newMeld = Rummy.Meld.Melder(SelectedCards, PlayerID);
            }
            catch(Exception E) { Console.WriteLine($"{Colors.Error.AnsiFGCode}{Constants.Translator.Translate("[ERROR]: ")}{E.Message}{Color.Reset}");success = false; }

            if (success) {
                Selection.Sort();
                for (int i = Selection.Count - 1; i >= 0; i--) Cards.RemoveAt(Selection[i]);
                for (int i = 0; i < newMeld.Cards.Count; i++) newMeld.Cards[i].MustBeUsed = true;
                melds.Add(newMeld);
                P.Melds.Add(newMeld);
            }
            Selection.Clear();
        }

        [PlayerInvokable(Name = "Add", Description = "Tries to extend a selected meld with the given card")]
        public void Add([AutoCompleteParameter]List<Meld> melds,[AutoCompleteParameter]int Score , int meldindex, int cardindex)
        {
            Card AddedCard = Cards[cardindex];
            Meld ExtendedMeld = melds[meldindex];
            if (ExtendedMeld.PlayerID != PlayerID) {
                if (Score < Constants.MinMeldScore) {
                    Console.WriteLine($"{Colors.Warning.AnsiFGCode}{Constants.Translator.Translate("[WARNING]: Action forbidden: Score is less than")} {Constants.MinMeldScore}{Color.Reset}");
                    return;
                }
                if (!ExtendedMeld.CanBeAddedTo) {
                    Console.WriteLine($"{Colors.Warning.AnsiFGCode}{Constants.Translator.Translate("[WARNING]: Action forbidden: the Owner of the meld hasn't reached the minimum score required")}{Color.Reset}");
                    return;
                }
            }

            bool success = false;
            if(ExtendedMeld.Validate(AddedCard)) {
                if (ExtendedMeld is SetMeld) {
                    ExtendedMeld.Cards.Add(AddedCard);
                    Cards.RemoveAt(cardindex);
                    if (ExtendedMeld.Cards.Count > Constants.Suit.Length) {
                        for (int i = 0; i < ExtendedMeld.Cards.Count; i++) {
                            if (ExtendedMeld.Cards[i].IsJoker) {
                                Cards.Add(ExtendedMeld.Cards[i]);
                                ExtendedMeld.Cards.RemoveAt(i);
                            }
                        }
                    }
                    return;
                }

                if (ExtendedMeld is RunMeld rm)
                {
                    if(!AddedCard.IsJoker){
                        if (rm.Cards[^1].IsJoker) {
                            Card JokerRef = new Card(rm.MeldSuit, rm.Cards[^2].Value + 1);
                            if (AddedCard.Value - 1 == JokerRef.Value) {
                                rm.Cards.Add(AddedCard);    
                                Cards.RemoveAt(cardindex);
                                return;
                            }
                            if (AddedCard.Value == (int)Rummy.Value.Ace) {
                                if (JokerRef.Value == (int)Rummy.Value.King) {
                                    rm.Cards.Add(AddedCard);
                                    Cards.RemoveAt(cardindex);
                                    return;
                                }
                            }
                        }
                        if (rm.Cards[0].IsJoker) {
                            Card JokerRef = new Card(rm.MeldSuit, rm.Cards[1].Value - 1);
                            if (AddedCard.Value + 1 == JokerRef.Value) {
                                rm.Cards.Insert(0, AddedCard);
                                Cards.RemoveAt(cardindex); 
                                return;
                            }
                            if (AddedCard.Value == (int)Rummy.Value.Ace) {
                                if (rm.Cards[0].Value == (int)Rummy.Value.N2) {
                                    rm.Cards.Insert(0, AddedCard);
                                    Cards.RemoveAt(cardindex);
                                    return;
                                }    
                            }
                        }

                        if (AddedCard.Value + 1 == rm.Cards[0].Value) {
                            rm.Cards.Insert(0, AddedCard);
                            Cards.RemoveAt(cardindex);
                            return;
                        }
                        
                        if (AddedCard.Value - 1 == rm.Cards[^1].Value) {
                            rm.Cards.Add(AddedCard);   
                            Cards.RemoveAt(cardindex);
                            return;
                        }
                        
                        if (AddedCard.Value == (int)Rummy.Value.Ace) {
                            if (rm.Cards[^1].Value == (int)Rummy.Value.King) {
                                rm.Cards.Add(AddedCard);    
                                Cards.RemoveAt(cardindex);
                                return;
                            }
                            if (rm.Cards[0].Value == (int)Rummy.Value.N2) {
                                rm.Cards.Insert(0, AddedCard);
                                Cards.RemoveAt(cardindex);
                                return;
                            }
                        }
                        for (int i = 0; i < rm.Cards.Count; i++) {
                            if (rm.Cards[i].IsJoker) {
                                if(i > 0 && rm.Cards[i-1].Value == (AddedCard.Value == (int)Value.Ace? (int)Value.King : AddedCard.Value-1)){rm.Cards.Insert(i, AddedCard); Cards.RemoveAt(cardindex); Cards.Add(rm.Cards[i+1]); rm.Cards.RemoveAt(i+1); success = true; return; }
                                if(i < rm.Cards.Count-1 && rm.Cards[i+1].Value == (AddedCard.Value) + 1){rm.Cards.Insert(i, AddedCard); Cards.RemoveAt(cardindex); Cards.Add(rm.Cards[i+1]); rm.Cards.RemoveAt(i+1); success = true; return; }
                            }
                        }
                    }

                    if (AddedCard.IsJoker) {
                        Console.Write($"{Colors.Warning.AnsiFGCode}[WARNING]: The Joker card you referenced WILL be used up.{Colors.Important.AnsiFGCode} {Constants.Translator.Translate("Proceed? [y/N]")} ");
                        ConsoleKeyInfo? n_key = Constants.IH.Pull();
                        while(!n_key.HasValue){n_key = Constants.IH.Pull();}
                        char input = n_key.Value.KeyChar;
                        Console.Write("\n");
                        if (input == 'y') {
                            Console.Write($"{Colors.Important.AnsiFGCode}{Constants.Translator.Translate("Should the card be placed at the end of the meld? [y/N]")}{Color.Reset} ");
                            n_key = Constants.IH.Pull();
                            while(!n_key.HasValue){n_key = Constants.IH.Pull();}
                            input = n_key.Value.KeyChar;
                            Console.Write("\n");
                            if (input == 'y') {
                                rm.Cards.Add(AddedCard);
                                Console.WriteLine($"{Colors.Important.AnsiFGCode} {Constants.Translator.Translate("Card placed at the end of the meld")}{Color.Reset}");
                            }
                            else {
                                rm.Cards.Insert(0, AddedCard);
                                Console.WriteLine($"{Colors.Important.AnsiFGCode} {Constants.Translator.Translate("Card placed at the start of the meld")}{Color.Reset}");
                            }
                            Cards.RemoveAt(cardindex);
                        }
                    }
                }
            }
            else{Console.WriteLine($"{Colors.Warning.AnsiFGCode}{Constants.Translator.Translate("[WARNING]: Selected card wasn't added, as it couldn't be inserted anywhere.")}{Color.Reset}");}
            //if(!success){Console.WriteLine($"{Colors.Warning.AnsiFGCode}[WARNING]: Insert attempt was unsuccessful{Color.Reset}");}
        }
        
#if DEBUG
        [PlayerInvokable(Name = "/give", Description = "/give")]
        public void Give(Suit suit, Value v, int playerID = -1)
        {
            Hand h;
            if(playerID == -1){h = this;}
            else h = Program.Game.Players[playerID].Hand;
            h.Cards.Add(new Card(suit, (int)v));
        }
#endif
    }
}
