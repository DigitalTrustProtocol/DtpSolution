using System;
using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.DataEncoders;
using DtpCore.Extensions;
using DtpCore.Interfaces;

namespace DtpCore.Strategy
{
    public class DerivationSecp256k1PKH : IDerivationStrategy
    {
        public int Length { get; }
        public int AddressLength { get; }
        public string ScriptName { get; }
        public const string NAME = "secp256k1-pkh";


        public string NetworkName
        {
            get {
                return network.Name;
            }
            set
            {
                var n = Enum.Parse<NetworkType>(value, true);
                network = Bitcoin.Instance.GetNetwork(n);
            }
        }

        
        private Network network;

        public DerivationSecp256k1PKH()
        {
            Length = 32; // SHA 256 = 32 bytes
            AddressLength = 40; // Temp setting, need reajustment

            // pkh = public key hash, alternative would be sh = script hash.
            ScriptName = NAME;

            network = Network.Main;
        }

        public byte[] HashOf(byte[] data)
        {
            return Hashes.SHA256(Hashes.SHA256(data));
        }

        public byte[] KeyFromString(string wif)
        {
            var key = Key.Parse(wif);
            return key.ToBytes();
        }

        public byte[] GetKey(byte[] seed)
        {
            return new Key(HashOf(seed)).ToBytes();
        }

        public string GetAddress(byte[] key)
        {
            return new Key(key).PubKey.GetAddress(network).ToString();
        }

        public byte[] Sign(byte[] key, byte[] data)
        {
            var ecdsaKey = new Key(key);
            return ecdsaKey.SignCompact(new uint256(data));
        }

        public byte[] SignMessage(byte[] key, byte[] data)
        {
            var ecdsaKey = new Key(key);
            var message = ecdsaKey.SignMessage(data);
            return Convert.FromBase64String(message);
        }

        public bool VerifySignature(byte[] data, byte[] signature, string address)
        {
            var hashkeyid = new uint256(data); 
            var recoverAddress = PubKey.RecoverCompact(hashkeyid, signature);

            
            return recoverAddress.GetAddress(network).ToString() == address;

        }

        public bool VerifySignatureMessage(byte[] data, byte[] signature, string address)
        {
            var sig = Encoders.Base64.EncodeData(signature);
            var recoverAddress = PubKey.RecoverFromMessage(data, sig);
            return recoverAddress.GetAddress(network).ToString() == address;
        }
    }
}
