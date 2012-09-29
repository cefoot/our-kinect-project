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
using System.Net;
using System.Text;

namespace BugProviderService
{

    public class UrlGetWorker
    {
        #region Variablen

        #endregion

        #region Konstruktoren

        #endregion

        #region Eigenschaften

        #endregion

        #region Methoden

        public void WorkOnUrl(Uri requestUrl, HttpListenerResponse response)
        {
            var builder = new StringBuilder();
            foreach (var segment in requestUrl.Segments)
            {
                builder.Append(segment);
                builder.Append(';');
            }
            response.Close(Encoding.UTF8.GetBytes(builder.ToString()), false);
        }

        #endregion
    }
}
