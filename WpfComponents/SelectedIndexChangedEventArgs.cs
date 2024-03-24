using System.Windows;

namespace PokerTracker3000.WpfComponents
{
    public class SelectedIndexChangedEventArgs(RoutedEvent routedEvent, int newIndex) : RoutedEventArgs(routedEvent)
    {
        public int NewIndex { get; } = newIndex;
    }
}
