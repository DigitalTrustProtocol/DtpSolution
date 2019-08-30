using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Model.Schema;
using DtpGraphCore.Interfaces;

namespace DtpGraphCore.Model.Schema
{
    public class QueryRequestValidator : IQueryRequestValidator
    {

        private IValidatorFactory _validatorFactory;

        public QueryRequestValidator(IValidatorFactory validatorFactory)
        {
            _validatorFactory = validatorFactory;
        }

        public SchemaValidationResult Validate(object model)
        {
            var data = model as QueryRequest;
            var result = new SchemaValidationResult();
            var location = "QueryRequest";

            result.MissingCheck($"Issuer", data.Issuer, location);
            result.MissingCheck($"Issuer.Type", data.Issuer.Type, location);
            result.MaxRangeCheck($"Issuer.Type", data.Issuer.Type, location, SchemaValidationResult.DEFAULT_MAX_LENGTH);

            var identityValidator = _validatorFactory.GetIdentityValidator(data.Issuer);

            identityValidator.Validate("Issuer", data.Issuer, data, "QueryRequest", result);

            result.MaxRangeCheck($"Types", data.Types.Count, location, 127);
            var index = 0;
            foreach (var type in data.Types)
            {
                result.MaxRangeCheck($"Types[{index}]", type, location, 127);
                index++;
            }

            result.MaxRangeCheck($"Subjects", data.Subjects.Count, location, 1024);
            index = 0;
            foreach (var subject in data.Subjects)
            {
                result.MaxRangeCheck($"Subjects[{index}]", subject, location, 127);
                index++;
            }

            return result;
        }
    }
}
