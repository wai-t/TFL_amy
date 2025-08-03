using System.Windows;
using System.Windows.Controls;

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
