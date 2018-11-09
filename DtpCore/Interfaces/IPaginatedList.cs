using System.Collections.Generic;

namespace DtpCore.Interfaces
{
    public interface IPaginatedList<TView> : IList<TView>
    {
        bool HasNextPage { get; }
        bool HasPreviousPage { get; }
        int PageIndex { get; }
        int TotalPages { get; }
    }
}