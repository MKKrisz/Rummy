using System.IO;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Rummy {
    public static class Save_Load{
        private static string Folder => Constants.SavePath;
        public static string LastSavePath;

        public static string Serialize(Game G) {
            string output = "";
            output += "#General\n";
            output += $"{G.Round}: {G.Players.Length}/{G.CurrentPlayerId}\n";
            output += $"{Constants.ColorlessSuit[(int)G.TrumpCard.Suit]}{G.TrumpCard.Value}\n";
            output += SerializeCardCollection(G.Deck.cards) + "\n";
            output += SerializeCardCollection(G.DiscardPile.cards) + "\n";
            output += "#Players\n";
            for (int i = 0; i<G.Players.Length; i++) {
                Player P = G.Players[i];
                output += $"{i}: {SerializeCardCollection(P.Cards)}\n";
            }
            if(G.Melds.Count>0){output += "#Melds\n";}
            for (int i = 0; i < G.Melds.Count; i++) {
                Meld M = G.Melds[i];
                output += $"{M.PlayerID}, {M.GetType().Name}, {SerializeCardCollection(M.Cards)}\n";
            }
            output += "#";
            return output;
        }
        public static void Save(Game G) {
            if(!Directory.Exists(Folder)){Directory.CreateDirectory(Folder);}
            string UniqueID = DateTime.Now.ToShortDateString() +" "+ DateTime.Now.ToLongTimeString();
            string SavePath = Folder + Path.DirectorySeparatorChar + UniqueID + ".gst";
            StreamWriter sw = File.CreateText(SavePath);
            sw.WriteLine(Serialize(G));
            sw.Close();
            if(File.Exists(LastSavePath))File.Delete(LastSavePath);
            LastSavePath = SavePath;
        }

        public static Game Load(string[] SerializedGame) {
            Game output = null;
            for (int i = 0; i < SerializedGame.Length; i++) {
                if (SerializedGame[i] == "#General") {
                    if(output == null){output = new Game();}
                    i++;
                    string[] split = SerializedGame[i].Split(": ");
                    output.Round = Convert.ToInt32(split[0]);
                    split = split[1].Split('/');
                    output.Players = new Player[Convert.ToInt32(split[0])];
                    output.CurrentPlayerId = Convert.ToInt32(split[1]);
                    i++;
                    output.TrumpCard = DeserializeCardCollection(SerializedGame[i])[0];
                    i++;
                    output.Deck = new Deck(false);
                    output.Deck.cards.AddRange(DeserializeCardCollection(SerializedGame[i]));
                    //output.Deck.cards.Reverse();
                    i++;
                    if (SerializedGame[i] != "") {
                        output.DiscardPile.Clear();
                        output.DiscardPile.AddCards(DeserializeCardCollection(SerializedGame[i]));
                    }
                }

                if (SerializedGame[i] == "#Players") {
                    i++;
                    int j;
                    for (j = i; j < i + output.Players.Length; j++) {
                        string[] split = SerializedGame[j].Split(": ");
                        bool IsFirst = output.Round == 0 && j == 0;
                        int id = Convert.ToInt32(split[0]);
                        output.Players[j-i] = new Player{First = IsFirst, ID = id, Hand = new Hand(DeserializeCardCollection(split[1]), id)};
                        output.Players[j-i].SH = new Shell(output.Players[j-i]);
                    }
                    i = j-1;
                }

                if (SerializedGame[i] == "#Melds") {
                    i++;
                    int j = i;
                    output.Melds.Clear();
                    while (SerializedGame[j][0] != '#') {
                        string[] split = SerializedGame[j].Split(", ");
                        int pid = Convert.ToInt32(split[0]);
                        Meld M = null;
                        if (split[1] == typeof(RunMeld).Name) {
                            RunMeld rm = new RunMeld();
                            rm.Cards = new List<Card>();
                            rm.PlayerID = pid;
                            rm.Cards.AddRange(DeserializeCardCollection(split[2]));
                            rm.MeldSuit = rm.Cards.Find(x => !x.IsJoker).Suit;
                            M = rm;
                        }
                        if (split[1] == typeof(SetMeld).Name) {
                            SetMeld sm = new SetMeld();
                            sm.Cards = new List<Card>();
                            sm.PlayerID = pid;
                            sm.Cards.AddRange(DeserializeCardCollection(split[2]));
                            sm.MeldValue = sm.Cards.Find(x => !x.IsJoker).Value;
                            M = sm;
                        }
                        if(M != null){output.Melds.Add(M);}
                        if(M != null){output.Players[pid].Melds.Add(M);}
                        j++;
                    }
                    j--;
                }
            }
            return output;
        }
        public static void Load(string Path) {
            string[] raw = File.ReadAllLines(Path);
            LastSavePath = Path;
            Program.Game = Load(raw);
        }
        
        public static string SerializeCardCollection(IEnumerable<Card> Collection) {
            string output = "";
            Card[] arr = Collection.ToArray();
            for (int i = 0; i< arr.Length; i++) {
                Card c = arr[i];
                output += $"{Constants.ColorlessSuit[(int)c.Suit]}{c.Value}";
            }
            return output;
        }

        public static Card[] DeserializeCardCollection(string serial) {
            List<Card> Cards = new List<Card>();
            char[] chars = serial.ToCharArray();
            for (int i = 0; i < chars.Length; i++) {
                Suit S = (Suit)Array.FindIndex(Constants.ColorlessSuit, x => x == chars[i]);
                i++;
                string id = "";
                while (i<chars.Length && Array.IndexOf(Constants.ColorlessSuit, chars[i]) == -1) {
                    id += chars[i];
                    i++;
                }
                i--;
                int Value = Convert.ToInt32(id);
                Card C = new Card(S, Value);
                Cards.Add(C);
            }
            return Cards.ToArray();
        }
    }
}