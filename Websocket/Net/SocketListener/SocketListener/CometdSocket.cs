/* $HeadURL$
  ------------------------------------------------------------------------------
        (c) by data experts gmbh
              Postfach 1130
              Woldegker Str. 12
              17001 Neubrandenburg

  Dieses Dokument und die hierin enthaltenen Informationen unterliegen
  dem Urheberrecht und duerfen ohne die schriftliche Genehmigung des
  Herausgebers weder als ganzes noch in Teilen dupliziert oder reproduziert
  noch manipuliert werden.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using SimpleJson;
using WebSocket4Net;

namespace SocketListener
{

    public class CometdSocket
    {
        private int _msgId = 1;
        private int MsgId { get { return _msgId++; } }
        private EventWaitHandle OpenHandle { get; set; }

        private readonly Dictionary<String, HandleMsg> _registeredChannels = new Dictionary<string, HandleMsg>();
        public delegate void HandleMsg(JsonObject msg);

        private String ClienId { get; set; }

        private readonly WebSocket _socket;
        public bool Handshaked { get { return _socket.Handshaked; } }

        public CometdSocket(String url)
        { // Handshake
            OpenHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            _socket = new WebSocket(url)
                          {
                              AllowUnstrustedCertificate = true,
                              EnableAutoSendPing = true
                          };
            _socket.Error += SocketError;
            _socket.DataReceived += SocketDataReceived;
            _socket.MessageReceived += SocketMessageReceived;
            _socket.Opened += SocketOpened;
            _socket.Open();
            OpenHandle.WaitOne();
            Connect(_socket, true);
        }

        static void SocketError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Debug.WriteLine(e.Exception);
        }

        static void SocketDataReceived(object sender, WebSocket4Net.DataReceivedEventArgs e)
        {
            Debug.WriteLine(Encoding.UTF8.GetString(e.Data));
        }

        private void SocketMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var cometdMsg = SimpleJson.SimpleJson.DeserializeObject(e.Message) as JsonArray;
            HandleCometdMsg(sender as WebSocket, cometdMsg);
        }

        /// <summary>
        /// <para>Beispiel:</para>
        /// <para>{"id":"1","minimumVersion":"1.0","supportedConnectionTypes":["websocket","callback-polling","long-polling"],"successful":true,"channel":"/meta/handshake","clientId":"2ionl6tpnl9p91ghyxxrlilc5o","version":"1.0"}</para>
        /// </summary>
        /// <param name="dictionary"></param>
        private void HandleHandshake(IDictionary<string, object> dictionary)
        {
            ClienId = dictionary["clientId"] as String;
            OpenHandle.Set();
            OpenHandle.Close();
        }

        private void HandleCometdMsg(WebSocket socket, JsonArray cometdMsg)
        {
            var dictionary = cometdMsg[0] as JsonObject;

            var channel = dictionary["channel"] as String;
            switch (channel)
            {
                case "/meta/handshake":
                    HandleHandshake(dictionary);
                    break;
                case "/meta/subscribe":
                    Console.WriteLine("Subscribe auf channel:{0} war {1}Erfolgreich!", dictionary["subscription"], (bool)dictionary["successful"] ? "" : "nicht ");
                    break;
                case "/meta/connect":
                    if ((bool)dictionary["successful"])
                    {
                        Connect(socket);
                    }
                    else
                    {//da war wohl die Verbindung weg. wieder aufbauen!
                        //error wäre hier 402
                        DoHandshake(socket);
                    }
                    break;
                default:
                    HandleMsg handler;
                    if (_registeredChannels.TryGetValue(channel, out handler))
                    {
                        handler(dictionary);
                        break;
                    }
                    Console.WriteLine("ERROR MSG:");
                    Console.WriteLine(cometdMsg);
                    break;
            }
        }

        void SocketOpened(object sender, EventArgs e)
        {
            var webSocket = (WebSocket)sender;
            DoHandshake(webSocket);
        }

        public void send(String qname, object data)
        {
            var sendData = new JsonObject();
            sendData["channel"] = qname;
            sendData["data"] = data;
        }

        /// <summary>
        /// <para>Beispiel:</para>
        /// <para>{"version":"1.0","minimumVersion":"0.9","channel":"/meta/handshake","supportedConnectionTypes":["websocket","long-polling","callback-polling"],"advice":{"timeout":60000,"interval":0},"id":"1"}</para>
        /// </summary>
        /// <param name="webSocket"></param>
        private void DoHandshake(WebSocket webSocket)
        {
            var data = new JsonObject();
            data["version"] = "1.0";
            data["minimumVersion"] = "1.0";
            data["channel"] = "/meta/handshake";
            data["supportedConnectionTypes"] = new JsonArray
                                                   {
                                                       "websocket",
                                                       "long-polling",
                                                       "callback-polling"
                                                   };
            var advice = new JsonObject();
            advice["timeout"] = 60000L;//falls wir 60 sekunden nichts hören sollten wir uns wieder melden mit connect
            advice["interval"] = 0L;
            data["advice"] = advice;
            data["id"] = MsgId;
            webSocket.Send(new JsonArray
                {
                    data
                }.ToString());
        }

        /// <summary>
        /// <para>Beispiel:</para>
        /// <para>{"channel":"/meta/subscribe","subscription":"/channel","id":"2","clientId":"d4ltmqqcfjoyo7xqm79ciugni"}</para>
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="channel"></param>
        /// <param name="msgHandler"></param>
        public void Subscribe(string channel, HandleMsg msgHandler)
        {
            var subscribe = new JsonObject();
            subscribe["channel"] = "/meta/subscribe";
            subscribe["subscription"] = channel;
            subscribe["id"] = MsgId.ToString();
            subscribe["clientId"] = ClienId;
            var data = new JsonArray
                           {
                               subscribe
                           };
            _registeredChannels[channel] = msgHandler;
            _socket.Send(data.ToString());
        }

        /// <summary>
        /// <para>Beispiel:</para>
        /// <para>{"channel":"/meta/connect","connectionType":"websocket","advice":{"timeout":0},"id":"3","clientId":"d4ltmqqcfjoyo7xqm79ciugni"}</para>
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="sendAdvice"></param>
        public void Connect(WebSocket socket, bool sendAdvice = false)
        {
            var data = new JsonObject();
            data["channel"] = "/meta/connect";
            data["connectionType"] = "websocket";
            if (sendAdvice)
            {
                var advice = new JsonObject();
                advice["timeout"] = 0L;
                data["advice"] = advice;
            }
            data["id"] = MsgId;
            data["clientId"] = ClienId;
            socket.Send(new JsonArray
                            {
                                data
                            }.ToString());
        }
    }
}
