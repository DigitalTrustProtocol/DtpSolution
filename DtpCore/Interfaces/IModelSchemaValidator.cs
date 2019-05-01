using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface IModelSchemaValidator
    {
        SchemaValidationResult Validate(object model);
    }
}