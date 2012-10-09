using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;

namespace BugProviderService
{
    public partial class ServiceController : ServiceBase
    {

        private HttpListener RestListerner { get; set; }

        private UrlGetWorker _getWorker = new UrlGetWorker();

        public ServiceController()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                InitializeComponent();
            }
            catch (Exception e)
            {
                Log("===Constructor Unhandled Exception===");
                Log(e.ToString());
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log("===Domain Unhandled Exception===");
            Log(e.ExceptionObject.ToString());
        }

        public static void Log(String msg)
        {
            File.AppendAllText(@"D:\work\event.log", String.Concat(msg, Environment.NewLine));
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                RestListerner = new HttpListener();
                //var ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
                //foreach (var ipAddress in ipHostEntry.AddressList)
                //{
                //
                //    if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6) continue;
                //    AddPrefixes(ipAddress.ToString(), 9090);
                //}
                //foreach (var alias in ipHostEntry.Aliases)
                //{
                //    AddPrefixes(alias, 9090);
                //}
                AddPrefixes("+", 9090);
                RestListerner.Start();
                RestListerner.BeginGetContext(UrlRequest, null);
            }
            catch (Exception e)
            {
                Log("===Start Unhandled Exception===");
                Log(e.ToString());
            }
        }

        private void AddPrefixes(string host, int port)
        {
            var uriPrefix = String.Format("http://{0}:{1}/bug/", host, port);
            Log(String.Concat("Listening on:", uriPrefix));
            RestListerner.Prefixes.Add(uriPrefix);
        }

        private void UrlRequest(IAsyncResult ar)
        {
            var httpListenerContext = RestListerner.EndGetContext(ar);
            RestListerner.BeginGetContext(UrlRequest, null);
            switch (httpListenerContext.Request.HttpMethod)
            {
                case "GET":
                    _getWorker.WorkOnUrl(httpListenerContext.Request.Url, httpListenerContext.Response);
                    break;
                default:
                    FillDefaultResponse(httpListenerContext.Response);
                    break;
            }
        }

        private void FillDefaultResponse(HttpListenerResponse response)
        {
            response.StatusCode = 404;
            response.Close(Encoding.UTF8.GetBytes("Not Found"), false);
        }

        protected override void OnStop()
        {
            RestListerner.Stop();
        }
    }
}
