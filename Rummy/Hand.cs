using System;
using System.Collections;
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

        public Hand(Random r, Deck deck, bool firstPlayer, int playerId)
        {
            PlayerID = playerId;
            
            //[MKKrisz] Why?
            if (firstPlayer) { Cards.AddRange(deck.Deal(r, 15)); }
            else             { Cards.AddRange(deck.Deal(r));}
        }

        public void ResetUsingStatus()
        {
            for(int i = 0; i<Cards.Count; i++){Cards[i].MustBeUsed = false;}
        }
        
        public enum SortType{Suit = 0, Value = 1, Both = 2}
        [PlayerInvokable(Name = "Sort", Description = "Sorts the hand based on input (0/Suit, 1/Value, 2/Both)")]
        public void Sort(SortType T = SortType.Both)
        {
            Card[] buffer = Cards.ToArray();
            if (T == SortType.Suit || T == SortType.Both)
            {
                Array.Sort(buffer, new CardSuitComparer());
            }
            if (T == SortType.Value || T == SortType.Both)
            {
                Array.Sort(buffer, new CardValueComparer());
            }

            Cards.Clear();
            Cards.AddRange(buffer);
        }
        public class CardValueComparer : IComparer<Card>
        {
            public int Compare(Card x, Card y)
            {
                if (x == null || y == null)     {return  0;}
                //if (x.Value == (int)Value.Joker){return  1;}
                //if (y.Value == (int)Value.Joker){return -1;}
                if (x.Value <  y.Value)         {return -1;}
                if (x.Value == y.Value)         {return  0;}
                return  1;
            }
        }
        public class CardSuitComparer : IComparer<Card>
        {
            public int Compare(Card x, Card y)
            {
                return (int)x.Suit - (int)y.Suit;
            }
        }
        
        [PlayerInvokable(Name = "Swap", Description = "Switches two cards in the hand (useful for manual sorting)")]
        public void Swap(int a, int b)
        {
            if (a > Cards.Count || b > Cards.Count)
            {
                Console.WriteLine($"{Colors.Error.AnsiFGCode}[ERROR]: one index is out of range{Color.Reset}");
                return;
            }
            if (a == b)
            {
                Console.WriteLine($"{Colors.Warning.AnsiFGCode}[WARNING]: the ID-s are the same, switching is not required{Color.Reset}");
                return;
            }

            Card buffer = Cards[a].Copy();
            Cards[a] = Cards[b];
            Cards[b] = buffer;
        }
        
        [TurnEnder]
        [PlayerInvokable(Name = "Discard", Description = "Discards a card, and thus ends the turn")]
        public bool Discard([AutoCompleteParameter]Deck DiscardPile, int id = -1)
        {
            if(id == -1){id = Cards.Count - 1;}
            if(DiscardPile == null) throw new Exception("discard pile is null");
            while (id >= Cards.Count || id<0)
            {
                Console.Write($"{Colors.Warning.AnsiFGCode}[WARNING]: Invalid index. Give a valid number to proceed, or \"cancel\" to cancel the action{Colors.Reset}\nNew Number> ");
                string s = Console.ReadLine();
                if(Int32.TryParse(s, out int b)) {id = b;}
                else if(s.ToLower() == "cancel"){return false;}
            }
            for (int i = 0; i < Cards.Count; i++)
            {
                if(i != id && Cards[i].MustBeUsed){Console.Write($"{Colors.Error.AnsiFGCode}[ERROR]: Action forbidden, there are cards required to be used!\n");
                    return false;
                }
            }
            
            DiscardPile.PushCard(Cards[id]);
            Cards.RemoveAt(id);
            DiscardPile[DiscardPile.CardsLeft - 1].MustBeUsed = true;
            return true;
        }

        [PlayerInvokable(Name = "Ls", Description = "Alias for list")]
        public void Ls() => List(false);
        [PlayerInvokable(Name = "List", Description = "Lists the player's cards")]
        public void List(bool horizontal = true)
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                string BG = Color.Reset;
                string FG = "";
                bool Selected = Selection.IndexOf(i) != -1; 
                if(Selected){BG = Colors.Selected.AnsiBGCode;}
                if(Cards[i].MustBeUsed){FG = Colors.Warning.AnsiFGCode;}
                
                if(!horizontal) Console.WriteLine($"{FG}{i}:\t{BG}{Program.Suit[(int)Cards[i].Suit]}{BG}{FG}{Program.Value[Cards[i].Value]}{Color.Reset}");
                if (horizontal)
                {
                    Console.Write($"{BG}{FG}{Program.Suit[(int)Cards[i].Suit]}{BG}{FG}{Program.Value[Cards[i].Value]}{Color.Reset} ");
                }
            }
            if(horizontal){Console.Write("\n");}
        }

        [PlayerInvokable(Name = "Select", Description = "Selects the given cardIDs")]
        public void Select(params object[] selection)   //all objects must be typeof(int)!
        {
            if(selection.Length == 0){Selection.Clear();}
            for (int i = 0; i < selection.Length; i++)
            {
                try
                {
                    int s;
                    if((s = Convert.ToInt32(selection[i]))<Cards.Count && s>0) Selection.Add(s);
                }
                //Tempoorary fix for exception crashing the program
                catch(Exception E){Console.WriteLine($"{Colors.Error.AnsiFGCode}[ERROR]: {E.Message}{Color.Reset}");}
            }
        }
        [PlayerInvokable(Name = "Meld",  Description = "Creates a meld made of the selection given")]
        public void Meld([AutoCompleteParameter]List<Meld> melds, params object[] selection)
        {
            if (selection.Length == 0)
            {
                if(Selection.Count == 0){Console.WriteLine($"{Colors.Error.AnsiFGCode}[ERROR]: Nothing was selected, no melds were created{Color.Reset}");return;}
                if(Selection.Count  < 3){Console.WriteLine($"{Colors.Error.AnsiFGCode}[ERROR]: Less than the required amount of cards were selected, no melds were created{Color.Reset}");return;}
            }
            else
            {
                for (int i = 0; i < selection.Length; i++)
                {
                    try
                    {
                        int s;
                        if((s = Convert.ToInt32(selection[i]))<Cards.Count && s>=0) Selection.Add(s);
                    }
                    //Temporary fix for exception crashing the program
                    catch(Exception E){Console.WriteLine($"{Colors.Error.AnsiFGCode}[ERROR]: {E.Message}{Color.Reset}");}
                }
            }

            List<Card> SelectedCards = new List<Card>();
            for(int i = 0; i<Selection.Count; i++){SelectedCards.Add(Cards[Selection[i]]);}
            Meld newMeld = null;
            bool success = true;
            try
            {
                newMeld = Rummy.Meld.Melder(SelectedCards, PlayerID);
            }
            catch(Exception E) { Console.WriteLine($"{Colors.Error.AnsiFGCode}[ERROR]: {E.Message}{Color.Reset}"); success = false;}

            if (success)
            {
                int[] buffer = Selection.ToArray();
                Array.Sort(buffer);
                Selection = buffer.ToList();
                for(int i = Selection.Count-1; i>=0; i--)Cards.RemoveAt(Selection[i]);
                for(int i = 0; i < newMeld.Cards.Count; i++) newMeld.Cards[i].MustBeUsed = true;
            }
            if(newMeld != null){melds.Add(newMeld);}
            Selection.Clear();
        }

        [PlayerInvokable(Name = "Add", Description = "Tries to extend a selected meld with the given card")]
        public void Add(List<Meld> melds, int Score , int meldindex, int cardindex)
        {
            Card AddedCard = Cards[cardindex];
            Meld ExtendedMeld = melds[meldindex];
            if(Score<51 && ExtendedMeld.PlayerID != PlayerID){Console.WriteLine($"{Colors.Warning.AnsiFGCode}[WARNING]: Action forbidden: Score is less than 51{Color.Reset}");return;}
            if(!ExtendedMeld.CanBeAddedTo && ExtendedMeld.PlayerID != PlayerID){Console.WriteLine($"{Colors.Warning.AnsiFGCode}[WARNING]: Action forbidden: the Owner of the meld hasn't reached the minimum score required.{Color.Reset}");return;}
            bool success = false;
            if(ExtendedMeld.Validate(AddedCard))
            {
                if (ExtendedMeld is SetMeld)
                {
                    ExtendedMeld.Cards.Add(AddedCard);
                    success = true;
                    Cards.RemoveAt(cardindex);
                    if (ExtendedMeld.Cards.Count > Program.Suit.Length)
                    {
                        for (int i = 0; i < ExtendedMeld.Cards.Count; i++)
                        {
                            if (ExtendedMeld.Cards[i].Value == (int)Value.Joker)
                            {
                                Cards.Add(ExtendedMeld.Cards[i]);
                                ExtendedMeld.Cards.RemoveAt(i);
                            }
                        }
                    }
                    return;
                }

                if (ExtendedMeld is RunMeld rm)
                {
                    if(AddedCard.Value != (int)Value.Joker){
                        if (rm.Cards[0].Value == (int)Value.Joker) {
                            Card JokerRef = new Card(rm.MeldSuit, rm.Cards[1].Value - 1);
                            if(AddedCard.Value + 1 == JokerRef.Value)                  {rm.Cards.Insert(0, AddedCard); Cards.RemoveAt(cardindex); success = true; return; }
                        }
                        if (rm.Cards[rm.Cards.Count-1].Value == (int)Value.Joker) {
                            Card JokerRef = new Card(rm.MeldSuit, rm.Cards[rm.Cards.Count-2].Value + 1);
                            if(AddedCard.Value - 1 == JokerRef.Value)                  {rm.Cards.Add(AddedCard);       Cards.RemoveAt(cardindex); success = true; return; }
                        }
                        if(rm.Cards[0].Value != (int)Value.Joker && 
                           AddedCard.Value+1 == rm.Cards[0].Value)                     {rm.Cards.Insert(0, AddedCard); Cards.RemoveAt(cardindex); success = true; return; }
                        if(rm.Cards[rm.Cards.Count-1].Value != (int)Value.Joker && 
                           AddedCard.Value-1 == rm.Cards[rm.Cards.Count-1].Value)      {rm.Cards.Add(AddedCard);       Cards.RemoveAt(cardindex); success = true; return; }

                        for (int i = 0; i < rm.Cards.Count; i++)
                        {
                            if (rm.Cards[i].Value == (int)Value.Joker)
                            {
                                if(i > 0 && rm.Cards[i-1].Value == AddedCard.Value - 1){rm.Cards.Insert(i, AddedCard); Cards.RemoveAt(cardindex); Cards.Add(rm.Cards[i+1]); rm.Cards.RemoveAt(i+1); success = true; return; }
                                if(i < rm.Cards.Count-1 && rm.Cards[i+1].Value == AddedCard.Value + 1){rm.Cards.Insert(i, AddedCard); Cards.RemoveAt(cardindex); Cards.Add(rm.Cards[i+1]); rm.Cards.RemoveAt(i+1); success = true; return; }
                            }
                        }
                    }

                    if (AddedCard.Value == (int)Value.Joker)
                    {
                        Console.Write($"{Colors.Warning.AnsiFGCode}[WARNING]: The Joker card yoou referenced WILL be used up.{Colors.Important.AnsiFGCode} Proceed? [y/N] ");
                        char input = Console.ReadKey(false).KeyChar;
                        Console.Write("\n");
                        if (input == 'y')
                        {
                            Console.Write($"{Colors.Important.AnsiFGCode}Should the card be placed at the end of the meld? [y/N]{Color.Reset} ");
                            input = Console.ReadKey(false).KeyChar;
                            Console.Write("\n");
                            if (input == 'y')
                            {
                                rm.Cards.Add(AddedCard);
                                Console.WriteLine($"{Colors.Important.AnsiFGCode} Card placed at the end of the meld{Color.Reset}");
                            }
                            else
                            {
                                rm.Cards.Insert(0, AddedCard);
                                Console.WriteLine($"{Colors.Important.AnsiFGCode} Card placed at the start of the meld{Color.Reset}");
                            }

                            success = true;
                        }
                    }
                }
            }
            else{Console.WriteLine($"{Colors.Warning.AnsiFGCode}[WARNING]: Selected card wasn't added, as it couldn't be inserted anywhere.{Color.Reset}");}
            if(!success){Console.WriteLine($"{Colors.Warning.AnsiFGCode}[WARNING]: Insert attempt was unsuccessful{Color.Reset}");}
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
