using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rummy.TextColor;

namespace Rummy {
    public class Client {
        private TcpClient client;
        private NetworkStream stream;
        public int Port;
        public string Address;
        public string Name;
        public bool IsMyTurn;
        private List<char> UserInput = new List<char>();
        private List<string> NonMsgPackets = new List<string>();
        private List<string> bash_history = new List<string>();

        public bool Running { get; private set; }

        public Client(string name, string ServerAddress) {
            client = new TcpClient();
            Running = false;
            Name = name;
            if (ServerAddress.Contains(':')) {
                Address = ServerAddress.Split(':')[0];
                Port = (ServerAddress.Split(':').Length > 0 && ServerAddress.Split(':')[1] != "")
                    ? Convert.ToInt32(ServerAddress.Split(':')[1])
                    : 18274;
            }
            else {
                Address = ServerAddress;
                Port = 18274;
            }

            bash_history.Add("");
        }

        public void Connect() {
            try{client.Connect(Address, Port);}
            catch(Exception e){Console.WriteLine(e.Message);}

            if (client.Connected) {
                Console.WriteLine($"Successfully connected to {Address}");
                Running = true;
                stream = client.GetStream();
                Send($"player:{Name}");
                Constants.IH.StartHandler();
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
                Tasks.Add(UpdateUserInput());
                //DisplayUserInput();
                Thread.Sleep(10);
            }
        }

        public async Task Send(string msg) {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        private async Task ReceiveAndProcess() {
            string msg;
            if (NonMsgPackets.Count > 0) {
                msg = NonMsgPackets[0];
                NonMsgPackets.RemoveAt(0);
            }
            else if (client.Available > 0) {
                byte[] buffer = new byte[client.Available];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                msg = Encoding.UTF8.GetString(buffer);
            }
            else msg = "nada";

            if(msg == "nada") {return;}
            if (msg == "Your turn" ) {
                    IsMyTurn = true;
                    Console.WriteLine("Received turn request, sending response");
                    Send("OK, Gimme state");
            }
            else if (msg.StartsWith("#General")) {
                Console.WriteLine("Received Gamestate, updating");
                Program.Game = Save_Load.Load(msg.Split('\n'));
                Program.Game.Players[Program.Game.CurrentPlayerId].SH.History = bash_history;
                if (IsMyTurn) {
                    Console.WriteLine("Starting turn");
                    if (Program.Game.CurrentPlayerId == 0 && Program.Game.Round == 0) {
                        Program.Game.Players[Program.Game.CurrentPlayerId].First = true;
                    }

                    lock (Constants.IH){
                        Program.Game.Players[Program.Game.CurrentPlayerId].SH.StartTurn();
                    }
                    
                    bash_history = Program.Game.Players[Program.Game.CurrentPlayerId].SH.History;

                    await Send(Save_Load.Serialize(Program.Game));
                    IsMyTurn = false;
                }
            }
            else if (msg.StartsWith(":msg:")) {
                string proc = msg.Remove(0, 5);
                string[] queuedmessages = proc.Contains(":msg:")?proc.Split(":msg:") : new string[] { proc };
                for (int i = 0; i < queuedmessages.Length; i++) {
                    Console.WriteLine($"{Colors.Important.AnsiFGCode}[{queuedmessages[i].Split(':')[0]}]{Color.Reset} {queuedmessages[i].Remove(0, queuedmessages[i].Split(':')[0].Length + 1)}");
                    if(queuedmessages[i].Contains(":endofgame:")){Console.WriteLine("Game ended!");}
                    if(queuedmessages[i].Contains(":newplayer:")){Console.WriteLine($"{queuedmessages[i].Split(":newplayer:")[1]} arrived");}
                }
            }
            else if (msg.StartsWith(":newplayer:")) {
                string name = msg.Remove(0, 11);
                Console.WriteLine($"{name} has been accepted into our endeavour!");
            }
            else if(msg.StartsWith(":endofgmae:")){Console.WriteLine("Game ended!");}
        }

        public void ReceiveMsg() {
            if (client.Available > 0) {
                byte[] buffer = new byte[client.Available];
                stream.Read(buffer, 0, buffer.Length);
                string msg = Encoding.UTF8.GetString(buffer);
                if (msg.StartsWith(":msg:")) {
                    string proc = msg.Remove(0, 5);
                    string[] queuedmessages = proc.Contains(":msg:")?proc.Split(":msg:") : new string[] { proc };
                    Console.CursorLeft = 0;
                    for (int i = 0; i < queuedmessages.Length; i++) {
                        Console.WriteLine($"{Colors.Important.AnsiFGCode}[{queuedmessages[i].Split(':')[0]}]{Color.Reset} {queuedmessages[i].Remove(0, queuedmessages[i].Split(':')[0].Length + 1)}");
                        if(queuedmessages[i].Contains(":endofgame:")){NonMsgPackets.Add(":endofgame");}
                        if(queuedmessages[i].Contains(":newplayer:")){NonMsgPackets.Add($":newplayer:{queuedmessages[i].Split(":newplayer:")[1]}");}
                    }
                }
                else NonMsgPackets.Add(msg);
            }
        }

        private async Task UpdateUserInput() {
            ConsoleKeyInfo? n_key = Constants.IH.Pull();
            if (n_key.HasValue) {
                ConsoleKeyInfo key = n_key.Value;
                switch (key.Key) {
                    case ConsoleKey.Backspace:
                        UserInput.RemoveAt(UserInput.Count - 1);
                        break;
                    case ConsoleKey.Enter:
                        await HandleUserInput();
                        break;
                    default:
                        UserInput.Add(key.KeyChar);
                        break;
                }
		DisplayUserInput();
            }
        }

        private void DisplayUserInput() {
            Console.Write($"> {new string(UserInput.ToArray())} ");
            Console.CursorLeft = 0;
        }

        private async Task HandleUserInput() {
            string buff = ":msg:everyone:" + new String(UserInput.ToArray());
            await Send(buff);
            UserInput.Clear();
        }
        
        private void CleanupResources() {
            stream.Close();
            stream = null;
            client.Close();
        }
    }
}