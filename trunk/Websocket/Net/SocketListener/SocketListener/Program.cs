using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using SimpleJson;
using WebSocket4Net;

namespace SocketListener
{
    class Program
    {

        static void Main(string[] args)
        {
            var socket = new CometdSocket("ws://localhost:8080/socketBtn");
            socket.Subscribe("/paint/", HandleTextRecieved);

            Console.WriteLine(socket.Handshaked);
            Console.ReadKey();
        }

        private static void HandleTextRecieved(JsonObject msg)
        {
            //data enthält den text, wenn kein objekt übergeben wirdansonsten ist in data eine hashmap
            Console.WriteLine(msg["data"].ToString());
        }
    }
}
