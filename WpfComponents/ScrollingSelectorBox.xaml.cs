using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

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

        public ISelectorBoxNavigator NavigatorRelay
        {
            get { return (ISelectorBoxNavigator)GetValue(NavigatorRelayProperty); }
            set { SetValue(NavigatorRelayProperty, value); }
        }
        public static readonly DependencyProperty NavigatorRelayProperty = DependencyProperty.Register(
            nameof(NavigatorRelay),
            typeof(ISelectorBoxNavigator),
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

        public int SelectedIndex
        {
            get => (int)GetValue(s_selectedIndexProperty);
            private set => SetValue(s_selectedIndexPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_selectedIndexPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(SelectedIndex),
            typeof(int),
            typeof(ScrollingSelectorBox),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_selectedIndexProperty = s_selectedIndexPropertyKey.DependencyProperty;

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
        #endregion

        #region Private fields
        private readonly LinkedList<(TextBlock block, int currentOffset, double currentOpacity)> _boxes;
        private readonly PropertyPath _pathToTranslateYProperty = new("RenderTransform.(TranslateTransform.Y)");
        private readonly PropertyPath _pathToOpacityProperty = new("Opacity");
        private readonly TimeSpan _animationLength = new(0, 0, 0, 0, 350);
        private readonly IEasingFunction _movementEasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
        private const int DistanceBetweenItems = 30;

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
            _boxes.AddLast((first, -2 * DistanceBetweenItems, 0));
            _boxes.AddLast((second, -DistanceBetweenItems, 0.5));
            _boxes.AddLast((third, 0, 1));
            _boxes.AddLast((fourth, DistanceBetweenItems, 0.5));
            _boxes.AddLast((fifth, 2 * DistanceBetweenItems, 0));

            var node = _boxes.First;
            first.RenderTransform = new TranslateTransform(0, node!.Value.currentOffset);
            first.Opacity = node!.Value.currentOpacity;

            node = node.Next;
            second.RenderTransform = new TranslateTransform(0, node!.Value.currentOffset);
            second.Opacity = node!.Value.currentOpacity;

            node = node.Next;
            third.RenderTransform = new TranslateTransform(0, node!.Value.currentOffset);
            third.Opacity = node!.Value.currentOpacity;

            node = node.Next;
            fourth.RenderTransform = new TranslateTransform(0, node!.Value.currentOffset);
            fourth.Opacity = node!.Value.currentOpacity;

            node = node.Next;
            fifth.RenderTransform = new TranslateTransform(0, node!.Value.currentOffset);
            fifth.Opacity = node!.Value.currentOpacity;

            if (Options == default)
                return;

            Options.CollectionChanged += OptionsCollectionChanged;
            SetupBoxText();
        }

        private void SetupBoxText()
        {
            // Set-up text
            SelectedIndex = Options.Count > SelectedIndex ? SelectedIndex : Options.Count;
            third.Text = Options.Count > SelectedIndex ? Options[SelectedIndex] : string.Empty;

            var firstBoxTextIndex = (SelectedIndex - 2) < 0 ? (WrapAtEnds ? Options.Count - 2 : -1) : SelectedIndex - 2;
            var secondBoxTextIndex = (SelectedIndex - 1) < 0 ? (WrapAtEnds ? Options.Count - 1 : -1) : SelectedIndex - 1;
            var fourthBoxTextIndex = (SelectedIndex + 1) > (Options.Count - 1) ? (WrapAtEnds ? 0 : -1) : SelectedIndex + 1;
            var fifthBoxTextIndex = (SelectedIndex + 2) > (Options.Count - 1) ? (WrapAtEnds ? 1 : -1) : SelectedIndex + 2;

            second.Text = ((secondBoxTextIndex < 0) || (secondBoxTextIndex > Options.Count - 1)) ? string.Empty : Options[secondBoxTextIndex];
            first.Text = ((firstBoxTextIndex < 0) || (firstBoxTextIndex > Options.Count - 1)) ? string.Empty : Options[firstBoxTextIndex];
            fourth.Text = ((fourthBoxTextIndex < 0) || (fourthBoxTextIndex > Options.Count - 1)) ? string.Empty : Options[fourthBoxTextIndex];
            fifth.Text = ((fifthBoxTextIndex < 0) || (fifthBoxTextIndex > Options.Count - 1)) ? string.Empty : Options[fifthBoxTextIndex];

            // The above works for every initial number of options, except for 2
            // and when WrapAtEnds is true, so we deal with it separately
            if (WrapAtEnds && Options.Count == 2)
                fifth.Text = Options[0];

            // Set-up width
            var width = 0.0;
            foreach (var option in Options)
                width = Math.Max(width, MeasureWidthOfText(option));

            TextBoxWidth = width;
        }

        private void OptionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // TODO: Check if selected index was the deleted one
            }
            SetupBoxText();
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
                if (e == InputEvent.NavigationDirection.Down && SelectedIndex == Options.Count - 1)
                    return;
                else if (e == InputEvent.NavigationDirection.Up && SelectedIndex == 0)
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
            SelectedIndex += (isUp ? -1 : 1);
            if (WrapAtEnds && (SelectedIndex < 0 || SelectedIndex > (Options.Count - 1)))
                SelectedIndex = SelectedIndex < 0 ? (Options.Count - 1) : 0;

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
                    var willWrap = isUp ? ((SelectedIndex - 2) < 0) : ((SelectedIndex + 2) > (Options.Count - 1));
                    if (isUp)
                        block.Text = !willWrap ? Options[SelectedIndex - 2] : (WrapAtEnds ? Options[Math.Max(0, Options.Count - 2 + SelectedIndex)] : string.Empty);
                    else
                        block.Text = !willWrap ? Options[SelectedIndex + 2] : (WrapAtEnds ? Options[(SelectedIndex + 2) % Options.Count] : string.Empty);
                }
                sb.Begin(block, HandoffBehavior.Compose);

                node.ValueRef.currentOffset = newOffset;
                node.ValueRef.currentOpacity = newOpacity;
            }
        }

        private (Storyboard, int, double) GetStoryBoardForEndPosition(int currentOffset, EndPosition position)
        {
            var newOffset = DistanceBetweenItems * 2 * (position == EndPosition.Top ? -1 : 1);
            Storyboard sb = new();
            DoubleAnimation moveAnimation = new(currentOffset, newOffset, _animationLength);
            Storyboard.SetTargetProperty(moveAnimation, _pathToTranslateYProperty);
            sb.Children.Add(moveAnimation);
            return (sb, newOffset, 0.0);
        }

        private (Storyboard, int, double) GetStoryBoardForItemAnimation(int currentOffset, double currentOpacity, InputEvent.NavigationDirection direction, FadeDirection fadeType)
        {
            var newOffset = currentOffset + (DistanceBetweenItems * (direction == InputEvent.NavigationDirection.Down ? -1 : 1));
            var newOpacity = currentOpacity + (fadeType == FadeDirection.NoChange ? 0 : (fadeType == FadeDirection.In ? 0.5 : -0.5));
            DoubleAnimation moveAnimation = new(currentOffset, newOffset, _animationLength) { EasingFunction = _movementEasingFunction };
            DoubleAnimation fadeAnimation = new(currentOpacity, newOpacity, _animationLength);

            Storyboard sb = new();
            Storyboard.SetTargetProperty(moveAnimation, _pathToTranslateYProperty);
            Storyboard.SetTargetProperty(fadeAnimation, _pathToOpacityProperty);
            sb.Children.Add(moveAnimation);
            sb.Children.Add(fadeAnimation);
            return (sb, newOffset, newOpacity);
        }
        #endregion
    }
}
