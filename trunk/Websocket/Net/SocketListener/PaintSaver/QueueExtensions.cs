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
using System.Text;

namespace PaintSaver
{

    public static class QueueExtensions
    {
        public static float AverageGeo(this Queue<float> queue)
        {
            var val = 1f;
            var cnt = 0;
            var enumerator = queue.GetEnumerator();
            while (enumerator.MoveNext())
            {
                val *= enumerator.Current;
                cnt++;
            }
            return (float) Math.Pow(val, 1d/cnt);
        }
    }
}
