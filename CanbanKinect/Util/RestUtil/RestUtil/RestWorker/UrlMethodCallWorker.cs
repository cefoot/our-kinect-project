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
using System.Linq;
using System.Net;
using System.Text;
using log4net;

namespace De.Cefoot.RestUtil.RestWorker
{
    /// <summary>
    /// converts url to namespace, type and method and calls it
    /// </summary>
    public class UrlMethodCallWorker : IRestWorker
    {

        #region Variablen

        #endregion

        #region Konstruktoren

        public UrlMethodCallWorker(String rootNamespace = "")
        {
            Namespace = rootNamespace;
        }

        #endregion

        #region Eigenschaften

        private static ILog Logger
        {
            get
            {
                return LogManager.GetLogger("UrlMethodCallWorker");
            }
        }

        protected string Namespace { get; set; }

        #endregion

        #region Methoden

        public void WorkOnUrl(HttpListenerRequest request, HttpListenerResponse response)
        {
            var requestUrl = request.Url;
            var fullTypeName = Namespace;
            if (!String.IsNullOrEmpty(fullTypeName))
            {
                fullTypeName += ".";
            }
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
            fullTypeName += builder.ToString();
            var methodName = CleanSegment(requestUrl.Segments[i]);
            Logger.DebugFormat("Url Request : {0}", requestUrl);
            var sArgs = requestUrl.Query.Split("?&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var strings in sArgs.Select(sArg => sArg.Split("=".ToCharArray(), 2)))
            {
                args[Uri.UnescapeDataString(strings[0])] = strings.Length > 1 ? Uri.UnescapeDataString(strings[1]) : "";
            }
            Logger.DebugFormat("Type Lookup[{0}]", fullTypeName);
            var type = Type.GetType(fullTypeName, false, true);
            if (type == null)
            {
                response.StatusCode = 404;
                response.Close(Encoding.UTF8.GetBytes(String.Format("{0}.{1}({2}) not found", fullTypeName, methodName, ToString(args))), false);
                return;
            }
            CallMethod(type, methodName, args, response);
        }

        private static String ToString(IEnumerable<KeyValuePair<string, string>> args)
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

        private static void CallMethod(Type type, string methodName, IDictionary<string, string> args, HttpListenerResponse response)
        {
            var methodInfo = type.GetMethod(methodName);
            if (methodInfo == null)
            {
                Logger.ErrorFormat("Methode Not Found {0}", methodName);
                return;
            }
            var parameterInfos = methodInfo.GetParameters();
            var callArgs = new List<Object>();
            var responseHandled = false;
            foreach (var parameterInfo in parameterInfos)
            {
                if (parameterInfo.Name.Equals(Constants.ParameterResponseName, StringComparison.OrdinalIgnoreCase))
                {
                    callArgs.Add(response);
                    responseHandled = true;
                }
                else if (args.ContainsKey(parameterInfo.Name))
                {
                    callArgs.Add(args[parameterInfo.Name]);
                }
                else
                {
                    callArgs.Add(null);
                }
            }
            Logger.InfoFormat("Calling Method {0}.{1} with arguments:({2})", type.FullName, methodName, callArgs);
            var result = methodInfo.Invoke(null, callArgs.ToArray());
            if (responseHandled) return;
            if (result == null)
            {
                response.StatusCode = 204;
                response.Close();
            }
            else
            {
                response.StatusCode = 200;
                response.Close(Encoding.UTF8.GetBytes(result.ToString()), false);
            }
        }

        #endregion
    }
}
