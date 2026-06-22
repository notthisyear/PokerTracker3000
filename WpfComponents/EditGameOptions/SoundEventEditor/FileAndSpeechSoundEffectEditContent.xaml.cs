using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameSession.Sound;

namespace PokerTracker3000.WpfComponents.EditGameOptions
{
    public partial class FileAndSpeechSoundEffectEditContent : UserControl
    {
        public int MultipleEffectModeSelectedIndex
        {
            get => (int)GetValue(s_multipleEffectModeSelectedIndexProperty);
            private set => SetValue(s_multipleEffectModeSelectedIndexPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_multipleEffectModeSelectedIndexPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(MultipleEffectModeSelectedIndex),
            typeof(int),
            typeof(FileAndSpeechSoundEffectEditContent),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_multipleEffectModeSelectedIndexProperty = s_multipleEffectModeSelectedIndexPropertyKey.DependencyProperty;

        public int MultipleOptionModeSelectedIndex
        {
            get => (int)GetValue(s_multipleOptionModeSelectedIndexProperty);
            private set => SetValue(s_multipleOptionModeSelectedIndexPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_multipleOptionModeSelectedIndexPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(MultipleOptionModeSelectedIndex),
            typeof(int),
            typeof(FileAndSpeechSoundEffectEditContent),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_multipleOptionModeSelectedIndexProperty = s_multipleOptionModeSelectedIndexPropertyKey.DependencyProperty;


        private MultipleOptionSoundEffect? _effect;
        private FileOrSpeechEffectEditorBase? _baseEditor;

        public FileAndSpeechSoundEffectEditContent()
        {
            InitializeComponent();
            Loaded += FileAndSpeechSoundEffectEditContentLoaded;
            multipleOptionModeScroller.ControlInitialized += MultipleOptionModeScrollerInitialized;
            effectOptionsScroller.ControlInitialized += EffectOptionsScrollerControlInitialized;
        }

        public void EnsureTopVisibleEffectOptionSelected()
            => effectOptionsScroller.EnsureTopVisibleEffectOptionSelected();

        public void EnsureBottomVisibleEffectOptionSelected()
            => effectOptionsScroller.EnsureBottomVisibleEffectOptionSelected();

        private void FileAndSpeechSoundEffectEditContentLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= FileAndSpeechSoundEffectEditContentLoaded;
            // TODO: Remove event hooks on unload. Check how we did it in other places.
            if (DataContext is FromFileSoundEffectEditor fileEditor)
            {
                _baseEditor = fileEditor;
                _effect = fileEditor.SoundEffect;
            }
            else if (DataContext is SynthesizedSpeechSoundEffectEditor speechEditor)
            {
                _baseEditor = speechEditor;
                _effect = speechEditor.SoundEffect;
            }
            else
            {
                return;
            }

            _baseEditor.RenameEffectModel.ButtonAction = () =>
            {
                _baseEditor.ShowRenameEffectField = true;
                renameEffectBox.Focus();
                renameEffectBox.CaretIndex = _effect?.RawName.Length ?? 0;
            };

            _baseEditor.InitializeControl(_effect, _effect.Mode, _effect.EffectOptions.Count > 1, true);
        }

        private void EffectOptionsScrollerControlInitialized(object sender, RoutedEventArgs e)
        {
            effectOptionsScroller.ControlInitialized -= EffectOptionsScrollerControlInitialized;
            effectOptionsScroller.SelectedIndexChanged += (s, e) =>
            {
                MultipleEffectModeSelectedIndex = effectOptionsScroller.CurrentSelectedIndex;
            };
        }

        private void MultipleOptionModeScrollerInitialized(object sender, RoutedEventArgs e)
        {
            multipleOptionModeScroller.ControlInitialized -= MultipleOptionModeScrollerInitialized;
            multipleOptionModeScroller.SelectedIndexChanged += (s, e) =>
            {
                if (_baseEditor != default)
                    _effect?.Mode = _baseEditor.MultipleOptionModes[multipleOptionModeScroller.CurrentSelectedIndex];

                MultipleOptionModeSelectedIndex = multipleOptionModeScroller.CurrentSelectedIndex;
            };
        }
    }
}
