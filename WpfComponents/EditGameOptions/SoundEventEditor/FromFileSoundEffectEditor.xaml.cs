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
                if (s_loadSoundEffectDialog.ShowDialog() == true &&
                    Path.Exists(s_loadSoundEffectDialog.FileName))
                {
                    var numberOfOptionsBefore = SoundEffect.EffectOptions.Count;

                    SoundEffect.AddFileOption(s_loadSoundEffectDialog.FileName);
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

                }
            };
            ChangeEffectOptionModel.ButtonAction = () =>
            {
                var selectedOptionIndex = GetEffectOptionsModeScrollerIndex();
                if (selectedOptionIndex >= SoundEffect.EffectOptions.Count)
                    return;

                var option = SoundEffect.EffectOptions[selectedOptionIndex];
                if (s_loadSoundEffectDialog.ShowDialog() == true && Path.Exists(s_loadSoundEffectDialog.FileName))
                {
                    option.Path = s_loadSoundEffectDialog.FileName;
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

                SoundEffect.RemoveFileOption(option);
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
