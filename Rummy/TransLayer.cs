using System.Collections.Generic;
using System.IO;

namespace Rummy {
    public enum Lang{English, Magyar, Espanol}
    public class TransLayer {
        public Dictionary<string, string> Translator = new Dictionary<string, string>();
        public Dictionary<string, string> Rev = new Dictionary<string, string>();
        public Lang Language;

        public string Translate(string text) {
            if (Language == Lang.English) {
                return text;
            }
            if (Translator.ContainsKey(text)) {
                return Translator[text];
            }
            return text;
        }

        public string Reverse(string text) {
            if (Language == Lang.English) {
                return text;
            }
            if (Rev.ContainsKey(text)) {
                return Rev[text];
            }
            return text;
        }
        
        public TransLayer(Lang l) {
            Language = l;
            string LoadPath = "Lang_";
            if(Language == Lang.English){LoadPath += "EN";}
            if(Language == Lang.Magyar) {LoadPath += "HU";}
            if(Language == Lang.Espanol){LoadPath += "ES";}

            LoadPath += ".lang";

            if(l != Lang.English){
                string[] LangFile = File.ReadAllLines(LoadPath);
                foreach (string Line in LangFile) {
                    if (!Line.StartsWith('#') && Line.Length != 0) {
                        string OG = Line.Split(":=")[0];
                        string Trans = Line.Split(":=")[1];
                        Translator.Add(OG, Trans);
                        Rev.Add(Trans, OG);
                    }
                }
            }
        }
    }
}