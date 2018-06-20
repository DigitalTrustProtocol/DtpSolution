using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DtpCore.Collections.Generic;
using DtpCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;

namespace DtpServer.AspNetCore.Mvc.RazorPages
{
    public class ListPageModel<T> : PageModel
    {
        public int PageSize = 20;

        public RouteValueDictionary RouteValue { get; set; }

        public PaginatedList<T> List { get; set; }

        public string CurrentFilter { get; set; }
        public string CurrentSortField { get; set; }
        public string CurrentSortOrder { get; set; }
        public int PageIndex { get; set; }


        protected void InitProperties(string sortOrder, string sortField, string currentFilter, string searchString, int? pageIndex)
        {
            if (sortOrder.EndsWithIgnoreCase("!"))
                sortOrder = sortOrder == "!" ? "_desc" : "";

            CurrentSortField = sortField;
            CurrentSortOrder = sortOrder;

            PageIndex = (pageIndex != null) ? (int)pageIndex : 1;
            if (searchString != null)
                PageIndex = 1;
            else
                searchString = currentFilter;
            CurrentFilter = searchString;
        }

        protected RouteValueDictionary InitParam()
        {
            var dict = this.RouteValue == null ? new RouteValueDictionary() :
                                     new RouteValueDictionary(RouteValue);

            dict["sortOrder"] = CurrentSortOrder;
            dict["sortField"] = CurrentSortField;
            dict["currentFilter"] = CurrentFilter;
            dict["pageIndex"] = PageIndex;

            return dict;
        }


        protected IQueryable<T> AddSorting(IQueryable<T> query, string defaultField, string defaultSort)
        {
            if (!String.IsNullOrWhiteSpace(CurrentSortField))
            {
                if (CurrentSortOrder == "")
                    query = query.OrderBy(CurrentSortField);
                else
                    query = query.OrderByDescending(CurrentSortField);
            }
            else
            if (!String.IsNullOrWhiteSpace(defaultField))
            {
                if (defaultSort == "")
                    query = query.OrderBy(defaultField);
                else
                    query = query.OrderByDescending(defaultField);
            }
                
            return query;
        }
    }
}
