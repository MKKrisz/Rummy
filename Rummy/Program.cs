using System;
using Rummy.TextColor;

namespace Rummy
{
    class Program
    {
        public static readonly string[] Value = { "Joker", "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        public static readonly string[] Suit = { "♠", $"{Colors.Red.AnsiFGCode}♥{Color.Reset}", "♣", $"{Colors.Red.AnsiFGCode}♦{Color.Reset}"};
        public static readonly Random r = new Random();
        public static bool Run = true;

        public static Game Game;
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            while (Run)
            {
                Console.Write("How Many Players?\n> ");
                int Players;
                string input = Console.ReadLine();
                bool valid = Int32.TryParse(input, out Players) && Players < 4096 && Players > 0;
                if (valid) {Game = new Game(Players); Game.Loop();}
                if (!valid)
                {
                    if(input.ToLower() == "exit") {Console.Write("Exiting..."); Run = false;} 
                    else Console.WriteLine($"{Colors.Warning.AnsiFGCode}[WARNING]: Not An Interger{Color.Reset}");
                    
                }
            }
        }
    }
}
