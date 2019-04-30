using DtpCore.Interfaces;
using System.Text.RegularExpressions;

namespace DtpCore.Model.Schema
{
    public class DTPAddressIdentityValidator : IIdentityValidator
    {

        public const string NAME = "address.dtp1";
        public const string InvalidAddressErrorTemplate = "{0}{1} has invalid format.";

        public void Validate(string name, Identity identity, Claim claim, string location, SchemaValidationResult result)
        {
            result.MaxRangeCheck($"{name} Id", identity.Id, location, 40);
            result.NotEmptyCheck($"{name} Proof", identity.Proof, location);

            // TODO: Properly validate Id
            var regex = new Regex(@"^[13nmD][a-km-zA-HJ-NP-Z0-9]{26,33}$"); 
            if (!regex.IsMatch(identity.Id))
            {
                result.Errors.Add(string.Format(InvalidAddressErrorTemplate, location, $"{name} Id"));
                return;
            }
        }
    }
}
