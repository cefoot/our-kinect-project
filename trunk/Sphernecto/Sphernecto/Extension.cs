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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sphernecto
{

    public static class Extension
    {

        public static BitmapSource ToBitmapSource(this byte[] pixels, int width, int height)
        {
            return ToBitmapSource(pixels, width, height, PixelFormats.Bgr32);
        }

        public static bool IsInRange(this float cur,float desired,float range)
        {
            return cur > desired - range && cur < desired + range;
        }

        private static BitmapSource ToBitmapSource(this byte[] pixels, int width, int height, PixelFormat format)
        {
            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, width * format.BitsPerPixel / 8);
        }
    }
}
