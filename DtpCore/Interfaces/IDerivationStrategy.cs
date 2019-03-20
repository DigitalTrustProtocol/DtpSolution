namespace DtpCore.Interfaces
{
    public interface IDerivationStrategy
    {
        int Length { get; }
        int AddressLength { get; }
        string ScriptName { get; }
        string NetworkName { get; set; }

        byte[] HashOf(byte[] data);
        byte[] KeyFromString(string wif);
        byte[] GetKey(byte[] seed);
        string GetAddress(byte[] key);
        byte[] SignMessage(byte[] key, byte[] data);
        byte[] Sign(byte[] key, byte[] data);
        bool VerifySignature(byte[] hashkeyid, byte[] signature, string address);
        bool VerifySignatureMessage(byte[] data, byte[] signature, string address);
        bool VerifySignatureMessage(string message, byte[] signature, string address);
    }
}
