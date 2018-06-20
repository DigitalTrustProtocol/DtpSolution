using DtpCore.Interfaces;

namespace DtpCore.Interfaces
{
    public interface IDerivationStrategyFactory
    {
        IDerivationStrategy GetService(string name);
    }
}