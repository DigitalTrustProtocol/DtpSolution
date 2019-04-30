namespace DtpCore.Interfaces
{
    public interface IValidatorFactory
    {
        IIdentityValidator GetIdentityValidator(string type);
    }
}