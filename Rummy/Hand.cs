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
                if (x == null || y == null) { return 0;}
                if (x.Value <  y.Value) {return -1;}
                if (x.Value == y.Value) {return  0;}
                return  1;
            }
        }

        public class CardSuitComparer : IComparer<Card>
        {
            public int Compare(Card x, Card y)
            {
                if(x == null || y == null) return 0;
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
        
        [PlayerInvokable(Name = "Discard", Description = "Discards a card, and thus ends the turn")]
        [TurnEnder]
        public void Discard([AutoCompleteParameter]List<Card> DiscardPile, int id)
        {
            while (id >= Cards.Count || id<0)
            {
                Console.Write($"{Colors.Error.AnsiFGCode}[ERROR]: Invalid index{Colors.Reset}\nNew Number> ");
                if(Int32.TryParse(Console.ReadLine(), out int b)) {id = b;}
            }
            if(id<Cards.Count)DiscardPile.Add(Cards[id]);
            Cards.RemoveAt(id);
        }

        [PlayerInvokable(Name = "Ls", Description = "Alias for list")]
        public void Ls() => List(false);
        [PlayerInvokable(Name = "List", Description = "Lists the player's cards")]
        public void List(bool horizontal = true)
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                string BG = Color.Reset;
                
                bool Selected = Selection.IndexOf(i) != -1; 
                if(Selected){BG = Colors.Selected.AnsiBGCode;}
                
                if(!horizontal) Console.WriteLine($"{i}:\t{BG}{Program.Suit[(int)Cards[i].Suit]}{BG}{Program.Value[Cards[i].Value]}{Color.Reset}");
                if (horizontal)
                {
                    Console.Write($"{BG}{Program.Suit[(int)Cards[i].Suit]}{BG}{Program.Value[Cards[i].Value]}{Color.Reset} ");
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
                    //Tempoorary fix for exception crashing the program
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
            }
            if(newMeld != null){melds.Add(newMeld);}
            Selection.Clear();
        }

        [PlayerInvokable(Name = "Add", Description = "Tries to extend a selected meld with the given card")]
        public void Add(List<Meld> melds, int meldindex, int cardindex)
        {
            Card AddedCard = Cards[cardindex];
            Meld ExtendedMeld = melds[meldindex];
            if(ExtendedMeld.Validate(AddedCard))
            {
                if (ExtendedMeld is SetMeld)
                {
                    ExtendedMeld.Cards.Add(AddedCard);
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

                if (ExtendedMeld is RunMeld)
                {
                    if(AddedCard.Value+1 == ExtendedMeld.Cards[0].Value)        {ExtendedMeld.Cards.Insert(0, AddedCard); Cards.RemoveAt(cardindex); return; }
                    if(AddedCard.Value-1 == ExtendedMeld.Cards[ExtendedMeld.Cards.Count-1].Value){ExtendedMeld.Cards.Add(AddedCard);       Cards.RemoveAt(cardindex); return; }

                    for (int i = 0; i < ExtendedMeld.Cards.Count; i++)
                    {
                        if (ExtendedMeld.Cards[i].Value == (int)Value.Joker)
                        {
                            if(ExtendedMeld.Cards[i-1].Value == AddedCard.Value - 1){ExtendedMeld.Cards.Insert(i, AddedCard); Cards.RemoveAt(cardindex); Cards.Add(ExtendedMeld.Cards[i+1]); ExtendedMeld.Cards.RemoveAt(i+1);return; }
                            if(ExtendedMeld.Cards[i+1].Value == AddedCard.Value + 1){ExtendedMeld.Cards.Insert(i, AddedCard); Cards.RemoveAt(cardindex); Cards.Add(ExtendedMeld.Cards[i+1]); ExtendedMeld.Cards.RemoveAt(i+1);return; }
                        }
                    }
                }
            }
        }
    }
}
