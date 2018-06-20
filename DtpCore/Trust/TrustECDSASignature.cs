using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.DataEncoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustchainCore.Extensions;
using TrustchainCore.Model;

namespace TrustchainCore.Trust
{
    public class TrustECDSASignature
    {
        protected TrustModel Trust { get; set; }

        public TrustECDSASignature(TrustModel trust)
        {
            this.Trust = trust;
        }

        public List<string> VerifyTrustSignature()
        {
            var Errors = new List<string>();

            if (Trust.Signature == null || Trust.Signature.Length == 0)
            {
                Errors.Add("Missing issuer signature");
                return Errors;
            }

            var trustHash = new uint256(Trust.TrustId);

            if (!VerifySignature(trustHash, Trust.Signature, Trust.IssuerId))
            {
                Errors.Add("Invalid issuer signature");
                return Errors;
            }
            

            foreach (var subject in Trust.Issuer.Subjects)
            {
                if (subject.Signature == null || subject.Signature.Length == 0)
                    continue;

                if (!VerifySignature(trustHash, subject.Signature, subject.SubjectId))
                {
                    Errors.Add("Invalid issuer signature");
                    return Errors;
                }
                    
            }
            return Errors;
        }

        public List<string> VerifyTrustSignatureMessage()
        {
            var Errors = new List<string>();

            if (Trust.Issuer.Signature == null || Trust.Issuer.Signature.Length == 0)
            {
                Errors.Add("Missing issuer signature");
                return Errors;
            }

            if (!VerifySignatureMessage(Trust.TrustId, Trust.Issuer.Signature, Trust.Issuer.IssuerId))
            {
                Errors.Add("Invalid issuer signature");
                return Errors;
            }


            foreach (var subject in Trust.Issuer.Subjects)
            {
                if (subject.Signature == null || subject.Signature.Length == 0)
                    continue;

                if (!VerifySignatureMessage(Trust.TrustId, subject.Signature, subject.SubjectId))
                {
                    Errors.Add("Invalid issuer signature");
                    return Errors;
                }

            }
            return Errors;
        }


        public static byte[] GetHashOfBinary(byte[] data)
        {
            return Hashes.SHA256(Hashes.SHA256(data));
        }

        public static byte[] SignMessage(Key key, byte[] data)
        {
            var message = key.SignMessage(data);
            return Convert.FromBase64String(message);
        }

        public static byte[] Sign(Key key, byte[] data)
        {
            return key.SignCompact(new uint256(GetHashOfBinary(data)));
        }

        public bool VerifySignatureMessage(byte[] data, byte[] signature, byte[] address)
        {
            var sig = Encoders.Base64.EncodeData(signature);
            var recoverAddress = PubKey.RecoverFromMessage(data, sig);

            return recoverAddress.Hash.ToBytes().Compare(address) == 0;
        }

        public bool VerifySignature(uint256 hashkeyid, byte[] signature, byte[] address)
        {
            var recoverAddress = PubKey.RecoverCompact(hashkeyid, signature);

            return recoverAddress.Hash.ToBytes().Compare(address) == 0;

        }

    }
}

