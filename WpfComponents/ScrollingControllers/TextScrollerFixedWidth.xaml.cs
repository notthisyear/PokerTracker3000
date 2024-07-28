using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PokerTracker3000.WpfComponents
{
    public partial class TextScrollerFixedWidth : UserControl
    {
        #region Dependency properties
        public int NumberOfCharacters
        {
            get { return (int)GetValue(NumberOfCharactersProperty); }
            set { SetValue(NumberOfCharactersProperty, value); }
        }
        public static readonly DependencyProperty NumberOfCharactersProperty = DependencyProperty.Register(
            nameof(NumberOfCharacters),
            typeof(int),
            typeof(TextScrollerFixedWidth),
            new FrameworkPropertyMetadata(1,
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                PropertyAffectingSizeChanged));

        public double TextFontSize
        {
            get { return (double)GetValue(TextFontSizeProperty); }
            set { SetValue(TextFontSizeProperty, value); }
        }
        public static readonly DependencyProperty TextFontSizeProperty = DependencyProperty.Register(
            nameof(TextFontSize),
            typeof(double),
            typeof(TextScrollerFixedWidth),
            new FrameworkPropertyMetadata(TextBlock.FontSizeProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                PropertyAffectingSizeChanged));

        public FontStyle TextFontStyle
        {
            get { return (FontStyle)GetValue(TextFontStyleProperty); }
            set { SetValue(TextFontStyleProperty, value); }
        }
        public static readonly DependencyProperty TextFontStyleProperty = DependencyProperty.Register(
            nameof(TextFontStyle),
            typeof(FontStyle),
            typeof(TextScrollerFixedWidth),
            new FrameworkPropertyMetadata(TextBlock.FontStyleProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                PropertyAffectingSizeChanged));

        public FontWeight TextFontWeight
        {
            get { return (FontWeight)GetValue(TextFontWeightProperty); }
            set { SetValue(TextFontWeightProperty, value); }
        }
        public static readonly DependencyProperty TextFontWeightProperty = DependencyProperty.Register(
            nameof(TextFontWeight),
            typeof(FontWeight),
            typeof(TextScrollerFixedWidth),
            new FrameworkPropertyMetadata(TextBlock.FontWeightProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                PropertyAffectingSizeChanged));

        public Brush TextForeground
        {
            get { return (Brush)GetValue(TextForegroundProperty); }
            set { SetValue(TextForegroundProperty, value); }
        }
        public static readonly DependencyProperty TextForegroundProperty = DependencyProperty.Register(
            nameof(TextForeground),
            typeof(Brush),
            typeof(TextScrollerFixedWidth),
            new FrameworkPropertyMetadata(TextBlock.ForegroundProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                PropertyAffectingSizeChanged));

        public FontFamily TextFontFamily
        {
            get { return (FontFamily)GetValue(TextFontFamilyProperty); }
            set { SetValue(TextFontFamilyProperty, value); }
        }
        public static readonly DependencyProperty TextFontFamilyProperty = DependencyProperty.Register(
            nameof(TextFontFamily),
            typeof(FontFamily),
            typeof(TextScrollerFixedWidth),
            new FrameworkPropertyMetadata(TextBlock.FontFamilyProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                PropertyAffectingSizeChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(TextScrollerFixedWidth),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender, TextPropertyChanged));

        public bool AlwaysScroll
        {
            get { return (bool)GetValue(AlwaysScrollProperty); }
            set { SetValue(AlwaysScrollProperty, value); }
        }
        public static readonly DependencyProperty AlwaysScrollProperty = DependencyProperty.Register(
            nameof(AlwaysScroll),
            typeof(bool),
            typeof(TextScrollerFixedWidth),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, AlwaysScrollPropertyChanged));

        public double ScrollSequenceLengthSeconds
        {
            get { return (double)GetValue(ScrollSequenceLengthSecondsProperty); }
            set { SetValue(ScrollSequenceLengthSecondsProperty, value); }
        }
        public static readonly DependencyProperty ScrollSequenceLengthSecondsProperty = DependencyProperty.Register(
            nameof(ScrollSequenceLengthSeconds),
            typeof(double),
            typeof(TextScrollerFixedWidth),
            new FrameworkPropertyMetadata(30.0, FrameworkPropertyMetadataOptions.AffectsRender, SequenceLengthOrOverlapFactorPropertyChanged));

        public double OverlapFactor
        {
            get { return (double)GetValue(OverlapFactorProperty); }
            set { SetValue(OverlapFactorProperty, value); }
        }
        public static readonly DependencyProperty OverlapFactorProperty = DependencyProperty.Register(
            nameof(OverlapFactor),
            typeof(double),
            typeof(TextScrollerFixedWidth),
            new FrameworkPropertyMetadata(0.2, FrameworkPropertyMetadataOptions.AffectsRender, SequenceLengthOrOverlapFactorPropertyChanged, ForceOverlapFactorBetweenZeroAndOne));

        private static object ForceOverlapFactorBetweenZeroAndOne(DependencyObject d, object baseValue)
        {
            if (baseValue is double val)
                return val < 0.0 ? 0.0 : (val > 1.0 ? 1.0 : val);
            return baseValue;
        }

        private static void PropertyAffectingSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextScrollerFixedWidth control)
                return;

            var textWidth = e.NewValue switch
            {
                int numberOfCharacters => control.MeasureField(numberOfCharacters),
                double textFontSize => control.MeasureField(textFontSize),
                FontStyle textFontStyle => control.MeasureField(textFontStyle),
                FontWeight textFontWeight => control.MeasureField(textFontWeight),
                Brush textForeground => control.MeasureField(textForeground),
                FontFamily textFontFamily => control.MeasureField(textFontFamily),
                _ => 0.0
            };

            if (textWidth > 0)
            {
                control.rootCanvas.Width = textWidth;
                control.mainScrollerCanvas.Width = textWidth;
                control.UpdateTextScroller(textWidth, control.AlwaysScroll);
            }
        }

        private static void TextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextScrollerFixedWidth control && e.NewValue is string newText && e.OldValue is string oldText && !newText.Equals(oldText, StringComparison.InvariantCulture))
                control.UpdateTextScroller(control.MeasureString(newText), control.AlwaysScroll);
        }

        private static void AlwaysScrollPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextScrollerFixedWidth control && e.NewValue is bool newValue && e.OldValue is bool oldValue && newValue != oldValue)
                control.UpdateTextScroller(control.MeasureString(control.Text), newValue);
        }
        private static void SequenceLengthOrOverlapFactorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextScrollerFixedWidth control && e.NewValue is double newValue && e.OldValue is double oldValue && newValue != oldValue)
                control.UpdateTextScroller(control.MeasureString(control.Text), control.AlwaysScroll);
        }
        #endregion

        #region Private fields
        private (DoubleAnimationUsingKeyFrames first, DoubleAnimationUsingKeyFrames second) _scrollerAnimation = (new(), new());
        #endregion

        public TextScrollerFixedWidth()
        {
            InitializeComponent();
        }

        #region Private methods

        #region Measure methods
        private double MeasureField(int numberOfCharacters)
            => MeasureString(GetDummyText(numberOfCharacters), TextFontFamily, TextFontStyle, TextFontWeight, TextFontSize, TextForeground);

        private double MeasureField(double textFontSize)
          => MeasureString(GetDummyText(NumberOfCharacters), TextFontFamily, TextFontStyle, TextFontWeight, textFontSize, TextForeground);

        private double MeasureField(FontStyle textFontStyle)
         => MeasureString(GetDummyText(NumberOfCharacters), TextFontFamily, textFontStyle, TextFontWeight, TextFontSize, TextForeground);

        private double MeasureField(FontWeight textFontWeight)
            => MeasureString(GetDummyText(NumberOfCharacters), TextFontFamily, TextFontStyle, textFontWeight, TextFontSize, TextForeground);

        private double MeasureField(Brush textForeground)
            => MeasureString(GetDummyText(NumberOfCharacters), TextFontFamily, TextFontStyle, TextFontWeight, TextFontSize, textForeground);

        private double MeasureField(FontFamily textFontFamily)
           => MeasureString(GetDummyText(NumberOfCharacters), textFontFamily, TextFontStyle, TextFontWeight, TextFontSize, TextForeground);

        private double MeasureString(string newString)
           => MeasureString(newString, TextFontFamily, TextFontStyle, TextFontWeight, TextFontSize, TextForeground);

        private double MeasureString(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, double emSize, Brush foreground)
        {
            var formattedText = new FormattedText(text,
                   CultureInfo.CurrentCulture,
                   FlowDirection.LeftToRight,
                   new Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal),
                   emSize,
                   foreground,
                   new(),
                   VisualTreeHelper.GetDpi(scroller1).PixelsPerDip);
            return formattedText.Width;
        }
        #endregion

        private static string GetDummyText(int length)
        {
            var i = length;
            var sb = new StringBuilder();
            while (i-- > 0)
                sb.Append('*');
            return sb.ToString();
        }

        private void UpdateTextScroller(double textWidth, bool alwaysScroll)
        {
            // Note: Setting BeginTime to null removes any active animations
            _scrollerAnimation.first.BeginTime = null;
            _scrollerAnimation.second.BeginTime = null;
            scroller2.BeginAnimation(OpacityProperty, default);

            var shouldScroll = textWidth > 0 && mainScrollerCanvas.Width > 0 && (textWidth > mainScrollerCanvas.Width || alwaysScroll);
            if (shouldScroll)
            {
                _scrollerAnimation.first.KeyFrames = GetKeyFrames(textWidth);
                _scrollerAnimation.first.RepeatBehavior = RepeatBehavior.Forever;
                _scrollerAnimation.first.BeginTime = TimeSpan.FromSeconds(0);
                scroller1.BeginAnimation(Canvas.LeftProperty, _scrollerAnimation.first);

                // Note: The second scroller should behave identically, but the start of the animation
                //       should be delayed appropriately.
                //       It can be expressed as the sum if the the time it takes for the entire text
                //       to move its width (T_text) and the time it takes for the text to move 
                //       the the correct overlap spot (T_rem):
                //
                //       T_delay = T_text + T_rem
                //
                //       , where:
                //       T_text = textWidth / V
                //       T_rem = mainScrollerCanvas.Width * (1 - OverlapFactor) / V
                //       V = (mainScrollerCanvas.Width + textWidth) / ScrollSequenceLengthSeconds
                //
                //       , which yields
                //       T_delay = 1/ V * (textWidth + (mainScrollerCanvas.Width * (1 - OverlapFactor)))
                //               = ScrollSequenceLengthSeconds / (mainScrollerCanvas.Width + textWidth) *
                //                  (textWidth + (mainScrollerCanvas.Width * (1 - OverlapFactor)))

                var scrollSpeedInverse = ScrollSequenceLengthSeconds / (textWidth + mainScrollerCanvas.Width);
                var secondScrollerDelay = scrollSpeedInverse * (textWidth + (mainScrollerCanvas.Width * (1 - OverlapFactor)));

                _scrollerAnimation.second.KeyFrames = GetKeyFrames(textWidth);
                _scrollerAnimation.second.RepeatBehavior = RepeatBehavior.Forever;
                _scrollerAnimation.second.BeginTime = TimeSpan.FromSeconds(secondScrollerDelay);

                scroller2.Opacity = 0.0;
                scroller2.BeginAnimation(Canvas.LeftProperty, _scrollerAnimation.second);
                scroller2.BeginAnimation(OpacityProperty, new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames = [
                        new LinearDoubleKeyFrame
                        {
                            Value = 1.0,
                            KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))
                        }],
                    BeginTime = TimeSpan.FromSeconds(secondScrollerDelay)
                });
            }
            else
            {
                scroller1.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = 0
                });
                scroller2.Opacity = 0.0;
            }
        }

        private DoubleKeyFrameCollection GetKeyFrames(double textWidth)
        {
            // Note: The pause length should be such that the first scroller starts when OverlapFactor
            //       of the second scroller is still occuping the visible screen space.
            //       The length of that pause should be the sum of the time taken for the entire text
            //       to shift over (so that the endpoint of the second scroller is at its startpoint)
            //       and the remainder so that the correct overlap is on screen when the first scroller
            //       restarts.
            //
            //       This can be expressed as:
            //       T_pause = T_text + (T_visible - T_overlap) - T_overlap
            //               = T_text + T_visible - 2 * T_overlap

            //       If the speed of the text is V, we get:
            //       T_pause = textWidth / V + mainScrollerCanvas.Width / V - 2 * (-mainScrollerCanvas.Width * OverlapFactor / V)
            //               = 1 / V * (textWidth + mainScrollerCanvas.Width - 2 * mainScrollerCanvas.Width * OverlapFactor)
            //               = 1 / V * (textWidth + mainScrollerCanvas.Width * (1 - 2 * OverlapFactor))
            //       V is equal to (mainScrollerCanvas.Width + textWidth) / ScrollSequenceLengthSeconds

            var scrollSpeed = (mainScrollerCanvas.Width + textWidth) / ScrollSequenceLengthSeconds;
            var pauseLength = textWidth + mainScrollerCanvas.Width * (1 - 2 * OverlapFactor);
            var animationPauseLength = pauseLength / scrollSpeed;

            return [
                new LinearDoubleKeyFrame() { Value = mainScrollerCanvas.Width, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)) },
                new LinearDoubleKeyFrame() { Value = -textWidth, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(ScrollSequenceLengthSeconds)) },
                new LinearDoubleKeyFrame() { Value = -textWidth, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(ScrollSequenceLengthSeconds + animationPauseLength)) },
            ];
        }
        #endregion
    }
}
