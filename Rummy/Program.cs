using System;

namespace Rummy
{
    class Program
    {
        public static string[] Value = { "Joker", "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        public static readonly Random r = new Random();

        public static Game Game;
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            PlayerInvokableContainer.Init();
        }
    }
}
