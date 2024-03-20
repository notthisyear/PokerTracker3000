using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            nameof(SelectedIndex),
            typeof(int),
            typeof(ScrollingSelectorBox),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
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

        private void ComponentLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ComponentLoaded;

            Initialize();

            if (NavigatorRelay != default)
                NavigatorRelay.Navigate += Navigate;
        }

        private void Initialize()
        {
            _boxes.AddLast((first, WrapAtEnds ? (-2 * DistanceBetweenItems) : 0, WrapAtEnds ? 0 : 1));
            _boxes.AddLast((second, DistanceBetweenItems * (WrapAtEnds ? -1 : 1), 0.5));
            _boxes.AddLast((third, WrapAtEnds ? 0 : (DistanceBetweenItems * 2), WrapAtEnds ? 1 : 0));
            _boxes.AddLast((fourth, DistanceBetweenItems * (WrapAtEnds ? 1 : 3), WrapAtEnds ? 0.5 : 0));
            _boxes.AddLast((fifth, DistanceBetweenItems * (WrapAtEnds ? 2 : 4), 0));

            var node = _boxes.First;
            first.RenderTransform = new TranslateTransform(0, node!.Value.currentOffset);
            first.Opacity = 1; //node!.Value.currentOpacity;

            node = node.Next;
            second.RenderTransform = new TranslateTransform(0, node!.Value.currentOffset);
            second.Opacity = 1; //node!.Value.currentOpacity;

            node = node.Next;
            third.RenderTransform = new TranslateTransform(0, node!.Value.currentOffset);
            third.Opacity = 1; //node!.Value.currentOpacity;

            node = node.Next;
            fourth.RenderTransform = new TranslateTransform(0, node!.Value.currentOffset);
            fourth.Opacity = 1; //node!.Value.currentOpacity;

            node = node.Next;
            fifth.RenderTransform = new TranslateTransform(0, node!.Value.currentOffset);
            fifth.Opacity = 1; //node!.Value.currentOpacity;

            if (Options != default)
            {
                first.Text = Options.Count > 0 ? Options[0] : string.Empty;
                second.Text = Options.Count > 1 ? Options[1] : string.Empty;
                third.Text = Options.Count > 2 ? Options[2] : string.Empty;
                fourth.Text = Options.Count > 3 ? Options[3] : string.Empty;
                fifth.Text = Options.Count > 4 ? Options[4] : string.Empty;
            }
        }

        private void Navigate(object? sender, InputEvent.NavigationDirection e)
        {
            Debug.WriteLine("-- Before navigation --");
            Debug.WriteLine($"\tselected index: {SelectedIndex}");
            if (!WrapAtEnds)
            {
                if (SelectedIndex < 2 || SelectedIndex > (Options.Count - 4))
                {
                    NavigateWithoutWrapping(e);
                    LinkedListNode<(TextBlock block, int currentOffset, double currentOpacity)>? node = default;
                    Debug.WriteLine("-- After navigation --");
                    Debug.WriteLine($"\tselected index: {SelectedIndex}");
                    for (var i = 0; i < _boxes.Count; i++)
                    {
                        node = (node == default) ? _boxes.First : node.Next;
                        var (block, currentOffset, currentOpacity) = node!.Value;

                        Debug.WriteLine($"\t\t[{i}] {block.Text}, {block.Name} ({currentOffset}, {currentOpacity})");
                    }
                    return;
                }
            }

            NavigateWithWrapping(e);
            LinkedListNode<(TextBlock block, int currentOffset, double currentOpacity)>? node2 = default;
            Debug.WriteLine("-- After navigation --");
            Debug.WriteLine($"\tselected index: {SelectedIndex}");
            for (var i = 0; i < _boxes.Count; i++)
            {
                node2 = (node2 == default) ? _boxes.First : node2.Next;
                var (block, currentOffset, currentOpacity) = node2!.Value;

                Debug.WriteLine($"\t\t[{i}] {block.Text}, {block.Name} ({currentOffset}, {currentOpacity})");
            }
        }

        private void NavigateWithWrapping(InputEvent.NavigationDirection e)
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
            LinkedListNode<(TextBlock block, int currentOffset, double currentOpacity)>? node = default;
            for (var i = 0; i < _boxes.Count; i++)
            {
                node = (node == default) ? _boxes.First : node.Next;
                var (block, currentOffset, currentOpacity) = node!.Value;

                (var sb, var newOffset, var newOpacity) = isItemThatWrapped(i) ?
                    GetBoardForEndPosition(currentOffset, isUp ? EndPosition.Top : EndPosition.Bottom) :
                    GetBoardForItemAnimation(currentOffset, currentOpacity, e, itemFadeDirection(i));
                sb.Begin(block, HandoffBehavior.Compose);

                node.ValueRef.currentOffset = newOffset;
                //node.ValueRef.currentOpacity = newOpacity;
            }
            SelectedIndex += (isUp ? -1 : 1);
        }

        private void NavigateWithoutWrapping(InputEvent.NavigationDirection e)
        {
            var isUp = e == InputEvent.NavigationDirection.Up;
            if (isUp && SelectedIndex == 0)
                return;
            else if (!isUp && SelectedIndex == Options.Count - 1)
                return;

            SelectedIndex += (isUp ? -1 : 1);
            LinkedListNode<(TextBlock block, int currentOffset, double currentOpacity)>? node = default;
            for (var i = 0; i < _boxes.Count; i++)
            {
                node = (node == default) ? _boxes.First : node.Next;
                var (block, currentOffset, currentOpacity) = node!.Value;
                (Storyboard sb, int newOffset, double newOpacity)? animationResult = default;
                if (isUp && i )
                if (!isUp && i < SelectedIndex)
                {
                    animationResult = GetBoardForItemAnimation(currentOffset, currentOpacity,
                        e,
                        FadeDirection.Out);
                }
                else if (!isUp && i < SelectedIndex + 2)
                {
                    animationResult = GetBoardForItemAnimation(currentOffset, currentOpacity,
                        e,
                        FadeDirection.In);
                }
                else if (!isUp)
                {
                    animationResult = GetBoardForItemAnimation(currentOffset, currentOpacity,
                        e,
                        FadeDirection.NoChange);
                }

                animationResult!.Value.sb.Begin(block, HandoffBehavior.Compose);
                node.ValueRef.currentOffset = animationResult.Value.newOffset;
                //node.ValueRef.currentOpacity = animationResult.Value.newOpacity;
            }
        }
        private (Storyboard, int, double) GetBoardForEndPosition(int currentOffset, EndPosition position)
        {
            var newOffset = DistanceBetweenItems * 2 * (position == EndPosition.Top ? -1 : 1);
            Storyboard sb = new();
            DoubleAnimation moveAnimation = new(currentOffset, newOffset, _animationLength);
            Storyboard.SetTargetProperty(moveAnimation, _pathToTranslateYProperty);
            sb.Children.Add(moveAnimation);
            return (sb, newOffset, 0.0);
        }

        private (Storyboard, int, double) GetBoardForItemAnimation(int currentOffset, double currentOpacity, InputEvent.NavigationDirection direction, FadeDirection fadeType)
        {
            var newOffset = currentOffset + (DistanceBetweenItems * (direction == InputEvent.NavigationDirection.Down ? -1 : 1));
            var newOpacity = currentOpacity + (fadeType == FadeDirection.NoChange ? 0 : (fadeType == FadeDirection.In ? 0.5 : -0.5));
            DoubleAnimation moveAnimation = new(currentOffset, newOffset, _animationLength) { EasingFunction = _movementEasingFunction };
            DoubleAnimation fadeAnimation = new(currentOpacity, newOpacity, _animationLength);

            Storyboard sb = new();
            Storyboard.SetTargetProperty(moveAnimation, _pathToTranslateYProperty);
            Storyboard.SetTargetProperty(fadeAnimation, _pathToOpacityProperty);
            sb.Children.Add(moveAnimation);
            //sb.Children.Add(fadeAnimation);
            return (sb, newOffset, newOpacity);
        }
    }
}
