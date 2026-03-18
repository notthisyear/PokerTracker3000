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
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

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

            if (NavigationRelay == default || SoundEffect == default)
                return;

            ControlLoadedBase(SoundEffect, SoundEffect.Mode, SoundEffect.EffectOptions.Count > 1);

            foreach (var s in Enum.GetValues<TextVariable>())
            {
                var (attr, _) = s.GetCustomAttributeFromEnum<SynthesizedSpeechVariableAttribute>();
                SpeechVariables.Add(attr!);
            }

            AddOptionModel.ButtonAction = () =>
            {
                if (SoundEffect != default)
                {
                    var defaultText = "New speech";

                    SoundEffect.AddSpeechOption(defaultText);
                    ShowMultipleOptionsScroller = SoundEffect.EffectOptions.Count > 1;
                    SoundEffect.Mode = ShowMultipleOptionsScroller ?
                        MultipleOptionModes[mainContent.MultipleOptionModeSelectedIndex] :
                        MultipleOptionMode.None;

                    NavigationManager.ReplaceNavigation(NavigationId, GetNavigationNodes());

                    // Select the new option
                    AddOptionModel.IsSelected = false;
                    SelectedElementIndex = SoundEffect.EffectOptions.Count - 1;
                    SelectedElementMap[SelectedElementIndex].IsSelected = true;

                    ShowSpeechEditBox = true;
                    SpeechText = defaultText;
                    speechEffectBox.Focus();
                    speechEffectBox.CaretIndex = defaultText.Length;
                }
            };
            ChangeEffectOptionModel.ButtonAction = () =>
            {
                if (!SelectedElementMap.TryGetValue(SelectedElementIndex, out var entity) ||
                    entity is not SoundEffectOption option)
                {
                    return;
                }

                ShowSpeechEditBox = true;
                SpeechText = option.Name;
                speechEffectBox.Focus();
                speechEffectBox.CaretIndex = SpeechText.Length;
            };
            RemoveEffectOptionModel.ButtonAction = () =>
            {
                if (!SelectedElementMap.TryGetValue(SelectedElementIndex, out var entity) ||
                   entity is not SoundEffectOption option ||
                   SoundEffect == default)
                {
                    return;
                }
                ShowChangeRemoveTestEffectOptions = false;
                option.IsSelected = false;

                SoundEffect.RemoveSpeechOption(option);
                ShowMultipleOptionsScroller = SoundEffect.EffectOptions.Count > 1;
                SoundEffect.Mode = ShowMultipleOptionsScroller ?
                        MultipleOptionModes[mainContent.MultipleOptionModeSelectedIndex] :
                        MultipleOptionMode.None;

                NavigationManager.ReplaceNavigation(NavigationId, GetNavigationNodes());
                SelectedElementIndex = Math.Max(SelectedElementIndex - 1, 0);

                if (SelectedElementMap.TryGetValue(SelectedElementIndex, out var newSelectedEntity))
                    newSelectedEntity.IsSelected = true;

            };
        }

        protected override int GetMultipleOptionsModeScrollerIndex()
             => mainContent.MultipleOptionModeSelectedIndex;

        protected override NavigationManager.Node[] GetNavigationNodes()
             => GetNavigationNodesForOptions(SoundEffect?.EffectOptions ?? []);

    }
}
