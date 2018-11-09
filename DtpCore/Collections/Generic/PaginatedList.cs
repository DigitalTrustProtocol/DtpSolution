using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DtpCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DtpCore.Collections.Generic
{
    public class PaginatedList<TView> : List<TView>, IPaginatedList<TView>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

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

        /// <summary>
        /// Ignores the item if it is null.
        /// </summary>
        /// <param name="item"></param>
        public void AddOrIgnore(TView item)
        {
            if (item == null)
                return;
            Add(item);
        }


        public static async Task<PaginatedList<TView>> CreateAsync<TSource>(IQueryable<TSource> query, int pageIndex, int pageSize) where TSource : TView
        {
            return await CreateAsync(query, (p) => p, pageIndex, pageSize);
        }

        public static async Task<PaginatedList<TView>> CreateAsync<TSource>(IQueryable<TSource> query, Func<TSource, TView> converter, int pageIndex, int pageSize)
        {
            if (pageSize > 0)
            {
                var count = await query.CountAsync();
                var items = await query.Skip(
                    (pageIndex - 1) * pageSize)
                    .Take(pageSize).ToListAsync();

                var list = new PaginatedList<TView>
                {
                    PageIndex = pageIndex,
                    TotalPages = (int)Math.Ceiling(count / (double)pageSize)
                };

                foreach (var item in items)
                {
                    list.AddOrIgnore(converter.Invoke(item));
                }
                return list;

            }
            else
            {
                var list = new PaginatedList<TView>();
                foreach (var item in query)
                {
                    list.AddOrIgnore(converter.Invoke(item));
                }
                return list;
            }
        }
    }
}
