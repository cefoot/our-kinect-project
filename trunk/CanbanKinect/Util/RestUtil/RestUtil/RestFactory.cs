/* 
  ------------------------------------------------------------------------------
        (c) cefoot

  Dieses Dokument und die hierin enthaltenen Informationen unterliegen
  dem Urheberrecht und duerfen ohne die schriftliche Genehmigung des
  Herausgebers weder als ganzes noch in Teilen dupliziert oder reproduziert
  noch manipuliert werden.
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Xml;
using System.Linq;
using System.Linq.Expressions;
using De.Cefoot.RestUtil.RestWorker;
using HttpNamespaceManager.Lib;
using HttpNamespaceManager.Lib.AccessControl;
using log4net;

namespace De.Cefoot.RestUtil
{

    public class RestFactory
    {

        #region Variablen

        #endregion

        #region Konstruktoren

        private RestFactory()
        {

        }

        #endregion

        #region Eigenschaften

        private static ILog Logger
        {
            get
            {
                return LogManager.GetLogger("RestFactory");
            }
        }
        public IRestWorker GetWorker { get; set; }
        public IRestWorker PutWorker { get; set; }
        public IRestWorker PostWorker { get; set; }
        public IRestWorker DeleteWorker { get; set; }
        private HttpApi _httpApi;

        public HttpApi HttpApi
        {
            get
            {
                return _httpApi ?? (_httpApi = new HttpApi());
            }
        }

        protected HttpListener RestListerner { get; set; }

        #endregion

        #region Methoden
        [DllImport("shell32.dll")]
        internal static extern bool IsUserAnAdmin();

        public static RestFactory CreateRestWorker(int port, string urlPreString, IRestWorker getWorker, IRestWorker putWorker = null, IRestWorker postWorker = null, IRestWorker deleteWorker = null)
        {
            if(!IsUserAnAdmin())
            {
                throw new UnauthorizedAccessException("Muss als Admin ausgeführt werden");
            }
            var factory = new RestFactory
                              {
                                  GetWorker = getWorker ?? new NotFoundWorker(),
                                  PutWorker = putWorker ?? new NotFoundWorker(),
                                  PostWorker = postWorker ?? new NotFoundWorker(),
                                  DeleteWorker = deleteWorker ?? new NotFoundWorker(),
                                  RestListerner = new HttpListener()
                              };
            factory.AddPrefix(port, urlPreString);
            return factory;
        }

        ~RestFactory()
        {
            HttpApi.Dispose();
        }

        private void AddPrefix(int port, string urlPreString)
        {
            var uriPrefix = String.Format("http://+:{0}/{1}", port, urlPreString);
            Logger.InfoFormat("Listening on:{0}", uriPrefix);
            AddRights(uriPrefix);
            RestListerner.Prefixes.Add(uriPrefix);
        }

        private void AddRights(string uriPrefix)
        {
            if(HttpApi.QueryHttpNamespaceAcls().ContainsKey(uriPrefix))
            {
                HttpApi.RemoveHttpHamespaceAcl(uriPrefix);
            }
            var newSd = new SecurityDescriptor
                            {
                                DACL = new AccessControlList()
                            };
            var sId = SecurityIdentity.SecurityIdentityFromName(WindowsIdentity.GetCurrent().Name);
            var ace = new AccessControlEntry(sId)
                          {
                              AceType = AceType.AccessAllowed
                          };
            ace.Add(AceRights.GenericAll);
            ace.Add(AceRights.GenericExecute);
            ace.Add(AceRights.GenericRead);
            ace.Add(AceRights.GenericWrite);
            newSd.DACL.Add(ace);

            HttpApi.SetHttpNamespaceAcl(uriPrefix, newSd);
            return;
        }

        private void UrlRequest(IAsyncResult ar)
        {
            var httpListenerContext = RestListerner.EndGetContext(ar);
            RestListerner.BeginGetContext(UrlRequest, null);
            ProcessRequest(httpListenerContext);
        }

        private void ProcessRequest(HttpListenerContext httpListenerContext)
        {
            try
            {

                switch (httpListenerContext.Request.HttpMethod)
                {
                    case "GET":
                        GetWorker.WorkOnUrl(httpListenerContext.Request, httpListenerContext.Response);
                        break;
                    case "PUT":
                        PutWorker.WorkOnUrl(httpListenerContext.Request, httpListenerContext.Response);
                        break;
                    case "POST":
                        PostWorker.WorkOnUrl(httpListenerContext.Request, httpListenerContext.Response);
                        break;
                    case "DELETE":
                        DeleteWorker.WorkOnUrl(httpListenerContext.Request, httpListenerContext.Response);
                        break;
                }
            }
            catch (Exception ex)
            {
                httpListenerContext.Response.StatusCode = 500;
                httpListenerContext.Response.Close(Encoding.UTF8.GetBytes(ex.ToString()), false);
            }
        }

        public void Start()
        {
            RestListerner.Start();
            RestListerner.BeginGetContext(UrlRequest, null);
        }

        public void Stop()
        {
            RestListerner.Stop();
        }

        #endregion
    }
}
