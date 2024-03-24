using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameComponents;
using PokerTracker3000.Interfaces;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000.WpfComponents
{
    public partial class NumericalScrollingEditor : UserControl
    {
        public enum Mode
        {
            Money,
            Time
        };

        #region Dependency properties
        public IInputRelay NavigatorRelay
        {
            get { return (IInputRelay)GetValue(NavigatorRelayProperty); }
            set { SetValue(NavigatorRelayProperty, value); }
        }
        public static readonly DependencyProperty NavigatorRelayProperty = DependencyProperty.Register(
            nameof(NavigatorRelay),
            typeof(IInputRelay),
            typeof(NumericalScrollingEditor),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        public decimal CurrentMoneyValue
        {
            get { return (decimal)GetValue(CurrentMoneyValueProperty); }
            set { SetValue(CurrentMoneyValueProperty, value); }
        }
        public static readonly DependencyProperty CurrentMoneyValueProperty = DependencyProperty.Register(
            nameof(CurrentMoneyValue),
            typeof(decimal),
            typeof(NumericalScrollingEditor),
            new FrameworkPropertyMetadata((decimal)0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int CurrentTimeValueSeconds
        {
            get { return (int)GetValue(CurrentTimeValueSecondsProperty); }
            set { SetValue(CurrentTimeValueSecondsProperty, value); }
        }
        public static readonly DependencyProperty CurrentTimeValueSecondsProperty = DependencyProperty.Register(
            nameof(CurrentTimeValueSeconds),
            typeof(int),
            typeof(NumericalScrollingEditor),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int NumberOfDigits
        {
            get { return (int)GetValue(NumberOfDigitsProperty); }
            set { SetValue(NumberOfDigitsProperty, value); }
        }
        public static readonly DependencyProperty NumberOfDigitsProperty = DependencyProperty.Register(
            nameof(NumberOfDigits),
            typeof(int),
            typeof(NumericalScrollingEditor),
            new FrameworkPropertyMetadata(2, FrameworkPropertyMetadataOptions.AffectsArrange, NumberOfDigitsChanged));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(NumericalScrollingEditor),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, IsSelectedChanged));

        public Mode ScrollerMode
        {
            get { return (Mode)GetValue(ScrollerModeProperty); }
            set { SetValue(ScrollerModeProperty, value); }
        }
        public static readonly DependencyProperty ScrollerModeProperty = DependencyProperty.Register(
            nameof(ScrollerMode),
            typeof(Mode),
            typeof(NumericalScrollingEditor),
            new FrameworkPropertyMetadata(Mode.Money, FrameworkPropertyMetadataOptions.AffectsRender));

        private static void IsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericalScrollingEditor editor && e.OldValue is bool oldValue && e.NewValue is bool newValue && oldValue != newValue)
            {
                if (newValue)
                    editor.SetDigitsFromCurrentValue();

                if (editor.ScrollerMode == Mode.Money)
                {
                    List<ScrollingSelectorBox> scrollerBoxes = [];
                    if (!editor.selectorControl.TryFindAllChildrenOfType(scrollerBoxes, editor.NumberOfDigits))
                        return;

                    foreach (var box in scrollerBoxes)
                    {
                        if (box.DataContext is not ScrollDigit digit)
                            continue;

                        if (newValue)
                            SetSyncToDigit(box, digit);
                        else
                            digit.UnhookToScrollingSelectorBox();
                    }
                }
                else if (editor.ScrollerMode == Mode.Time)
                {
                    foreach (var (digit, box) in editor._timeScrollers)
                    {
                        if (newValue)
                            SetSyncToDigit(box, digit);
                        else
                            digit.UnhookToScrollingSelectorBox();
                    }
                }

                if (!newValue)
                    editor.SetCurrentValueFromDigits();
            }
        }

        public CurrencyType Currency
        {
            get { return (CurrencyType)GetValue(CurrencyProperty); }
            set { SetValue(CurrencyProperty, value); }
        }
        public static readonly DependencyProperty CurrencyProperty = DependencyProperty.Register(
            nameof(Currency),
            typeof(CurrencyType),
            typeof(NumericalScrollingEditor),
            new FrameworkPropertyMetadata(CurrencyType.SwedishKrona, FrameworkPropertyMetadataOptions.AffectsRender));

        private static void NumberOfDigitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericalScrollingEditor editor && e.OldValue is int oldValue && e.NewValue is int newValue && oldValue != newValue)
            {
                if (editor.ScrollerMode == Mode.Money)
                    editor.ChangeDigitsCollection(newValue);
            }
        }

        #region Read-only properties
        public ObservableCollection<ScrollDigit> Digits
        {
            get => (ObservableCollection<ScrollDigit>)GetValue(s_digitsProperty);
            private set => SetValue(s_digitsPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_digitsPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Digits),
            typeof(ObservableCollection<ScrollDigit>),
            typeof(ScrollingSelectorBox),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_digitsProperty = s_digitsPropertyKey.DependencyProperty;

        public ScrollDigit TensHourDigit { get; } = new() { Index = 5 };

        public ScrollDigit OnesHourDigit { get; } = new() { Index = 4 };

        public ScrollDigit TensMinuteDigit { get; } = new() { Index = 3, IsLimitedRange = true };

        public ScrollDigit OnesMinuteDigit { get; } = new() { Index = 2 };

        public ScrollDigit TensSecondDigit { get; } = new() { Index = 1, IsLimitedRange = true };

        public ScrollDigit OnesSecondDigit { get; } = new() { Index = 0, IsSelected = true };
        #endregion

        #endregion

        #region Private fields
        private readonly List<(ScrollDigit digit, ScrollingSelectorBox box)> _timeScrollers = [];
        private const int SecondsPerMinute = 60;
        private const int MinutesPerHour = 60;
        private const int SecondsPerHour = SecondsPerMinute * MinutesPerHour;
        #endregion

        public ObservableCollection<string> NumberOptions { get; } = [];

        public ObservableCollection<string> NumberOptionsLimited { get; } = [];

        public NumericalScrollingEditor()
        {
            InitializeComponent();
            Loaded += NumericalScrollingEditorLoaded;
        }

        private void NumericalScrollingEditorLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= NumericalScrollingEditorLoaded;

            _timeScrollers.Add((TensHourDigit, tensHourBox));
            _timeScrollers.Add((OnesHourDigit, onesHourBox));
            _timeScrollers.Add((TensMinuteDigit, tensMinuteBox));
            _timeScrollers.Add((OnesMinuteDigit, onesMinuteBox));
            _timeScrollers.Add((TensSecondDigit, tensSecondBox));
            _timeScrollers.Add((OnesSecondDigit, onesSecondBox));

            for (var i = 9; i >= 0; i--)
                NumberOptions.Add($"{i}");

            for (var i = 5; i >= 0; i--)
                NumberOptionsLimited.Add($"{i}");


            if (ScrollerMode == Mode.Money)
            {
                ChangeDigitsCollection(NumberOfDigits);
                Digits.Last().IsSelected = true;
            }

            SetDigitsFromCurrentValue();

            if (NavigatorRelay != default)
            {
                NavigatorRelay.Navigate += (s, e) =>
                {
                    var selectedDigit = ScrollerMode == Mode.Money ? Digits.FirstOrDefault(x => x.IsSelected) : _timeScrollers.FirstOrDefault(x => x.digit.IsSelected).digit;
                    if (selectedDigit != default)
                    {
                        var isUpOrDown = e == InputEvent.NavigationDirection.Up || e == InputEvent.NavigationDirection.Down;
                        if (isUpOrDown)
                        {
                            // Note: It feels more natural to invert the direction here
                            selectedDigit.FireNavigationEvent(e);
                        }
                        else
                        {
                            selectedDigit.IsSelected = false;
                            var newIndex = selectedDigit.Index + (e == InputEvent.NavigationDirection.Left ? 1 : -1);
                            var numberOfDigits = ScrollerMode == Mode.Money ? NumberOfDigits : 6;
                            newIndex = newIndex < 0 ? numberOfDigits - 1 : newIndex % numberOfDigits;
                            if (ScrollerMode == Mode.Money)
                                Digits[NumberOfDigits - 1 - newIndex].IsSelected = true;
                            else if (ScrollerMode == Mode.Time)
                                _timeScrollers.First(x => x.digit.Index == newIndex)!.digit.IsSelected = true;
                        }
                    }
                };
            }
        }

        private void ChangeDigitsCollection(int numberOfDigits)
        {
            if (Digits == default)
                Digits = [];

            while (numberOfDigits != Digits.Count)
            {
                if (numberOfDigits > Digits.Count)
                {
                    Digits.Insert(0, new() { Index = Digits.Count });
                }
                else
                {
                    Digits.First().UnhookToScrollingSelectorBox();
                    Digits.Remove(Digits.First());
                }
            }
        }

        private void SetDigitsFromCurrentValue()
        {
            if (ScrollerMode == Mode.Money)
            {
                var currentDivisor = 1;
                for (var i = 0; i < NumberOfDigits; i++)
                {
                    Digits[NumberOfDigits - 1 - i].Value = ((int)CurrentMoneyValue / currentDivisor) % 10;
                    currentDivisor *= 10;
                }
            }
            else if (ScrollerMode == Mode.Time)
            {
                var numberOfSeconds = CurrentTimeValueSeconds;
                var numberOfHours = numberOfSeconds / SecondsPerHour;
                numberOfSeconds -= numberOfHours * SecondsPerHour;
                var numberOfMinutes = numberOfSeconds / SecondsPerMinute;
                numberOfSeconds -= numberOfMinutes * SecondsPerMinute;

                TensHourDigit.Value = numberOfHours / 10;
                OnesHourDigit.Value = numberOfHours % 10;
                TensMinuteDigit.Value = numberOfMinutes / 10;
                OnesMinuteDigit.Value = numberOfMinutes % 10;
                TensSecondDigit.Value = numberOfSeconds / 10;
                OnesSecondDigit.Value = numberOfSeconds % 10;
            }
        }

        private void SetCurrentValueFromDigits()
        {
            if (ScrollerMode == Mode.Money)
            {
                CurrentMoneyValue = 0;
                for (var i = 0; i < NumberOfDigits; i++)
                    CurrentMoneyValue += ((int)Math.Pow(10, i)) * Digits[NumberOfDigits - 1 - i].Value;
            }
            else if (ScrollerMode == Mode.Time)
            {
                CurrentTimeValueSeconds =
                    ((10 * TensHourDigit.Value + OnesHourDigit.Value) * SecondsPerHour) +
                    ((10 * TensMinuteDigit.Value + OnesMinuteDigit.Value) * SecondsPerMinute) +
                    10 * TensSecondDigit.Value + OnesSecondDigit.Value;
            }
        }

        private static void SetSyncToDigit(ScrollingSelectorBox box, ScrollDigit digit)
        {
            digit.SyncScrollerToValue(box);
            if (!digit.IsHookedToScrollBox)
                digit.HookToScrollingSelectorBox(box);
        }

    }
}
