using System;
using DtpGraphCore.Model;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpGraphCore.Interfaces;

namespace DtpGraphCore.Services
{
    public class QueryRequestService : IQueryRequestService
    {
        private IDerivationStrategy _derivationStrategy;

        public QueryRequestService(IDerivationStrategy derivationStrategy)
        {
            _derivationStrategy = derivationStrategy;
        }

        public void Verify(QueryRequest query)
        {
            if (query.Issuer == null)
                throw new ApplicationException("Missing issuers");

            if (query.Issuer.Length > _derivationStrategy.AddressLength)
                throw new ApplicationException("Invalid byte length on Issuer : " + query.Issuer);

            foreach (var subject in query.Subjects)
            {
                if (subject.Address.Length > _derivationStrategy.AddressLength)
                    throw new ApplicationException("Invalid byte length on subject id: " +subject.Address);
            }
        }
    }
}
