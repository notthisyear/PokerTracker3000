using System.Windows;
using System.Windows.Input;
using PokerTracker3000.Common;
using PokerTracker3000.Common.Messages;
using PokerTracker3000.Interfaces;
using PokerTracker3000.ViewModels;

using InputEvent = PokerTracker3000.Input.UserInputEvent;
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

        public InputManager InputManager { get; }

        private const string SettingsFileName = "Settings.json";

        private readonly GameEventBus _eventBus;

        public MainWindow()
        {
            InitializeComponent();
            Settings.Initalize(SettingsFileName);

            _eventBus = new();

            ViewModel = new(_eventBus, Settings.App, new());
            InputManager = new(ViewModel.HandleInputEvent);
            InitializeKeyboardMappings();
            InitializeGamepadMappings();

            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            _eventBus.RegisterListener(this, (t, m) => ApplicationClosing(m), GameEventBus.EventType.ApplicationClosing);
        }

        private void InitializeKeyboardMappings()
        {
            InputManager.RegisterKeyboardEvent(Key.Escape, InputEvent.ButtonEventType.Start);
            InputManager.RegisterKeyboardEvent(Key.Enter, InputEvent.ButtonEventType.Select);
            InputManager.RegisterKeyboardEvent(Key.RightShift, InputEvent.ButtonEventType.GoBack);
            InputManager.RegisterKeyboardEvent(Key.Left, InputEvent.NavigationDirection.Left);
            InputManager.RegisterKeyboardEvent(Key.Right, InputEvent.NavigationDirection.Right);
            InputManager.RegisterKeyboardEvent(Key.Down, InputEvent.NavigationDirection.Down);
            InputManager.RegisterKeyboardEvent(Key.Up, InputEvent.NavigationDirection.Up);
            InputManager.RegisterKeyboardEvent(Key.LeftCtrl, InputEvent.ButtonEventType.InfoButton, InputEvent.ButtonAction.Down);
            InputManager.RegisterKeyboardEvent(Key.LeftCtrl, InputEvent.ButtonEventType.InfoButton, InputEvent.ButtonAction.Up);
            InputManager.RegisterKeyboardEvent(Key.RightCtrl, InputEvent.ButtonEventType.SecondInfoButton, InputEvent.ButtonAction.Down);
            InputManager.RegisterKeyboardEvent(Key.RightCtrl, InputEvent.ButtonEventType.SecondInfoButton, InputEvent.ButtonAction.Up);
        }

        private void InitializeGamepadMappings()
        {
            InputManager.RegisterGamepadEvent(DotXInput.GamepadInput.GamepadDigitalInput.StartButton, InputEvent.ButtonEventType.Start);
            InputManager.RegisterGamepadEvent(DotXInput.GamepadInput.GamepadDigitalInput.AButton, InputEvent.ButtonEventType.Select);
            InputManager.RegisterGamepadEvent(DotXInput.GamepadInput.GamepadDigitalInput.BButton, InputEvent.ButtonEventType.GoBack);
            InputManager.RegisterGamepadEvent(DotXInput.GamepadInput.GamepadDigitalInput.DPadLeft, InputEvent.NavigationDirection.Left);
            InputManager.RegisterGamepadEvent(DotXInput.GamepadInput.GamepadDigitalInput.DPadRight, InputEvent.NavigationDirection.Right);
            InputManager.RegisterGamepadEvent(DotXInput.GamepadInput.GamepadDigitalInput.DPadDown, InputEvent.NavigationDirection.Down);
            InputManager.RegisterGamepadEvent(DotXInput.GamepadInput.GamepadDigitalInput.DPadUp, InputEvent.NavigationDirection.Up);
            InputManager.RegisterGamepadEvent(DotXInput.GamepadInput.GamepadDigitalInput.RightSholderButton, InputEvent.ButtonEventType.InfoButton, DotXInput.GamepadInput.GamepadDigitalInputState.Pressed);
            InputManager.RegisterGamepadEvent(DotXInput.GamepadInput.GamepadDigitalInput.RightSholderButton, InputEvent.ButtonEventType.InfoButton, DotXInput.GamepadInput.GamepadDigitalInputState.Released);
            InputManager.RegisterGamepadEvent(DotXInput.GamepadInput.GamepadDigitalInput.LeftShoulderButton, InputEvent.ButtonEventType.SecondInfoButton, DotXInput.GamepadInput.GamepadDigitalInputState.Pressed);
            InputManager.RegisterGamepadEvent(DotXInput.GamepadInput.GamepadDigitalInput.LeftShoulderButton, InputEvent.ButtonEventType.SecondInfoButton, DotXInput.GamepadInput.GamepadDigitalInputState.Released);

        }

        private void KeyDownOrUpInWindow(object sender, KeyEventArgs e)
        {
            InputManager.OnKeyboardEvent(e);
        }

        private void ApplicationClosing(IInternalMessage m)
        {
            if (m is not ApplicationClosingMessage msg)
                return;

            InputManager.Dispose();

            msg.NumberOfClosingCallbacksCalled++;
            if (msg.NumberOfClosingCallbacksCalled != msg.TotalNumberOfClosingCallbacks)
            {
                msg.CallbackOnClosingCallbackCalled = () =>
                {
                    if (msg.NumberOfClosingCallbacksCalled == msg.TotalNumberOfClosingCallbacks)
                        Application.Current.Dispatcher.Invoke(Close);
                };
            }
            else
            {
                Application.Current.Dispatcher.Invoke(Close);
            }
        }
    }
}
