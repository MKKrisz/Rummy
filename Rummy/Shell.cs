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
        
        //method parameters which don't require to be input
        private static readonly Type[] AutoCompleteArgs = { typeof(Deck), typeof(Hand), typeof(List<Card>), typeof(Card) };
        
        
        //references to all needed variables
        private Hand Hand;                                          //reference to the player's hand
        private Player Player;
        int Round => Program.Game.Round;
        Deck GameDeck => Program.Game.Deck;                         //reference to the game deck
        List<Card> DiscardPile => Program.Game.DiscardPile;         //reference to the discard pile, for picking it up, or for ending the turn
        Card TrumpCard => Program.Game.TrumpCard;
        //TODO:A tercek/lepakolt lapok tárolója
        
        
        public Shell(Player P)
        {
            PlayerID = P.ID;
            Hand = P.Hand;
            Player = P;
        }

        public void StartTurn()
        {
            
            //the search script used
            //TODO: search/autocomplete on method parameters
            PlayerInvokable[] Search(string input, bool exact)
            {
                List<PlayerInvokable> output = new List<PlayerInvokable>();
                List<PlayerInvokable> query = PlayerInvokableContainer.Methods;
                for (int i = 0; i < query.Count(); i++)
                {
                    string sample = "";
                    //if "exact" switch is not set, only compares the length of the input 
                    if (!exact)
                    {
                        try   {sample = query[i].Name.Remove(input.Length);}
                        catch {sample = query[i].Name;}
                    }
                    if(exact) {sample = query[i].Name;}
                    
                    //if method (or its partial) name and input match, add it to output
                    if(input.ToLower() == sample.ToLower()){output.Add(query[i]);}
                }

                return output.ToArray();
            }
            Console.Clear();
            Console.Write($"Player {PlayerID}, Round {Round}\n");
            Hand.List();
            Console.Write("> ");

            List<char> Input = new List<char>();
            
            bool run = true;
            while (run)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                PlayerInvokable[] matches;
                //checks if pressed key is one of the ones below
                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        //deletes last char of input, and rewrites it with a ' '(space) on the screen
                        if (Input.Count > 0)
                        {
                            Input.RemoveAt(Input.Count()-1);
                            Console.CursorLeft--;
                            Console.Write(" ");
                            Console.CursorLeft--;
                        }
                        break;
                    case ConsoleKey.Tab:
                        //searches if there is a match to the input, if there is only one, autocompletes
                        matches = Search(new string(Input.ToArray()), false);
                        if (matches.Length == 1)
                        {
                            Input = matches[0].Name.ToCharArray().ToList();
                            Console.CursorLeft = 2;
                            Console.Write(new string(Input.ToArray()));
                        }
                        //TODO: output, for when there is more/less matches
                        break;
                    case ConsoleKey.Enter:
                        if(Input.Count() == 0) {break;}
                        
                        //searches if there is (only) one exact mach for the given input, if there is, invokes it
                        
                        //splits up the string 
                        //should not give back null, as it is checked for a few lines above
                        string[] splitInput = new string(Input.ToArray()).Split(' ');
                        string command = splitInput[0];
                        
                        List<string> b = splitInput.ToList();
                        //removes first object in the split up input (the MethodName) -> leftover = parameters
                        b.RemoveAt(0);
                        string[] RawArgs = b.ToArray();
                        
                        matches = Search(command, true);
                        if (matches.Length == 1)
                        {
                            Console.Write("\n");
                            
                            //makes reading a bit easier (otherwise unnecessary line)
                            PlayerInvokable match = matches[0];
                            //---------------------------------
                            
                            //Creates an array with the length of the method(match)'s parameters 
                            object[] Args = new object[match.Params.Length];
                            int j = 0;
                            for (int i = 0; i < Args.Length; i++)
                            {
                                
                                ParameterInfo CurrentParameter = match.Params[i];

                                //if the parameter's type is in AutocompleteArgs, autocomplete  
                                if (Array.IndexOf(AutoCompleteArgs, CurrentParameter.ParameterType) != -1)
                                {
                                    Type t = CurrentParameter.ParameterType;
                                    
                                    //IMPORTANT!!!!:
                                    //If you wish to add new autocompleted variables to the array above, DON'T forget to add functionality here!!
                                    
                                    //----------------------------!!---------------------------------------------
                                    if(t == typeof(Deck))       {Args[CurrentParameter.Position] = GameDeck;}
                                    if(t == typeof(Hand))       {Args[CurrentParameter.Position] = Hand;}
                                    if(t == typeof(List<Card>)) {Args[CurrentParameter.Position] = DiscardPile;}
                                    if(t == typeof(Card))       {Args[CurrentParameter.Position] = TrumpCard;}
                                    //----------------------------!!---------------------------------------------
                                }
                                else
                                {
                                    if (RawArgs.Length > j && RawArgs[j] != null)
                                    {
                                        if (CurrentParameter.ParameterType.IsEnum) { Args[CurrentParameter.Position] = Enum.Parse(CurrentParameter.ParameterType, RawArgs[j], true);}
                                        else {Args[CurrentParameter.Position] = Convert.ChangeType(RawArgs[j], CurrentParameter.ParameterType); j++;}
                                    }
                                }
                                
                            }
                            //invokes the function
                            //NOTICE: this might not work, as these functions are not static. Will be fixed in next commit (probably) Note: I added a check in Attribute_PlayerInvokable.cs that throws an exception if the attribute is placed on a non-static method.
                            //Note2: Also added a whitelist for types allowed to have instance method commands.

                            // Test for (optimally) each type contained in PlayerInvokableContainer.instanceMethodWhitelist, and provide an appropriate object instance. Otherwise: Method assumed to be static, instance is null.
                            if(match.Info.DeclaringType == typeof(Hand)) match.Invoke(Args.ToList(), instance: Hand);
                            else match.Invoke(Args.ToList());
                            
                            //Checks if the invoked function has the "TurnEnder" attribute, if yes, exits this loop, and thus, ending the player's turn
                            if (match.Info.GetCustomAttributes().OfType<TurnEnder>().Any()) { run = false;}
                            
                            //displays a new prompt, discards last input
                            //TODO: command history
                            Console.Write("> ");
                            Input.Clear();
                            
                        }
                        break;
                    default:
                        //Adds key to the input
                        Input.Add(key.KeyChar);
                        Console.Write(key.KeyChar);
                        break;
                }
            }
        }
    }
}
