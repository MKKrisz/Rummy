using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Rummy;


namespace RummyServer {
    
    public class Server {
        public TcpListener JoinListener;
        public List<TcpClient> Players = new List<TcpClient>();
        public Dictionary<TcpClient, string> Names = new Dictionary<TcpClient, string>();
        public Dictionary<string, TcpClient> NamesReversed = new Dictionary<string, TcpClient>();
        public int BufferSize = 2 * 1024;

        public List<string> MessageQueue = new List<string>();

        public readonly int Port;
        public string Name;
        public Rummy.Game Game;
        public int TargetPlayerCount;
        public bool IsInGame { get; private set; }
        public bool Running { get; private set; }
        
        private bool SentTurnStartRequest = false;
        private bool ReceivedGamestateTransitRequest = false;

        public Server(string Name, int TargetPlayerCount = 4, int Port = 18274) {
            this.Port = Port;
            this.Name = Name;
            this.TargetPlayerCount = TargetPlayerCount;
            IsInGame = false;
            Running = false;
            
            JoinListener = new TcpListener(IPAddress.Any, Port);
        }

        public void Stop() {
            Console.WriteLine("Shutting down!");
            Rummy.Save_Load.Save(Game);
            IsInGame = false;
            Running = false;
        }

        public async void Start() {
            Console.WriteLine($"Starting Server using Port:{Port}");
            JoinListener.Start();
            Running = true;
            List<Task> ConnectionHandlers = new List<Task>();
            Console.WriteLine("Waiting for players...");

            List<Task> shid = new List<Task>();
            while (Running) {
                HandleDisconnects();
                if (!IsInGame) {
                    if (JoinListener.Pending()) {
                        ConnectionHandlers.Add(NewConnectionHandler());
                    }

                    if (Players.Count >= TargetPlayerCount) {
                        IsInGame = true;
                        Console.WriteLine("Starting Game");
                        JoinListener.Stop();
                        Game = new Game(Players.Count);
                        string GameState = Save_Load.Serialize(Game);
                        /*Console.WriteLine("Sending GameState to all players");
                        foreach (TcpClient Player in Players) {
                            await Send(Player, GameState);
                        }*/
                    }
                }

                if (IsInGame) {
                    if (!SentTurnStartRequest) {
                        Console.WriteLine($"Current Player: {Names[Players[Game.CurrentPlayerId]]}");
                        await Send(Players[Game.CurrentPlayerId], "Your turn");
                        Console.WriteLine("Turn request sent");
                        SentTurnStartRequest = true;
                    }
                }

                for (int i = 0; i < Players.Count; i++) {
                    shid.Add(ProcessIncomingMessage(await Receive(Players[i]), Players[i]));
                }

                shid.Add(TransmitMessages());
            }
        }

        public async Task NewConnectionHandler() {
            TcpClient Client = await JoinListener.AcceptTcpClientAsync();
            Console.WriteLine($"Connecting: {Client.Client.RemoteEndPoint}");
            NetworkStream Stream = Client.GetStream();

            bool Verified = false;

            Client.ReceiveBufferSize = BufferSize;
            Client.SendBufferSize = BufferSize;

            byte[] MsgBuffer = new byte[BufferSize];
            int BytesRead = await Stream.ReadAsync(MsgBuffer, 0, MsgBuffer.Length);
            while(BytesRead<=0){BytesRead = await Stream.ReadAsync(MsgBuffer, 0, MsgBuffer.Length);}
            string Msg = Encoding.UTF8.GetString(MsgBuffer);
            if (Msg.StartsWith("player:")) {
                string name = Msg.Substring(Msg.IndexOf(':')+1);
                if (name != "" && !Names.ContainsValue(name) && Players.Count()<6) {
                    Verified = true;
                    Names.Add(Client, name);
                    NamesReversed.Add(name, Client);
                    Players.Add(Client);
                    Console.WriteLine($"{name} joined!");
                    for (int i = 0; i < Players.Count; i++) {
                        await Send(Players[i], $":newplayer:{name}");
                    }
                }
                else{_cleanupClient(Client);}
            }
        }

        public void HandleDisconnects() {
            for(int i = 0; i<Players.Count; i++) {
                TcpClient c = Players[i];
                if (IsDead(c)) {
                    Console.Write($"{Names[c]} has unexpectedly disconnected");
                    if (Players.Count == Constants.MinPlayerCount && IsInGame) {
                        Console.Write(", not enough players left, ending game\n");
                        IsInGame = false;
                        MessageQueue.Add(":endofgame:");
                    }
                    else if(IsInGame){
                        Console.Write(", addinng their cards to the deck\n");
                        Game.Deck.AddCards(Game.Players[i].Cards);
                        for (int j = 0; j < Game.Melds.Count; j++) {
                            if(Game.Melds[j].PlayerID == i){
                                Game.Deck.AddCards(Game.Melds[j].Cards);
                                Game.Melds.RemoveAt(j);
                                j--;
                            }
                        }

                        List<Player> buffer = Game.Players.ToList();
                        buffer.RemoveAt(i);
                        Game.Players = buffer.ToArray();
                    }
                    Players[i].Close();
                    Players.RemoveAt(i);
                }
            }
        }

        private bool IsDead(TcpClient c) {
            try {
                Socket s = c.Client;
                return s.Poll(1 * 1000, SelectMode.SelectRead) && c.Available == 0; // && s.Available == 0;
            }
            catch (SocketException) {
                return true;
            }
        }
        
        
        public async Task Send(TcpClient Client, string Message) {
            byte[] MsgBytes = Encoding.UTF8.GetBytes(Message);
            await Client.GetStream().WriteAsync(MsgBytes);
        }

        public async Task ProcessIncomingMessage(string message, TcpClient Client) {      //ex.:      :msg:everyone:Szervusztok!
            if (message.StartsWith(":msg:")) {
                string proc = message.Remove(0, 5);
                string tosend = ":msg:";
                if (proc.Contains(':')) {
                    string receiver = proc.Split(':')[0];
                    tosend += Names[Client] + ":" + proc.Remove(0, proc.Split(':')[0].Length + 1);
                    if (receiver == "everyone") {
                        MessageQueue.Add(tosend);
                    }
                    else {
                        Send(NamesReversed[receiver], tosend);
                    }
                }
            }
            if (message == "OK, Gimme state" && IsInGame && !ReceivedGamestateTransitRequest) {
                Console.WriteLine("Response received, sending current GameState");
                ReceivedGamestateTransitRequest = true;
                string GameState = Save_Load.Serialize(Game);
                await Send(Players[Game.CurrentPlayerId], GameState);
            }
            if (message.StartsWith("#General") && IsInGame && ReceivedGamestateTransitRequest && SentTurnStartRequest) {
                Console.WriteLine("Player has finished their turn, moving on to next player");
                Game = Save_Load.Load(message.Split('\n'));
                Game.CurrentPlayerId++;
                SentTurnStartRequest = false;
                ReceivedGamestateTransitRequest = false;
            }
        }

        private async Task TransmitMessages() {
            for (int i = 0; i < MessageQueue.Count; i++) {
                for (int j = 0; j < Players.Count; j++) {
                    await Send(Players[j], MessageQueue[i]);
                }
            }
            MessageQueue.Clear();
        }
        public async Task<string> Receive(TcpClient Client) {
            try {
                int Length = Client.Available;
                if (Length > 0) {
                    byte[] buffer = new byte[Length];
                    await Client.GetStream().ReadAsync(buffer, 0, Length);
                    string msg = Encoding.UTF8.GetString(buffer);
                    return msg;
                }
            }
            catch (Exception e) {
                Console.WriteLine($"Error: {e.Message}");
            }
            return "";
        }
        private static void _cleanupClient(TcpClient client)
        {
            client.GetStream().Close();     // Close network stream
            client.Close();                 // Close client
        }
    }
    
}