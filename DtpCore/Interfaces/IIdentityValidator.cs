using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface IIdentityValidator
    {
        void Validate(string name, Identity identity, Claim claim, string location, SchemaValidationResult result);
    }
}