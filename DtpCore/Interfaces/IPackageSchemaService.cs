using DtpCore.Enumerations;
using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface IPackageSchemaService
    {
        string GetTrustTypeString(Claim trust);
        TrustType GetTrustTypeObject(Claim trust);

        SchemaValidationResult Validate(Claim claim, TrustSchemaValidationOptions options = TrustSchemaValidationOptions.Full);
        SchemaValidationResult Validate(Package package, TrustSchemaValidationOptions options = TrustSchemaValidationOptions.Full);
    }
}