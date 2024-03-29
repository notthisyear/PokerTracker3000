using System.Windows;

namespace PokerTracker3000.WpfComponents
{
    public class SelectedIndexChangedEventArgs : RoutedEventArgs
    {
        public int NewIndex { get; }

        public SelectedIndexChangedEventArgs(RoutedEvent routedEvent, int newIndex)
            : base(routedEvent)
        {
            NewIndex = newIndex;
        }

        public SelectedIndexChangedEventArgs(int newIndex)
        {
            NewIndex = newIndex;
        }
    }
}
