using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Strategy;
using NBitcoin;
using System;
using System.Text.RegularExpressions;

namespace DtpCore.Model.Schema
{
    public class Secp256k1PKHIdentityValidator : IIdentityValidator
    {

        public const string NAME = DerivationSecp256k1PKH.NAME;
        public const string InvalidProofErrorTemplate = "{0}{1} is invalid, do not match claim binary.";
        public const string InvalidAddressErrorTemplate = "{0}{1} has invalid format.";


        private DerivationSecp256k1PKH _derivationSecp256K1PKH;
        private IClaimBinary _claimBinary;

        public Secp256k1PKHIdentityValidator(DerivationSecp256k1PKH derivationSecp256K1PKH, IClaimBinary claimBinary)
        {
            _derivationSecp256K1PKH = derivationSecp256K1PKH;
            _claimBinary = claimBinary;
        }

        public void Validate(string name, Identity identity, object source, string location, SchemaValidationResult result)
        {
            var claim = source as Claim;
            result.MaxRangeCheck($"{name} Id", identity.Id, location, 40);
            if (claim != null)
            {
                result.MissingCheck($"{name} Proof", identity.Id, location);
                result.MaxRangeCheck($"{name} Proof", identity.Proof, location, SchemaValidationResult.DEFAULT_MAX_LENGTH);
            }

            // TODO: Properly validate Id
            var regex = new Regex(@"^[13nmD][a-km-zA-HJ-NP-Z0-9]{26,33}$"); 
            if (!regex.IsMatch(identity.Id))
            {
                result.Errors.Add(string.Format(InvalidAddressErrorTemplate, location, $"{name} Id"));
                return;
            }
            if (claim != null) 
            {
                var message = _claimBinary.GetIdSource(claim).ConvertToBase64();
                if (!_derivationSecp256K1PKH.VerifySignatureMessage(message, identity.Proof, identity.Id))
                {
                    result.Errors.Add(string.Format(InvalidProofErrorTemplate, location, $"{name} Proof"));
                }
            }
        }
    }
}
