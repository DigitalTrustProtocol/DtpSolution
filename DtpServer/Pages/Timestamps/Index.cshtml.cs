using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using DtpCore.Repository;
using DtpCore.Collections.Generic;
using DtpCore.Extensions;
using System.Collections;
using DtpServer.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using System.Linq.Expressions;

namespace DtpServer.Pages.Timestamps
{
    public class IndexModel : ListPageModel<Timestamp>
    {
        private readonly TrustDBContext _context;

        public IndexModel(TrustDBContext context)
        {
            _context = context;
        }


        public async Task OnGetAsync(string sortOrder, string sortField, string currentFilter, string searchString, byte[] source, int? pageIndex)
        {
            InitProperties(sortOrder, sortField, currentFilter, searchString, pageIndex);

            var query = from s in _context.Timestamps.AsNoTracking()
                        select s;


            if (source != null)
                query = query.Where(p => p.Source == source);
            else
                query = BuildQuery(CurrentFilter, query);

            query = AddSorting(query, "Registered", "_desc");

            List = await PaginatedList<Timestamp>.CreateAsync(query.AsNoTracking(), PageIndex, PageSize);
        }

        public RouteValueDictionary GetParam(Timestamp timestamp)
        {
            var dict = InitParam();

            dict["source"] = Convert.ToBase64String(timestamp.Source);

            return dict;
        }

        private IQueryable<Timestamp> BuildQuery(string searchString, IQueryable<Timestamp> query)
        {
            if (String.IsNullOrEmpty(searchString))
                return query;

            if (searchString.IsHex() && searchString.Length > 7)
            {
                var searchBytes = searchString.FromHexToBytes();
                query = query.Where(s => s.Source == searchBytes || s.Path == searchBytes);
                
                return query;
            }

            if (DateTime.TryParse(searchString, out DateTime time))
            {
                var unixTime = time.ToUnixTime();
                query = query.Where(s => s.Registered == unixTime);
                return query;
            }

            Expression<Func<Timestamp, bool>> q = null;
            if (int.TryParse(searchString, out int proofId))
                q = s => s.Proof != null && s.Proof.DatabaseID == proofId;

            var likeSearch = $"%{searchString}%";
            q = q.Or(s => EF.Functions.Like(s.Blockchain, likeSearch)
                || EF.Functions.Like(s.Type, likeSearch)
                || EF.Functions.Like(s.Service, likeSearch));

            return query.Where(q);
        }

    }
}
