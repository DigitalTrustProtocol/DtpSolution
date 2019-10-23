using DtpCore.Interfaces;
using System.Text.RegularExpressions;

namespace DtpCore.Model.Schema
{
    public class AlphaNumericIdentityValidator : IIdentityValidator
    {

        public const string NAME = "alphanumeric";
        public const string NotAlphaNumericErrorTemplate = "{0}{1} is invalid, has to be alpha numeric.";


        public void Validate(string name, Identity identity, object source, string location, SchemaValidationResult result)
        {
            result.MaxRangeCheck($"{name} Id", identity.Id, location, SchemaValidationResult.DEFAULT_TITLE_LENGTH);
            result.MaxRangeCheck($"{name} Proof", identity.Proof, location, 0);

            var regex = new Regex(@"^\w+$"); // ^[0-9]+$

            if (!regex.IsMatch(identity.Id))
            {
                result.Errors.Add(string.Format(NotAlphaNumericErrorTemplate, location, $"{name}.Id"));
            }
        }
    }
}
