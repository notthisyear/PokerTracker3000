using System;
using System.IO;
using System.Windows;
using Ookii.Dialogs.Wpf;
using PokerTracker3000.GameSession;
using PokerTracker3000.GameSession.Sound;

namespace PokerTracker3000.WpfComponents.EditGameOptions
{
    public partial class FromFileSoundEffectEditor : FileOrSpeechEffectEditorBase
    {
        #region Dependency properties
        public FromFileSoundEffect SoundEffect
        {
            get => (FromFileSoundEffect)GetValue(SoundEffectProperty);
            set => SetValue(SoundEffectProperty, value);
        }
        public static readonly DependencyProperty SoundEffectProperty = DependencyProperty.Register(
            nameof(SoundEffect),
            typeof(FromFileSoundEffect),
            typeof(FromFileSoundEffectEditor),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender, SoundEffectChangedCallback));

        private static void SoundEffectChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FromFileSoundEffectEditor editor && editor.IsLoaded && e.NewValue is FromFileSoundEffect effect)
                editor.InitializeControl(effect, effect.Mode, effect.EffectOptions.Count > 1, false);
        }
        #endregion

        #region Private fields
        private static readonly VistaOpenFileDialog s_loadSoundEffectDialog = new()
        {
            Title = "Load sound effect",
            Multiselect = false,
            Filter = "WAV file (*.wav)|*.wav|All files (*.*)|*.*"
        };
        #endregion

        public FromFileSoundEffectEditor()
        {
            InitializeComponent();
            Loaded += ControlLoaded;
        }

        private void ControlLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ControlLoaded;

            AddOptionModel.ButtonAction = () =>
            {
                if (SoundEffect != default &&
                    s_loadSoundEffectDialog.ShowDialog() == true &&
                    Path.Exists(s_loadSoundEffectDialog.FileName))
                {
                    SoundEffect.AddFileOption(s_loadSoundEffectDialog.FileName);
                    ShowMultipleOptionsScroller = SoundEffect.EffectOptions.Count > 1;
                    SoundEffect.Mode = ShowMultipleOptionsScroller ?
                        MultipleOptionModes[mainContent.MultipleOptionModeSelectedIndex] :
                        MultipleOptionMode.None;

                    NavigationManager.ReplaceNavigation(NavigationId, GetNavigationNodes());

                    // Select the new option
                    AddOptionModel.IsSelected = false;
                    SelectedElementIndex = SoundEffect.EffectOptions.Count - 1;
                    SelectedElementMap[SelectedElementIndex].IsSelected = true;
                }
            };
            ChangeEffectOptionModel.ButtonAction = () =>
            {
                if (!SelectedElementMap.TryGetValue(SelectedElementIndex, out var entity) ||
                    entity is not SoundEffectOption option)
                {
                    return;
                }

                if (s_loadSoundEffectDialog.ShowDialog() == true && Path.Exists(s_loadSoundEffectDialog.FileName))
                {
                    option.Path = s_loadSoundEffectDialog.FileName;
                }
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

                SoundEffect.RemoveFileOption(option);
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
