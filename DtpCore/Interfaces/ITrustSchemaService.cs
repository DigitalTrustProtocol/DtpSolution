using DtpCore.Enumerations;
using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface ITrustSchemaService
    {
        string GetTrustTypeString(Trust trust);
        TrustType GetTrustTypeObject(Trust trust);

        SchemaValidationResult Validate(Trust trust, TrustSchemaValidationOptions options = TrustSchemaValidationOptions.Full);
        SchemaValidationResult Validate(Package package, TrustSchemaValidationOptions options = TrustSchemaValidationOptions.Full);
    }
}