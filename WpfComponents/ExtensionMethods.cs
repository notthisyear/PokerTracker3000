using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace PokerTracker3000.WpfComponents
{
    internal static class ExtensionMethods
    {
        public static bool TryFindAllChildrenOfType<T>(this DependencyObject parent, List<T> children, int exitAfter = -1, string childName = "") where T : DependencyObject
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is not T childOfMatchingType)
                {
                    _ = TryFindAllChildrenOfType(child, children, exitAfter, childName);
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                        children.Add(childOfMatchingType);
                }
                else
                {
                    children.Add(childOfMatchingType);
                }

                if (exitAfter == children.Count)
                    break;
            }
            return children.Count > 0;
        }
        public static T? TryFindChildOfType<T>(this DependencyObject parent, string childName = "") where T : DependencyObject
        {
            T? foundChild = null;
            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is not T childOfMatchingType)
                {
                    foundChild = TryFindChildOfType<T>(child, childName);
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
