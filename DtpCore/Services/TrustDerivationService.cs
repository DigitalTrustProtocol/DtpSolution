using System.Text;
using DtpCore.Interfaces;
using DtpCore.Strategy;

namespace DtpCore.Services
{
    public class TrustDerivationService
    {
        public static IDerivationStrategy DerivationStrategy = new DerivationSecp256k1PKH();

        public IDerivationStrategy Derivation { get; }

        public TrustDerivationService()
        {
            Derivation = DerivationStrategy;
        }

        public TrustDerivationService(IDerivationStrategy derivationService)
        {
            Derivation = derivationService;
        }

        public byte[] GetKeyFromPassword(string password)
        {
            var data = Encoding.UTF8.GetBytes(password);
            var key = Derivation.GetKey(data);
            return key;
        }

        public string GetAddressFromPassword(string password)
        {
            return Derivation.GetAddress(GetKeyFromPassword(password));
        }

        

    }
}
