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

        public decimal CurrentValue
        {
            get { return (decimal)GetValue(CurrentValueProperty); }
            set { SetValue(CurrentValueProperty, value); }
        }
        public static readonly DependencyProperty CurrentValueProperty = DependencyProperty.Register(
            nameof(CurrentValue),
            typeof(decimal),
            typeof(NumericalScrollingEditor),
            new FrameworkPropertyMetadata(new decimal(0), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

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

        private static void IsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericalScrollingEditor editor && e.OldValue is bool oldValue && e.NewValue is bool newValue && oldValue != newValue)
            {
                if (newValue)
                {
                    // TODO: It seems to find the wrong boxes here...
                    List<ScrollingSelectorBox> scrollingBoxes = [];
                    if (!editor.selectorControl.TryFindAllChildrenOfType(scrollingBoxes, editor.NumberOfDigits))
                        return;

                    foreach (var box in scrollingBoxes)
                    {
                        if (box.DataContext is not ScrollDigit digit)
                            continue;

                        if (!digit.IsHookedToScrollBox)
                        {
                            digit.SyncScrollerToValue(box);
                            digit.HookToScrollingSelectorBox(box);
                        }
                    }
                }
                else
                {
                    editor.SetCurrentValueFromDigits();
                }
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
                editor.ChangeDigitsCollection(newValue);
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
            new FrameworkPropertyMetadata(new ObservableCollection<ScrollDigit>(), FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_digitsProperty = s_digitsPropertyKey.DependencyProperty;
        #endregion

        #endregion

        public ObservableCollection<string> NumberOptions { get; } = [];

        public NumericalScrollingEditor()
        {
            InitializeComponent();
            Loaded += NumericalScrollingEditorLoaded;
        }

        private void NumericalScrollingEditorLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= NumericalScrollingEditorLoaded;
            for (var i = 0; i < 10; i++)
                NumberOptions.Add($"{i}");

            ChangeDigitsCollection(NumberOfDigits);
            SetDigitsFromCurrentValue();
            Digits.Last().IsSelected = true;

            if (NavigatorRelay != default)
            {
                NavigatorRelay.Navigate += (s, e) =>
                {
                    var selectedDigit = Digits.FirstOrDefault(x => x.IsSelected);
                    if (selectedDigit != default)
                    {
                        var isUpOrDown = e == InputEvent.NavigationDirection.Up || e == InputEvent.NavigationDirection.Down;
                        if (isUpOrDown)
                        {
                            // Note: It feels more natural to invert the direction here
                            selectedDigit.FireNavigationEvent(
                                e == InputEvent.NavigationDirection.Up ?
                                InputEvent.NavigationDirection.Down : InputEvent.NavigationDirection.Up);
                        }
                        else
                        {
                            selectedDigit.IsSelected = false;
                            var newIndex = selectedDigit.Index + (e == InputEvent.NavigationDirection.Left ? 1 : -1);
                            newIndex = newIndex < 0 ? NumberOfDigits - 1 : newIndex % NumberOfDigits;
                            Digits[NumberOfDigits - 1 - newIndex].IsSelected = true;
                        }
                    }
                };
            }
        }

        private void ChangeDigitsCollection(int numberOfDigits)
        {
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
            var currentDivisor = 1;
            var currentModulus = 10;
            for (var i = 0; i < NumberOfDigits; i++)
            {
                Digits[NumberOfDigits - 1 - i].Value = ((int)CurrentValue / currentDivisor) % currentModulus;
                currentDivisor *= 10;
                currentModulus *= 10;
            }
        }

        private void SetCurrentValueFromDigits()
        {
            CurrentValue = 0;
            for (var i = 0; i < NumberOfDigits; i++)
                CurrentValue += ((int)Math.Pow(10, i)) * Digits[NumberOfDigits - 1 - i].Value;
        }
    }
}
