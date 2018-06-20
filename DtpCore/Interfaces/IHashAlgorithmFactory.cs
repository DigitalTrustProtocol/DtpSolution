using DtpCore.Interfaces;

namespace DtpCore.Interfaces
{
    public interface IHashAlgorithmFactory
    {
        IHashAlgorithm GetAlgorithm(string name);
        string DefaultAlgorithmName();
    }
}