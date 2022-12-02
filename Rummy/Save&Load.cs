using System.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rummy {
    public static class Save_Load{
        private static string Folder => Constants.SavePath;
        public static string LastSavePath;
        
        public static void Save(Game G) {
            if(!Directory.Exists(Folder)){Directory.CreateDirectory(Folder);}
            string UniqueID = DateTime.Now.ToShortDateString() +" "+ DateTime.Now.ToLongTimeString();
            string SavePath = Folder + Path.DirectorySeparatorChar + UniqueID + ".gst";
            LastSavePath = SavePath;
            StreamWriter sw = File.CreateText(SavePath);
            //sw.;
            sw.WriteLine("#General");
            sw.WriteLine($"{G.Round}: {G.Players.Length}/{G.CurrentPlayerId}");
            sw.WriteLine($"{Constants.ColorlessSuit[(int)G.TrumpCard.Suit]}{G.TrumpCard.Value}");
            sw.WriteLine(SerializeCardCollection(G.Deck.cards));
            sw.WriteLine(SerializeCardCollection(G.DiscardPile.cards));
            sw.WriteLine("#Players");
            for (int i = 0; i<G.Players.Length; i++) {
                Player P = G.Players[i];
                sw.WriteLine($"{i}: {SerializeCardCollection(P.Cards)}");
            }
            if(G.Melds.Count>0){sw.WriteLine("#Melds");}
            for (int i = 0; i < G.Melds.Count; i++) {
                Meld M = G.Melds[i];
                sw.WriteLine($"{M.PlayerID}, {M.GetType().Name}, {SerializeCardCollection(M.Cards)}");
            }
            sw.WriteLine("#");

            sw.Close();
        }

        public static void Load(string Path) {
            string[] raw = File.ReadAllLines(Path);
            LastSavePath = Path;
            for (int i = 0; i < raw.Length; i++) {
                if (raw[i] == "#General") {
                    if(Program.Game == null){Program.Game = new Game();}
                    i++;
                    string[] split = raw[i].Split(": ");
                    Program.Game.Round = Convert.ToInt32(split[0]);
                    split = split[1].Split('/');
                    Program.Game.Players = new Player[Convert.ToInt32(split[0])];
                    Program.Game.CurrentPlayerId = Convert.ToInt32(split[1]);
                    i++;
                    Program.Game.TrumpCard = DeserializeCardCollection(raw[i])[0];
                    i++;
                    Program.Game.Deck = new Deck(false);
                    Program.Game.Deck.AddCards(DeserializeCardCollection(raw[i]));
                    i++;
                    if (raw[i] != "") {
                        Program.Game.DiscardPile.AddCards(DeserializeCardCollection(raw[i]));
                    }
                }

                if (raw[i] == "#Players") {
                    i++;
                    int j;
                    for (j = i; j < i + Program.Game.Players.Length; j++) {
                        string[] split = raw[j].Split(": ");
                        bool IsFirst = Program.Game.Round == 0 && j == 0;
                        int id = Convert.ToInt32(split[0]);
                        Program.Game.Players[j-i] = new Player{First = IsFirst, ID = id, Hand = new Hand(DeserializeCardCollection(split[1]), id)};
                        Program.Game.Players[j-i].SH = new Shell(Program.Game.Players[j-i]);
                    }
                    i = j-1;
                }

                if (raw[i] == "#Melds") {
                    i++;
                    int j = i;
                    while (raw[j][0] != '#') {
                        string[] split = raw[j].Split(", ");
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
                        if(M != null){Program.Game.Melds.Add(M);}
                        if(M != null){Program.Game.Players[Array.FindIndex(Program.Game.Players, x => x.ID == M.PlayerID)].Melds.Add(M);}
                        j++;
                    }
                    j--;
                }
            }
        }
        
        public static string SerializeCardCollection(IEnumerable<Card> Collection) {
            string output = "";
            foreach (Card c in Collection) {
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