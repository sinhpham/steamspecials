using System.Windows;
using System.Windows.Media;

namespace SteamSpecialsWp.ViewModel
{
    public static class Helper
    {
        public static ChildItem FindVisualChild<ChildItem>(DependencyObject obj)
            where ChildItem : DependencyObject {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is ChildItem) {
                    return (ChildItem)child;
                } else {
                    ChildItem childOfChild = FindVisualChild<ChildItem>(child);
                    if (childOfChild != null) {
                        return childOfChild;
                    }
                }
            }
            return null;
        }
    }
}
