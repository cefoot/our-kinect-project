/* $HeadURL: https://our-kinect-project.googlecode.com/svn/trunk/conhIT/conhITApp/conhITApp/NegatingConverter.cs $
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
using System.Windows.Data;
using System.Windows.Media;

namespace De.DataExperts.conhITApp
{
    [ValueConversion(typeof(System.Drawing.Color), typeof(System.Windows.Media.Brush))]
    public class BrushConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is System.Drawing.Color)
            {
                var clr = (System.Drawing.Color)value;
                return new SolidColorBrush(Color.FromArgb(clr.A, clr.R, clr.G, clr.B));
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is SolidColorBrush)
            {
                var brush = (SolidColorBrush)value;
                return System.Drawing.Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
            }
            return value;
        }
    }
}
