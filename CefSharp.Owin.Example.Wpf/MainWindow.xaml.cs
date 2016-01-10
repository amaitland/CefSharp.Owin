using System.Windows;

namespace CefSharp.Owin.Example.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Browser.IsBrowserInitializedChanged += OnIsBrowserInitializedChanged;
        }

        private void OnIsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            //Browser.ShowDevTools();
        }
    }
}
