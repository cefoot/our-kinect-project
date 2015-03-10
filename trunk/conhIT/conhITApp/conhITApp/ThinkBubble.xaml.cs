using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace De.DataExperts.conhITApp
{
    /// <summary>
    /// Interaktionslogik für ThinkBubble.xaml
    /// </summary>
    public partial class ThinkBubble : UserControl
    {
        public ThinkBubble()
        {
            InitializeComponent();
        }

        public String LabelText
        {
            get
            {
                return txt.Content.ToString();
            }
            set
            {
                txt.Content = value;
            }
        }

        private void Ctrl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            grd.Margin = new Thickness(0, -e.NewSize.Height, 0, e.NewSize.Height);
        }
        private static Action EmptyDelegate = delegate() { };
    }
}
