namespace DtpCore.Interfaces
{
    public interface IServerIdentityService
    {
        string Id { get; set; }
        byte[] Key { get; set; }

        byte[] Sign(byte[] data);
        byte[] Sign(string text);
        bool Verify(byte[] data, byte[] signature);
        IDerivationStrategy Derivation { get; set; }
    }
}