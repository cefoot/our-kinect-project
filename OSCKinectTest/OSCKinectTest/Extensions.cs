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
using OSCKinectTest.Properties;

namespace OSCKinectTest
{

    public static class Extensions
    {

        public static byte ConvertPosition(this float value)
        {
            if (value <= -Settings.Default.MaxLeft || value >= Settings.Default.MaxRight)
            {
                throw new ArgumentOutOfRangeException("value", "muss zwischen -1 und 1 liegen");
            }
            value += Settings.Default.MaxLeft;
            value /= Settings.Default.MaxRight + Settings.Default.MaxLeft;
            value *= byte.MaxValue;
            return (byte)value;
        }

        public static byte ConvertDepth(this float value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("value", "muss zwischen -1 und 1 liegen");
            }
            var depth = Settings.Default.MaxDepth;
            value = value > depth ? depth : value;
            value /= depth;
            value *= byte.MaxValue;
            return (byte)value;
        }
    }
}
