using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PokerTracker3000.Common;
using PokerTracker3000.GameSession;
using PokerTracker3000.GameSession.Sound;
using static PokerTracker3000.Interfaces.IInputRelay;
using InputEvent = PokerTracker3000.Input.UserInputEvent;

namespace PokerTracker3000.WpfComponents.EditGameOptions
{
    public sealed class ScrollerSelectable : SelectableEntity { }

    public abstract class SoundEffectEditorBase : ConditionAndEffectEditorBase
    {
        #region Dependency property
        public AudioManager AudioManager
        {
            get { return (AudioManager)GetValue(AudioManagerProperty); }
            set { SetValue(AudioManagerProperty, value); }
        }
        public static readonly DependencyProperty AudioManagerProperty = DependencyProperty.Register(
            nameof(AudioManager),
            typeof(AudioManager),
            typeof(SoundEffectEditorBase),
            new FrameworkPropertyMetadata(default));
        #endregion

        #region Public properties
        public ButtonOptionModel TestEffectModel { get; } = new("Test", type: ButtonOptionModel.OptionType.Info);

        public ButtonOptionModel AddOptionModel { get; } = new("Add", type: ButtonOptionModel.OptionType.Success);

        public NavigationOnlyRelay ScrollerNavRelay { get; } = new();

        public ScrollerSelectable ScrollerSelectable { get; } = new();
        #endregion

        #region Private fields
        private SoundEffect? _currentSoundEffect;
        #endregion

        #region Protected properties and methods
        protected Dictionary<int, SelectableEntity> SelectedElementMap { get; } = [];

        protected int SelectedElementIndex { get; set; } = 0;

        protected void ControlLoadedBase(SoundEffect effect)
        {
            _currentSoundEffect = effect;

            if (SelectedElementMap.TryGetValue(SelectedElementIndex, out var element))
                element?.IsSelected = true;

            TestEffectModel.ButtonAction = () =>
            {
                TaskCompletionSource tcs = new();
                TestEffectModel.IsAvailable = false;
                TestSoundEffect(tcs);
                Task.Run(() =>
                {
                    tcs.Task.Wait();
                    Application.Current.Dispatcher.Invoke(() => { TestEffectModel.IsAvailable = true; });
                });
            };

            ControlLoadedBase();
        }

        protected void TestSoundEffect(TaskCompletionSource tcs, int optionId = -1)
        {
            if (AudioManager == default || _currentSoundEffect == default)
                return;

            AudioManager.TestSoundEffect(_currentSoundEffect, tcs, optionId);
        }
        #endregion
    }

    public abstract class FileOrSpeechEffectEditorBase : SoundEffectEditorBase
    {
        #region Public and protected properties
        public ButtonOptionModel RenameEffectModel { get; } = new("Rename");

        public ButtonOptionModel ChangeEffectOptionModel { get; } = new("Change");

        public ButtonOptionModel TestEffectOptionModel { get; } = new("Test", type: ButtonOptionModel.OptionType.Info);

        public ButtonOptionModel RemoveEffectOptionModel { get; } = new("Remove", type: ButtonOptionModel.OptionType.Cancel);

        public ObservableCollection<string> AvailableMultipleOptionModeTypes { get; } = [];

        public List<MultipleOptionMode> MultipleOptionModes { get; } = [];
        #endregion

        #region Read-only dependency property
        public bool ShowRenameEffectField
        {
            get => (bool)GetValue(s_showRenameEffectFieldProperty);
            set => SetValue(s_showRenameEffectFieldPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_showRenameEffectFieldPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ShowRenameEffectField),
            typeof(bool),
            typeof(FileOrSpeechEffectEditorBase),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_showRenameEffectFieldProperty = s_showRenameEffectFieldPropertyKey.DependencyProperty;

        public bool ShowMultipleOptionsScroller
        {
            get => (bool)GetValue(s_showMultipleOptionsScrollerProperty);
            protected set => SetValue(s_showMultipleOptionsScrollerPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_showMultipleOptionsScrollerPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ShowMultipleOptionsScroller),
            typeof(bool),
            typeof(FileOrSpeechEffectEditorBase),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_showMultipleOptionsScrollerProperty = s_showMultipleOptionsScrollerPropertyKey.DependencyProperty;

        public bool ShowChangeRemoveTestEffectOptions
        {
            get => (bool)GetValue(s_showChangeRemoveTestEffectOptionsProperty);
            protected set => SetValue(s_showChangeRemoveTestEffectOptionsPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_showChangeRemoveTestEffectOptionsPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ShowChangeRemoveTestEffectOptions),
            typeof(bool),
            typeof(FileOrSpeechEffectEditorBase),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_showChangeRemoveTestEffectOptionsProperty = s_showChangeRemoveTestEffectOptionsPropertyKey.DependencyProperty;

        public bool ShowSpeechEditBox
        {
            get => (bool)GetValue(s_showSpeechEditBoxProperty);
            protected set => SetValue(s_showSpeechEditBoxPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_showSpeechEditBoxPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ShowSpeechEditBox),
            typeof(bool),
            typeof(FileOrSpeechEffectEditorBase),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_showSpeechEditBoxProperty = s_showSpeechEditBoxPropertyKey.DependencyProperty;
        #endregion

        #region Private fields
        private readonly Dictionary<int, ButtonOptionModel> _selectedChangeRemoveTestMap = [];
        private int _selectedChangeRemoveTestIndex = 0;
        #endregion

        #region Protected methods
        protected abstract int GetMultipleOptionsModeScrollerIndex();

        protected void ControlLoadedBase(SoundEffect effect, MultipleOptionMode multipleOptionsMode, bool showMultipleOptionsModeScroller)
        {
            PopulateOptionsList(AvailableMultipleOptionModeTypes, MultipleOptionModes);
            if (multipleOptionsMode != MultipleOptionMode.None)
            {
                while (MultipleOptionModes[GetMultipleOptionsModeScrollerIndex()] != multipleOptionsMode)
                    ScrollerNavRelay.RaiseEvent(InputEvent.NavigationDirection.Down);
            }

            ShowMultipleOptionsScroller = showMultipleOptionsModeScroller;
            _selectedChangeRemoveTestMap.Add(0, ChangeEffectOptionModel);
            _selectedChangeRemoveTestMap.Add(1, RemoveEffectOptionModel);
            _selectedChangeRemoveTestMap.Add(2, TestEffectOptionModel);

            TestEffectOptionModel.ButtonAction = () =>
            {
                var selectedOption = SelectedElementMap.Values.Where(x => x is SoundEffectOption).FirstOrDefault(x => x.IsSelected);
                if (selectedOption is not SoundEffectOption option)
                    return;

                TaskCompletionSource tcs = new();
                TestEffectOptionModel.IsAvailable = false;
                TestSoundEffect(tcs, option.Id);
                Task.Run(() =>
                {
                    tcs.Task.Wait();
                    Application.Current.Dispatcher.Invoke(() => { TestEffectOptionModel.IsAvailable = true; });
                });
            };

            ControlLoadedBase(effect);
        }

        protected override void HandleNavigation(object? sender, NavigationEventArgs e)
        {
            if (!IsActive)
                return;

            if (ShowRenameEffectField || ShowSpeechEditBox)
            {
                e.Handled = true;
                return;
            }

            if (ShowChangeRemoveTestEffectOptions)
            {
                _selectedChangeRemoveTestMap[_selectedChangeRemoveTestIndex].IsSelected = false;
                if (e.Direction == InputEvent.NavigationDirection.Left)
                {
                    _selectedChangeRemoveTestIndex = _selectedChangeRemoveTestIndex > 0 ?
                        _selectedChangeRemoveTestIndex - 1 :
                        _selectedChangeRemoveTestMap.Count - 1;
                }
                else if (e.Direction == InputEvent.NavigationDirection.Right)
                {
                    _selectedChangeRemoveTestIndex = _selectedChangeRemoveTestIndex < (_selectedChangeRemoveTestMap.Count - 1) ?
                        _selectedChangeRemoveTestIndex + 1 : 0;
                }
                _selectedChangeRemoveTestMap[_selectedChangeRemoveTestIndex].IsSelected = true;
                e.Handled = true;
                return;
            }

            if (!SelectedElementMap.TryGetValue(SelectedElementIndex, out var element))
                return;

            // A scroller was selected
            if (element is ScrollerSelectable)
            {
                var oldIndex = GetMultipleOptionsModeScrollerIndex();
                ScrollerNavRelay.RaiseEvent(e.Direction);
                e.Handled = oldIndex != GetMultipleOptionsModeScrollerIndex();
                if (e.Handled)
                    return;
            }

            // Deselect the old element, if applicable
            element?.IsSelected = false;

            SelectedElementIndex = NavigationManager.Navigate(NavigationId, SelectedElementIndex, e.Direction);

            // If the new element index points to a selectable element
            if (SelectedElementMap.TryGetValue(SelectedElementIndex, out element))
                element?.IsSelected = true;
            e.Handled = true;
        }

        protected override void HandleButton(object? sender, ButtonEventArgs e)
        {
            if (!IsActive)
                return;

            if (!SelectedElementMap.TryGetValue(SelectedElementIndex, out var element))
                return;

            if (e.ButtonEvent == InputEvent.ButtonEventType.Select)
            {
                // If the change/remove/test option dialog is open, invoke the action
                if (ShowChangeRemoveTestEffectOptions && _selectedChangeRemoveTestMap[_selectedChangeRemoveTestIndex].IsAvailable)
                {
                    _selectedChangeRemoveTestMap[_selectedChangeRemoveTestIndex].ButtonAction?.Invoke();
                    e.Handled = true;
                }
                // If the rename field or speech edit box is open
                else if (ShowRenameEffectField || ShowSpeechEditBox)
                {
                    ShowRenameEffectField = false;
                    ShowSpeechEditBox = false;
                    e.Handled = true;
                }
                // If a button was selected, invoke its action.
                else if (element is ButtonOptionModel button && button.IsAvailable)
                {
                    button.ButtonAction?.Invoke();
                    e.Handled = true;
                }
                // If a sound effect option was pressed, option the change/remove/test option dialog
                else if (element is SoundEffectOption)
                {
                    ShowChangeRemoveTestEffectOptions = true;
                    _selectedChangeRemoveTestMap[_selectedChangeRemoveTestIndex].IsSelected = true;
                    e.Handled = true;
                }
            }
            // If the change/remove/test option dialog, rename or speech edit box dialog is open, close it on go back button press
            else if (e.ButtonEvent == InputEvent.ButtonEventType.GoBack)
            {
                e.Handled = ShowChangeRemoveTestEffectOptions || ShowRenameEffectField || ShowSpeechEditBox;

                _selectedChangeRemoveTestMap[_selectedChangeRemoveTestIndex].IsSelected = false;
                _selectedChangeRemoveTestIndex = 0;

                ShowChangeRemoveTestEffectOptions = false;
                ShowRenameEffectField = false;
                ShowSpeechEditBox = false;
            }
        }

        protected NavigationManager.Node[] GetNavigationNodesForOptions(ObservableCollection<SoundEffectOption> options)
        {
            var numberOfOptions = options.Count;
            // The file options, the add button, potentially one scroller and lastly two buttons
            var numberOfElements = numberOfOptions + 1 + (ShowMultipleOptionsScroller ? 1 : 0) + 2;
            var navigationNodes = new NavigationManager.Node[numberOfElements];
            SelectedElementMap.Clear();
            var elementCtr = 0;

            // The options
            for (var i = 0; i < numberOfOptions; i++)
            {
                SelectedElementMap.Add(elementCtr, options[i]);
                navigationNodes[elementCtr] = new(0, i);
                elementCtr++;
            }

            // The add button
            SelectedElementMap.Add(elementCtr, AddOptionModel);
            navigationNodes[elementCtr++] = new(1, 0);

            // The scroller
            if (ShowMultipleOptionsScroller)
            {
                SelectedElementMap.Add(elementCtr, ScrollerSelectable);
                navigationNodes[elementCtr++] = new(2, 0);
            }

            // Add the two buttons
            var buttonRow = Math.Max(1, numberOfOptions);
            SelectedElementMap.Add(elementCtr, RenameEffectModel);
            navigationNodes[elementCtr++] = new(0, buttonRow);

            SelectedElementMap.Add(elementCtr, RemoveButtonModel);
            navigationNodes[elementCtr] = new(2, buttonRow);

            return navigationNodes;
        }
        #endregion
    }
}
