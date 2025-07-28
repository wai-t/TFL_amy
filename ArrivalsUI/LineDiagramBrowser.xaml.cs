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

namespace ArrivalsUI
{
    /// <summary>
    /// Interaction logic for LineDiagramBrowser.xaml
    /// </summary>
    public partial class LineDiagramBrowser : UserControl
    {
        public LineDiagramBrowser()
        {
            InitializeComponent();
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (textBlock != null)
            {
                if (textBlock.DataContext is Label label)
                {
                    label.OnStationSelect?.Invoke(label.Station);
                }
            }
        }
    }
}
