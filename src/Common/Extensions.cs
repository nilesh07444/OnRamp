using Common.Collections;
using Common.Command;
using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class Extensions
    {
        public static IDictionary<string, string> ToDictionaryFromConnectionString(this string input)
        {
            var parts = input.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach (var p in parts)
            {
                var i = p.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (i.Length > 1)
                    parameters[i[0]] = i[1];
            }

            return parameters;
        }

        public static string ToConnectionString(this IDictionary<string, string> dictionary)
        {
            return string.Join(";", dictionary.Select(c => string.Format("{0}={1}", c.Key, c.Value)).ToArray());
        }
        public static DateTime? AtBeginningOfDay(this DateTime? date)
        {
            return GetDateAtTime(date, TimeOfDay.Beggining);
        }
        public static DateTime AtBeginningOfDay(this DateTime date)
        {
            return new DateTime?(date).AtBeginningOfDay().CovertToDateTime();
        }
        public static DateTime AtEndOfDay(this DateTime date)
        {
            return new DateTime?(date).AtEndOfDay().CovertToDateTime();
        }
        public static DateTime? AtEndOfDay(this DateTime? date)
        {
            return GetDateAtTime(date, TimeOfDay.End);
        }
        private static DateTime? GetDateAtTime(DateTime? date, TimeOfDay tod)
        {
            if (!date.HasValue)
                return new DateTime?();
            switch (tod)
            {
                case TimeOfDay.Beggining:
                    return new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 0, 0, 0);
                case TimeOfDay.End:
                    return new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 23, 59, 59);
                default:
                    return new DateTime?();
            }
        }
        private static DateTime CovertToDateTime(this DateTime? date)
        {
            if (!date.HasValue)
                return new DateTime();
            return date.Value;
        }
        private enum TimeOfDay
        {
            Beggining,
            End
        }
        public static Guid? ConvertToGuid(this string id)
        {
            Guid gId = Guid.Empty;
            if (Guid.TryParse(id, out gId))
                return gId;
            return new Guid?();
        }
        private static int Skip(this IPagedQuery query)
        {
            return (query.PageSize ?? 0) * query.Page;
        }

        private static int Take(this IPagedQuery query)
        {
            return query.PageSize ?? 0;
        }
        private static IEnumerable<T> Apply<T>(this IPagedQuery query, IEnumerable<T> list)
        {
            var l = list.AsQueryable();

            if (query.Take() > 0)
                l = l.Skip(query.Skip()).Take(query.Take());

            return l;
        }
        public static IPaged<TViewModel> GetPaged<TModel, TViewModel>(this IPagedQuery query,
         IEnumerable<TModel> enumerable, Func<TModel, TViewModel> projection)
        {
            var count = 0;
            var list = enumerable as IList<TModel> ?? enumerable.ToList();
            if (query.PageSize > 0)
                count = list.AsQueryable().Count();

            var pagedList = query.Apply(list).ToList();

            if (query.PageSize <= 0)
                count = pagedList.Count;

            return new Paged<TViewModel>(pagedList.AsQueryable().Select(projection), count, query.PageSize ?? 0, query.Page);
        }
        public static IPaged<TViewModel> GetPagedWithoutProjection<TViewModel>(this IPagedQuery query,
         IEnumerable<TViewModel> enumerable)
        {
            var count = 0;
            var list = enumerable as IList<TViewModel> ?? enumerable.ToList();
            if (query.PageSize > 0)
                count = list.AsQueryable().Count();
            var pagedList = query.Apply(list).ToList();
            if (query.PageSize <= 0)
                count = pagedList.Count;

            return new Paged<TViewModel>(pagedList.AsQueryable(), count, query.PageSize ?? 0, query.Page);
        }
    }
}