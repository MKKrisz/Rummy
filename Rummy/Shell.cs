using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Rummy.TextColor;

namespace Rummy
{
    public class Shell
    {
        public int PlayerID;
        
        //method parameters which don't require to be input
        private static readonly Type[] AutoCompleteArgs = { typeof(Deck), typeof(Hand), typeof(List<Card>), typeof(Card), typeof(List<Meld>)};
        
        
        //references to all needed variables
        private Hand Hand;                                          //reference to the player's hand
        private Player Player;
        int Round => Program.Game.Round;
        Deck GameDeck => Program.Game.Deck;                         //reference to the game deck
        Deck DiscardPile => Program.Game.DiscardPile;         //reference to the discard pile, for picking it up, or for ending the turn
        Card TrumpCard => Program.Game.TrumpCard;
        private List<Meld> Melds => Program.Game.Melds;


        public Shell(Player P)
        {
            PlayerID = P.ID;
            Hand = P.Hand;
            Player = P;
        }

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
        public void StartTurn()
        {
            static bool IsParams(ParameterInfo param)
            {
                return param.GetCustomAttributes(typeof (ParamArrayAttribute), false).Length > 0;
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
                                    if(t == typeof(Deck) && CurrentParameter.Name == nameof(DiscardPile) /* HACK */)       {Args[CurrentParameter.Position] = DiscardPile; }
                                    if(t == typeof(Hand))       {Args[CurrentParameter.Position] = Hand;}
                                    if(t == typeof(Card))       {Args[CurrentParameter.Position] = TrumpCard;}
                                    if(t == typeof(List<Meld>)) {Args[CurrentParameter.Position] = Melds;}
                                    //----------------------------!!---------------------------------------------
                                }
                                else
                                {
                                    if (IsParams(CurrentParameter))
                                    {
                                        //TODO: global params [] parameter conversion 
                                        Type T = CurrentParameter.ParameterType.GetElementType();
                                        List<object> list = new List<object>();
                                        for (int k = j; k < RawArgs.Length; k++)
                                        {
                                            list.Add(Convert.ChangeType(RawArgs[k], T));
                                        }
                                        Args[CurrentParameter.Position] = list.ToArray();
                                        j = RawArgs.Length;
                                    }

                                    if (RawArgs.Length > j && RawArgs[j] != null)
                                    {
                                        try{
                                            if (CurrentParameter.ParameterType.IsEnum) { Args[CurrentParameter.Position] = Enum.Parse(CurrentParameter.ParameterType, RawArgs[j], true); j++;}
                                            else {Args[CurrentParameter.Position] = Convert.ChangeType(RawArgs[j], CurrentParameter.ParameterType); j++;}
                                        }
                                        catch(Exception E){Console.WriteLine($"{Colors.Error.AnsiFGCode}[ERROR]: {E.Message}{Color.Reset}");}
                                    }
                                }
                                
                            }
                            //invokes the function
                            //NOTICE: this might not work, as these functions are not static. Will be fixed in next commit (probably) Note: I added a check in Attribute_PlayerInvokable.cs that throws an exception if the attribute is placed on a non-static method.
                            //Note2: Also added a whitelist for types allowed to have instance method commands.

                            // Test for (optimally) each type contained in PlayerInvokableContainer.instanceMethodWhitelist, and provide an appropriate object instance. Otherwise: Method assumed to be static, instance is null.
                            bool failed = false;
                            try {
                                if (match.Info.DeclaringType == typeof(Hand)) match.Invoke(Args.ToList(), instance: Hand);
                                else if (match.Info.DeclaringType == typeof(Shell)) match.Invoke(Args.ToList(), instance: this);
                                else match.Invoke(Args.ToList());
                            }
                            catch (Exception E) {
                                failed = true;
                                Console.WriteLine($"{Colors.Error.AnsiFGCode}[ERROR]: {E.Message}{Color.Reset}");
#if DEBUG
                                Console.WriteLine($"\n\nStack trace:\n{E.StackTrace}");
#endif
                                Console.ReadKey(true); // [Dit05] The console would be immediately cleared after printing the exception. Good thing this didn't cause anyone problems, right?
                            }
                            //Checks if the invoked function has the "TurnEnder" attribute, if yes, exits this loop, and thus, ending the player's turn
                            if (!failed && match.Info.GetCustomAttributes().OfType<TurnEnder>().Any()) { run = false;}
                            
                            //displays a new prompt, discards last input
                            //TODO: command history
                            Console.Write("> ");
                        }
                        else
                        {
                            Console.Write($"\n{Colors.Warning.AnsiFGCode}[WARNING]: No method found. Type \"help\" for available commands{Color.Reset}\n> ");
                        }
                        Input.Clear();
                        break;
                    default:
                        //Adds key to the input
                        if(key.KeyChar != '\0')Input.Add(key.KeyChar);
                        Console.Write(key.KeyChar);
                        break;
                }
            }
        }

        [PlayerInvokable(Name = "Help", Description = "Displays this message")]
        public void Help(string s = null)
        {
            bool success = false;
            if (s != null)
            {
                PlayerInvokable[] matches = Search(s, exact: true);
                if (matches.Length == 1)
                {
                    success = true;
                    PlayerInvokable match = matches[0];
                    Console.WriteLine($"{match.Name}:\t{match.Description}");
                    if(match.Params.Length != 0){Console.WriteLine("Parameters:");}
                    for (int i = 0; i < match.Params.Length; i++)
                    {
                        Console.Write($"{i}:\t");
                        if(match.Params[i].GetCustomAttributes().OfType<AutoCompleteParameter>().Any()){Console.Write($"{Colors.Ignorable.AnsiFGCode}(AutoCompleted) ");}
                        Console.Write($"{match.Params[i].Name}\t{match.Params[i].ParameterType}\u001b[m");
                        Console.Write("\n");
                    }
                }
                else Console.WriteLine($"{Colors.Warning.AnsiFGCode}[WARNING]: No method named {Colors.Important.AnsiFGCode}\"{s}\" {Colors.Warning.AnsiFGCode}was found, or the input wasn't the exact name{Color.Reset}");
            }
            if(!success)
            {
                for (int i = 0; i < PlayerInvokableContainer.Methods.Count; i++)
                {
                    Console.WriteLine($"{PlayerInvokableContainer.Methods[i].Name}\t\t{PlayerInvokableContainer.Methods[i].Description}");
                }
            }
        }

        [PlayerInvokable(Name = "Clear", Description = "Clears the console")]
        public void Clear() => Console.Clear();
        
        [PlayerInvokable(Name = "Exit", Description = "Exits the game")]
        public void Exit(bool save = false)
        {
            //TODO: save
            if(save == true){throw new NotImplementedException();}

            Program.Game.Run = false;
            Program.Run = false;
        }


        [PlayerInvokable(Name = "Melds", Description = "Alias for ListMelds")]
        public void LMelds() => ListMelds();
        [PlayerInvokable(Name = "ListMelds", Description = "Lists all melds")]
        public void ListMelds()
        {
            for (int i = 0; i < Melds.Count; i++)
            {
                string BG = Color.Reset;
                if(Melds[i].PlayerID == PlayerID){BG = Colors.Selected.AnsiBGCode;}
                Console.Write($"{BG}{i}: ID: {Melds[i].PlayerID}\t");
                for (int j = 0; j < Melds[i].Cards.Count; j++)
                {
                    Console.Write($"{Program.Suit[(int)Melds[i].Cards[j].Suit]}{BG}{Program.Value[Melds[i].Cards[j].Value]} ");
                }
                Console.Write($"{Color.Reset}\n");
            }
        }
    }
}
