using System.Windows;
using System.Windows.Input;
using PokerTracker3000.Common;
using PokerTracker3000.Common.Messages;
using PokerTracker3000.Interfaces;
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
        private readonly IGameEventBus _eventBus;

        public MainWindow()
        {
            InitializeComponent();
            Settings.Initalize(SettingsFileName);

            _eventBus = new GameEventBus();

            ViewModel = new(_eventBus, Settings.App, new());
            _inputManager = new(ViewModel.HandleInputEvent);
            InitializeKeyboardMappings();
            InitializeGamepadMappings();

            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
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
                        _eventBus.NotifyListeners(GameEventBus.EventType.ApplicationClosing, new ApplicationClosingMessage());
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

        private void InitializeGamepadMappings()
        {
            _inputManager.RegisterGamepadEvent(GamepadInput.GamepadDigitalInput.StartButton, InputEvent.ButtonEventType.Start);
            _inputManager.RegisterGamepadEvent(GamepadInput.GamepadDigitalInput.AButton, InputEvent.ButtonEventType.Select);
            _inputManager.RegisterGamepadEvent(GamepadInput.GamepadDigitalInput.BButton, InputEvent.ButtonEventType.GoBack);
            _inputManager.RegisterGamepadEvent(GamepadInput.GamepadDigitalInput.DPadLeft, InputEvent.NavigationDirection.Left);
            _inputManager.RegisterGamepadEvent(GamepadInput.GamepadDigitalInput.DPadRight, InputEvent.NavigationDirection.Right);
            _inputManager.RegisterGamepadEvent(GamepadInput.GamepadDigitalInput.DPadDown, InputEvent.NavigationDirection.Down);
            _inputManager.RegisterGamepadEvent(GamepadInput.GamepadDigitalInput.DPadUp, InputEvent.NavigationDirection.Up);
            _inputManager.RegisterGamepadEvent(GamepadInput.GamepadDigitalInput.RightSholderButton, InputEvent.ButtonEventType.InfoButton, GamepadInput.GamepadDigitalInputState.Pressed);
            _inputManager.RegisterGamepadEvent(GamepadInput.GamepadDigitalInput.RightSholderButton, InputEvent.ButtonEventType.InfoButton, GamepadInput.GamepadDigitalInputState.Released);
        }

        private void KeyDownOrUpInWindow(object sender, KeyEventArgs e)
        {
            _inputManager.OnKeyboardEvent(e);
        }
    }
}
