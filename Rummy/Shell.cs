using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rummy.TextColor;

namespace Rummy
{
    public class Shell
    {
        public int PlayerID;
        
        //method parameters which don't require to be input
        private static readonly Type[] AutoCompleteArgs = {typeof(Player), typeof(Deck), typeof(Hand), typeof(List<Card>), typeof(Card), typeof(List<Meld>), typeof(int)/* you will murder me for this*/};
        
        
        //references to all needed variables
        private Hand Hand;                                          //reference to the player's hand
        private Player Player;
        private List<Card> Cards => Hand.Cards;
        private int Score => Player.Score;
        private int Round => Program.Game.Round;
        private Deck GameDeck => Program.Game.Deck;                         //reference to the game deck
        private int CardsLeft => GameDeck.CardsLeft;
        private Deck DiscardPile => Program.Game.DiscardPile;         //reference to the discard pile, for picking it up, or for ending the turn
        private Card TrumpCard => Program.Game.TrumpCard;
        private List<Meld> Melds => Program.Game.Melds;
        private List<PlayerInvokable> Methods => PlayerInvokableContainer.Methods;

        private string PS1 = "> ";
        public Shell(Player P)
        {
            PlayerID = P.ID;
            Hand = P.Hand;
            Player = P;
            History.Add("");
        }

        List<string> History = new List<string>();
        
        
        //the search script used
        //TODO: search/autocomplete on method parameters
        PlayerInvokable[] Search(string input, bool exact) {
            List<PlayerInvokable> output = new List<PlayerInvokable>();
            List<PlayerInvokable> query = Methods;
            for (int i = 0; i < query.Count(); i++) {
                string sample = "";
                //if "exact" switch is not set, only compares the length of the input 
                if (!exact) {
                    try   {sample = query[i].Name.Remove(input.Length);}
                    catch {sample = query[i].Name;}
                }
                if(exact) {sample = query[i].Name;}
                    
                //if method (or its partial) name and input match, add it to output
                if(input.ToLower() == sample.ToLower()){output.Add(query[i]);}
            }
            return output.ToArray();
        }
        
        [PlayerInvokable(Name = "Draw", Description = "Used to reinitalize drawing if and when postponed")]
        public void Draw() {
            if (!Player.First && !Hand.HasDrawn) {
                bool chose = false;
                bool CanUseTrumpCard = Cards.Count >= Constants.MaxCardCount && TrumpCard != null;
                bool CanUseDiscardPile = Score >= Constants.MinMeldScore; 
                int CursorState = 0;
                int Possibilities = 4;
                
                //TODO:(design) do the choosing with \t 
                Console.WriteLine( "Where to draw from?");
                Console.WriteLine( " Deck (Random)");
                if(CanUseTrumpCard){Console.WriteLine($" TrumpCard {TrumpCard.name}");}
                else {Possibilities--;}
                if (CanUseDiscardPile) { Console.WriteLine($" DiscardPile ({DiscardPile[DiscardPile.CardsLeft - 1].name})"); }         //TODO: Refactor this when refactoring DiscardPile
                else {Possibilities--;}
                Console.WriteLine(" Postpone (draw later)");
                //Console.CursorLeft = 0;
                Console.CursorTop -= Possibilities;
                Console.Write(">");
                Console.CursorLeft = 0;
                if (Possibilities == 1) {
                    if(CardsLeft > 0) {Cards.Add(GameDeck.Draw());}
                    else if(TrumpCard != null) {Cards.Add(TrumpCard.Copy()); Program.Game.TrumpCard = null;}
                    chose = true;
                    Console.CursorTop++;
                }
                while (!chose) {
                    Console.CursorVisible = false;
                    ConsoleKey K = Console.ReadKey(true).Key;
                    switch (K) {
                        //TODO: Do something with this, this is ugly
                        case ConsoleKey.UpArrow:
                            if (CursorState > 0) {
                                CursorState--;
                                Console.Write(" ");
                                Console.CursorLeft = 0;
                                Console.CursorTop--;
                                Console.Write(">");
                                Console.CursorLeft = 0;
                            }
                            break;
                        case ConsoleKey.DownArrow:
                            if (CursorState < Possibilities-1) {
                                CursorState++;
                                Console.Write(" ");
                                Console.CursorLeft = 0;
                                Console.CursorTop++;
                                Console.Write(">");
                                Console.CursorLeft = 0;
                            }
                            break;
                        case ConsoleKey.Enter:
                            if (CursorState == 0) {
                                if (CardsLeft > 0) {
                                    Cards.Add(GameDeck.Draw()); chose = true;
                                    Hand.HasDrawn = true;
                                }
                                else if(TrumpCard != null) {
                                    Cards.Add(TrumpCard.Copy()); 
                                    Program.Game.TrumpCard = null; 
                                    chose = true;
                                    Hand.HasDrawn = true;
                                }
                            }

                            //This line seems redundant
                            //if (CursorState == 1 && CanUseTrumpCard && TrumpCard == null) { chose = false; }
                            if (CursorState == 1 && !CanUseDiscardPile && !CanUseTrumpCard) {
                                Hand.HasDrawn = false;
                                chose = true;
                            }

                            else if (CursorState == 1 && CanUseTrumpCard && TrumpCard != null) { 
                                if(Cards.Count >= Constants.MaxCardCount)
                                {
                                    Cards.Add(TrumpCard.Copy()); Program.Game.TrumpCard = null;
                                    for (int i = 0; i<Cards.Count; i++){Cards[i].MustBeUsed = true;}
                                    chose = true;
                                    Hand.HasDrawn = true;
                                }
                            }

                            else if (CursorState == 1 && !CanUseTrumpCard) {
                                Cards.Add(DiscardPile.PopCard()); chose = true;
                                Hand.HasDrawn = true;
                            }

                                
                            if (CursorState == 2 && CanUseTrumpCard && CanUseDiscardPile) {
                                Cards.Add(DiscardPile.PopCard()); chose = true;
                                Hand.HasDrawn = true;
                            }

                            else if (CursorState == 2 && !CanUseDiscardPile) {
                                Hand.HasDrawn = false;
                                chose = true;
                            }

                            else if (CursorState == 2 && !CanUseTrumpCard) {
                                Hand.HasDrawn = false;
                                chose = true;
                            }

                            if (CursorState == 3) {
                                Hand.HasDrawn = false;
                                chose = true;
                            }
                            if(chose){Console.CursorTop += (Possibilities - CursorState);}
                            break;
                    }
                }
                if(Hand.HasDrawn)Console.WriteLine($"New Card: {Cards[^1].name}");
            }
            else if(!Player.First && Hand.HasDrawn){Console.WriteLine($"{Colors.Warning.AnsiFGCode}[WARNING]: Action forbidden: Player has already drawn{Color.Reset}");}
            else {
                Player.First = false;
                Hand.WasFirst = true;
            }
            Console.CursorVisible = true;
        }
        public void StartTurn()
        {
            static bool IsParams(ParameterInfo param) => param.GetCustomAttributes(typeof (ParamArrayAttribute), false).Length > 0;

            Console.Clear();
            Console.Write($"Player {PlayerID}, Round {Round}\n");
#if RELEASE
            Console.WriteLine("Press any key to start the turn!");
            Console.ReadKey(true);
#endif
            Hand.ResetUsingStatus();
            Hand.List();

            Hand.HasDrawn = false;
            Hand.WasFirst = false;
            Draw();
            
            Console.Write(PS1);

            List<char> Input = new List<char>();
            int HistoryIndex = 0;
            bool run = true;

            while (run) {
                ConsoleKeyInfo key = Console.ReadKey(true);
                PlayerInvokable[] matches;
                //checks if pressed key is one of the ones below
                switch (key.Key) {
                    case ConsoleKey.Backspace:
                        //deletes last char of input, and rewrites it with a ' '(space) on the screen
                        if (Input.Count > 0) {
                            Input.RemoveAt(Input.Count()-1);
                            Console.CursorLeft--;
                            Console.Write(" ");
                            Console.CursorLeft--;
                        }
                        break;
                    case ConsoleKey.Tab:
                        //searches if there is a match to the input, if there is only one, autocompletes
                        matches = Search(new string(Input.ToArray()), false);
                        if (matches.Length == 1) {
                            Input = matches[0].Name.ToList();
                            Console.CursorLeft = 2;
                            Console.Write(new string(Input.ToArray()));
                        }
                        //TODO: output, for when there is more/less matches
                        break;
                    case ConsoleKey.UpArrow:
                        HistoryIndex++;
                        try {
                            Input = History[HistoryIndex].ToList();
                            while(Console.CursorLeft != 0){
                                Console.CursorLeft--;
                                Console.Write(" ");
                                Console.CursorLeft--;
                            }
                            Console.Write(" ");
                            Console.CursorLeft--;
                            Console.Write(PS1 + History[HistoryIndex]);
                        }
                        catch{Console.Write("\a"); HistoryIndex = History.Count-1;}

                        break;
                    case ConsoleKey.DownArrow:
                        HistoryIndex--;
                        try {
                            Input = History[HistoryIndex].ToList();
                            while(Console.CursorLeft != 0){
                                Console.CursorLeft--;
                                Console.Write(" ");
                                Console.CursorLeft--;
                            }
                            Console.Write(" ");
                            Console.CursorLeft--;
                            Console.Write(PS1 + History[HistoryIndex]);
                        }
                        catch{Console.Write("\a"); HistoryIndex = 0;}

                        break;
                    case ConsoleKey.Enter:
                        if(!Input.Any()) {break;}
                        History.Insert(1, new string(Input.ToArray()));
                        HistoryIndex = 0;
                        
                        //splits up the string 
                        //should not give back null, as it is checked for a few lines above
                        string[] splitInput = History[1].Split(' ');
                        string command = splitInput[0];
                        
                        List<string> b = splitInput.ToList();
                        //removes first object in the split up input (the MethodName) -> leftover = parameters
                        b.RemoveAt(0);
                        string[] RawArgs = b.ToArray();
                        
                        //searches if there is (only) one exact mach for the given input, if there is, invokes it
                        matches = Search(command, true);
                        if (matches.Length == 1) {
                            Console.Write("\n");
                            
                            //makes reading a bit easier (otherwise unnecessary line)           I sometimes hate my old self for dumbass shit like this.... I mean, who would've thought this would turn out to be this big??
                            PlayerInvokable match = matches[0];
                            //---------------------------------
                            //Creates an array with the length of the method(match)'s parameters 
                            object[] Args = new object[match.Params.Length];
                            int j = 0;
                            //for (int i = 0; i < Args.Length; i++) {
                            foreach (ParameterInfo CurrentParameter in match.Params){
                                bool completed = false;

                                //if the parameter's type is in AutocompleteArgs, autocomplete  
                                if (Array.IndexOf(AutoCompleteArgs, CurrentParameter.ParameterType) != -1)
                                {
                                    Type t = CurrentParameter.ParameterType;
                                    
                                    //IMPORTANT!!!!:
                                    //If you wish to add new autocompleted variables to the array above, DON'T forget to add functionality here!!
                                    
                                    //----------------------------!!---------------------------------------------
                                    if(t == typeof(Hand))            {Args[CurrentParameter.Position] = Hand;      completed = true;}
                                    if(t == typeof(Card))            {Args[CurrentParameter.Position] = TrumpCard; completed = true;}
                                    if(t == typeof(List<Meld>))      {Args[CurrentParameter.Position] = Melds;     completed = true;}
                                    if(t == typeof(int) && CurrentParameter.Name == "Score")                         {Args[CurrentParameter.Position] = Score;       completed = true;}
                                    
                                    if(t == typeof(Deck) && CurrentParameter.Name == "DiscardPile" /* HACK */)       {Args[CurrentParameter.Position] = DiscardPile; completed = true;}
                                    else if(t == typeof(Deck))       {Args[CurrentParameter.Position] = GameDeck;  completed = true;}
                                    
                                    if(t == typeof(Player))          {Args[CurrentParameter.Position] = Player;    completed = true;}
                                    //----------------------------!!---------------------------------------------
                                }
                                if(!completed) {
                                    if (IsParams(CurrentParameter)) {
                                        //TODO: global params [] parameter conversion 
                                        Type T = CurrentParameter.ParameterType.GetElementType();
                                        List<object> list = new List<object>();
                                        
                                        for (int k = j; k < RawArgs.Length; k++) {
                                            list.Add(Convert.ChangeType(RawArgs[k], T));
                                        }
                                        Args[CurrentParameter.Position] = list.ToArray();
                                        j = RawArgs.Length;
                                    }

                                    if (RawArgs.Length > j && RawArgs[j] != null) {
                                        string currentRawArgument = RawArgs[j];
                                        try{
                                            if (CurrentParameter.ParameterType.IsEnum) { Args[CurrentParameter.Position] = Enum.Parse(CurrentParameter.ParameterType, currentRawArgument, true);}
                                            else {Args[CurrentParameter.Position] = Convert.ChangeType(currentRawArgument, CurrentParameter.ParameterType);}
                                            j++;
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
                            object ReturnValue = null;
                            try {
                                if (match.Info.DeclaringType == typeof(Hand))       ReturnValue = match.Invoke(Args.ToList(), instance: Hand);
                                else if (match.Info.DeclaringType == typeof(Shell)) ReturnValue = match.Invoke(Args.ToList(), instance: this);
                                else ReturnValue = match.Invoke(Args.ToList());
                            }
                            catch (Exception E) {
                                failed = true;
                                Console.WriteLine($"{Colors.Error.AnsiFGCode}[ERROR]: {E.Message}{Color.Reset}");
#if DEBUG
                                Console.WriteLine($"\n\nStack trace:\n{E.StackTrace}");
#endif
                                Console.ReadKey(true); // [Dit05] The console would be immediately cleared after printing the exception. Good thing this didn't cause anyone problems, right?
                                                       // [MKKrisz] It actually does not clear the console, it just creates a new prompt so you can continue, so this feature is only an attention-grabber
                            }
                            //Checks if the invoked function has the "TurnEnder" attribute, if yes, exits this loop, and thus, ending the player's turn
                            if (!failed && match.Info.GetCustomAttributes().OfType<TurnEnder>().Any()) {
                                if(ReturnValue is true){run = false;}
                                if(ReturnValue == null){run = false;}
                            }
                            
                            //displays a new prompt, discards last input
                            Meld.UpdateAllMeldStatus(Player);
                            Console.Write(PS1);
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

        [PlayerInvokable(Name = "Help", Description = "Displays this message. If given an argument, displays required params")]
        public void Help(string s = null) {
            bool success = false;
            if (s != null) {
                PlayerInvokable[] matches = Search(s, exact: true);
                if (matches.Length == 1) {
                    success = true;
                    PlayerInvokable match = matches[0];
                    Console.WriteLine($"{match.Name}:\t{match.Description}");
                    if(match.Params.Length != 0){Console.WriteLine("Parameters:");}
                    for (int i = 0; i < match.Params.Length; i++) {
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
                for (int i = 0; i < Methods.Count; i++)
                {
                    Console.WriteLine($"{Methods[i].Name}\t\t{Methods[i].Description}");
                }
            }
        }

        [PlayerInvokable(Name = "Clear", Description = "Clears the console")]
        public void Clear() => Console.Clear();
        
        [PlayerInvokable(Name = "Exit", Description = "Exits the game")]
        [TurnEnder]
        public void Exit(bool save = false)
        {
            if(save == true){Save_Load.Save(Program.Game);}

            Program.Game.Run = false;
            Program.Run = false;
        }

        [PlayerInvokable(Name = "Reload",
            Description = "Reloads the last saved gamestate, usually sets you back to the start of your turn")]
        [TurnEnder]
        public void Reload() {
            Save_Load.Load(Save_Load.LastSavePath);
            //Program.Game.CurrentPlayerId--;
        }

        [TurnEnder]
        public static void EndTurn(){}
        
        
        [PlayerInvokable(Name = "Melds", Description = "Alias for ListMelds")]
        public void LMelds() => ListMelds();
        [PlayerInvokable(Name = "ListMelds", Description = "Lists all melds")]
        public void ListMelds()
        {
            for (int i = 0; i < Melds.Count; i++)
            {
                string BG = Color.Reset;
                string FG = Colors.Text.AnsiFGCode;
                if(Melds[i].PlayerID == PlayerID){BG = Colors.Selected.AnsiBGCode;}
                if(!Melds[i].CanBeAddedTo && Melds[i].PlayerID != PlayerID){FG = Colors.Ignorable.AnsiFGCode;}
                Console.Write($"{BG}{FG}{i}: ID: {Melds[i].PlayerID}\t");
                for (int j = 0; j < Melds[i].Cards.Count; j++)
                {
                    Console.Write($"{Constants.Suit[(int)Melds[i].Cards[j].Suit]}{BG}{FG}{Constants.Value[Melds[i].Cards[j].Value]} ");
                }
                Console.Write($"{Color.Reset}\n");
            }
        }

        [PlayerInvokable(Name = "Info", Description = "Displays your hand, and the table")]
        public void Info()
        {
            Console.WriteLine($"{PlayerID}'s hand:");
            Hand.Ls();
            Console.WriteLine("Melds:");
            LMelds();
        }

        [PlayerInvokable(Name = "SI", Description = "Sorts with default sorter and display info")]
        public void SortInfo() {
            Hand.Sort();
            Info();
        }
    }
}
