﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleJson;
using SocketListener;

namespace PaintSaver
{
    class Program
    {
        static void Main(string[] args)
        {
            CometdSocket socket = new CometdSocket("ws://localhost:8080/socketBtn");
            socket.Subscribe("/paint/", PaintHandler);
        }

        private static void PaintHandler(JsonObject msg)
        {
            throw new NotImplementedException();
        }
    }
}
