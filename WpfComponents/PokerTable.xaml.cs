using System;
using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameSession;

namespace PokerTracker3000.WpfComponents
{
    public partial class PokerTable : UserControl
    {
        #region Dependency properties
        public GameSessionManager ViewModel
        {
            get => (GameSessionManager)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(GameSessionManager),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        public PokerTable()
        {
            InitializeComponent();
            Loaded += PokerTableLoaded;
        }

        private void PokerTableLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= PokerTableLoaded;

            if (ViewModel == default)
                return;

            ViewModel.FocusManager.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName?.Equals(nameof(ViewModel.FocusManager.CurrentFocusArea), StringComparison.InvariantCulture) ?? false)
                {
                    if (ViewModel != default && ViewModel.FocusManager.CurrentFocusArea == MainWindowFocusManager.FocusArea.EditNameBox)
                    {
                        changeNameBox.Focus();
                        changeNameBox.Select(changeNameBox.Text.Length, 0);
                    }
                }
            };
        }
    }
}
