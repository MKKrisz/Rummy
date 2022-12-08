using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rummy {
    public class Client {
        private TcpClient client;
        private NetworkStream stream;
        public int Port;
        public string Address;
        public string Name;
        public bool IsMyTurn;
        public bool Running { get; private set; }

        public Client(string name, string ServerAddress) {
            client = new TcpClient();
            Running = false;
            Name = name;
            Address = ServerAddress.Split(':')[0];
            Port = (ServerAddress.Split(':').Length > 0 && ServerAddress.Split(':')[1] != "")
                ? Convert.ToInt32(ServerAddress.Split(':')[1])
                : 18274;
            
        }

        public void Connect() {
            try{client.Connect(Address, Port);}
            catch(Exception e){Console.WriteLine(e.Message);}

            if (client.Connected) {
                Console.WriteLine($"Successfully connected to {Address}");
                Running = true;
                stream = client.GetStream();
                Send($"player:{Name}");
            }
            else{CleanupResources();}
        }

        public void Disconnect() {
            Console.WriteLine("Disconnecting from the server...");
            Running = false;
            Send("Goodbye, nibba");
        }

        public void Run() {
            Running = true;
            Constants.AutoSave = false;
            List<Task> Tasks = new List<Task>();
            while (Running) {
                Tasks.Add(ReceiveAndProcess());
                Thread.Sleep(10);
            }
        }

        private async Task Send(string msg) {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        private async Task ReceiveAndProcess() {
            if (client.Available > 0) {
                byte[] buffer = new byte[client.Available];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                string msg = Encoding.UTF8.GetString(buffer);
                if (msg == "Your turn" ) {
                    IsMyTurn = true;
                    Console.WriteLine("Received turn request, sending response");
                    Send("OK, Gimme state");
                }
                else if (msg.StartsWith("#General")) {
                    Console.WriteLine("Received Gamestate, updating");
                    Program.Game = Save_Load.Load(msg.Split('\n'));
                    if (IsMyTurn) {
                        Console.WriteLine("Starting turn");
                        if (Program.Game.CurrentPlayerId == 0 && Program.Game.Round == 0) {
                            Program.Game.Players[0].First = true;
                        }
                        Program.Game.Players[Program.Game.CurrentPlayerId].SH.StartTurn();
                        Send(Save_Load.Serialize(Program.Game));
                        IsMyTurn = false;
                    }
                }
            }
        }
        private void CleanupResources() {
            stream.Close();
            stream = null;
            client.Close();
        }
    }
}