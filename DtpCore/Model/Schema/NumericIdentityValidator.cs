using DtpCore.Interfaces;
using System.Text.RegularExpressions;

namespace DtpCore.Model.Schema
{
    public class NumericIdentityValidator : IIdentityValidator
    {

        public const string NAME = "numeric";
        public const string NotNumericErrorTemplate = "{0}{1} is invalid, has to be numeric.";


        public void Validate(string name, Identity identity, object source, string location, SchemaValidationResult result)
        {
            result.MaxRangeCheck($"{name} Id", identity.Id, location, SchemaValidationResult.DEFAULT_MAX_LENGTH);
            result.NotEmptyCheck($"{name} Proof", identity.Proof, location);

            var regex = new Regex(@"^\d+$"); // ^[0-9]+$

            if (!regex.IsMatch(identity.Id))
            {
                result.Errors.Add(string.Format(NotNumericErrorTemplate, location, $"{name}.Id"));
            }
        }
    }
}
