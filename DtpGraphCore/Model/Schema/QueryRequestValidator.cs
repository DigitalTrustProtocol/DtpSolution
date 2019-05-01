using DtpCore.Model;
using DtpGraphCore.Interfaces;

namespace DtpGraphCore.Model.Schema
{
    public class QueryRequestValidator : IQueryRequestValidator
    {
        public SchemaValidationResult Validate(object model)
        {
            var data = model as QueryRequest;
            // TODO: Implement
            return new SchemaValidationResult();
        }
    }
}
