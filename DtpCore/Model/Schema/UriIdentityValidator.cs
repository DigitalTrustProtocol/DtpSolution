using DtpCore.Interfaces;
using System;

namespace DtpCore.Model.Schema
{
    public class UriIdentityValidator : IIdentityValidator
    {

        public const string NAME = "uri";
        public const string NotUriErrorTemplate = "{0}{1} is invalid, has to be a valid uri.";


        public void Validate(string name, Identity identity, object source, string location, SchemaValidationResult result)
        {
            result.MaxRangeCheck($"{name} Id", identity.Id, location, SchemaValidationResult.DEFAULT_MAX_LENGTH);
            result.MaxRangeCheck($"{name} Proof", identity.Proof, location, 0);

            if (!Uri.IsWellFormedUriString(identity.Id, UriKind.Absolute))
            {
                result.Errors.Add(string.Format(NotUriErrorTemplate, location, $"{name}.Id"));
            }
        }
    }
}
