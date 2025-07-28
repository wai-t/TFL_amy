using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for ArrivalsPanel.xaml
    /// </summary>
    public partial class ArrivalsPanel : UserControl
    {
        public ArrivalsPanel()
        {
            InitializeComponent();
        }

        public IEnumerable<PlatformArrivals> Arrivals
        {
            get => (IEnumerable<PlatformArrivals>)GetValue(ArrivalsProperty);
            set => SetValue(ArrivalsProperty, value);

        }

        public static readonly DependencyProperty ArrivalsProperty =
            DependencyProperty.Register(nameof(Arrivals),
                typeof(IEnumerable<PlatformArrivals>), 
                typeof(ArrivalsPanel),
                new PropertyMetadata(null));

    }
}
