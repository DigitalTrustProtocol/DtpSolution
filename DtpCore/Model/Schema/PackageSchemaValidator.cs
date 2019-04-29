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

        public PackageSchemaValidator(IDerivationStrategyFactory derivationServiceFactory, IMerkleStrategyFactory merkleStrategyFactory, IHashAlgorithmFactory hashAlgorithmFactory, IPackageBinary packageBinary)
        {
            _derivationServiceFactory = derivationServiceFactory;
            _merkleStrategyFactory = merkleStrategyFactory;
            _hashAlgorithmFactory = hashAlgorithmFactory;
            _packageBinary = packageBinary;
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


        public SchemaValidationResult Validate(Claim claim, TrustSchemaValidationOptions options = TrustSchemaValidationOptions.Full)
        {
            var engine = new ValidationEngine(_derivationServiceFactory, _merkleStrategyFactory, _hashAlgorithmFactory,  _packageBinary, options);
            return engine.Validate(claim);
        }

        public SchemaValidationResult Validate(Package package, TrustSchemaValidationOptions options = TrustSchemaValidationOptions.Full)
        {
            var engine = new ValidationEngine(_derivationServiceFactory, _merkleStrategyFactory, _hashAlgorithmFactory, _packageBinary, options);
            return engine.Validate(package);
        }


        private class ValidationEngine
        {
            public const int DEFAULT_MAX_LENGTH = 127;

            public const int ALGORITHM_MAX_LENGTH = DEFAULT_MAX_LENGTH;
            public const int ID_MAX_LENGTH = DEFAULT_MAX_LENGTH;
            public const int IDENTITY_ID_MAX_LENGTH = DEFAULT_MAX_LENGTH;
            public const int SIGNATURE_MAX_LENGTH = DEFAULT_MAX_LENGTH;
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


            public ValidationEngine(IDerivationStrategyFactory derivationStrategyFactory, IMerkleStrategyFactory merkleStrategyFactory, IHashAlgorithmFactory hashAlgorithmFactory, IPackageBinary packageBinary, TrustSchemaValidationOptions options)
            {
                _derivationStrategyFactory = derivationStrategyFactory;
                _merkleStrategyFactory = merkleStrategyFactory;
                _hashAlgorithmFactory = hashAlgorithmFactory;
                _packageBinary = packageBinary;
                _options = options;
            }

            public SchemaValidationResult Validate(Claim claim)
            {
                try
                {
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
                MaxRangeCheck("Package Algorithm", package.Algorithm, "", ALGORITHM_MAX_LENGTH);
                MaxRangeCheck("Package Id", package.Id, "", ID_MAX_LENGTH);
                
                ValidateServer(package, result);

                try
                {
                    var script = _merkleStrategyFactory.GetStrategy(package.Algorithm);
                
                    var testBuilder = new PackageBuilder(_merkleStrategyFactory, _hashAlgorithmFactory, _packageBinary);
                    var claimIndex = 0;
                    if(package.Claims.Count == 0)
                    {
                        MissingCheck("Package claims", "", "");
                    }

                    foreach (var claim in package.Claims)
                    {
                        testBuilder.AddClaim(claim);
                        ValidateClaim(claimIndex++, claim, result);
                    }

                    if (package.Id != null && package.Id.Length > 0)
                    {
                        testBuilder.Package.Algorithm = package.Algorithm;
                        testBuilder.Package.Created = package.Created;
                        testBuilder.Package.Server = package.Server;

                        var testPackageID = testBuilder.Build().Package.Id;

                        if (testPackageID.Compare(package.Id) != 0)
                            result.Errors.Add("Package Id is not same as merkle tree root of all claim ID");
                    }

                    ValidateTimestamps(package.Timestamps, "Package ", result);
                }
                catch (Exception ex)
                {
                    result.Errors.Add(ex.Message);
                }

                return result;
            }

            private void ValidateServer(Package package, SchemaValidationResult result)
            {
                if (package.Server == null)
                    return;

                MaxRangeCheck("Package Server Type", package.Server.Type, "", TYPE_MAX_LENGTH);
                MaxRangeCheck("Package Server Id", package.Server.Id, "", IDENTITY_ID_MAX_LENGTH);
                MaxRangeCheck("Package Server Signature", package.Server.Proof, "", SIGNATURE_MAX_LENGTH);
            }

            private void ValidateClaim(int claimIndex, Claim claim, SchemaValidationResult result)
            {
                var location = $"Claim Index: {claimIndex} - ";

                MaxRangeCheck("Id", claim.Id, location, ID_MAX_LENGTH);
                MaxRangeCheck("Type", claim.Type, location, TYPE_MAX_LENGTH);
                MaxRangeCheck("Value", claim.Value, location, CLAIM_MAX_LENGTH);
                MaxRangeCheck("Metadata", claim.Metadata, location, METADATA_MAX_LENGTH);

                ValidateIssuer(claim, location, result);
                ValidateSubject(claim, location, result);
                ValidateScope(claim, location, result);
                ValidateTimestamps(claim.Timestamps, location, result);
            }

            private void ValidateTimestamps(IList<Timestamp> timestamps, string location, SchemaValidationResult result)
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
                    MaxRangeCheck("Timestamp Algorithm", timestamp.Algorithm, location, ALGORITHM_MAX_LENGTH);
                    MaxRangeCheck("Timestamp Blockchain", timestamp.Blockchain, location, TEXT50_MAX_LENGTH);
                    MaxRangeCheck("Timestamp Receipt", timestamp.Path, location, TIMESTAMP_RECEIPT_MAX_LENGTH);
                    MaxRangeCheck("Timestamp Service", timestamp.Service, location, TEXT200_MAX_LENGTH);
                    MaxRangeCheck("Timestamp Source", timestamp.Source, location, ID_MAX_LENGTH);
                }
            }


            private void ValidateIssuer(Claim claim, string location, SchemaValidationResult result)
            {
                if (!MissingCheck("Claim Issuer", claim.Issuer, location))
                    return; 

                MissingCheck("Issuer Id", claim.Issuer.Id, location);

                MaxRangeCheck("claim.Issuer.Type", claim.Issuer.Type, location, TYPE_MAX_LENGTH);
                MaxRangeCheck("claim.Issuer.Id", claim.Issuer.Id, location, IDENTITY_ID_MAX_LENGTH);
                MaxRangeCheck("claim.Issuer.Signature", claim.Issuer.Proof, location, SIGNATURE_MAX_LENGTH);

                if (_options == TrustSchemaValidationOptions.Full)
                {
                    var scriptService = _derivationStrategyFactory.GetService(claim.Issuer.Type);

                    var message = _packageBinary.ClaimBinary.GetIdSource(claim).ConvertToBase64();
                    if (!scriptService.VerifySignatureMessage(message, claim.Issuer.Proof, claim.Issuer.Id))
                    {
                        result.Errors.Add(location + "Invalid issuer signature");
                    }
                }
            }



            private void ValidateSubject(Claim claim, string location, SchemaValidationResult result)
            {
                if (!MissingCheck("Claim Subject", claim.Subject, location))
                    return;

                MissingCheck("claim.Subject.Id", claim.Subject.Id, location);

                MaxRangeCheck("claim.Subject.Type", claim.Subject.Type, location, TYPE_MAX_LENGTH);
                MaxRangeCheck("claim.Subject.Id", claim.Subject.Id, location, IDENTITY_ID_MAX_LENGTH);
                MaxRangeCheck("claim.Subject.Signature", claim.Subject.Proof, location, SIGNATURE_MAX_LENGTH);


                if (_options == TrustSchemaValidationOptions.Full)
                {
                    if (claim.Subject.Proof != null)
                    {
                        var scriptService = _derivationStrategyFactory.GetService(claim.Subject.Type);
                        var message = _packageBinary.ClaimBinary.GetIdSource(claim).ConvertToBase64();
                        if (!scriptService.VerifySignatureMessage(message, claim.Subject.Proof, claim.Subject.Id))
                        {
                            result.Errors.Add(location + "Invalid subject signature");
                        }
                    }
                }
            }

            private void ValidateScope(Claim claim, string location, SchemaValidationResult result)
            {
                if (claim.Scope == null)
                    return;

                MaxRangeCheck("claim.Scope", claim.Scope, location, SCOPE_MAX_LENGTH);
            }


            public const string MssingErrorTemplate = "{0}{1} is missing.";
            public const string MaxRangeErrorTemplate = "{0}{1} may not be longer than {2} - is {3} bytes.";

            private bool MissingCheck(string name, Identity value, string location)
            {
                if (value == null)
                {
                    result.Errors.Add(string.Format(MssingErrorTemplate, location, name));
                    return false;
                }

                return true;
            }


            private bool MissingCheck(string name, string value, string location)
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    result.Errors.Add(string.Format(MssingErrorTemplate, location, name));
                    return false;
                }

                return true;
            }

            private bool MissingCheck(string name, byte[] value, string location)
            {
                if (value == null || value.Length == 0)
                {
                    result.Errors.Add(string.Format(MssingErrorTemplate, location, name));
                    return false;
                }

                return true;
            }

            private bool MaxRangeCheck(string name, string value, string location, int maxLength)
            {
                if (value == null)
                    return true;

                if (value.Length > maxLength)
                {
                    result.Errors.Add(string.Format(MaxRangeErrorTemplate, location, name, maxLength, value.Length));
                    return false;
                }
                return true;
            }

            private bool MaxRangeCheck(string name, byte[] value, string location, int maxLength)
            {
                if (value == null)
                    return true;

                if (value.Length > maxLength)
                {
                    result.Errors.Add(string.Format(MaxRangeErrorTemplate, location, name, maxLength, value.Length));
                    return false;
                }
                return true;
            }
        }
    }
}
