using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PokerTracker3000.Interfaces;

using InputEvent = PokerTracker3000.Input.UserInputEvent;

namespace PokerTracker3000.WpfComponents
{
    public partial class ScrollingSelectorBox : UserControl
    {
        #region Dependency properties
        public ObservableCollection<string> Options
        {
            get { return (ObservableCollection<string>)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }
        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(
            nameof(Options),
            typeof(ObservableCollection<string>),
            typeof(ScrollingSelectorBox),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        public IInputRelay NavigatorRelay
        {
            get { return (IInputRelay)GetValue(NavigatorRelayProperty); }
            set { SetValue(NavigatorRelayProperty, value); }
        }
        public static readonly DependencyProperty NavigatorRelayProperty = DependencyProperty.Register(
            nameof(NavigatorRelay),
            typeof(IInputRelay),
            typeof(ScrollingSelectorBox),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool WrapAtEnds
        {
            get { return (bool)GetValue(WrapAtEndsProperty); }
            set { SetValue(WrapAtEndsProperty, value); }
        }
        public static readonly DependencyProperty WrapAtEndsProperty = DependencyProperty.Register(
            nameof(WrapAtEnds),
            typeof(bool),
            typeof(ScrollingSelectorBox),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public TextAlignment OptionTextAlignment
        {
            get { return (TextAlignment)GetValue(OptionTextAlignmentProperty); }
            set { SetValue(OptionTextAlignmentProperty, value); }
        }
        public static readonly DependencyProperty OptionTextAlignmentProperty = DependencyProperty.Register(
            nameof(OptionTextAlignment),
            typeof(TextAlignment),
            typeof(ScrollingSelectorBox),
            new FrameworkPropertyMetadata(TextAlignment.Left, FrameworkPropertyMetadataOptions.AffectsRender));

        public int VerticalSpacing
        {
            get { return (int)GetValue(VerticalSpacingProperty); }
            set { SetValue(VerticalSpacingProperty, value); }
        }
        public static readonly DependencyProperty VerticalSpacingProperty = DependencyProperty.Register(
            nameof(VerticalSpacing),
            typeof(int),
            typeof(ScrollingSelectorBox),
            new FrameworkPropertyMetadata(30, FrameworkPropertyMetadataOptions.AffectsRender, VerticalSpacingUpdated));

        public bool ShowNextAndPreviousValue
        {
            get { return (bool)GetValue(ShowNextAndPreviousValueProperty); }
            set { SetValue(ShowNextAndPreviousValueProperty, value); }
        }
        public static readonly DependencyProperty ShowNextAndPreviousValueProperty = DependencyProperty.Register(
            nameof(ShowNextAndPreviousValue),
            typeof(bool),
            typeof(ScrollingSelectorBox),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, ShowNextAndPreviousUpdated));

        private static void ShowNextAndPreviousUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollingSelectorBox b && e.OldValue is bool oldValue && e.NewValue is bool newValue && oldValue != newValue)
            {
                foreach (var (block, _, currentOpacity) in b._boxes)
                {
                    if (currentOpacity == 1)
                        continue;

                    DoubleAnimation fadeAnimation = new(currentOpacity, newValue ? currentOpacity : 0, b._animationLength);
                    Storyboard sb = new();
                    Storyboard.SetTargetProperty(fadeAnimation, b._pathToOpacityProperty);
                    sb.Children.Add(fadeAnimation);
                    sb.Begin(block, HandoffBehavior.Compose);
                }
            }
        }

        private static void VerticalSpacingUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollingSelectorBox b && e.OldValue is int oldValue && e.NewValue is int newValue && oldValue != newValue)
            {
                var node = b._boxes.First;
                while (node != default)
                {
                    var currentOffset = node.Value.currentOffset;
                    node.ValueRef.currentOffset = (int)((double)currentOffset / (double)oldValue * (double)newValue);
                    node = node.Next;
                }
            }
        }

        #region Read-only properties
        public double TextBoxWidth
        {
            get => (double)GetValue(s_textBoxWidthProperty);
            private set => SetValue(s_textBoxWidthPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_textBoxWidthPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(TextBoxWidth),
            typeof(double),
            typeof(ScrollingSelectorBox),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_textBoxWidthProperty = s_textBoxWidthPropertyKey.DependencyProperty;

        public int CurrentSelectedIndex
        {
            get => (int)GetValue(s_currentSelectedIndexProperty);
            private set => SetValue(s_currentSelectedIndexPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_currentSelectedIndexPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(CurrentSelectedIndex),
            typeof(int),
            typeof(ScrollingSelectorBox),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_currentSelectedIndexProperty = s_currentSelectedIndexPropertyKey.DependencyProperty;
        #endregion

        #endregion

        #region Routed events
        public static readonly RoutedEvent SelectedIndexChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(SelectedIndexChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(int));

        public event RoutedEventHandler SelectedIndexChanged
        {
            add { AddHandler(SelectedIndexChangedEvent, value); }
            remove { RemoveHandler(SelectedIndexChangedEvent, value); }
        }

        private void RaiseSelectedIndexChangedEvent(int newIndex)
        {
            var newEventArgs = new SelectedIndexChangedEventArgs(SelectedIndexChangedEvent, newIndex);
            RaiseEvent(newEventArgs);
        }
        #endregion

        #region Private fields
        private readonly LinkedList<(TextBlock block, int currentOffset, double currentOpacity)> _boxes;
        private readonly PropertyPath _pathToTranslateYProperty = new("RenderTransform.(TranslateTransform.Y)");
        private readonly PropertyPath _pathToOpacityProperty = new("Opacity");
        private readonly TimeSpan _animationLength = new(0, 0, 0, 0, 350);
        private readonly IEasingFunction _movementEasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
        private int _selectedIndex;

        private enum FadeDirection
        {
            In,
            Out,
            NoChange
        };

        private enum EndPosition
        {
            Top,
            Bottom
        };
        #endregion

        public ScrollingSelectorBox()
        {
            InitializeComponent();
            Loaded += ComponentLoaded;
            _boxes = new();
        }

        #region Private methods
        private void ComponentLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ComponentLoaded;

            Initialize();

            if (NavigatorRelay != default)
                NavigatorRelay.Navigate += Navigate;
        }

        private void Initialize()
        {
            _boxes.AddLast((first, -2 * VerticalSpacing, 0));
            _boxes.AddLast((second, -VerticalSpacing, 0.5));
            _boxes.AddLast((third, 0, 1));
            _boxes.AddLast((fourth, VerticalSpacing, 0.5));
            _boxes.AddLast((fifth, 2 * VerticalSpacing, 0));

            var node = _boxes.First;
            first.RenderTransform = new TranslateTransform(0, node!.Value.currentOffset);
            first.Opacity = ShowNextAndPreviousValue ? node!.Value.currentOpacity : 0;

            node = node.Next;
            second.RenderTransform = new TranslateTransform(0, node!.Value.currentOffset);
            second.Opacity = ShowNextAndPreviousValue ? node!.Value.currentOpacity : 0;

            node = node.Next;
            third.RenderTransform = new TranslateTransform(0, node!.Value.currentOffset);
            third.Opacity = node!.Value.currentOpacity;

            node = node.Next;
            fourth.RenderTransform = new TranslateTransform(0, node!.Value.currentOffset);
            fourth.Opacity = ShowNextAndPreviousValue ? node!.Value.currentOpacity : 0;

            node = node.Next;
            fifth.RenderTransform = new TranslateTransform(0, node!.Value.currentOffset);
            fifth.Opacity = ShowNextAndPreviousValue ? node!.Value.currentOpacity : 0;

            if (Options == default)
                return;

            Options.CollectionChanged += (s, e) => SetupBoxText();
            SetupBoxText();
        }

        public void SetupBoxText(ObservableCollection<string>? options = default, int forcedSelectedIndex = -1)
        {
            options ??= Options;
            if (options == default)
                return;

            // Find the middle box
            List<TextBlock> boxes = [];
            var middleBoxIndex = 0;
            foreach (var (block, currentOffset, _) in _boxes)
            {
                boxes.Add(block);
                if (currentOffset == 0)
                    middleBoxIndex = boxes.Count - 1;
            }

            // Set-up text
            var selectedIndexAtStart = _selectedIndex;
            _selectedIndex = forcedSelectedIndex >= 0 ? forcedSelectedIndex : (options.Count > _selectedIndex ? _selectedIndex : options.Count);
            CurrentSelectedIndex = _selectedIndex;

            boxes[middleBoxIndex].Text = options.Count > _selectedIndex ? options[_selectedIndex] : string.Empty;

            var firstBoxTextIndex = (_selectedIndex - 2) < 0 ? (WrapAtEnds ? options.Count - 2 : -1) : _selectedIndex - 2;
            var secondBoxTextIndex = (_selectedIndex - 1) < 0 ? (WrapAtEnds ? options.Count - 1 : -1) : _selectedIndex - 1;
            var fourthBoxTextIndex = (_selectedIndex + 1) > (options.Count - 1) ? (WrapAtEnds ? 0 : -1) : _selectedIndex + 1;
            var fifthBoxTextIndex = (_selectedIndex + 2) > (options.Count - 1) ? (WrapAtEnds ? 1 : -1) : _selectedIndex + 2;

            boxes[GetWrappedOffsetIndex(middleBoxIndex, -1, 5)].Text = ((secondBoxTextIndex < 0) || (secondBoxTextIndex > options.Count - 1)) ? string.Empty : options[secondBoxTextIndex];
            boxes[GetWrappedOffsetIndex(middleBoxIndex, -2, 5)].Text = ((firstBoxTextIndex < 0) || (firstBoxTextIndex > options.Count - 1)) ? string.Empty : options[firstBoxTextIndex];
            boxes[GetWrappedOffsetIndex(middleBoxIndex, 1, 5)].Text = ((fourthBoxTextIndex < 0) || (fourthBoxTextIndex > options.Count - 1)) ? string.Empty : options[fourthBoxTextIndex];
            boxes[GetWrappedOffsetIndex(middleBoxIndex, 2, 5)].Text = ((fifthBoxTextIndex < 0) || (fifthBoxTextIndex > options.Count - 1)) ? string.Empty : options[fifthBoxTextIndex];

            // The above works for every initial number of options, except for 2
            // and when WrapAtEnds is true, so we deal with it separately
            if (WrapAtEnds && options.Count == 2)
                fifth.Text = options[0];

            // Set-up width
            var width = 0.0;
            foreach (var option in options)
                width = Math.Max(width, MeasureWidthOfText(option));

            TextBoxWidth = width;
            if (forcedSelectedIndex < 0 && selectedIndexAtStart != _selectedIndex)
                RaiseSelectedIndexChangedEvent(_selectedIndex);
        }

        private double MeasureWidthOfText(string text)
        {
            var t = new FormattedText(text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(first.FontFamily, first.FontStyle, first.FontWeight, first.FontStretch),
                first.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                VisualTreeHelper.GetDpi(first).PixelsPerDip);
            return t.Width;
        }

        private void Navigate(object? sender, InputEvent.NavigationDirection e)
        {
            if (!WrapAtEnds)
            {
                if (e == InputEvent.NavigationDirection.Down && _selectedIndex == Options.Count - 1)
                    return;
                else if (e == InputEvent.NavigationDirection.Up && _selectedIndex == 0)
                    return;
            }

            Navigate(e);
        }

        private void Navigate(InputEvent.NavigationDirection e)
        {
            Predicate<int>? isItemThatWrapped = default;
            Func<int, FadeDirection>? itemFadeDirection = default;
            switch (e)
            {
                case InputEvent.NavigationDirection.Down:
                    var head = _boxes.First!;
                    _boxes.RemoveFirst();
                    _boxes.AddLast(head);
                    isItemThatWrapped = (int i) => i == _boxes.Count - 1;
                    itemFadeDirection = (int i) => (i == 0 || i == 1) ? FadeDirection.Out : ((i == 2 || i == 3) ? FadeDirection.In : FadeDirection.NoChange);
                    break;

                case InputEvent.NavigationDirection.Up:
                    var tail = _boxes.Last!;
                    _boxes.RemoveLast();
                    _boxes.AddFirst(tail);
                    isItemThatWrapped = (int i) => i == 0;
                    itemFadeDirection = (int i) => (i == 1 || i == 2) ? FadeDirection.In : ((i == 3 || i == 4) ? FadeDirection.Out : FadeDirection.NoChange);
                    break;
            }

            if (isItemThatWrapped == default || itemFadeDirection == default)
                return;

            var isUp = e == InputEvent.NavigationDirection.Up;
            var originalSelectedIndex = _selectedIndex;
            _selectedIndex += (isUp ? -1 : 1);
            if (WrapAtEnds && (_selectedIndex < 0 || _selectedIndex > (Options.Count - 1)))
                _selectedIndex = _selectedIndex < 0 ? (Options.Count - 1) : 0;
            CurrentSelectedIndex = _selectedIndex;

            LinkedListNode<(TextBlock block, int currentOffset, double currentOpacity)>? node = default;
            for (var i = 0; i < _boxes.Count; i++)
            {
                node = (node == default) ? _boxes.First : node.Next;
                var (block, currentOffset, currentOpacity) = node!.Value;

                (var sb, var newOffset, var newOpacity) = isItemThatWrapped(i) ?
                    GetStoryBoardForEndPosition(currentOffset, isUp ? EndPosition.Top : EndPosition.Bottom) :
                    GetStoryBoardForItemAnimation(currentOffset, currentOpacity, e, itemFadeDirection(i));

                if (isItemThatWrapped(i))
                {
                    var willWrap = isUp ? ((_selectedIndex - 2) < 0) : ((_selectedIndex + 2) > (Options.Count - 1));
                    if (isUp)
                        block.Text = !willWrap ? Options[_selectedIndex - 2] : (WrapAtEnds ? Options[Math.Max(0, Options.Count - 2 + _selectedIndex)] : string.Empty);
                    else
                        block.Text = !willWrap ? Options[_selectedIndex + 2] : (WrapAtEnds ? Options[(_selectedIndex + 2) % Options.Count] : string.Empty);
                }
                sb.Begin(block, HandoffBehavior.Compose);

                node.ValueRef.currentOffset = newOffset;
                node.ValueRef.currentOpacity = newOpacity;

                if (!ShowNextAndPreviousValue && newOpacity < 1)
                    block.Opacity = 0;
            }

            if (_selectedIndex != originalSelectedIndex)
                RaiseSelectedIndexChangedEvent(_selectedIndex);
        }

        private (Storyboard, int, double) GetStoryBoardForEndPosition(int currentOffset, EndPosition position)
        {
            var newOffset = VerticalSpacing * 2 * (position == EndPosition.Top ? -1 : 1);
            Storyboard sb = new();
            DoubleAnimation moveAnimation = new(currentOffset, newOffset, _animationLength);
            Storyboard.SetTargetProperty(moveAnimation, _pathToTranslateYProperty);
            sb.Children.Add(moveAnimation);
            return (sb, newOffset, 0.0);
        }

        private (Storyboard, int, double) GetStoryBoardForItemAnimation(int currentOffset, double currentOpacity, InputEvent.NavigationDirection direction, FadeDirection fadeType)
        {
            var newOffset = currentOffset + (VerticalSpacing * (direction == InputEvent.NavigationDirection.Down ? -1 : 1));
            var newOpacity = currentOpacity + (fadeType == FadeDirection.NoChange ? 0 : (fadeType == FadeDirection.In ? 0.5 : -0.5));
            DoubleAnimation moveAnimation = new(currentOffset, newOffset, _animationLength) { EasingFunction = _movementEasingFunction };

            var animationShouldShow = !(!ShowNextAndPreviousValue && newOpacity < 1);
            DoubleAnimation fadeAnimation = new(animationShouldShow ? currentOpacity : 0, animationShouldShow ? newOpacity : 0, _animationLength);

            Storyboard sb = new();
            Storyboard.SetTargetProperty(moveAnimation, _pathToTranslateYProperty);
            Storyboard.SetTargetProperty(fadeAnimation, _pathToOpacityProperty);
            sb.Children.Add(moveAnimation);
            sb.Children.Add(fadeAnimation);
            return (sb, newOffset, newOpacity);
        }

        private static int GetWrappedOffsetIndex(int from, int offset, int length)
        {
            var rawNewIndex = from + offset;
            if (rawNewIndex < 0)
                return length + rawNewIndex;
            return rawNewIndex % length;

        }
        #endregion
    }
}
