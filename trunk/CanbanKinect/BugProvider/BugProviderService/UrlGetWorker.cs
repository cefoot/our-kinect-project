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
using System.Reflection;
using System.Text;
using System.Web;

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
            String fullTypeName;
            String methodName;
            var args = new Dictionary<string, string>();
            var builder = new StringBuilder();
            builder.Append(GetType().Namespace);
            var i = 0;
            for (; i < requestUrl.Segments.Length - 1; i++)
            {
                var tmpSeg = requestUrl.Segments[i];
                tmpSeg = CleanSegment(tmpSeg);
                if (String.IsNullOrEmpty(tmpSeg)) continue;
                builder.Append('.');
                builder.Append(tmpSeg);
            }
            fullTypeName = builder.ToString();
            methodName = CleanSegment(requestUrl.Segments[i]);
            ServiceController.Log(requestUrl.Query);
            var sArgs = requestUrl.Query.Split("?&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var sArg in sArgs)
            {
                var strings = sArg.Split("=".ToCharArray(), 2);
                args[Uri.UnescapeDataString(strings[0])] = strings.Length > 1 ? Uri.UnescapeDataString(strings[1]) : "";
            }
            var type = Type.GetType(fullTypeName, false, true);
            if (type == null)
            {
                response.StatusCode = 404;
                response.Close(Encoding.UTF8.GetBytes(String.Format("{0}.{1}({2}) not found", fullTypeName, methodName, ToString(args))), false);
                return;
            }
            CallMethod(type, methodName, args, response);
        }

        private static String ToString(Dictionary<string, string> args)
        {
            var builder = new StringBuilder();
            var wasFirst = false;
            foreach (var arg in args)
            {
                if (wasFirst)
                {
                    builder.Append(",");
                }
                builder.Append(arg.Key);
                builder.Append("=");
                builder.Append(arg.Value);
                wasFirst = true;
            }
            return builder.ToString();
        }

        private static string CleanSegment(string segment)
        {
            if (segment.EndsWith("/"))
            {
                segment = segment.Substring(0, segment.Length - 1);
            }
            return Uri.UnescapeDataString(segment);
        }

        private void CallMethod(Type type, string methodName, Dictionary<string, string> args, HttpListenerResponse response)
        {
            var methodInfo = type.GetMethod(methodName);
            var parameterInfos = methodInfo.GetParameters();
            var callArgs = new List<Object>();
            foreach (var parameterInfo in parameterInfos)
            {
                if (parameterInfo.Name == "response")
                {
                    callArgs.Add(response);
                }
                else if (args.ContainsKey(parameterInfo.Name))
                {
                    callArgs.Add(args[parameterInfo.Name]);
                }
                else
                {
                    callArgs.Add("");
                }
            }
            methodInfo.Invoke(null, callArgs.ToArray());
        }

        #endregion
    }
}
