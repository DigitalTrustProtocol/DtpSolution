namespace DtpCore.Interfaces
{
    public interface IDerivationStrategy
    {
        int Length { get; }
        int AddressLength { get; }
        string ScriptName { get; }

        byte[] HashOf(byte[] data);
        byte[] KeyFromString(string wif);
        byte[] GetKey(byte[] seed);
        byte[] GetAddress(byte[] key);
        string StringifyAddress(byte[] key);
        byte[] SignMessage(byte[] key, byte[] data);
        byte[] Sign(byte[] key, byte[] data);
        bool VerifySignature(byte[] hashkeyid, byte[] signature, byte[] address);
        bool VerifySignatureMessage(byte[] data, byte[] signature, byte[] address);
    }
}
