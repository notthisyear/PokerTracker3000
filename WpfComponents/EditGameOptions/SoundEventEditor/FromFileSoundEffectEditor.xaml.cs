using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Ookii.Dialogs.Wpf;
using PokerTracker3000.GameSession;
using PokerTracker3000.GameSession.Sound;
using InputEvent = PokerTracker3000.Input.UserInputEvent;

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
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));
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

            if (NavigationRelay == default || SoundEffect == default)
                return;

            ControlLoadedBase(SoundEffect.Mode, SoundEffect.EffectOptions.Count > 1);

            multipleOptionModeScroller.SelectedIndexChanged += (s, e) =>
            {
                SoundEffect?.Mode = MultipleOptionModes[multipleOptionModeScroller.CurrentSelectedIndex];
            };

            RenameEffectModel.ButtonAction = () =>
            {
                ShowRenameEffectField = true;
                renameEffectBox.Focus();
                renameEffectBox.CaretIndex = SoundEffect?.RawName.Length ?? 0;
            };
            AddOptionModel.ButtonAction = () =>
            {
                if (SoundEffect != default &&
                    s_loadSoundEffectDialog.ShowDialog() == true &&
                    Path.Exists(s_loadSoundEffectDialog.FileName))
                {
                    SoundEffect.AddFileOption(s_loadSoundEffectDialog.FileName);
                    ShowMultipleOptionsScroller = SoundEffect.EffectOptions.Count > 1;
                    SoundEffect.Mode = ShowMultipleOptionsScroller ?
                        MultipleOptionModes[multipleOptionModeScroller.CurrentSelectedIndex] :
                        MultipleOptionMode.None;

                    NavigationManager.ReplaceNavigation(NavigationId, GetNavigationNodes());
                    // The map has changes, so we have to remark the add button as selected
                    var addButtonEntry = SelectedElementMap.FirstOrDefault(x => x.Value == AddOptionModel);
                    if (addButtonEntry.Value != default)
                        SelectedElementIndex = addButtonEntry.Key;
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
                        MultipleOptionModes[multipleOptionModeScroller.CurrentSelectedIndex] :
                        MultipleOptionMode.None;

                NavigationManager.ReplaceNavigation(NavigationId, GetNavigationNodes());
                SelectedElementIndex = Math.Max(SelectedElementIndex - 1, 0);

                if (SelectedElementMap.TryGetValue(SelectedElementIndex, out var newSelectedEntity))
                    newSelectedEntity.IsSelected = true;

            };
        }

        protected override int GetMultipleOptionsModeScrollerIndex()
             => multipleOptionModeScroller.CurrentSelectedIndex;

        protected override NavigationManager.Node[] GetNavigationNodes()
            => GetNavigationNodesForOptions(SoundEffect?.EffectOptions ?? []);
    }
}
