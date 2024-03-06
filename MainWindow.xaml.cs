using System.Windows;
using System.Windows.Input;
using PokerTracker3000.Common;
using PokerTracker3000.ViewModels;
using PokerTracker3000.WpfComponents;
using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;
using InputManager = PokerTracker3000.Input.InputManager;

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

        private readonly InputManager _inputManager;

        public MainWindow()
        {
            InitializeComponent();
            Settings.Initalize(SettingsFileName);

            _inputManager = new();
            InitializeKeyboardMappings();

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

        private void InitializeKeyboardMappings()
        {
            _inputManager.RegisterKeyboardEvent(Key.Escape, InputEvent.ButtonEventType.Start);
            _inputManager.RegisterKeyboardEvent(Key.Enter, InputEvent.ButtonEventType.Select);
            _inputManager.RegisterKeyboardEvent(Key.RightShift, InputEvent.ButtonEventType.GoBack);
            _inputManager.RegisterKeyboardEvent(Key.Left, InputEvent.NavigationDirection.Left);
            _inputManager.RegisterKeyboardEvent(Key.Right, InputEvent.NavigationDirection.Right);
            _inputManager.RegisterKeyboardEvent(Key.Down, InputEvent.NavigationDirection.Down);
            _inputManager.RegisterKeyboardEvent(Key.Up, InputEvent.NavigationDirection.Up);
            _inputManager.RegisterKeyboardEvent(Key.LeftCtrl, InputEvent.ButtonEventType.InfoButton, InputEvent.ButtonAction.Down);
            _inputManager.RegisterKeyboardEvent(Key.LeftCtrl, InputEvent.ButtonEventType.InfoButton, InputEvent.ButtonAction.Up);
        }

        private void KeyDownOrUpInWindow(object sender, KeyEventArgs e)
        {
            ViewModel.HandleInputEvent(_inputManager.GetUserInputEventFromKeyboard(e));
        }
    }
}
