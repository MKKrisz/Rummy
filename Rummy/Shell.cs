using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Rummy
{
    public class Shell
    {
        public int PlayerID;
        private static Type[] AutoCompleteArgs = { typeof(Deck), typeof(Hand), typeof(List<Card>), typeof(Card) };
        
        
        //references to all needed variables
        private Hand Hand;                                          //reference to the player's hand

        int Round => Program.Game.Round;
        Deck GameDeck => Program.Game.Deck;                         //reference to the game deck
        List<Card> DiscardPile => Program.Game.DiscardPile;         //reference to the discard pile, for picking it up, or for ending the turn
        Card TrumpCard => Program.Game.TrumpCard;
        //TODO:A tercek/lepakolt lapok tárolója
        
        
        public Shell(Player P)
        {
            PlayerID = P.ID;
            Hand = P.Hand;
        }

        public void StartTurn()
        {
            PlayerInvokable[] Search(string input, bool exact)
            {
                List<PlayerInvokable> output = new List<PlayerInvokable>();
                List<PlayerInvokable> query = PlayerInvokableContainer.Methods;
                for (int i = 0; i < query.Count(); i++)
                {
                    string sample = "";
                    if(!exact) {sample = query[i].Name.Remove(input.Length);}
                    if(exact)  {sample = query[i].Name;}
                    
                    if(input == sample){output.Add(query[i]);}
                }

                return output.ToArray();
            }
            
            Console.Clear();
            Console.Write($"Player {PlayerID}, Round {Round}/n > ");

            List<char> Input = new List<char>();
            
            bool run = true;
            while (run)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                PlayerInvokable[] matches = null;
                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        if (Input.Count > 0)
                        {
                            Input.RemoveAt(Input.Count()-1);
                            Console.CursorLeft--;
                            Console.Write(" ");
                            Console.CursorLeft--;
                        }
                        break;
                    case ConsoleKey.Tab:
                        matches = Search(Input.ToString(), false);
                        if (matches.Length == 1)
                        {
                            Input = matches[0].Name.ToCharArray().ToList();
                            Console.CursorLeft = 2;
                            Console.Write(Input.ToArray().ToString());
                        }
                        break;
                    case ConsoleKey.Enter:
                        if(Input.Count() == 0) {break;}
                        
                        string[] splitInput = Input.ToArray().ToString().Split(' ');
                        string command = splitInput[0];
                        
                        List<string> b = splitInput.ToList();
                        b.RemoveAt(0);
                        string[] RawArgs = b.ToArray();
                        
                        matches = Search(Input.ToString().Split(' ')[0], true);
                        if (matches.Length == 1)
                        {
                            Console.Write("\n");

                            PlayerInvokable match = matches[0];
                            object[] Args = new object[match.Params.Length];
                            for (int i = 0; i < RawArgs.Length; i++)
                            {
                                ParameterInfo CP = match.Params[i];
                                
                                if(Array.IndexOf(AutoCompleteArgs, CP.ParameterType) == -1){Args[CP.Position] = Convert.ChangeType(RawArgs[i], CP.ParameterType);}
                                else
                                {
                                    Type t = CP.ParameterType;
                                    if(t == typeof(Deck))       {Args[CP.Position] = GameDeck;}
                                    if(t == typeof(Hand))       {Args[CP.Position] = Hand;}
                                    if(t == typeof(List<Card>)) {Args[CP.Position] = DiscardPile;}
                                    if(t == typeof(Card))       {Args[CP.Position] = TrumpCard;}
                                }
                                
                            }
                            match.Invoke(Args.ToList());
                            if (match.Info.GetCustomAttributes().OfType<TurnEnder>().Any()) { run = false;}
                            Console.Write("\n>");
                            Input = new List<char>();
                            
                        }
                        break;
                    default:
                        Input.Add(key.KeyChar);
                        Console.Write(key.KeyChar);
                        break;
                }
            }
        }
    }
}