using DtpCore.Interfaces;

namespace DtpCore.Model.Schema
{
    public class StringIdentityValidator : IIdentityValidator
    {

        public const string NAME = "string.dtp1";
        public const string NotStringErrorTemplate = "{0}{1} is invalid, has to be string.";


        public void Validate(string name, Identity identity, object source, string location, SchemaValidationResult result)
        {
            result.MaxRangeCheck($"{name} Id", identity.Id, location, SchemaValidationResult.DEFAULT_MAX_LENGTH);
            result.MaxRangeCheck($"{name} Proof", identity.Proof, location, 0);
        }
    }
}
