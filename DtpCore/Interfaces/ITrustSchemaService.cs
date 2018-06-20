using DtpCore.Enumerations;
using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface ITrustSchemaService
    {
        SchemaValidationResult Validate(Trust trust, TrustSchemaValidationOptions options = TrustSchemaValidationOptions.Full);
        SchemaValidationResult Validate(Package package, TrustSchemaValidationOptions options = TrustSchemaValidationOptions.Full);
    }
}