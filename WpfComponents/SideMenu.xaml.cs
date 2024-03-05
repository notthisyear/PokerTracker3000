using System.Collections.Generic;
using System.Windows.Controls;
using PokerTracker3000.GameSession;

namespace PokerTracker3000.WpfComponents
{
    public partial class SideMenu : UserControl
    {
        public List<SideMenuOptionModel> SideMenuOptions { get; } = new()
        {
            new() { OptionText = "Pause game", IsHighlighted = true, IsSubOption = false },
            new() { OptionText = "Go to next stage", IsHighlighted = false, IsSubOption = false },
            new() { OptionText = "Go to previous stage", IsHighlighted = false, IsSubOption = false },
            new() { OptionText = "Load game", IsHighlighted = false, IsSubOption = false },
            new() { OptionText = "Save game", IsHighlighted = false, IsSubOption = false },
            new() { OptionText = "Settings game", IsHighlighted = false, IsSubOption = false },
            new() { OptionText = "Add player", IsHighlighted = false, IsSubOption = true },
            new() { OptionText = "Load player", IsHighlighted = false, IsSubOption = true },
            new() { OptionText = "Add/remove chip", IsHighlighted = false, IsSubOption = true }
        };

        public SideMenu()
        {
            InitializeComponent();
        }
    }
}
