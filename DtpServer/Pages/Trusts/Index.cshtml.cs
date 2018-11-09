using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using DtpCore.Repository;
using DtpCore.Interfaces;
using DtpCore.Extensions;
using System.Collections;
using DtpCore.Collections.Generic;
using System.Linq.Expressions;

namespace DtpServer.Pages.Trusts
{
    public class IndexModel : PageModel
    {
        public int PageSize = 20;

        private readonly ITrustDBService _trustDBService;

        public PaginatedList<Trust> Trusts { get; set; }

        public string CurrentFilter { get; set; }
        public string CurrentSortField { get; set; }
        public string CurrentSortOrder { get; set; }

        public IndexModel(ITrustDBService trustDBService)
        {
            _trustDBService = trustDBService;
        }


        public async Task OnGetAsync(string sortOrder, string sortField, string currentFilter, string searchString, string issuerAddress, string subjectAddress, string scopeValue, int? pageIndex)
        {
            if (sortOrder.EndsWithIgnoreCase("!"))
                sortOrder = sortOrder == "!" ? "_desc" : "";

            CurrentSortField = sortField;
            CurrentSortOrder = sortOrder;


            if (searchString != null)
                pageIndex = 1;
            else
                searchString = currentFilter;
            CurrentFilter = searchString;


            var query = BuildQuery(searchString);

            if (issuerAddress != null)
                query = query.Where(p => p.Issuer.Address == issuerAddress);

            if (subjectAddress != null)
                query = query.Where(p => p.Subject.Address == subjectAddress);

            if (scopeValue != null)
                query = query.Where(p => p.Scope == scopeValue);

            switch (CurrentSortField + CurrentSortOrder)
            {
                case "Created":
                    query = query.OrderBy(s => s.Created);
                    break;
                case "Created_desc":
                    query = query.OrderByDescending(s => s.Created);
                    break;
                default:
                    query = query.OrderByDescending(s => s.Created);
                    break;
            }

            Trusts = await PaginatedList<Trust>.CreateAsync(query, pageIndex ?? 1, PageSize);
        }

        private IQueryable<Trust> BuildQuery(string searchString)
        {
            var query = from s in _trustDBService.DBContext.Trusts.AsNoTracking()
                        select s;

            if (String.IsNullOrEmpty(searchString))
                return query;

            if (searchString.IsHex() && searchString.Length > 39)
            {
                var searchBytes = searchString.FromHexToBytes();
                if (searchBytes.Length == 32)
                    query = query.Where(s => s.Id == searchBytes);

                return query;
            }

            if (DateTime.TryParse(searchString, out DateTime time))
            {
                var unixTime = time.ToUnixTime();
                query = query.Where(s => s.Created == unixTime
                    || s.Activate == unixTime
                    || s.Expire == unixTime);

                return query;
            }

            query = query.Where(s => s.Issuer.Address == searchString || s.Subject.Address == searchString);

            Expression<Func<Trust, bool>> q = null;

            var likeSearch = $"%{searchString}%";
            Expression<Func<Trust, bool>> search = s => EF.Functions.Like(s.Type, likeSearch)
                || EF.Functions.Like(s.Claim, likeSearch)
                || EF.Functions.Like(s.Scope, likeSearch);

            q = q.Or(search);

            query = query.Where(q);

            return query;
        }


    }
}
