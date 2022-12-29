using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Collections
{
    public interface IPaged<out T> : IPaged
    {
        IEnumerable<T> Items { get; }
    }
    public interface IPaged
    {
        bool HasNextPage { get; }
        bool HasPreviousPage { get; }
        bool IsFirstPage { get; }
        bool IsLastPage { get; }
        int PageIndex { get; }
        int Pages { get; }
        int PageSize { get; }
        int TotalItems { get; }
    }
}
