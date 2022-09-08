using System;
using TextColor;

namespace Rummy
{
    class Program
    {
        public static readonly string[] Value = { "Joker", "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        public static readonly string[] Suit = { "♠", "\u001b[38;2;255;0;0m♥\u001b[m", "♣", "\u001b[38;2;255;0;0m♦\u001b[m"};
        public static readonly Random r = new Random();

        public static Game Game;
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool run = true;
            while (run)
            {
                Console.Write("How Many Players?\n> ");
                int Players;
                string input = Console.ReadLine();
                bool valid = Int32.TryParse(input, out Players);
                if (valid) {Game = new Game(Players); Game.Loop();}
                if (!valid)
                {
                    if(input.ToLower() == "exit") {Console.Write("Exiting..."); run = false;} 
                    else Console.WriteLine($"{Colors.Warning.AnsiFGCode}[WARNING]: Not An Interger{Color.Reset}");
                    
                }
            }
        }
    }
}