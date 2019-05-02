using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface IIdentityValidator
    {
        void Validate(string name, Identity identity, object source, string location, SchemaValidationResult result);
    }
}