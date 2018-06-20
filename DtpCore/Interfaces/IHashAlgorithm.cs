namespace DtpCore.Interfaces
{
    public interface IHashAlgorithm
    {
        string AlgorithmName { get; }
        int Length { get; }

        byte[] HashOf(byte[] data);
    }
}
