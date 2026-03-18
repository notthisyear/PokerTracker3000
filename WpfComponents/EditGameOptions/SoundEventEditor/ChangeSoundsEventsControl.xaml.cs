using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.Common;
using PokerTracker3000.GameSession;
using PokerTracker3000.GameSession.Sound;
using PokerTracker3000.Interfaces;
using ButtonEventArgs = PokerTracker3000.Interfaces.IInputRelay.ButtonEventArgs;
using InputEvent = PokerTracker3000.Input.UserInputEvent;
using NavigationEventArgs = PokerTracker3000.Interfaces.IInputRelay.NavigationEventArgs;
using SoundCondition = PokerTracker3000.GameSession.Sound.Condition;

namespace PokerTracker3000.WpfComponents.EditGameOptions
{
    public partial class ChangeSoundsEventsControl : UserControl, IInputRelay
    {
        #region Dependency properties
        public GameSessionManager SessionManager
        {
            get { return (GameSessionManager)GetValue(SessionManagerProperty); }
            set { SetValue(SessionManagerProperty, value); }
        }
        public static readonly DependencyProperty SessionManagerProperty = DependencyProperty.Register(
            nameof(SessionManager),
            typeof(GameSessionManager),
            typeof(ChangeSoundsEventsControl),
            new FrameworkPropertyMetadata(default));

        public AudioManager AudioManager
        {
            get { return (AudioManager)GetValue(AudioManagerProperty); }
            set { SetValue(AudioManagerProperty, value); }
        }
        public static readonly DependencyProperty AudioManagerProperty = DependencyProperty.Register(
            nameof(AudioManager),
            typeof(AudioManager),
            typeof(ChangeSoundsEventsControl),
            new FrameworkPropertyMetadata(default));
        #endregion

        #region Read-only dependency properties
        public SoundEvent? SelectedSoundEvent
        {
            get => (SoundEvent?)GetValue(s_selectedSoundEventProperty);
            private set => SetValue(s_selectedSoundEventPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_selectedSoundEventPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(SelectedSoundEvent),
            typeof(SoundEvent),
            typeof(ChangeSoundsEventsControl),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_selectedSoundEventProperty = s_selectedSoundEventPropertyKey.DependencyProperty;

        public SelectableEntity? SelectedSoundConditionOrEffect
        {
            get => (SelectableEntity?)GetValue(s_selectedSoundConditionOrEffectProperty);
            private set => SetValue(s_selectedSoundConditionOrEffectPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_selectedSoundConditionOrEffectPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(SelectedSoundConditionOrEffect),
            typeof(SelectableEntity),
            typeof(ChangeSoundsEventsControl),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_selectedSoundConditionOrEffectProperty = s_selectedSoundConditionOrEffectPropertyKey.DependencyProperty;

        public bool SoundConditionAndEditAreaActive
        {
            get => (bool)GetValue(s_soundConditionAndEditAreaActiveProperty);
            private set => SetValue(s_soundConditionAndEditAreaActivePropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_soundConditionAndEditAreaActivePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(SoundConditionAndEditAreaActive),
            typeof(bool),
            typeof(ChangeSoundsEventsControl),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_soundConditionAndEditAreaActiveProperty = s_soundConditionAndEditAreaActivePropertyKey.DependencyProperty;

        public bool AddSoundEffectBoxOpen
        {
            get => (bool)GetValue(s_addSoundEffectBoxOpenProperty);
            private set => SetValue(s_addSoundEffectBoxOpenPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_addSoundEffectBoxOpenPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(AddSoundEffectBoxOpen),
            typeof(bool),
            typeof(ChangeSoundsEventsControl),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_addSoundEffectBoxOpenProperty = s_addSoundEffectBoxOpenPropertyKey.DependencyProperty;
        #endregion

        #region Public properties
        public ButtonOptionModel AddSoundEventModel { get; } = new("Add sound event");

        public ButtonOptionModel LoadSoundEventsModel { get; } = new("Load...");

        public ButtonOptionModel SaveSoundEventsModel { get; } = new("Save...", type: ButtonOptionModel.OptionType.Success);

        public ButtonOptionModel AddSoundConditionModel { get; } = new("Add condition", type: ButtonOptionModel.OptionType.Success);

        public ButtonOptionModel AddSoundEffectModel { get; } = new("Add effect", type: ButtonOptionModel.OptionType.Success);

        public ButtonOptionModel RenameSoundEventModel { get; } = new("Rename event");

        public ButtonOptionModel TestSoundEventModel { get; } = new("Test event", type: ButtonOptionModel.OptionType.Info);

        public ButtonOptionModel RemoveSoundEventModel { get; } = new("Remove event", type: ButtonOptionModel.OptionType.Cancel);

        public ButtonOptionModel AddBuiltInSoundEffectModel { get; } = new("Built-in", type: ButtonOptionModel.OptionType.Success);

        public ButtonOptionModel AddSoundEffectFromFileModel { get; } = new("From file", type: ButtonOptionModel.OptionType.Success);

        public ButtonOptionModel AddSynthesizedSpeechSoundEffectModel { get; } = new("Speech", type: ButtonOptionModel.OptionType.Success);
        #endregion

        #region Events
        public event EventHandler<NavigationEventArgs>? Navigate;
        public event EventHandler<ButtonEventArgs>? ButtonEvent;
        #endregion

        #region Private fields
        private enum SelectedArea
        {
            None,
            SoundEvent,
            ConditionAndEffect,
            SelectNewEffectTypeBox,
            ConditionAndEffectEditBox
        }

        private readonly Dictionary<SelectedArea, (int navigationId, int selectedElementIndex)> _navigationIdAndSelectedIndex = [];
        private readonly Dictionary<SelectedArea, Dictionary<int, SelectableEntity?>> _selectedEntityMap = [];
        private SelectedArea _selectedArea = SelectedArea.None;

        private const int MaxSoundConditions = 5;
        private const int MaxSoundEffects = 5;
        #endregion

        public ChangeSoundsEventsControl()
        {
            InitializeComponent();
            Loaded += ControlLoadedEvent;
        }

        private void ControlLoadedEvent(object sender, RoutedEventArgs e)
        {
            Loaded -= ControlLoadedEvent;
            if (SessionManager == default || AudioManager == default)
                return;

            SetButtonActions();

            foreach (var areaType in Enum.GetValues<SelectedArea>())
            {
                if (areaType == SelectedArea.None || areaType == SelectedArea.ConditionAndEffectEditBox)
                    continue;

                _navigationIdAndSelectedIndex.Add(
                    areaType,
                    (SessionManager.NavigationManager.RegisterNavigation(GetNavigationNodesForArea(areaType)), 0));
            }

            // Do an up and down navigation to set the top element in the sound event area as selected
            _selectedArea = SelectedArea.SoundEvent;
            HandleNavigationInArea(InputEvent.NavigationDirection.Up);
            HandleNavigationInArea(InputEvent.NavigationDirection.Down);

            WeakEventManager<IInputRelay, NavigationEventArgs>.
                AddHandler(SessionManager, nameof(SessionManager.Navigate), HandleNavigate);
            WeakEventManager<IInputRelay, ButtonEventArgs>.
                AddHandler(SessionManager, nameof(SessionManager.ButtonEvent), HandleButton);
        }

        private void SetButtonActions()
        {
            AddSoundEventModel.ButtonAction = () =>
            {
                AudioManager.AddSoundEvent(new() { Name = "<NEW EVENT>" });

                // Recreate the navigation for the current area
                var (id, selectedIndex) = _navigationIdAndSelectedIndex[SelectedArea.SoundEvent];
                SessionManager.NavigationManager.ReplaceNavigation(id, GetNavigationNodesForArea(SelectedArea.SoundEvent));
                _navigationIdAndSelectedIndex[SelectedArea.SoundEvent] = (id, selectedIndex + 1);
            };
            LoadSoundEventsModel.ButtonAction = () =>
            {
                // TODO: Implement;
            };
            SaveSoundEventsModel.ButtonAction = () =>
            {
                // TODO: Implement;
            };

            AddSoundConditionModel.ButtonAction = () =>
            {
                if (!AddSoundConditionModel.IsAvailable || SelectedSoundEvent == default)
                    return;

                SelectedSoundEvent.AddConditionToEvent(new EventCondition(GameEventBus.EventType.GameStarted));
                RecreateNavigationInConditionAndEffectArea(SelectedSoundEvent.Conditions.Count);

                AddSoundConditionModel.IsAvailable = SelectedSoundEvent.Conditions.Count < MaxSoundConditions;
            };
            AddSoundEffectModel.ButtonAction = () =>
            {
                _selectedArea = SelectedArea.SelectNewEffectTypeBox;
                AddSoundEffectBoxOpen = true;
                var (_, selectedIndex) = _navigationIdAndSelectedIndex[_selectedArea];
                if (_selectedEntityMap[SelectedArea.SelectNewEffectTypeBox].TryGetValue(selectedIndex, out var entity))
                    entity?.IsSelected = true;
            };
            RenameSoundEventModel.ButtonAction = () =>
            {
                // TODO: Implement
            };
            TestSoundEventModel.ButtonAction = () =>
            {
                if (SelectedSoundEvent == default)
                    return;

                TaskCompletionSource tcs = new();
                TestSoundEventModel.IsAvailable = false;
                AudioManager.TestSoundEvent(SelectedSoundEvent, tcs);
                Task.Run(() =>
                {
                    tcs.Task.Wait();
                    Application.Current.Dispatcher.Invoke(() => { TestSoundEventModel.IsAvailable = true; });
                });
            };
            RemoveSoundEventModel.ButtonAction = () =>
            {
                if (SelectedSoundEvent == default)
                    return;

                var (_, selectedElementIndex) = _navigationIdAndSelectedIndex[_selectedArea];
                var idToRemove = SelectedSoundEvent.Id;

                // Move back to the sound event area
                HandleButtonPressInConditionAndEffectArea(InputEvent.ButtonEventType.GoBack, selectedElementIndex);

                // Select something else before removing the element
                HandleNavigationInArea(InputEvent.NavigationDirection.Up);

                // Remove the element
                AudioManager.RemoveSoundEvent(idToRemove);

                // Recreate the navigation
                var (id, oldIdx) = _navigationIdAndSelectedIndex[SelectedArea.SoundEvent];
                SessionManager.NavigationManager.ReplaceNavigation(id, GetNavigationNodesForArea(SelectedArea.SoundEvent));
                _navigationIdAndSelectedIndex[SelectedArea.SoundEvent] = (id, Math.Min(oldIdx, _selectedEntityMap[SelectedArea.SoundEvent].Count - 1));
            };

            AddBuiltInSoundEffectModel.ButtonAction = () =>
            {
                if (!AddSoundEffectModel.IsAvailable || SelectedSoundEvent == default)
                    return;

                var (_, selectedIndex) = _navigationIdAndSelectedIndex[SelectedArea.ConditionAndEffect];
                SelectedSoundEvent.AddSoundEffectToEvent(new BuiltInSoundEffect(BuiltInSoundEffectType.BluesRiff));
                RecreateNavigationInConditionAndEffectArea(selectedIndex + 1);

                AddSoundEffectModel.IsAvailable = SelectedSoundEvent.SoundEffectsOnEvent.Count < MaxSoundEffects;
            };
            AddSoundEffectFromFileModel.ButtonAction = () =>
            {
                if (!AddSoundEffectModel.IsAvailable || SelectedSoundEvent == default)
                    return;

                var (_, selectedIndex) = _navigationIdAndSelectedIndex[SelectedArea.ConditionAndEffect];
                SelectedSoundEvent.AddSoundEffectToEvent(new FromFileSoundEffect("<NEW EFFECT>"));
                RecreateNavigationInConditionAndEffectArea(selectedIndex + 1);

                AddSoundEffectModel.IsAvailable = SelectedSoundEvent.SoundEffectsOnEvent.Count < MaxSoundEffects;
            };
            AddSynthesizedSpeechSoundEffectModel.ButtonAction = () =>
            {
                if (!AddSoundEffectModel.IsAvailable || SelectedSoundEvent == default)
                    return;

                var (_, selectedIndex) = _navigationIdAndSelectedIndex[SelectedArea.ConditionAndEffect];
                SelectedSoundEvent.AddSoundEffectToEvent(new SpeechSoundEffect("<NEW SPEECH EFFECT>"));
                RecreateNavigationInConditionAndEffectArea(selectedIndex + 1);

                AddSoundEffectModel.IsAvailable = SelectedSoundEvent.SoundEffectsOnEvent.Count < MaxSoundEffects;
            };
        }

        private void RecreateNavigationInConditionAndEffectArea(int selectedIndex)
        {
            var (id, _) = _navigationIdAndSelectedIndex[SelectedArea.ConditionAndEffect];
            SessionManager.NavigationManager.ReplaceNavigation(id, GetNavigationNodesForArea(SelectedArea.ConditionAndEffect));
            _navigationIdAndSelectedIndex[SelectedArea.ConditionAndEffect] = (id, selectedIndex);
        }

        private NavigationManager.Node[] GetNavigationNodesForArea(SelectedArea area)
        {
            if (!_selectedEntityMap.ContainsKey(area))
                _selectedEntityMap.Add(area, []);

            if (_selectedEntityMap[area].Count > 0)
                _selectedEntityMap[area].Clear();

            switch (area)
            {
                case SelectedArea.SoundEvent:
                    {
                        // The sound event and three button
                        var numberOfSoundEvents = AudioManager.GameSounds.Count;
                        var navigationNodes = new NavigationManager.Node[numberOfSoundEvents + 3];
                        for (var i = 0; i < numberOfSoundEvents; i++)
                        {
                            navigationNodes[i] = new(0, i);
                            _selectedEntityMap[area].Add(i, AudioManager.GameSounds[i]);
                        }

                        var buttonIdx = numberOfSoundEvents;

                        // Add sound event button 
                        navigationNodes[buttonIdx] = new(0, numberOfSoundEvents);
                        _selectedEntityMap[area].Add(buttonIdx, AddSoundEventModel);
                        buttonIdx++;

                        // Load sound events button
                        navigationNodes[buttonIdx] = new(1, numberOfSoundEvents);
                        _selectedEntityMap[area].Add(buttonIdx, LoadSoundEventsModel);
                        buttonIdx++;

                        // Save sound events button
                        navigationNodes[buttonIdx] = new(2, numberOfSoundEvents);
                        _selectedEntityMap[area].Add(buttonIdx, SaveSoundEventsModel);
                        return navigationNodes;
                    }
                case SelectedArea.ConditionAndEffect:
                    {
                        // The sound event conditions and effects, as well as five buttons
                        var numberOfConditions = SelectedSoundEvent == default ? 0 : SelectedSoundEvent.Conditions.Count;
                        var numberOfEffects = SelectedSoundEvent == default ? 0 : SelectedSoundEvent.SoundEffectsOnEvent.Count;

                        var totalNumberOfElements = numberOfConditions + numberOfEffects + 5;
                        var navigationNodes = new NavigationManager.Node[totalNumberOfElements];
                        var elementCtr = 0;
                        for (var i = 0; i < numberOfConditions; i++)
                        {
                            navigationNodes[elementCtr] = new(0, i);
                            _selectedEntityMap[area].Add(elementCtr, SelectedSoundEvent!.Conditions[i]);
                            elementCtr++;
                        }

                        // Add sound event condition button
                        navigationNodes[elementCtr] = new(0, numberOfConditions);
                        _selectedEntityMap[area].Add(elementCtr, AddSoundConditionModel);
                        elementCtr++;

                        for (var i = 0; i < numberOfEffects; i++)
                        {
                            navigationNodes[elementCtr] = new(1, i);
                            _selectedEntityMap[area].Add(elementCtr, SelectedSoundEvent!.SoundEffectsOnEvent[i]);
                            elementCtr++;
                        }

                        // Add sound effect button
                        navigationNodes[elementCtr] = new(1, numberOfEffects);
                        _selectedEntityMap[area].Add(elementCtr, AddSoundEffectModel);
                        elementCtr++;

                        // Rename sound event button
                        var buttonRow = Math.Max(numberOfConditions, numberOfEffects) + 1;
                        navigationNodes[elementCtr] = new(0, buttonRow);
                        _selectedEntityMap[area].Add(elementCtr, RenameSoundEventModel);
                        elementCtr++;

                        // Test sound event button
                        navigationNodes[elementCtr] = new(0.5f, buttonRow);
                        _selectedEntityMap[area].Add(elementCtr, TestSoundEventModel);
                        elementCtr++;

                        // Remove sound event button
                        navigationNodes[elementCtr] = new(1, buttonRow);
                        _selectedEntityMap[area].Add(elementCtr, RemoveSoundEventModel);

                        return navigationNodes;
                    }
                case SelectedArea.SelectNewEffectTypeBox:
                    {
                        // Three buttons
                        var navigationNodes = new NavigationManager.Node[3];

                        navigationNodes[0] = new(0, 0);
                        _selectedEntityMap[area].Add(0, AddBuiltInSoundEffectModel);

                        navigationNodes[1] = new(1, 0);
                        _selectedEntityMap[area].Add(1, AddSoundEffectFromFileModel);

                        navigationNodes[2] = new(2, 0);
                        _selectedEntityMap[area].Add(2, AddSynthesizedSpeechSoundEffectModel);

                        return navigationNodes;
                    }
                default:

                    throw new InvalidOperationException($"{nameof(ChangeSoundsEventsControl)} should not handle navigation for {area}");
            }

        }

        #region Navigation
        private void HandleNavigate(object? sender, NavigationEventArgs e)
        {
            if (SessionManager == default ||
                SessionManager.CurrentGameEditOption != SideMenuViewModel.GameEditOption.ChangeSoundEvents)
            {
                return;
            }

            Navigate?.Invoke(this, e);

            if (!e.Handled)
                HandleNavigationInArea(e.Direction);
        }

        private void HandleNavigationInArea(InputEvent.NavigationDirection e)
        {
            if (_selectedArea == SelectedArea.ConditionAndEffectEditBox)
                throw new InvalidOperationException($"Navigation for {_selectedArea} cannot be handled here");

            var (id, oldIdx) = _navigationIdAndSelectedIndex[_selectedArea];
            var newIdx = SessionManager.NavigationManager.Navigate(id, oldIdx, e);
            _navigationIdAndSelectedIndex[_selectedArea] = (id, newIdx);

            switch (_selectedArea)
            {
                case SelectedArea.SoundEvent:
                    HandleNavigationInSoundEventArea(oldIdx, newIdx);
                    break;
                case SelectedArea.ConditionAndEffect:
                    HandleNavigationInConditionAndEffectArea(oldIdx, newIdx);
                    break;
                case SelectedArea.SelectNewEffectTypeBox:
                    HandleNavigationInSelectNewEffectTypeBox(oldIdx, newIdx);
                    break;
            }
        }

        private void HandleNavigationInSoundEventArea(int oldElementIndex, int newElementIndex)
        {
            // Clear any previous selection
            if (_selectedEntityMap[SelectedArea.SoundEvent].TryGetValue(oldElementIndex, out var oldSelectedEntity))
                oldSelectedEntity?.IsSelected = false;

            // Set the add sound event button as selected if needed
            if (_selectedEntityMap[SelectedArea.SoundEvent].TryGetValue(newElementIndex, out var newSelectedEntity))
            {
                newSelectedEntity?.IsSelected = true;
                if (newSelectedEntity is SoundEvent soundEvent)
                    SelectedSoundEvent = soundEvent;
                else
                    SelectedSoundEvent = default;
            }

            if (SelectedSoundEvent != default)
            {
                // Reset the selected element index so that we'll start at the top
                var (id, _) = _navigationIdAndSelectedIndex[SelectedArea.ConditionAndEffect];
                _navigationIdAndSelectedIndex[SelectedArea.ConditionAndEffect] = (id, 0);
            }
        }

        private void HandleNavigationInConditionAndEffectArea(int oldElementIndex, int newElementIndex)
        {
            // Clear any previous selection
            if (_selectedEntityMap[SelectedArea.ConditionAndEffect].TryGetValue(oldElementIndex, out var oldSelectedEntity))
                oldSelectedEntity?.IsSelected = false;

            // Set the buttons as selected if needed
            if (_selectedEntityMap[SelectedArea.ConditionAndEffect].TryGetValue(newElementIndex, out var newSelectedEntity))
            {
                newSelectedEntity?.IsSelected = true;
                if (newSelectedEntity is SoundCondition || newSelectedEntity is SoundEffect)
                    SelectedSoundConditionOrEffect = newSelectedEntity;
                else
                    SelectedSoundConditionOrEffect = default;
            }
            else
            {
                SelectedSoundConditionOrEffect = default;
            }
        }

        private void HandleNavigationInSelectNewEffectTypeBox(int oldElementIndex, int newElementIndex)
        {
            // Clear previous selection
            if (_selectedEntityMap[SelectedArea.SelectNewEffectTypeBox].TryGetValue(oldElementIndex, out var oldSelectedEntity))
                oldSelectedEntity?.IsSelected = false;

            // Set new selection
            if (_selectedEntityMap[SelectedArea.SelectNewEffectTypeBox].TryGetValue(newElementIndex, out var newSelectedIntity))
                newSelectedIntity?.IsSelected = true;
        }
        #endregion

        #region Button presses
        private void HandleButton(object? sender, ButtonEventArgs e)
        {
            if (SessionManager == default ||
                SessionManager.CurrentGameEditOption != SideMenuViewModel.GameEditOption.ChangeSoundEvents)
            {
                return;
            }

            ButtonEvent?.Invoke(this, e);

            var handleEvent =
                (e.ButtonEvent == InputEvent.ButtonEventType.Select) ||
                (e.ButtonEvent == InputEvent.ButtonEventType.GoBack && _selectedArea != SelectedArea.SoundEvent);

            if (!e.Handled && handleEvent)
            {
                HandleButtonPressInArea(e.ButtonEvent);
                e.Handled = true;
            }
        }

        private void HandleButtonPressInArea(InputEvent.ButtonEventType e)
        {
            switch (_selectedArea)
            {
                case SelectedArea.SoundEvent:
                    HandleButtonPressInSoundEventArea(e, _navigationIdAndSelectedIndex[_selectedArea].selectedElementIndex);
                    break;

                case SelectedArea.ConditionAndEffect:
                    HandleButtonPressInConditionAndEffectArea(e, _navigationIdAndSelectedIndex[_selectedArea].selectedElementIndex);
                    break;

                case SelectedArea.SelectNewEffectTypeBox:
                    HandleButtonPressInSelectNewEffectTypeBox(e);
                    break;

                case SelectedArea.ConditionAndEffectEditBox:
                    HandleButtonPressInConditionAndEffectAreaEditBox(e);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private void HandleButtonPressInSoundEventArea(InputEvent.ButtonEventType e, int selectedElementIndex)
        {
            if (e != InputEvent.ButtonEventType.Select)
                return;

            if (_selectedEntityMap[SelectedArea.SoundEvent].TryGetValue(selectedElementIndex, out var entity))
            {
                if (entity is SoundEvent _)
                {
                    _selectedArea = SelectedArea.ConditionAndEffect;

                    // Setup the navigation for the area
                    var (id, effectAndConditionIndex) = _navigationIdAndSelectedIndex[SelectedArea.ConditionAndEffect];
                    SessionManager.NavigationManager.ReplaceNavigation(id, GetNavigationNodesForArea(SelectedArea.ConditionAndEffect));
                    HandleNavigationInConditionAndEffectArea(effectAndConditionIndex, effectAndConditionIndex);
                }
                else if (entity is ButtonOptionModel model && model.IsAvailable)
                {
                    model.ButtonAction?.Invoke();
                }
            }
        }

        private void HandleButtonPressInConditionAndEffectArea(InputEvent.ButtonEventType e, int selectedElementIndex)
        {
            if (e == InputEvent.ButtonEventType.GoBack)
            {
                // Deselect the currently selected element in the effect and condition area
                HandleNavigationInConditionAndEffectArea(selectedElementIndex, -1);
                _selectedArea = SelectedArea.SoundEvent;
                return;
            }

            if (e != InputEvent.ButtonEventType.Select)
                return;

            if (!_selectedEntityMap[SelectedArea.ConditionAndEffect].TryGetValue(selectedElementIndex, out var entity))
                return;

            // If a button is pressed
            if (entity is ButtonOptionModel button && button.IsAvailable)
            {
                button.ButtonAction?.Invoke();
                return;
            }

            // If either a condition or a effect was pressed
            if (entity is SoundCondition || entity is SoundEffect)
            {
                _selectedArea = SelectedArea.ConditionAndEffectEditBox;
                SoundConditionAndEditAreaActive = true;

                // If applicable, subscribe to the condition control change events
                var editCondition = editConditionOrEffectControl.TryFindChildOfType<EditSoundCondition>();
                editCondition?.NewValidContentEvent += HandleNewValidCondition;
                editCondition?.RemoveEvent += HandleRemoveCondition;

                // If applicable, subscribe to the condition control change events
                var editEffect = editConditionOrEffectControl.TryFindChildOfType<SoundEffectEditorBase>();
                editEffect?.NewValidContentEvent += HandleNewValidEffect;
                editEffect?.RemoveEvent += HandleRemoveEffect;
            }
        }

        private void HandleButtonPressInSelectNewEffectTypeBox(InputEvent.ButtonEventType e)
        {
            if (e == InputEvent.ButtonEventType.GoBack)
            {
                var (id, selectedIndex) = _navigationIdAndSelectedIndex[_selectedArea];
                if (_selectedEntityMap[_selectedArea].TryGetValue(selectedIndex, out var entity))
                    entity?.IsSelected = false;

                _navigationIdAndSelectedIndex[_selectedArea] = (id, 0);
                _selectedArea = SelectedArea.ConditionAndEffect;
                AddSoundEffectBoxOpen = false;
            }
            else if (e == InputEvent.ButtonEventType.Select)
            {
                var (_, selectedIndex) = _navigationIdAndSelectedIndex[_selectedArea];
                if (_selectedEntityMap[_selectedArea].TryGetValue(selectedIndex, out var entity) &&
                    entity is ButtonOptionModel button &&
                    button.IsAvailable)
                {
                    // Actually add the new sound effect
                    button.ButtonAction?.Invoke();

                    // Go back one step
                    HandleButtonPressInSelectNewEffectTypeBox(InputEvent.ButtonEventType.GoBack);

                    // Since we must have been on the add sound effect button, navigate up one step
                    // to get to the newly added event
                    HandleNavigationInArea(InputEvent.NavigationDirection.Up);

                    // Now select the new event
                    // Note: We need to let the UI thread finish creating the sound effect editor before we select it.
                    //       Hence, we schedule a task on the thread pool that does the actual selection after a small delay.
                    Task.Run(() =>
                    {
                        Task.Delay(50).Wait();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            HandleButtonPressInArea(InputEvent.ButtonEventType.Select);
                        });
                    });
                }
            }
        }

        private void HandleButtonPressInConditionAndEffectAreaEditBox(InputEvent.ButtonEventType e)
        {
            if (e == InputEvent.ButtonEventType.GoBack)
            {
                _selectedArea = SelectedArea.ConditionAndEffect;
                SoundConditionAndEditAreaActive = false;

                // If applicable, unsubscribe to sound condition control change events
                var editCondition = editConditionOrEffectControl.TryFindChildOfType<EditSoundCondition>();
                editCondition?.NewValidContentEvent -= HandleNewValidCondition;
                editCondition?.RemoveEvent -= HandleRemoveCondition;

                // If applicable, unsubscribe to sound effect control change events
                var editEffect = editConditionOrEffectControl.TryFindChildOfType<SoundEffectEditorBase>();
                editEffect?.NewValidContentEvent -= HandleNewValidEffect;
                editEffect?.RemoveEvent -= HandleRemoveEffect;
            }
        }
        #endregion

        #region Sound condition control change event handlers
        private void HandleNewValidCondition(object? sender, EventArgs e)
        {
            if (sender is not EditSoundCondition control)
                return;

            if (SelectedSoundEvent == default)
                return;

            if (SelectedSoundConditionOrEffect is not SoundCondition oldCondition)
                return;

            if (SoundCondition.TryGet(control.SelectedConditionVariable,
                control.SelectedConditionCheckType,
                control.CurrentValue,
                control.SelectedGameEventType,
                out var condition))
            {
                var i = SelectedSoundEvent.GetIndexOfCondition(oldCondition);
                if (i == -1)
                    return;

                // Add and remove the condition
                SelectedSoundEvent.AddConditionToEvent(condition!, i);
                SelectedSoundEvent.RemoveConditionAtFromEvent(i + 1);

                // Recreate the navigation
                var (id, effectAndConditionIndex) = _navigationIdAndSelectedIndex[SelectedArea.ConditionAndEffect];
                SessionManager.NavigationManager.ReplaceNavigation(id, GetNavigationNodesForArea(SelectedArea.ConditionAndEffect));
                HandleNavigationInConditionAndEffectArea(effectAndConditionIndex, effectAndConditionIndex);
            }
        }

        private void HandleRemoveCondition(object? sender, EventArgs e)
        {
            if (SelectedSoundEvent == default)
                return;

            if (SelectedSoundConditionOrEffect is not SoundCondition condition)
                return;

            // Move back to the sound condition and effect area
            HandleButtonPressInArea(InputEvent.ButtonEventType.GoBack);

            // Move back one more time to deselect the condition as well
            HandleButtonPressInArea(InputEvent.ButtonEventType.GoBack);

            // Find and remove the condition
            var i = SelectedSoundEvent.GetIndexOfCondition(condition);
            if (i == -1)
                return;

            SelectedSoundEvent.RemoveConditionAtFromEvent(i);

            // Inject a select button press, which will navigate us back to the condition and effect area,
            // recreate the navigation in the process
            HandleButtonPressInArea(InputEvent.ButtonEventType.Select);

            AddSoundConditionModel.IsAvailable = SelectedSoundEvent.Conditions.Count < MaxSoundConditions;
        }
        #endregion

        #region Sound effect control change event handlers
        private void HandleNewValidEffect(object? sender, EventArgs e)
        {
            if (sender is not SoundEffectEditorBase control)
                return;

            if (SelectedSoundEvent == default)
                return;

            if (SelectedSoundConditionOrEffect is not SoundEffect oldEffect)
                return;

            if (control is BuiltInSoundEffectEditor builtIn)
            {
                var effect = new BuiltInSoundEffect(builtIn.SelectedBuiltInSoundEffect);
                var i = SelectedSoundEvent.GetIndexOfSoundEffect(oldEffect);
                if (i == -1)
                    return;

                // Add and remove the effect
                SelectedSoundEvent.AddSoundEffectToEvent(effect!, i);
                SelectedSoundEvent.RemoveSoundEffectFromEvent(i + 1);

                // Recreate the navigation and trigger a navigation to select the new item
                var (id, effectAndConditionIndex) = _navigationIdAndSelectedIndex[SelectedArea.ConditionAndEffect];
                SessionManager.NavigationManager.ReplaceNavigation(id, GetNavigationNodesForArea(SelectedArea.ConditionAndEffect));
                HandleNavigationInConditionAndEffectArea(effectAndConditionIndex, effectAndConditionIndex);
            }
        }

        private void HandleRemoveEffect(object? sender, EventArgs e)
        {
            if (SelectedSoundEvent == default)
                return;

            if (SelectedSoundConditionOrEffect is not SoundEffect effect)
                return;

            // Move back to the sound condition and effect area
            HandleButtonPressInArea(InputEvent.ButtonEventType.GoBack);

            // Move back one more time to deselect the condition as well
            HandleButtonPressInArea(InputEvent.ButtonEventType.GoBack);

            // Find and remove the condition
            var i = SelectedSoundEvent.GetIndexOfSoundEffect(effect);
            if (i == -1)
                return;

            SelectedSoundEvent.RemoveSoundEffectFromEvent(i);

            // Inject a select button press, which will navigate us back to the condition and effect area,
            // recreate the navigation in the process
            HandleButtonPressInArea(InputEvent.ButtonEventType.Select);

            AddSoundEffectModel.IsAvailable = SelectedSoundEvent.SoundEffectsOnEvent.Count < MaxSoundEffects;
        }
        #endregion

        ~ChangeSoundsEventsControl()
        {
            Debug.WriteLine("Finalizer for ChangeSoundsEventsControl ran");
        }
    }
}
