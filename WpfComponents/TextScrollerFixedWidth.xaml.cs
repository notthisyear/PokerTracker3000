﻿using System;
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
            var shouldScroll = textWidth > 0 && mainScrollerCanvas.Width > 0 && (textWidth > mainScrollerCanvas.Width || alwaysScroll);
            if (shouldScroll)
            {
                scroller1.BeginAnimation(Canvas.LeftProperty, new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames =
                    [
                        new LinearDoubleKeyFrame() { Value = mainScrollerCanvas.Width, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)) },
                        new LinearDoubleKeyFrame() { Value = -textWidth, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(30)) },
                        new LinearDoubleKeyFrame() { Value = -textWidth, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(50)) },
                    ],
                    RepeatBehavior = RepeatBehavior.Forever
                });

                scroller2.Visibility = Visibility.Visible;
                scroller2.BeginAnimation(Canvas.LeftProperty, new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames =
                    [
                        new LinearDoubleKeyFrame() { Value = mainScrollerCanvas.Width, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)) },
                        new LinearDoubleKeyFrame() { Value = mainScrollerCanvas.Width, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(25)) },
                        new LinearDoubleKeyFrame() { Value = -textWidth, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(55)) },
                    ],
                    RepeatBehavior = RepeatBehavior.Forever
                });
            }
            else
            {
                scroller1.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = 0
                });
                scroller2.Visibility = Visibility.Collapsed;
            }
        }
        #endregion
    }
}
