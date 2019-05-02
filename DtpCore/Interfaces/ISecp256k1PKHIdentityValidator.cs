using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface ISecp256k1PKHIdentityValidator
    {
        void Validate(string name, Identity identity, Claim claim, string location, SchemaValidationResult result);
    }
}