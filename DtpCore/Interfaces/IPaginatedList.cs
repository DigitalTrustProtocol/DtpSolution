using System.Collections.Generic;

namespace DtpCore.Interfaces
{
    public interface IPaginatedList<T> : IList<T>
    {
        bool HasNextPage { get; }
        bool HasPreviousPage { get; }
        int PageIndex { get; }
        int TotalPages { get; }
    }
}