using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TextColor;

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
                return (int)x.Suit - (int)y.Suit;
            }
        }

        [PlayerInvokable(Name = "Switch", Description = "Switches two cards in the hand (useful for manual sorting)")]
        public void Switch(int a, int b)
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
            DiscardPile.Add(Cards[id]);
            Cards.RemoveAt(id);
        }

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
                Selection.Add(Convert.ToInt32(selection[i]));   
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
                for(int i = 0; i<selection.Length; i++)Selection.Add(Convert.ToInt32(selection[i]));
            }

            List<Card> SelectedCards = new List<Card>();
            for(int i = 0; i<Selection.Count; i++){SelectedCards.Add(Cards[Selection[i]]);}
            Meld newMeld = null;
            bool success = true;
            try
            {
                newMeld = Rummy.Meld.Melder(SelectedCards);
            }
            catch(Exception E) { Console.WriteLine($"{Colors.Error.AnsiFGCode}[ERROR]: {E.Message}"); success = false;}

            if (success)
            {
                int[] buffer = Selection.ToArray();
                Array.Sort(buffer);
                Selection = buffer.ToList();
                for(int i = Selection.Count-1; i>=0; i--)Cards.RemoveAt(Selection[i]);
                Selection.Clear();
            }
            if(newMeld != null){melds.Add(newMeld);}
        }
    }

    
}
