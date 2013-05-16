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

        public static string SSUrl(string sortBy, string sortOrder, int pageNum)
        {
            var ret = string.Format("http://store.steampowered.com/search/?sort_by={0}&sort_order={1}&specials=1&page={2}",
                sortBy,
                sortOrder.ToUpper(),
                pageNum);

            return ret;
        }
    }
}
