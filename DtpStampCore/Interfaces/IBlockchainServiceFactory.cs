namespace DtpStampCore.Interfaces
{
    public interface IBlockchainServiceFactory
    {
        IBlockchainService GetService(string name = null);
    }
}