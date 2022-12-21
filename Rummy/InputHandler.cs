using System;
using System.Collections.Generic;
using System.Threading;

namespace Rummy {
    public class InputHandler
    {
        private bool run = false;
        public bool IsRunning
        {
            get { return run; }
            set {}
        }
        public bool IsIntercepting;
        public int BufferLength => InputBuffer.Count;
        private List<ConsoleKeyInfo> InputBuffer = new List<ConsoleKeyInfo>();

        public InputHandler(bool intercept = true, bool startAtInit = true)
        {
            THandler = new Thread(Handler);
            IsIntercepting = intercept;
            if(startAtInit){StartHandler();}
        }

        private Thread THandler;
        private void Handler() {
            while (run) {
                if(Console.KeyAvailable){InputBuffer.Add(Console.ReadKey(IsIntercepting));}
                Thread.Sleep(10);
            }
        }
        public void StartHandler() {
            if (!run) {
                run = true;
                THandler.Start();
            }
        }
        public void StopHandler() {
            run = false;
        }
        public void SetHandlerIntercept(bool state){IsIntercepting = state;}
        

        public ConsoleKeyInfo? Pull() => (BufferLength > 0) ? Remove(0) : null;
        public ConsoleKeyInfo Remove(int id)
        {
            if(BufferLength<=id){throw new ArgumentOutOfRangeException(nameof(id), "id can't be bigger than the InputBuffer's size.");}
            ConsoleKeyInfo op = InputBuffer[id];
            InputBuffer.RemoveAt(id);
            return op;
        }
        

    }
}