using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Web.UI.Code.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> source,
            Func<T, string> value,
            Func<T, string> text,
            Func<T, bool> selectedValue = null)
            where T : class
        {
            if (source == null)
                return new SelectList(new List<string>());

            return source.Select(s => new SelectListItem
            {
                Text = text(s),
                Value = value(s),
                Selected = selectedValue != null ? selectedValue(s) : false
            });
        }

        public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> source,
            Func<T, string> value,
            Func<T, string> text,
            object selectedValue)
            where T : class
        {
            if (source == null)
                return new SelectList(new List<string>());

            return source.Select(s => new SelectListItem
            {
                Text = text(s),
                Value = value(s),
                Selected = selectedValue == null ? false : Equals(selectedValue.ToString(), value(s).ToString())
            });
        }

        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource item)
        {
            yield return item;
            foreach (TSource data in source)
            {
                yield return data;
            }
        }
    }
}