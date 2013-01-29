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
using System.Text;
using log4net;

namespace De.Cefoot.RestUtil.RestWorker
{

    public class NotFoundWorker : IRestWorker
    {
        #region Variablen

        #endregion

        #region Konstruktoren

        public NotFoundWorker(String notFoundText = "nothing here")
        {
            NotFoundText = notFoundText;
        }

        #endregion

        #region Eigenschaften

        private String NotFoundText { get; set; }

        private static ILog Logger
        {
            get
            {
                return LogManager.GetLogger("NotFoundWorker");
            }
        }

        #endregion

        #region Methoden

        public void WorkOnUrl(HttpListenerRequest request, HttpListenerResponse response)
        {
            Logger.DebugFormat("Url Request : {0}", request.Url);
            response.StatusCode = 404;
            response.Close(Encoding.UTF8.GetBytes(NotFoundText), false);
        }

        #endregion
    }
}
