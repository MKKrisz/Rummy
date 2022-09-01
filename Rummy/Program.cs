using System;

namespace Rummy
{
    class Program
    {
        public static string[] Value = { "Joker", "Ace", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        public static Random r;

        public static Game Game;
        static void Main(string[] args)
        {
            r = new Random();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
    }
}