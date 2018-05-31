using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ArkBot.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static IEnumerable<T> FindVisualChildrenWithPath<T>(this DependencyObject depObj, string path) where T : DependencyObject
        {
            if (path == null || depObj == null) yield break;

            var r = new Regex(@"^(?<type>.+?)(?:\[(?<name>.+?)\])?$", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var segments = path.Split('/').Select(x =>
            {
                var m = r.Match(x);
                return Tuple.Create(m?.Groups["type"].Value, m?.Groups["name"].Value);
            }).ToArray();

            foreach (var item in FindVisualChildrenWithPathInternal<T>(depObj, segments))
            {
                yield return item;
            }
        }

        private static IEnumerable<T> FindVisualChildrenWithPathInternal<T>(this DependencyObject depObj, Tuple<string, string>[] path, int currentIndex = 0) where T : DependencyObject
        {
            if (depObj == null || !(path?.Length > 0)) yield break;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                if (child?.GetType().Name.Equals(path[currentIndex].Item1) == true
                    && (path[currentIndex].Item2 == null || (child as FrameworkElement)?.Name.Equals(path[currentIndex].Item2) == true))
                {
                    if (currentIndex == path.Length - 1) yield return child as T;
                    else
                    {
                        foreach (T childOfChild in FindVisualChildrenWithPathInternal<T>(child, path, currentIndex + 1))
                        {
                            yield return childOfChild;
                        }
                    }
                }
                else
                {
                    foreach (T childOfChild in FindVisualChildrenWithPathInternal<T>(child, path, 0))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static IEnumerable<DependencyObject> FindVisualParents(this DependencyObject depObj)
        {
            if (depObj != null)
            {

                DependencyObject parent = System.Windows.Media.VisualTreeHelper.GetParent(depObj);
                if (parent != null)
                {
                    yield return parent;
                }

                foreach (var parentOfParent in FindVisualParents(parent))
                {
                    yield return parentOfParent;
                }
            }
        }
    }
}
