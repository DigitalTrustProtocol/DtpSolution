using DtpCore.Interfaces;
using System;
using System.Text.RegularExpressions;

namespace DtpCore.Model.Schema
{
    public class Hash160IdentityValidator : IIdentityValidator
    {

        public const string NAME = "hash160";
        public const string NotStringErrorTemplate = "{0}{1} is invalid, has to be string.";

        public IFormatProvider InvalidAddressErrorTemplate { get; private set; }

        public void Validate(string name, Identity identity, object source, string location, SchemaValidationResult result)
        {
            result.MaxRangeCheck($"{name} Id", identity.Id, location, SchemaValidationResult.DEFAULT_MAX_LENGTH);
            result.MaxRangeCheck($"{name} Proof", identity.Proof, location, 0);

            var regex = new Regex(@"^[13nmD][a-km-zA-HJ-NP-Z0-9]{26,33}$");
            if (!regex.IsMatch(identity.Id))
            {
                result.Errors.Add(string.Format(InvalidAddressErrorTemplate, location, $"{name} Id"));
                return;
            }
        }
    }
}
