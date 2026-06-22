using System;
using System.Collections.ObjectModel;
using System.Windows;
using PokerTracker3000.Common;
using PokerTracker3000.GameSession;
using PokerTracker3000.GameSession.Sound;
using static PokerTracker3000.GameSession.Sound.SpeechSoundEffect;

namespace PokerTracker3000.WpfComponents.EditGameOptions
{
    public partial class SynthesizedSpeechSoundEffectEditor : FileOrSpeechEffectEditorBase
    {
        #region Dependency properties
        public SpeechSoundEffect SoundEffect
        {
            get => (SpeechSoundEffect)GetValue(SoundEffectProperty);
            set => SetValue(SoundEffectProperty, value);
        }
        public static readonly DependencyProperty SoundEffectProperty = DependencyProperty.Register(
            nameof(SoundEffect),
            typeof(SpeechSoundEffect),
            typeof(SynthesizedSpeechSoundEffectEditor),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender, SoundEffectChangedCallback));

        private static void SoundEffectChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SynthesizedSpeechSoundEffectEditor editor && editor.IsLoaded && e.NewValue is SpeechSoundEffect effect)
                editor.InitializeControl(effect, effect.Mode, effect.EffectOptions.Count > 1, false);
        }

        public string SpeechText
        {
            get => (string)GetValue(SpeechTextProperty);
            set => SetValue(SpeechTextProperty, value);
        }

        public static readonly DependencyProperty SpeechTextProperty = DependencyProperty.Register(
            nameof(SpeechText),
            typeof(string),
            typeof(SynthesizedSpeechSoundEffectEditor),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                SpeechTextUpdated));

        private static void SpeechTextUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SynthesizedSpeechSoundEffectEditor editor && e.NewValue is string s)
            {
                if (editor.SelectedElementMap.TryGetValue(editor.SelectedElementIndex, out var entity)
                    && entity is SoundEffectOption option)
                {
                    option.Name = s;
                }
            }
        }
        #endregion

        #region Public properties
        public ObservableCollection<SynthesizedSpeechVariableAttribute> SpeechVariables { get; } = [];
        #endregion

        public SynthesizedSpeechSoundEffectEditor()
        {
            InitializeComponent();
            Loaded += ControlLoaded;
        }

        private void ControlLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ControlLoaded;

            foreach (var s in Enum.GetValues<TextVariable>())
            {
                var (attr, _) = s.GetCustomAttributeFromEnum<SynthesizedSpeechVariableAttribute>();
                SpeechVariables.Add(attr!);
            }

            AddOptionModel.ButtonAction = () =>
            {
                var defaultText = "New speech";

                var numberOfOptionsBefore = SoundEffect.EffectOptions.Count;
                SoundEffect.AddSpeechOption(defaultText);
                ShowMultipleOptionsScroller = SoundEffect.EffectOptions.Count > 1;
                SoundEffect.Mode = ShowMultipleOptionsScroller ?
                    MultipleOptionModes[mainContent.MultipleOptionModeSelectedIndex] :
                    MultipleOptionMode.None;

                // Note: The navigation only changes when we go from 0 -> 1 or from 1 -> 2, as that's
                //       when we get actual options as well as the multiple option mode scroller.
                if (numberOfOptionsBefore < 2)
                {
                    NavigationManager.ReplaceNavigation(NavigationId, GetNavigationNodes());

                    // We need to reselect the element as all elements gets deselect as when
                    // we regenerate the navigation nodes
                    if (SelectedElementMap.TryGetValue(SelectedElementIndex, out var newSelectedEntity))
                        newSelectedEntity.IsSelected = true;
                }
            };
            ChangeEffectOptionModel.ButtonAction = () =>
            {
                var selectedOptionIndex = GetEffectOptionsModeScrollerIndex();
                if (selectedOptionIndex >= SoundEffect.EffectOptions.Count)
                    return;

                var option = SoundEffect.EffectOptions[selectedOptionIndex];

                if (ShowSpeechEditBox)
                {
                    ShowSpeechEditBox = false;
                    option.Name = SpeechText;
                }
                else
                {
                    ShowSpeechEditBox = true;
                    SpeechText = option.Name;
                    speechEffectBox.Focus();
                    speechEffectBox.CaretIndex = SpeechText.Length;
                }
            };
            RemoveEffectOptionModel.ButtonAction = () =>
            {
                var selectedOptionIndex = GetEffectOptionsModeScrollerIndex();
                if (selectedOptionIndex >= SoundEffect.EffectOptions.Count)
                    return;

                var numberOfOptionsBefore = SoundEffect.EffectOptions.Count;
                var option = SoundEffect.EffectOptions[selectedOptionIndex];

                ShowChangeRemoveTestEffectOptions = false;
                option.IsSelected = false;

                SoundEffect.RemoveSpeechOption(option);
                ShowMultipleOptionsScroller = SoundEffect.EffectOptions.Count > 1;
                SoundEffect.Mode = ShowMultipleOptionsScroller ?
                        MultipleOptionModes[mainContent.MultipleOptionModeSelectedIndex] :
                        MultipleOptionMode.None;

                // Note: The navigation only changes when we go from 0 -> 1 or from 1 -> 2, as that's
                //       when we get actual options as well as the multiple option mode scroller.
                if (numberOfOptionsBefore <= 2)
                {
                    NavigationManager.ReplaceNavigation(NavigationId, GetNavigationNodes());
                    SelectedElementIndex = 0;
                    if (SelectedElementMap.TryGetValue(SelectedElementIndex, out var newSelectedEntity))
                        newSelectedEntity.IsSelected = true;
                }
            };
        }

        protected override int GetEffectOptionsModeScrollerIndex()
            => mainContent.MultipleEffectModeSelectedIndex;

        protected override int GetMultipleOptionsModeScrollerIndex()
             => mainContent.MultipleOptionModeSelectedIndex;

        protected override int GetIdOfSelectedOption()
        {
            if (SoundEffect == null)
                throw new InvalidOperationException("The SoundEffect is unset");

            if (mainContent.MultipleEffectModeSelectedIndex >= SoundEffect.EffectOptions.Count)
                throw new InvalidOperationException("Selected effect index larger than the count");

            return SoundEffect.EffectOptions[mainContent.MultipleEffectModeSelectedIndex].Id;
        }

        protected override void EnsureTopVisibleEffectOptionSelected()
            => mainContent.EnsureTopVisibleEffectOptionSelected();

        protected override void EnsureBottomVisibleEffectOptionSelected()
            => mainContent.EnsureBottomVisibleEffectOptionSelected();

        protected override NavigationManager.Node[] GetNavigationNodes()
             => GetNavigationNodesForOptions(SoundEffect.EffectOptions.Count > 0);

    }
}
