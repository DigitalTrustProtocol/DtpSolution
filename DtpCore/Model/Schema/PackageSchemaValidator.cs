using DtpCore.Builders;
using DtpCore.Interfaces;
using DtpCore.Extensions;
using System;
using DtpCore.Enumerations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DtpCore.Model.Schema
{


    public class PackageSchemaValidator : IPackageSchemaValidator
    {
        private IDerivationStrategyFactory _derivationServiceFactory;
        private IMerkleStrategyFactory _merkleStrategyFactory;
        private IHashAlgorithmFactory _hashAlgorithmFactory;
        private IPackageBinary _packageBinary;
        private IValidatorFactory _validatorFactory;

        public PackageSchemaValidator(IDerivationStrategyFactory derivationServiceFactory, IMerkleStrategyFactory merkleStrategyFactory, IHashAlgorithmFactory hashAlgorithmFactory, IPackageBinary packageBinary, IValidatorFactory validatorFactory)
        {
            _derivationServiceFactory = derivationServiceFactory;
            _merkleStrategyFactory = merkleStrategyFactory;
            _hashAlgorithmFactory = hashAlgorithmFactory;
            _packageBinary = packageBinary;
            _validatorFactory = validatorFactory;
        }




        /// <summary>
        /// Gets the trust type string in a sanitized form, always in lowercase.
        /// </summary>
        /// <param name="claim"></param>
        /// <returns></returns>
        public string GetTrustTypeString(Claim claim)
        {
            if (IsTrustTypeAnObject(claim))
            {
                var trustType = GetTrustTypeObject(claim);
                return trustType.ToString().ToLower();
            }

            switch (claim.Type.ToLower())
            {
                case PackageBuilder.BINARY_TRUST_DTP1_SHORTFORM : return PackageBuilder.BINARY_TRUST_DTP1;
                case PackageBuilder.CONFIRM_TRUST_DTP1_SHORTFORM: return PackageBuilder.CONFIRM_TRUST_DTP1;
                case PackageBuilder.RATING_TRUST_DTP1_SHORTFORM : return PackageBuilder.RATING_TRUST_DTP1;
            }
            
            return claim.Type.ToLower();
        }

        public TrustType GetTrustTypeObject(Claim claim)
        {
            TrustType result;
            if (!IsTrustTypeAnObject(claim))
            {
                result = new TrustType();
                var parts = claim.Type.Split(".");
                if (parts.Length > 0) result.Attribute = parts[0];
                if (parts.Length > 1) result.Group = parts[1];
                if (parts.Length > 2) result.Protocol = parts[2];
            }
            else
            {
                result = JsonConvert.DeserializeObject<TrustType>(claim.Type);
            }

            return result;
        }

        public bool IsTrustTypeAnObject(Claim claim)
        {
            if (claim.Type == null)
                return false;

            var data = claim.Type.Trim();
            if (data.StartsWith("{") && data.EndsWith("}"))
                return true;

            return false;
        }

        public SchemaValidationResult Validate(object model)
        {
            return null;
        }

        public SchemaValidationResult Validate(Claim claim, TrustSchemaValidationOptions options = TrustSchemaValidationOptions.Full)
        {
            var engine = new ValidationEngine(_packageBinary, _derivationServiceFactory, _merkleStrategyFactory, _hashAlgorithmFactory,  options, _validatorFactory);
            return engine.Validate(claim);
        }

        public SchemaValidationResult Validate(Package package, TrustSchemaValidationOptions options = TrustSchemaValidationOptions.Full)
        {
            var engine = new ValidationEngine(_packageBinary, _derivationServiceFactory, _merkleStrategyFactory, _hashAlgorithmFactory, options, _validatorFactory);
            return engine.Validate(package);
        }

        private class ValidationEngine
        {
            public const int DEFAULT_MAX_LENGTH = 127;

            public const int ALGORITHM_MAX_LENGTH = DEFAULT_MAX_LENGTH;
            public const int ID_MAX_LENGTH = DEFAULT_MAX_LENGTH;
            public const int IDENTITY_ID_MAX_LENGTH = DEFAULT_MAX_LENGTH;
            public const int PROOF_MAX_LENGTH = DEFAULT_MAX_LENGTH;
            public const int METADATA_MAX_LENGTH = DEFAULT_MAX_LENGTH;
            public const int CLAIM_MAX_LENGTH = 1024;
            public const int TYPE_MAX_LENGTH = DEFAULT_MAX_LENGTH;
            public const int SCOPE_MAX_LENGTH = DEFAULT_MAX_LENGTH;
            public const int TIMESTAMP_MAX_COUNT = 10;
            public const int COST_LIMIT = 100;
            public const int TEXT50_MAX_LENGTH = 50;
            public const int TEXT200_MAX_LENGTH = 200;
            public const int TIMESTAMP_RECEIPT_MAX_LENGTH = 1024; // Allows for 4 billion timestamps with 32 byte hash
            



            private SchemaValidationResult result = new SchemaValidationResult();
            private IPackageBinary _packageBinary;

            private IDerivationStrategyFactory _derivationStrategyFactory;
            private IMerkleStrategyFactory _merkleStrategyFactory;
            private IHashAlgorithmFactory _hashAlgorithmFactory;

            private TrustSchemaValidationOptions _options;
            private IValidatorFactory _validatorFactory;

            public ValidationEngine(IPackageBinary packageBinary, IDerivationStrategyFactory derivationStrategyFactory, IMerkleStrategyFactory merkleStrategyFactory, IHashAlgorithmFactory hashAlgorithmFactory, TrustSchemaValidationOptions options, IValidatorFactory validatorFactory)
            {
                _packageBinary = packageBinary;
                _derivationStrategyFactory = derivationStrategyFactory;
                _merkleStrategyFactory = merkleStrategyFactory;
                _hashAlgorithmFactory = hashAlgorithmFactory;
                _options = options;
                _validatorFactory = validatorFactory;
            }

            public SchemaValidationResult Validate(Claim claim)
            {
                try
                {
                    result = new SchemaValidationResult();
                    var testBuilder = new PackageBuilder(_merkleStrategyFactory, _hashAlgorithmFactory, _packageBinary);
                    var claimIndex = 0;
                    testBuilder.AddClaim(claim);
                    ValidateClaim(claimIndex++, claim, result);
                }
                catch (Exception ex)
                {
                    result.Errors.Add(ex.Message);
                }
                return result;
            }

            public SchemaValidationResult Validate(Package package)
            {
                result.MaxRangeCheck("Package Type", package.Type, "", TYPE_MAX_LENGTH);
                result.MaxRangeCheck("Package Id", package.Id, "", ID_MAX_LENGTH);
                
                ValidateServer(package);

                try
                {
                    var script = _merkleStrategyFactory.GetStrategy(package.Type);
                
                    var testBuilder = new PackageBuilder(_merkleStrategyFactory, _hashAlgorithmFactory, _packageBinary);
                    var claimIndex = 0;
                    if(package.Claims.Count == 0)
                    {
                        result.MissingCheck("Package claims", "", "");
                    }

                    foreach (var claim in package.Claims)
                    {
                        testBuilder.AddClaim(claim);
                        ValidateClaim(claimIndex++, claim, result);
                    }

                    if (package.Id != null && package.Id.Length > 0)
                    {
                        testBuilder.Package.Type = package.Type;
                        testBuilder.Package.Created = package.Created;
                        testBuilder.Package.Server = package.Server;

                        var testPackageID = testBuilder.Build().Package.Id;

                        if (testPackageID.Compare(package.Id) != 0)
                            result.Errors.Add("Package Id is not same as merkle tree root of all claim ID");
                    }

                    ValidateTimestamps(package.Timestamps, "Package ");
                }
                catch (Exception ex)
                {
                    result.Errors.Add(ex.Message);
                }

                return result;
            }

            private void ValidateServer(Package package)
            {
                if (package.Server == null)
                    return;

                result.MaxRangeCheck("Package Server Type", package.Server.Type, "", TYPE_MAX_LENGTH);
                result.MaxRangeCheck("Package Server Id", package.Server.Id, "", IDENTITY_ID_MAX_LENGTH);
                result.MaxRangeCheck("Package Server Signature", package.Server.Proof, "", PROOF_MAX_LENGTH);
            }

            private void ValidateClaim(int claimIndex, Claim claim, SchemaValidationResult result)
            {
                var location = $"Claim Index: {claimIndex} - ";

                result.MaxRangeCheck("Id", claim.Id, location, ID_MAX_LENGTH);
                result.MaxRangeCheck("Type", claim.Type, location, TYPE_MAX_LENGTH);
                result.MaxRangeCheck("Value", claim.Value, location, CLAIM_MAX_LENGTH);
                result.MaxRangeCheck("Metadata", claim.Metadata, location, METADATA_MAX_LENGTH);

                ValidateIssuer(claim, location);
                ValidateSubject(claim, location);
                ValidateScope(claim, location);
                ValidateTimestamps(claim.Timestamps, location);
            }

            private void ValidateTimestamps(IList<Timestamp> timestamps, string location)
            {
                if (timestamps == null || timestamps.Count == 0)
                    return; // Zero timestamps are allowed.

                if(timestamps.Count > TIMESTAMP_MAX_COUNT)
                {
                    result.Errors.Add(string.Format("{0}To many timestamps in claim, there may not be more than {1}", location, timestamps.Count));
                    return; // Return before checking timestamps. Avoid attack vector.
                }

                foreach (var timestamp in timestamps)
                {
                    result.MaxRangeCheck("Timestamp Type", timestamp.Type, location, TYPE_MAX_LENGTH);
                    result.MaxRangeCheck("Timestamp Blockchain", timestamp.Blockchain, location, TEXT50_MAX_LENGTH);
                    result.MaxRangeCheck("Timestamp Receipt", timestamp.Path, location, TIMESTAMP_RECEIPT_MAX_LENGTH);
                    result.MaxRangeCheck("Timestamp Service", timestamp.Service, location, TEXT200_MAX_LENGTH);
                    result.MaxRangeCheck("Timestamp Source", timestamp.Source, location, ID_MAX_LENGTH);
                }
            }


            private void ValidateIssuer(Claim claim, string location)
            {
                if (result.MissingCheck("Claim Issuer", claim.Issuer, location))
                    return;

                ValidateIdentity("Issuer", claim.Issuer, claim, location);
            }



            private void ValidateSubject(Claim claim, string location)
            {
                if (result.MissingCheck("Claim Subject", claim.Subject, location))
                    return;

                ValidateIdentity("Subject", claim.Subject, claim, location);
            }

            private void ValidateIdentity(string name, Identity identity, Claim claim, string location)
            {
                var missing = result.MissingCheck($"{name} Type", identity.Type, location);
                result.MaxRangeCheck($"{name} Type", identity.Type, location, SchemaValidationResult.DEFAULT_MAX_LENGTH);
                missing |= result.MissingCheck(name + " Id", identity.Id, location);

                if (missing)
                    return;
                
                var validator = _validatorFactory.GetIdentityValidator(identity.Type);
                if(validator == null)
                {
                    result.Errors.Add(string.Format(SchemaValidationResult.NotSupportedErrorTemplate, location, $"{name} Type"));
                    return;
                }

                validator.Validate(name, identity, claim, location, result);
            }

            private void ValidateScope(Claim claim, string location)
            {
                if (claim.Scope == null)
                    return;

                result.MaxRangeCheck("claim.Scope", claim.Scope, location, SCOPE_MAX_LENGTH);
            }
        }
    }
}
