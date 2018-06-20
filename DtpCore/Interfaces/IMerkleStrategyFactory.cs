using DtpCore.Interfaces;

namespace DtpCore.Interfaces
{
    public interface IMerkleStrategyFactory
    {
        IMerkleTree GetStrategy(string name);
    }
}