using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Rummy;


namespace RummyServer {
    
    public class Server {
        public TcpListener JoinListener;
        public List<TcpClient> Players = new List<TcpClient>();
        public Dictionary<TcpClient, string> Names = new Dictionary<TcpClient, string>();
        public int BufferSize = 2 * 1024;

        public readonly int Port;
        public string Name;
        public Rummy.Game Game;
        public int TargetPlayerCount;
        public bool IsInGame { get; private set; }
        public bool Running { get; private set; }
        

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

            bool SentTurnStartRequest = false;
            bool ReceivedGamestateTransitRequest = false;

            while (Running) {
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

                    if (!ReceivedGamestateTransitRequest) {
                        string request = await Receive(Players[Game.CurrentPlayerId]);
                        if (request == "OK, Gimme state") {
                            Console.WriteLine("Response received, sending current GameState");
                            ReceivedGamestateTransitRequest = true;
                            string GameState = Save_Load.Serialize(Game);
                            await Send(Players[Game.CurrentPlayerId], GameState);
                        }
                    }

                    if (SentTurnStartRequest && ReceivedGamestateTransitRequest) {
                        string GameState = await Receive(Players[Game.CurrentPlayerId]);
                        if (GameState.StartsWith("#General")) {
                            Console.WriteLine("Player has finished their turn, moving on to next player");
                            Game = Save_Load.Load(GameState.Split('\n'));
                            Game.CurrentPlayerId++;
                            SentTurnStartRequest = false;
                            ReceivedGamestateTransitRequest = false;
                        }
                    }
                }
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
                    Players.Add(Client);
                    Console.WriteLine($"{name} joined!");
                }
                else{_cleanupClient(Client);}
            }
        }

        public async Task Send(TcpClient Client, string Message) {
            byte[] MsgBytes = Encoding.UTF8.GetBytes(Message);
            await Client.GetStream().WriteAsync(MsgBytes);
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