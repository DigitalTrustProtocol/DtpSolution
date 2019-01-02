using DtpCore.Enumerations;
using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface ITrustSchemaService
    {
        string GetTrustTypeString(Claim trust);
        TrustType GetTrustTypeObject(Claim trust);

        SchemaValidationResult Validate(Claim trust, TrustSchemaValidationOptions options = TrustSchemaValidationOptions.Full);
        SchemaValidationResult Validate(Package package, TrustSchemaValidationOptions options = TrustSchemaValidationOptions.Full);
    }
}