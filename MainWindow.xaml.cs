using System.Windows;
using System.Windows.Input;
using PokerTracker3000.Common;
using PokerTracker3000.ViewModels;
using PokerTracker3000.WpfComponents;

namespace PokerTracker3000
{
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel
        {
            get => (MainWindowViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(MainWindowViewModel),
            typeof(MainWindow),
            new FrameworkPropertyMetadata(null));

        private const string SettingsFileName = "Settings.json";

        public MainWindow()
        {
            InitializeComponent();
            Settings.Initalize(SettingsFileName);

            ViewModel = new(Settings.App);

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

            var verticalMargin = MaxHeight - Height;
            Top = verticalMargin / 4;
        }

        private void TitleBarButtonPressed(object sender, RoutedEventArgs e)
        {
            if (e is TitleBarButtonClickedEventArgs eventArgs)
            {
                switch (eventArgs.ButtonClicked)
                {
                    case TitleBarButton.Minimize:
                        WindowState = WindowState.Minimized;
                        break;
                    case TitleBarButton.Maximize:
                        WindowState = WindowState.Maximized;
                        break;
                    case TitleBarButton.Restore:
                        WindowState = WindowState.Normal;
                        break;
                    case TitleBarButton.Close:
                        ViewModel.NotifyWindowClosed();
                        Close();
                        break;
                };
            }
        }

        private void KeyDownInWindow(object sender, KeyEventArgs e)
        {
            if (ViewModel == default)
                return;
            ViewModel.InputManager.HandleKeyPressed(e);
        }
    }
}
