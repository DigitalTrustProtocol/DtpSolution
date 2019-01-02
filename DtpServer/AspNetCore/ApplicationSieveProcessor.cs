using DtpCore.Model;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DtpServer.AspNetCore
{

    public class ApplicationSieveProcessor : SieveProcessor
    {
        public ApplicationSieveProcessor(
            IOptions<SieveOptions> options,
            ISieveCustomSortMethods customSortMethods,
            ISieveCustomFilterMethods customFilterMethods)
            : base(options, customSortMethods, customFilterMethods)
        {
        }

        protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
        {
            mapper.Property<Package>(p => p.Created)
                .CanFilter()
                .CanSort();

            //mapper.Property<Package>(p => p.Server.Id)
            //    .CanFilter()
            //    .CanSort();

            return mapper;
        }
    }
}
