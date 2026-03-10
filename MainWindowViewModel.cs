using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.Common;
using PokerTracker3000.Common.Messages;
using PokerTracker3000.GameSession;
using PokerTracker3000.GameSession.Sound;
using PokerTracker3000.Interfaces;

using InputEvent = PokerTracker3000.Input.UserInputEvent;

namespace PokerTracker3000.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        #region Public properties

        #region Backing fields
        public bool _leftSideMenuOpen = false;
        public bool _rightSideMenuOpen = false;
        public bool _spotifyInfoOpen = false;
        #endregion

        public bool LeftSideMenuOpen
        {
            get => _leftSideMenuOpen;
            private set => SetProperty(ref _leftSideMenuOpen, value);
        }

        public bool RightSideMenuOpen
        {
            get => _rightSideMenuOpen;
            private set => SetProperty(ref _rightSideMenuOpen, value);
        }

        public string ProgramDescription { get; init; } = string.Empty;

        public bool SpotifyInfoOpen
        {
            get => _spotifyInfoOpen;
            private set => SetProperty(ref _spotifyInfoOpen, value);
        }

        public GameSessionManager SessionManager { get; }

        public SideMenuViewModel SideMenuViewModel { get; }

        public SpotifyClientViewModel SpotifyViewModel { get; }
        #endregion

        #region Private fields
        private readonly MainWindowFocusManager _focusManager;
        private readonly AudioManager _audioManager;
        private readonly IGameEventBus _eventBus;
        #endregion

        public MainWindowViewModel(IGameEventBus eventBus, ApplicationSettings settings, MainWindowFocusManager focusManager)
        {
            _focusManager = focusManager;
            _eventBus = eventBus;

            //new() {
            //    { AudioManager.SoundType.StageRiff, [ settings.RiffSoundEffectPath ] },
            //    { AudioManager.SoundType.PlayerBuyIn,
            //    [
            //        "C:\\Users\\programming\\C#\\poker\\sounds\\chips_please.wav",
            //        "C:\\Users\\programming\\C#\\poker\\sounds\\jackass_chips.wav"
            //    ] },
            //    { AudioManager.SoundType.PlayerEliminated,
            //    [
            //        "C:\\Users\\programming\\C#\\poker\\sounds\\cya.wav",
            //        "C:\\Users\\programming\\C#\\poker\\sounds\\goodbye_darling.wav",
            //        "C:\\Users\\programming\\C#\\poker\\sounds\\thanks_for_money.wav",
            //        "C:\\Users\\programming\\C#\\poker\\sounds\\you_sucks.wav",
            //        "C:\\Users\\programming\\C#\\poker\\sounds\\tu_quieres.wav",
            //    ] }
            //});

            _audioManager = new(eventBus, settings.RiffSoundEffectPath);
            AddDefaultSounds();

            var gameSettings = new GameSettings();
            var clock = new GameClock(_eventBus);
            SessionManager = new(eventBus,
                                 gameSettings,
                                 focusManager,
                                 _audioManager,
                                 new GameStagesManager(_eventBus, clock, gameSettings),
                                 new(),
                                 clock,
                                 settings.DefaultPlayerImagePath);

            SpotifyViewModel = new(focusManager,
                                   settings.ClientId,
                                   settings.LocalHttpListenerPort,
                                   settings.PkceAuthorizationVerifierLength,
                                   3000);

            SideMenuViewModel = new(eventBus, focusManager, SessionManager, SpotifyViewModel);
#if DEBUG
            HandleInputEvent(new()
            {
                Button = InputEvent.ButtonEventType.Start,
                Action = InputEvent.ButtonAction.Down,
                Direction = InputEvent.NavigationDirection.None,
                AllowRepeat = false,
                IsButtonEvent = true,
                IsNavigationEvent = false
            });

            for (var i = 0; i < 3; i++)
            {
                HandleInputEvent(new()
                {
                    Button = InputEvent.ButtonEventType.None,
                    Action = InputEvent.ButtonAction.Down,
                    Direction = InputEvent.NavigationDirection.Down,
                    AllowRepeat = false,
                    IsButtonEvent = false,
                    IsNavigationEvent = true
                });
            }

            HandleInputEvent(new()
            {
                Button = InputEvent.ButtonEventType.Select,
                Action = InputEvent.ButtonAction.Down,
                Direction = InputEvent.NavigationDirection.None,
                AllowRepeat = false,
                IsButtonEvent = true,
                IsNavigationEvent = false
            });

            for (var i = 0; i < 5; i++)
            {
                HandleInputEvent(new()
                {
                    Button = InputEvent.ButtonEventType.None,
                    Action = InputEvent.ButtonAction.Down,
                    Direction = InputEvent.NavigationDirection.Down,
                    AllowRepeat = false,
                    IsButtonEvent = false,
                    IsNavigationEvent = true
                });
            }
#endif
        }

        public void HandleInputEvent(InputEvent inputEvent)
        {
            if (inputEvent.IsButtonEvent)
            {
                if (inputEvent.Button == InputEvent.ButtonEventType.InfoButton)
                {
                    RightSideMenuOpen = !RightSideMenuOpen;
                }
                else
                {
                    _focusManager.HandleButtonPressedEvent(inputEvent.Button);
                    LeftSideMenuOpen = _focusManager.CurrentFocusArea == MainWindowFocusManager.FocusArea.LeftSideMenu ||
                        _focusManager.CurrentFocusArea == MainWindowFocusManager.FocusArea.SideMenuEditOption;
                    SpotifyInfoOpen = _focusManager.CurrentFocusArea == MainWindowFocusManager.FocusArea.SpotifyInformationBox;
                }
            }
            else if (inputEvent.IsNavigationEvent)
            {
                _focusManager.HandleNavigationEvent(inputEvent.Direction);
            }
        }

        private void AddDefaultSounds()
        {
            // Game started sound
            {
                SoundEvent gameStartedEvent = new()
                {
                    Name = "Game started"
                };
                gameStartedEvent.AddConditionToEvent(new EventCondition(GameEventBus.EventType.GameStarted));
                gameStartedEvent.AddSoundEffectToEvent(new BuiltInSoundEffect(BuiltInSoundEffectType.Riff));
                _audioManager.AddSoundEvent(gameStartedEvent);
            }

            // Stage end notification
            {
                var stageEndNotificationSeconds = 63;
                _eventBus.NotifyListeners(GameEventBus.EventType.StageTimeRemainingEventRequest,
                new StageTimeRemainingEventRequestMessage()
                {
                    Type = RequestType.AddEvent,
                    EventAtTimeSeconds = stageEndNotificationSeconds
                });
                SoundEvent stageEndNotification = new()
                {
                    Name = "Stage end notification"
                };
                stageEndNotification.AddConditionToEvent(new TimeCondition(stageEndNotificationSeconds));
                stageEndNotification.AddSoundEffectToEvent(new BuiltInSoundEffect(BuiltInSoundEffectType.Riff));
                var speechEffectStageEnd = new SpeechSoundEffect("Stage ending notification");
                speechEffectStageEnd.AddSpeechOption("Attention players! This stage will end in one minute");
                stageEndNotification.AddSoundEffectToEvent(speechEffectStageEnd);
                _audioManager.AddSoundEvent(stageEndNotification);
            }

            // New chip-lead
            {
                SoundEvent newChipLead = new()
                {
                    Name = "New chip-lead"
                };
                newChipLead.AddConditionToEvent(new EventCondition(GameEventBus.EventType.NewChipLead));
                var newChipLeadSoundEffect = new FromFileSoundEffect("Chip leader");
                newChipLeadSoundEffect.AddFileOption("C:\\Users\\programming\\C#\\poker\\sounds\\chip_leader.wav");
                newChipLeadSoundEffect.AddFileOption("C:\\Users\\programming\\C#\\poker\\sounds\\chips_please.wav");
                newChipLeadSoundEffect.Mode = MultipleOptionMode.Sequential;

                newChipLead.AddSoundEffectToEvent(newChipLeadSoundEffect);
                _audioManager.AddSoundEvent(newChipLead);
            }

            //var isFirstBetFromPlayer = (msg.MessageType == PlayerEventMessage.Type.AddOn
            //    || msg.MessageType == PlayerEventMessage.Type.BuyIn)
            //    && (msg.PlayerTotal - msg.AddOnOrBuyInAmount == 0);

            //var (attr, _) = msg.Currency.GetCustomAttributeFromEnum<CurrencyAttribute>();
            //var (speechMsg, soundType) = msg.MessageType switch
            //{
            //    PlayerEventMessage.Type.AddOn => (isFirstBetFromPlayer ?
            //        $"{msg.PlayerName} joined the game with a {GetFormattedDecimal(msg.AddOnOrBuyInAmount)} {attr!.Code} bet. Welcome!" :
            //        $"{msg.PlayerName} added another {GetFormattedDecimal(msg.AddOnOrBuyInAmount)} {attr!.Code} to the pot. The pot total is now {GetFormattedDecimal(msg.PotTotal)} {attr!.Code}. Nice.",
            //        SoundType.None),
            //    PlayerEventMessage.Type.BuyIn => (isFirstBetFromPlayer ?
            //        $"{msg.PlayerName} joined the game with a {msg.AddOnOrBuyInAmount} {attr!.Code} bet. Welcome!" :
            //        $"Chips please! {msg.PlayerName} bought in and added {GetFormattedDecimal(msg.AddOnOrBuyInAmount)} {attr!.Code} to the pot. The total is now {GetFormattedDecimal(msg.PotTotal)} {attr!.Code}.",
            //         SoundType.PlayerBuyIn),
            //    PlayerEventMessage.Type.Eliminated => (msg.PlayerTotal > 0 ?
            //        $"{msg.PlayerName} is eliminated. That's {GetFormattedDecimal(msg.PlayerTotal)} {attr!.Code} you won't see again." : string.Empty,
            //         SoundType.PlayerEliminated),
            //    _ => (string.Empty, SoundType.None)
            //};

            //var msg = type switch
            //{
            //    GameEventBus.EventType.GameStarted => "The game has now started. Good luck to all players.",
            //    GameEventBus.EventType.GamePaused => $"The game has been paused. {GetPauseComment()}",
            //    GameEventBus.EventType.GameDone => "All stages done, well done players. Goodnight, I'm out",
            //    _ => default
            //};

            //if (!string.IsNullOrEmpty(msg))
            //{
            //    if (type == GameEventBus.EventType.GameStarted || type == GameEventBus.EventType.GameDone)
            //        _audioQueue.Enqueue(new SoundItem(SoundType.StageRiff));
            //    _audioQueue.Enqueue(new SoundItem(SoundType.Speech, msg));
            //}

            //if (!string.IsNullOrEmpty(speechMsg))
            //{
            //    var useSpeech = isFirstBetFromPlayer; //  || Random.Shared.Next(1) == 1;
            //    if (useSpeech || soundType == SoundType.None)
            //        _audioQueue.Enqueue(new SoundItem(SoundType.Speech, speechMsg));
            //    else
            //        _audioQueue.Enqueue(new SoundItem(soundType));
            //}

            //var stageNumberOfMinutes = msg.StageNumberOfSeconds / 60;
            //var useMinutes = stageNumberOfMinutes > 0;
            //_audioQueue.Enqueue(new SoundItem(SoundType.StageRiff));
            //_audioQueue.Enqueue(new SoundItem(SoundType.Speech, msg.IsPause ?
            //    $"Attention players, it's time for a break. Let's go get a new beer or have a smoke. " +
            //    $"See you in {GetNumberString(useMinutes ? stageNumberOfMinutes : msg.StageNumberOfSeconds, useMinutes ? "minute" : "second")}." :
            //    $"Attention players, we're now at stage {msg.StageNumber} and the blinds have changed. The small blind is {GetFormattedDecimal(msg.SmallBlind)} and the big blind is {GetFormattedDecimal(msg.BigBlind)}."));

        }

        //    private static string GetNumberString(int number, string unit)
        //=> $"{number} {unit}{(number > 1 ? "s" : "")}";

        //    private static string GetFormattedDecimal(decimal value)
        //        => decimal.IsInteger(value) ? $"{value:F0}" : $"{value:F2}";

        //    private static string GetPauseComment()
        //    {
        //        string[] options =
        //        {
        //            "Finally, I need to pee.",
        //            string.Empty,
        //            "See you in a bit.",
        //            string.Empty,
        //            "Don't forget to turn me back on again.",
        //            string.Empty,
        //            "Is it snack time?"
        //        };
        //        var r = new Random();
        //        var idx = r.Next(0, options.Length);
        //        return options[idx];
        //    }
    }
}
