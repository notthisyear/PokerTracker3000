using System.Windows;
using System.Windows.Media;

namespace PokerTracker3000.WpfComponents
{
    internal static class ExtensionMethods
    {
        public static T? TryFindChild<T>(this DependencyObject parent, string childName = "") where T : DependencyObject
        {
            T? foundChild = null;
            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is not T childOfMatchingType)
                {
                    foundChild = TryFindChild<T>(child, childName);
                    if (foundChild != null)
                        break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                    {
                        foundChild = childOfMatchingType;
                        break;
                    }
                }
                else
                {
                    foundChild = childOfMatchingType;
                    break;
                }
            }

            return foundChild;
        }
    }
}
