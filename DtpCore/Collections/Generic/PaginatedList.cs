using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DtpCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DtpCore.Collections.Generic
{
    public class PaginatedList<T> : List<T>, IPaginatedList<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        private PaginatedList(IQueryable<T> query)
        {
            foreach (var item in query)
            {
                Add(item);
            }
        }

        private PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }

        public static async Task<PaginatedList<T>> CreateAsync(
            IQueryable<T> source, int pageIndex, int pageSize)
        {
            if (pageSize > 0)
            {
                var count = await source.CountAsync();
                var items = await source.Skip(
                    (pageIndex - 1) * pageSize)
                    .Take(pageSize).ToListAsync();
                return new PaginatedList<T>(items, count, pageIndex, pageSize);
            }
            else
            {
                return new PaginatedList<T>(source);
            }
        }
    }
}
