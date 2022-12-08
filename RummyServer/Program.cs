using System.Xml.Schema;

namespace RummyServer {
    public class Program {
        private static Server Server;
        static void Main() {
            Server = new Server("Test", 3);
            Server.Start();
        }
    }
}