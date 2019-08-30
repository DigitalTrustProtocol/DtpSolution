using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface IValidatorFactory
    {
        IIdentityValidator GetIdentityValidator(Identity identity);
    }
}