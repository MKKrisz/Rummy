namespace RummyServer {
    public class Program {
        private static Server Server;
        static void Main() {
            Server = new Server("Test", Rummy.Constants.MinPlayerCount);
            Server.Start();
        }
    }
}