using System.Windows;
using System.Windows.Controls;

namespace PokerTracker3000.WpfComponents.EditGameOptions
{
    public class EditOptionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? EditGameStagesTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is not SideMenuViewModel.GameEditOption option)
                return base.SelectTemplate(item, container);

            return option switch
            {
                SideMenuViewModel.GameEditOption.GameStages => EditGameStagesTemplate ?? new(),
                _ => new(),
            };
        }
    }
}
