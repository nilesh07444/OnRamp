using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Collections
{
    [JsonObject]
    public sealed class Paged<T> : IPaged<T>
    {
        public Paged(IEnumerable<T> enumerable, int totalCount, int pageSize, int pageIndex)
        {
            Items = enumerable ?? Enumerable.Empty<T>();
            _totalRecords = totalCount;
            _pageSize = pageSize;
            PageIndex = pageIndex;
        }

        public Paged(IEnumerable<T> enumerable)
        {
            Items = enumerable;
        }

        public bool HasNextPage => Pages > PageIndex + 1;

        public bool HasPreviousPage => PageIndex > 0;

        public bool IsFirstPage => PageIndex == 0;

        public bool IsLastPage => (PageIndex + 1) == Pages;

        public IEnumerable<T> Items { get; }
        public int PageIndex { get; }

        public int Pages 
        {
            get
            {
                if (PageSize == 0)
                    return 1;

                var pages = TotalItems / (double)PageSize;

                return (int)Math.Ceiling(pages);
            }
        }

        public int PageSize
        {
            get { return _pageSize <= 0 ? TotalItems : _pageSize; }
        }

        public int TotalItems
        {
            get { return _totalRecords ?? Items.Count(); }
        }

        private readonly int _pageSize;
        private readonly int? _totalRecords;
    }
}
