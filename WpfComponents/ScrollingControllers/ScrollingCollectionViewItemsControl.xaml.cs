using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.VisualBasic;
using PokerTracker3000.GameSession.Sound;
using PokerTracker3000.Interfaces;

using InputEvent = PokerTracker3000.Input.UserInputEvent;
using NavigationEventArgs = PokerTracker3000.Interfaces.IInputRelay.NavigationEventArgs;

namespace PokerTracker3000.WpfComponents
{
    public partial class ScrollingCollectionViewItemsControl : UserControl
    {
        #region Dependency properties
        public ObservableCollection<SoundEffectOption> Options
        {
            get { return (ObservableCollection<SoundEffectOption>)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }
        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(
            nameof(Options),
            typeof(ObservableCollection<SoundEffectOption>),
            typeof(ScrollingCollectionViewItemsControl),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender, InitializeControl));

        public IInputRelay NavigatorRelay
        {
            get { return (IInputRelay)GetValue(NavigatorRelayProperty); }
            set { SetValue(NavigatorRelayProperty, value); }
        }
        public static readonly DependencyProperty NavigatorRelayProperty = DependencyProperty.Register(
            nameof(NavigatorRelay),
            typeof(IInputRelay),
            typeof(ScrollingCollectionViewItemsControl),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender, InitializeControl));

        public TextAlignment OptionTextAlignment
        {
            get { return (TextAlignment)GetValue(OptionTextAlignmentProperty); }
            set { SetValue(OptionTextAlignmentProperty, value); }
        }
        public static readonly DependencyProperty OptionTextAlignmentProperty = DependencyProperty.Register(
            nameof(OptionTextAlignment),
            typeof(TextAlignment),
            typeof(ScrollingCollectionViewItemsControl),
            new FrameworkPropertyMetadata(TextAlignment.Left, FrameworkPropertyMetadataOptions.AffectsRender));

        public int VerticalSpacing
        {
            get { return (int)GetValue(VerticalSpacingProperty); }
            set { SetValue(VerticalSpacingProperty, value); }
        }
        public static readonly DependencyProperty VerticalSpacingProperty = DependencyProperty.Register(
            nameof(VerticalSpacing),
            typeof(int),
            typeof(ScrollingCollectionViewItemsControl),
            new FrameworkPropertyMetadata(30, FrameworkPropertyMetadataOptions.AffectsRender, VerticalSpacingUpdated));

        public bool HighlightSelectedElement
        {
            get { return (bool)GetValue(HighlightSelectedElementProperty); }
            set { SetValue(HighlightSelectedElementProperty, value); }
        }
        public static readonly DependencyProperty HighlightSelectedElementProperty = DependencyProperty.Register(
            nameof(HighlightSelectedElement),
            typeof(bool),
            typeof(ScrollingCollectionViewItemsControl),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, HighlightSelectedElementUpdated));

        public double NonSelectedElementsOpacity
        {
            get { return (double)GetValue(NonSelectedElementsOpacityProperty); }
            set { SetValue(NonSelectedElementsOpacityProperty, value); }
        }
        public static readonly DependencyProperty NonSelectedElementsOpacityProperty = DependencyProperty.Register(
            nameof(NonSelectedElementsOpacity),
            typeof(double),
            typeof(ScrollingCollectionViewItemsControl),
            new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.AffectsRender, NonSelectedElementsOpacityUpdated));

        public double ElementOutsideVisibleRangeOpacity
        {
            get { return (double)GetValue(ElementOutsideVisibleRangeOpacityProperty); }
            set { SetValue(ElementOutsideVisibleRangeOpacityProperty, value); }
        }
        public static readonly DependencyProperty ElementOutsideVisibleRangeOpacityProperty = DependencyProperty.Register(
            nameof(ElementOutsideVisibleRangeOpacity),
            typeof(double),
            typeof(ScrollingCollectionViewItemsControl),
            new FrameworkPropertyMetadata(0.3, FrameworkPropertyMetadataOptions.AffectsRender, ElementsOutsideOpacityUpdated));

        public Brush SelectedElementColor
        {
            get => (Brush)GetValue(SelectedElementColorProperty);
            set => SetValue(SelectedElementColorProperty, value);
        }
        public static readonly DependencyProperty SelectedElementColorProperty = DependencyProperty.Register(
            nameof(SelectedElementColor),
            typeof(Brush),
            typeof(ScrollingCollectionViewItemsControl),
            new FrameworkPropertyMetadata(new SolidColorBrush(new() { A = 255, R = 255, G = 255, B = 255 }), FrameworkPropertyMetadataOptions.AffectsRender));

        public int NumberOfVisibleOptions
        {
            get { return (int)GetValue(NumberOfVisibleOptionsProperty); }
            set { SetValue(NumberOfVisibleOptionsProperty, value); }
        }
        public static readonly DependencyProperty NumberOfVisibleOptionsProperty = DependencyProperty.Register(
            nameof(NumberOfVisibleOptions),
            typeof(int),
            typeof(ScrollingCollectionViewItemsControl),
            new FrameworkPropertyMetadata(5, FrameworkPropertyMetadataOptions.AffectsRender, NumberOfVisibleElementsUpdated));

        private static void InitializeControl(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollingCollectionViewItemsControl b && !b._controlInitialized)
            {
                b.Initialize();
            }
        }

        private static void HighlightSelectedElementUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollingCollectionViewItemsControl b && e.OldValue is bool oldValue && e.NewValue is bool newValue && oldValue != newValue)
            {
                for (var i = 0; i < b._boxes.Count; i++)
                {
                    if ((i == 0) || (i == (b._boxes.Count - 1)))
                    {
                        b._boxes[i].CurrentOpacity = b.ElementOutsideVisibleRangeOpacity;
                    }
                    else if (i == b.CurrentListViewIndex)
                    {
                        b._boxes[i].CurrentOpacity = newValue ? 1.0 : b.NonSelectedElementsOpacity;
                    }
                    else
                    {
                        b._boxes[i].CurrentOpacity = b.NonSelectedElementsOpacity;
                    }
                }
                b.StyleTextElements();
            }
        }

        private static void NonSelectedElementsOpacityUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollingCollectionViewItemsControl b && e.OldValue is double oldValue && e.NewValue is double newValue && oldValue != newValue)
            {
                for (var i = 0; i < b._boxes.Count; i++)
                {
                    if ((i == 0) || (i == (b._boxes.Count - 1)) || (i == b.CurrentListViewIndex))
                        continue;

                    b._boxes[i].CurrentOpacity = newValue;
                }
                b.StyleTextElements();
            }
        }

        private static void ElementsOutsideOpacityUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollingCollectionViewItemsControl b && e.OldValue is double oldValue && e.NewValue is double newValue && oldValue != newValue)
            {
                if (b._boxes.Count > 0)
                {
                    b._boxes[0].CurrentOpacity = newValue;
                    b._boxes[^1].CurrentOpacity = newValue;
                    b.StyleTextElements();
                }
            }
        }

        private static void VerticalSpacingUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollingCollectionViewItemsControl b && e.OldValue is int oldValue && e.NewValue is int newValue && oldValue != newValue)
            {
                for (var i = 0; i < b._boxes.Count; i++)
                {
                    if (oldValue == 0)
                        oldValue = 1;

                    if (newValue == 0)
                        newValue = 1;

                    var currentOffset = b._boxes[i].CurrentOffset;
                    b._boxes[i].CurrentOffset = (int)((double)currentOffset / (double)oldValue * (double)newValue);
                    b._boxes[i].Block.RenderTransform = new TranslateTransform(0, b._boxes[i].CurrentOffset);
                }
                b.mainPanel.Height = b.NumberOfVisibleOptions * (double)newValue;
            }
        }

        private static void NumberOfVisibleElementsUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollingCollectionViewItemsControl b &&
                e.OldValue is int oldValue &&
                e.NewValue is int newValue &&
                oldValue != newValue &&
                b._controlInitialized)
            {
                b.InitializeElements(newValue);
                b.CheckIfItemsOutsideOfList(newValue);
                b.SetupBlockText(b.Options);
                b.StyleTextElements();
            }
        }

        #region Read-only properties
        public int CurrentSelectedIndex
        {
            get => (int)GetValue(s_currentSelectedIndexProperty);
            private set => SetValue(s_currentSelectedIndexPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_currentSelectedIndexPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(CurrentSelectedIndex),
            typeof(int),
            typeof(ScrollingCollectionViewItemsControl),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_currentSelectedIndexProperty = s_currentSelectedIndexPropertyKey.DependencyProperty;

        public int CurrentListViewIndex
        {
            get => (int)GetValue(s_currentListViewIndexProperty);
            private set => SetValue(s_currentListViewIndexPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_currentListViewIndexPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(CurrentListViewIndex),
            typeof(int),
            typeof(ScrollingCollectionViewItemsControl),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_currentListViewIndexProperty = s_currentListViewIndexPropertyKey.DependencyProperty;

        public int CurrentLowestVisibleIndex
        {
            get => (int)GetValue(s_currentLowestVisibleIndexProperty);
            private set => SetValue(s_currentLowestVisibleIndexPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_currentLowestVisibleIndexPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(CurrentLowestVisibleIndex),
            typeof(int),
            typeof(ScrollingCollectionViewItemsControl),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_currentLowestVisibleIndexProperty = s_currentLowestVisibleIndexPropertyKey.DependencyProperty;

        public bool HasElementsAboveTop
        {
            get => (bool)GetValue(s_hasElementsAboveTopProperty);
            private set => SetValue(s_hasElementsAboveTopPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_hasElementsAboveTopPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(HasElementsAboveTop),
            typeof(bool),
            typeof(ScrollingCollectionViewItemsControl),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_hasElementsAboveTopProperty = s_hasElementsAboveTopPropertyKey.DependencyProperty;

        public bool HasElementsBelowLast
        {
            get => (bool)GetValue(s_hasElementsBelowLastProperty);
            private set => SetValue(s_hasElementsBelowLastPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_hasElementsBelowLastPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(HasElementsBelowLast),
            typeof(bool),
            typeof(ScrollingCollectionViewItemsControl),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_hasElementsBelowLastProperty = s_hasElementsBelowLastPropertyKey.DependencyProperty;
        #endregion

        #endregion

        #region Routed events
        public static readonly RoutedEvent SelectedIndexChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(SelectedIndexChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ScrollingCollectionViewItemsControl));

        public static readonly RoutedEvent ControlInitializedEvent = EventManager.RegisterRoutedEvent(
            nameof(ControlInitialized),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ScrollingCollectionViewItemsControl));

        public event RoutedEventHandler SelectedIndexChanged
        {
            add { AddHandler(SelectedIndexChangedEvent, value); }
            remove { RemoveHandler(SelectedIndexChangedEvent, value); }
        }

        public event RoutedEventHandler ControlInitialized
        {
            add { AddHandler(ControlInitializedEvent, value); }
            remove { RemoveHandler(ControlInitializedEvent, value); }
        }
        #endregion

        #region Private fields
        private sealed record TextElement
        {
            public required TextBlock Block { get; init; }

            public int CurrentOffset { get; set; }

            public double CurrentOpacity { get; set; }
        }
        private readonly record struct MoveAnimation(Storyboard Board, int NewOffset, double NewOpacity);

        private readonly List<TextElement> _boxes;
        private readonly PropertyPath _pathToTranslateYProperty = new("RenderTransform.(TranslateTransform.Y)");
        private readonly PropertyPath _pathToOpacityProperty = new("Opacity");
        private readonly TimeSpan _animationLength = new(0, 0, 0, 0, 350);
        private readonly IEasingFunction _movementEasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
        private double _currentMaxTextLength = 0.0;
        private bool _controlInitialized = false;
        private const string EmptyText = "";
        private enum FadeDirection
        {
            In,
            Out,
            NoChange
        };
        private enum ShiftDirection
        {
            Up,
            Down
        };
        #endregion

        public ScrollingCollectionViewItemsControl()
        {
            InitializeComponent();
            Loaded += ComponentLoaded;
            _boxes = [];
        }

        public void EnsureTopVisibleEffectOptionSelected()
        {
            while (CurrentSelectedIndex > CurrentLowestVisibleIndex)
                Navigate(InputEvent.NavigationDirection.Up);
        }

        public void EnsureBottomVisibleEffectOptionSelected()
        {
            while (CurrentSelectedIndex < (CurrentLowestVisibleIndex + Math.Min(Options.Count, NumberOfVisibleOptions) - 1))
                Navigate(InputEvent.NavigationDirection.Down);
        }

        #region Private methods
        private void ComponentLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ComponentLoaded;
            Initialize();
        }

        private void Initialize()
        {
            if (Options == null || NavigatorRelay == null || _controlInitialized)
                return;

            InitializeElements(NumberOfVisibleOptions);
            CheckIfItemsOutsideOfList(NumberOfVisibleOptions);
            SetupBlockText(Options);
            StyleTextElements();

            if (NavigatorRelay != default)
            {
                WeakEventManager<IInputRelay, NavigationEventArgs>
                    .AddHandler(NavigatorRelay, nameof(IInputRelay.Navigate), Navigate);
            }

            if (Options != default)
            {
                WeakEventManager<ObservableCollection<SoundEffectOption>, NotifyCollectionChangedEventArgs>
                    .AddHandler(Options, nameof(Options.CollectionChanged), SetupElements);

                // We need to manually hook to the elements of the list
                // here as we don't directly bind to the name property
                foreach (var option in Options)
                {
                    PropertyChangedEventManager
                        .AddHandler(option, OptionNameChanged, nameof(SoundEffectOption.Name));
                }
            }

            _controlInitialized = true;
            RaiseEvent(new(ControlInitializedEvent));
        }

        private void OptionNameChanged(object? sender, PropertyChangedEventArgs e)
        {
            SetupBlockText(Options);
            StyleTextElements();
        }

        private void SetupElements(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is ObservableCollection<SoundEffectOption> collection)
            {
                var moveUp = false;
                var moveDown = false;
                // If the collection is empty, we re-init the list to clear out everything
                if (collection.Count == 0)
                {
                    InitializeElements(NumberOfVisibleOptions);
                }
                else
                {
                    var affectedElementBelowCurrent = true;
                    // Only if the affected element is the current or above the current, it can affect us
                    if (e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        affectedElementBelowCurrent = e.OldStartingIndex > CurrentSelectedIndex;
                        // We can disregard removing the event listeners, as it won't block GC
                        // anyway. However, for symmetry, let's just remove what we can
                        if (e.OldItems != null)
                        {
                            foreach (var item in e.OldItems)
                            {
                                if (item is SoundEffectOption option)
                                {
                                    PropertyChangedEventManager
                                        .RemoveHandler(option, OptionNameChanged, nameof(SoundEffectOption.Name));
                                }
                            }
                        }
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        affectedElementBelowCurrent = e.NewStartingIndex > CurrentSelectedIndex;
                        // We also need the manual hook here to deal with new elements
                        if (e.NewItems != null)
                        {
                            foreach (var item in e.NewItems)
                            {
                                if (item is SoundEffectOption option)
                                {
                                    PropertyChangedEventManager
                                        .AddHandler(option, OptionNameChanged, nameof(SoundEffectOption.Name));
                                }
                            }
                        }
                    }

                    moveUp = !affectedElementBelowCurrent && e.Action == NotifyCollectionChangedAction.Remove;
                    moveDown = !affectedElementBelowCurrent && e.Action == NotifyCollectionChangedAction.Add;
                }

                CheckIfItemsOutsideOfList(NumberOfVisibleOptions);
                SetupBlockText(collection);
                StyleTextElements();

                if (moveUp)
                    Navigate(this, new NavigationEventArgs() { Direction = InputEvent.NavigationDirection.Up });
                else if (moveDown)
                    Navigate(this, new NavigationEventArgs() { Direction = InputEvent.NavigationDirection.Down });
            }
        }

        private void InitializeElements(int numberOfElements)
        {
            _boxes.Clear();
            mainPanel.Children.Clear();

            mainPanel.Height = numberOfElements * VerticalSpacing;

            if (Options == default)
                return;

            // Add two extra elements to occur for the two just outside the range
            for (var i = 0; i < numberOfElements + 2; i++)
            {
                var currentOffset = (i - 1) * VerticalSpacing;
                var currentOpacity = NonSelectedElementsOpacity;

                if ((i == 0) || i == (numberOfElements + 1))
                    currentOpacity = ElementOutsideVisibleRangeOpacity;
                else if (i == CurrentListViewIndex && HighlightSelectedElement)
                    currentOpacity = 1.0;

                var tb = new TextBlock()
                {
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new TranslateTransform(0, currentOffset),
                    Opacity = currentOpacity,
                    Margin = new Thickness(0)
                };

                mainPanel.Children.Add(tb);
                _boxes.Add(new TextElement()
                {
                    Block = tb,
                    CurrentOffset = currentOffset,
                    CurrentOpacity = currentOpacity
                });
            }
        }

        private void SetupBlockText(ObservableCollection<SoundEffectOption>? options = default)
        {
            if (options == default || options.Count == 0)
                return;

            if (CurrentLowestVisibleIndex > 0)
            {
                _boxes[0].Block.Text = Options[CurrentLowestVisibleIndex - 1].Name;
            }
            else
            {
                _boxes[0].Block.Text = EmptyText;
            }

            // Now, iterate from next box
            var optionIdx = CurrentLowestVisibleIndex;
            _currentMaxTextLength = 0;
            for (var i = 1; i < _boxes.Count; i++)
            {
                _boxes[i].Block.Text = (optionIdx < Options.Count) ? Options[optionIdx].Name : EmptyText;
                optionIdx++;
                _currentMaxTextLength = Math.Max(MeasureWidthOfText(_boxes[i].Block), _currentMaxTextLength);
            }
            mainPanel.Margin = new Thickness(0.0, 0.0, 2 * _currentMaxTextLength, 0.0);

        }

        private void StyleTextElements()
        {
            for (var i = 0; i < _boxes.Count; i++)
            {
                _boxes[i].Block.FontSize = FontSize;
                _boxes[i].Block.FontFamily = FontFamily;
                _boxes[i].Block.TextAlignment = OptionTextAlignment;
                _boxes[i].Block.Width = _currentMaxTextLength;
                _boxes[i].Block.Opacity = _boxes[i].CurrentOpacity;

                if (i == CurrentListViewIndex && HighlightSelectedElement)
                {
                    _boxes[i].Block.Foreground = SelectedElementColor;
                    _boxes[i].Block.FontWeight = FontWeights.Bold;
                }
                else
                {
                    _boxes[i].Block.Foreground = Foreground;
                    _boxes[i].Block.FontWeight = FontWeights.Normal;
                }
            }
        }

        private void Navigate(object? sender, NavigationEventArgs e)
        {
            if (Options.Count == 0)
                return;

            if (e.Direction == InputEvent.NavigationDirection.Down && (CurrentSelectedIndex == (Options.Count - 1)))
                return;
            else if (e.Direction == InputEvent.NavigationDirection.Up && CurrentSelectedIndex == 0)
                return;

            Navigate(e.Direction);
        }

        private void Navigate(InputEvent.NavigationDirection e)
        {
            var selectedIndexBefore = CurrentSelectedIndex;
            switch (e)
            {
                case InputEvent.NavigationDirection.Down:
                    {
                        var isAtLastElement = CurrentListViewIndex == NumberOfVisibleOptions;
                        if (isAtLastElement && HasElementsBelowLast)
                        {
                            // Now, we want to shift all elements upwards. The top element should fade out.

                            // First, move the current top node to the end of the list. It's an invisible move.
                            var head = _boxes.First();
                            var tail = _boxes.Last();
                            head.CurrentOffset = tail.CurrentOffset + VerticalSpacing;
                            _boxes.RemoveAt(0);
                            _boxes.Add(head);

                            for (var i = 0; i < _boxes.Count; i++)
                            {
                                var moveAnimation =
                                    GetMoveAnimation(_boxes[i].CurrentOffset,
                                    _boxes[i].CurrentOpacity,
                                    i == CurrentListViewIndex - 1,
                                    ShiftDirection.Up,
                                    (i == 0) ? FadeDirection.Out :
                                    (i == NumberOfVisibleOptions) ? FadeDirection.In : FadeDirection.NoChange);


                                // The item that we moved to the bottom should possible get new text
                                if (i == _boxes.Count - 1)
                                {
                                    _boxes[i].Block.Text = Options.Count > (CurrentSelectedIndex + 2) ?
                                        Options[CurrentSelectedIndex + 2].Name : EmptyText;
                                }

                                moveAnimation.Board.Begin(_boxes[i].Block, HandoffBehavior.Compose);
                                _boxes[i].CurrentOffset = moveAnimation.NewOffset;
                                _boxes[i].CurrentOpacity = moveAnimation.NewOpacity;
                            }

                            CurrentLowestVisibleIndex++;

                        }
                        else
                        {
                            _boxes[CurrentListViewIndex].CurrentOpacity = NonSelectedElementsOpacity;
                            _boxes[CurrentListViewIndex + 1].CurrentOpacity = 1.0;
                            CurrentListViewIndex++;
                        }
                        CurrentSelectedIndex++;
                    }
                    break;

                case InputEvent.NavigationDirection.Up:
                    {
                        var isAtTopElement = CurrentListViewIndex == 1;
                        if (isAtTopElement && HasElementsAboveTop)
                        {
                            // Now, we want to shift all elements downwards. The bottom element should fade out.

                            // First, move the current bottom node to the end of the list. It's an invisible move.
                            var tail = _boxes.Last();
                            var head = _boxes.First();
                            tail.CurrentOffset = head.CurrentOffset - VerticalSpacing;
                            _boxes.RemoveAt(_boxes.Count - 1);
                            _boxes.Insert(0, tail);

                            for (var i = _boxes.Count - 1; i >= 0; i--)
                            {
                                var moveAnimation =
                                    GetMoveAnimation(_boxes[i].CurrentOffset,
                                    _boxes[i].CurrentOpacity,
                                    i == CurrentListViewIndex + 1,
                                    ShiftDirection.Down,
                                    (i == _boxes.Count - 1) ? FadeDirection.Out :
                                    (i == 1) ? FadeDirection.In : FadeDirection.NoChange);


                                // The item that we moved to the bottom should possible get new text
                                if (i == 0)
                                {
                                    _boxes[i].Block.Text = (CurrentSelectedIndex - 2) >= 0 ?
                                        Options[CurrentSelectedIndex - 2].Name : EmptyText;
                                }

                                moveAnimation.Board.Begin(_boxes[i].Block, HandoffBehavior.Compose);
                                _boxes[i].CurrentOffset = moveAnimation.NewOffset;
                                _boxes[i].CurrentOpacity = moveAnimation.NewOpacity;
                            }

                            CurrentLowestVisibleIndex--;

                        }
                        else
                        {
                            _boxes[CurrentListViewIndex].CurrentOpacity = NonSelectedElementsOpacity;
                            _boxes[CurrentListViewIndex - 1].CurrentOpacity = 1.0;
                            CurrentListViewIndex--;
                        }
                        CurrentSelectedIndex--;
                    }
                    break;

                default:
                    break;
            }

            StyleTextElements();
            CheckIfItemsOutsideOfList(NumberOfVisibleOptions);

            if (CurrentSelectedIndex != selectedIndexBefore)
                RaiseEvent(new(SelectedIndexChangedEvent));

        }

        private MoveAnimation GetMoveAnimation(int currentOffset,
                                               double currentOpacity,
                                               bool isPreviouslySelectedElement,
                                               ShiftDirection shiftDirection,
                                               FadeDirection fadeDirection)
        {
            var newOffset = currentOffset + VerticalSpacing * (shiftDirection == ShiftDirection.Up ? -1 : 1);
            Storyboard sb = new();
            DoubleAnimation moveAnimation = new(
                currentOffset,
                newOffset,
                _animationLength)
            {
                EasingFunction = _movementEasingFunction
            };
            Storyboard.SetTargetProperty(moveAnimation, _pathToTranslateYProperty);
            sb.Children.Add(moveAnimation);

            var newOpacity = isPreviouslySelectedElement ? NonSelectedElementsOpacity : currentOpacity;
            if (fadeDirection != FadeDirection.NoChange)
            {
                newOpacity = (fadeDirection == FadeDirection.In) ? 1.0 : ElementOutsideVisibleRangeOpacity;
                DoubleAnimation fadeAnimation = new(currentOpacity, newOpacity, _animationLength, FillBehavior.Stop);
                Storyboard.SetTargetProperty(fadeAnimation, _pathToOpacityProperty);
                sb.Children.Add(fadeAnimation);
            }

            return new(sb, newOffset, newOpacity);
        }

        private void CheckIfItemsOutsideOfList(int numberOfElements)
        {
            // Check if there are items outside of the list
            var hasMoreOptionsThanElements = Options.Count > numberOfElements;
            HasElementsAboveTop = hasMoreOptionsThanElements && CurrentLowestVisibleIndex > 0;
            HasElementsBelowLast = hasMoreOptionsThanElements && ((CurrentLowestVisibleIndex + numberOfElements) <= (Options.Count - 1));
        }

        private static double MeasureWidthOfText(TextBlock tb)
        {
            var t = new FormattedText(tb.Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch),
                tb.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                VisualTreeHelper.GetDpi(tb).PixelsPerDip);
            return 1.1 * t.Width;
        }
        #endregion
    }
}
