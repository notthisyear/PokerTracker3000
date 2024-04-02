using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.Common;
using PokerTracker3000.GameComponents;
using PokerTracker3000.GameSession;
using PokerTracker3000.Interfaces;

using InputEvent = PokerTracker3000.Input.UserInputEvent;

namespace PokerTracker3000.WpfComponents.EditGameOptions
{
    public partial class ChangeCurrencyControl : UserControl, IInputRelay
    {
        #region Dependency properties
        public GameSessionManager SessionManager
        {
            get { return (GameSessionManager)GetValue(SessionManagerProperty); }
            set { SetValue(SessionManagerProperty, value); }
        }
        public static readonly DependencyProperty SessionManagerProperty = DependencyProperty.Register(
            nameof(SessionManager),
            typeof(GameSessionManager),
            typeof(ChangeCurrencyControl),
            new FrameworkPropertyMetadata(default));

        public CurrencyType SelectedCurrency
        {
            get { return (CurrencyType)GetValue(SelectedCurrencyProperty); }
            set { SetValue(SelectedCurrencyProperty, value); }
        }
        public static readonly DependencyProperty SelectedCurrencyProperty = DependencyProperty.Register(
            nameof(SelectedCurrency),
            typeof(CurrencyType),
            typeof(ChangeCurrencyControl),
            new FrameworkPropertyMetadata(CurrencyType.SwedishKrona, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion

        public ObservableCollection<string> Currencies { get; } = [];

        #region Events
        public event EventHandler<InputEvent.NavigationDirection>? Navigate;
        public event EventHandler<IInputRelay.ButtonEventArgs>? ButtonEvent;
        #endregion

        private readonly List<CurrencyType> _currencyList = [];
        public ChangeCurrencyControl()
        {
            InitializeComponent();
            Loaded += ControlLoadedEvent;
        }

        private void ControlLoadedEvent(object sender, RoutedEventArgs e)
        {
            Loaded -= ControlLoadedEvent;
            if (SessionManager == default)
                return;

            foreach (var t in Enum.GetValues<CurrencyType>())
            {
                var name = t.GetCustomAttributeFromEnum<CurrencyAttribute>().attr!.Name;
                Currencies.Add(name);
                _currencyList.Add(t);
            }

            editor.SelectedIndexChanged += (s, e) =>
            {
                SelectedCurrency = _currencyList[editor.CurrentSelectedIndex];
            };
            SessionManager.Navigate += (s, e) =>
            {
                if (SessionManager == default || SessionManager.CurrentGameEditOption != SideMenuViewModel.GameEditOption.ChangeCurrency)
                    return;
                Navigate?.Invoke(this, e);
            };
        }
    }
}
