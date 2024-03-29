using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameComponents;
using PokerTracker3000.GameSession;

namespace PokerTracker3000.WpfComponents
{
    public partial class SessionOverview : UserControl
    {
        #region Dependency properties
        public GameStagesManager StageManager
        {
            get => (GameStagesManager)GetValue(StageManagerProperty);
            set => SetValue(StageManagerProperty, value);
        }
        public static readonly DependencyProperty StageManagerProperty = DependencyProperty.Register(
            nameof(StageManager),
            typeof(GameStagesManager),
            typeof(SessionOverview),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        public GameClock Clock
        {
            get => (GameClock)GetValue(ClockProperty);
            set => SetValue(ClockProperty, value);
        }
        public static readonly DependencyProperty ClockProperty = DependencyProperty.Register(
            nameof(Clock),
            typeof(GameClock),
            typeof(SessionOverview),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        public CurrencyType Currency
        {
            get => (CurrencyType)GetValue(CurrencyProperty);
            set => SetValue(CurrencyProperty, value);
        }
        public static readonly DependencyProperty CurrencyProperty = DependencyProperty.Register(
            nameof(Currency),
            typeof(CurrencyType),
            typeof(SessionOverview),
            new FrameworkPropertyMetadata(CurrencyType.SwedishKrona, FrameworkPropertyMetadataOptions.AffectsRender));
        #region Read-only dependency property
        public GameStage? NextStage
        {
            get => (GameStage?)GetValue(s_nextStageProperty);
            private set => SetValue(s_nextStagePropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_nextStagePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(NextStage),
            typeof(GameStage),
            typeof(SessionOverview),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_nextStageProperty = s_nextStagePropertyKey.DependencyProperty;
        #endregion

        #endregion

        public SessionOverview()
        {
            InitializeComponent();
            Loaded += SessionOverviewLoaded;
        }

        private void SessionOverviewLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= SessionOverviewLoaded;

            if (StageManager == default)
                return;

            StageManager.CurrentStageChanged += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (e.newStage == default)
                    {
                        NextStage = default;
                        return;
                    }

                    _ = StageManager.TryGetNextStage(e.newStage, out var stage);
                    NextStage = stage;
                });
            };

            StageManager.StageAdded += (s, e) =>
            {
                if (StageManager.Stages.Count == 2)
                {
                    _ = StageManager.TryGetStageByNumber(2, out var stage);
                    NextStage = stage;
                }
            };

            StageManager.StageRemoved += (s, e) =>
            {
                if (StageManager.Stages.Count == 1)
                    NextStage = default;
            };
        }
    }
}
