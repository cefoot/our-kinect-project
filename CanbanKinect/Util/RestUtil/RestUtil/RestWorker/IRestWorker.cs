/* 
  ------------------------------------------------------------------------------
        (c) cefoot

  Dieses Dokument und die hierin enthaltenen Informationen unterliegen
  dem Urheberrecht und duerfen ohne die schriftliche Genehmigung des
  Herausgebers weder als ganzes noch in Teilen dupliziert oder reproduziert
  noch manipuliert werden.
*/
using System.Net;

namespace De.Cefoot.RestUtil.RestWorker
{
    public interface IRestWorker
    {
        void WorkOnUrl(HttpListenerRequest request, HttpListenerResponse response);
    }
}