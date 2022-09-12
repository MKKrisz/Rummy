using System.Xml.Serialization;
using System.IO;
using System;

namespace Rummy
{
    public static class Save_Load
    {
        public static XmlSerializer Serializer = new XmlSerializer(typeof(Game));
        private static string Folder = @"SaveData";
        
        public static void Save(Game g)
        {
            string UniqueID = DateTime.Now.ToShortDateString() + DateTime.Now.ToLongTimeString();
            string SavePath = Folder + UniqueID + ".gst";
            FileStream file = File.Create(SavePath);
                
            Serializer.Serialize(file, g);
            file.Close();
        }
    }
}