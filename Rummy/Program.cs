using System;
using System.IO;
using Rummy.TextColor;

namespace Rummy
{
    static class Constants {
        public static readonly string[] Value = { "Joker", "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        public static readonly string[] Suit = { "♠", $"{Colors.Red.AnsiFGCode}♥{Color.Reset}", "♣", $"{Colors.Red.AnsiFGCode}♦{Color.Reset}"};
        public static readonly char[] ColorlessSuit = { '♠', '♥', '♣', '♦' };
        public static Random Random;
        public static bool AutoSave = true;

        public static readonly string SavePath = /*Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                                 Path.DirectorySeparatorChar +  "Rummy" +
                                                 Path.DirectorySeparatorChar + */"SaveData";

        //The amount of cards the player receives at the start of the game (first one gets +1)
        public static int MaxCardCount = 14;
        public static int MinMeldScore = 51;
    }
    class Program {
        public static bool Run = true;

        public static Game Game;
        static void Main(string[] args) {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool NewGame = false;
            bool LoadGame = false; 
            int CursorPos = 0;
            while (Run) {
                if (NewGame && !LoadGame) {
                    Console.Write("How Many Players?\n> ");
                    int Players;
                    string input = Console.ReadLine();
                    bool valid = Int32.TryParse(input, out Players) && (Players is < 6 and > 0);
                    if (valid) {
                        Game = new Game(Players);
                        Game.Loop();
                    }

                    if (!valid && input != null) {
                        if (input.ToLower() == "exit") {
                            Console.Write("Exiting...");
                            Run = false;
                        }
                        else Console.WriteLine($"{Colors.Warning.AnsiFGCode}[WARNING]: Not An Integer{Color.Reset}");
                    }
                }

                if (LoadGame && !NewGame) {
                    if (Directory.Exists(Constants.SavePath)) {
                        Directory.CreateDirectory(Constants.SavePath);
                    }
                    string[] saves = Directory.GetFiles(Constants.SavePath);
                    string LoadFile = saves[0];
                    for(int i = 1; i<saves.Length; i++) {
                        if (File.GetLastWriteTime(saves[i]) > File.GetLastWriteTime(LoadFile)) {
                            LoadFile = saves[i];
                        }
                    }
                    Constants.Random = new Random();
                    Save_Load.Load(LoadFile);
                    Game.Loop();
                    LoadGame = false;
                }

                if (!NewGame && !LoadGame) {
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("Rummy");
                    Console.WriteLine(" New Game");
                    Console.WriteLine(" Load Game");
                    Console.SetCursorPosition(0, 1 + CursorPos);
                    Console.Write('>');
                    switch (Console.ReadKey(true).Key) {
                        case ConsoleKey.DownArrow:
                            if (CursorPos < 1) {
                                CursorPos++;
                            }
                            break;
                        case ConsoleKey.UpArrow:
                            if (CursorPos > 0) {
                                CursorPos--;
                            }
                            break;
                        case ConsoleKey.Enter:
                            if (CursorPos == 0) { NewGame = true;}
                            if(CursorPos == 1){LoadGame = true;}
                            break;
                        case ConsoleKey.Escape:
                            Run = false;
                            break;
                    }
                }
            }
        }
    }
}
