using DtpCore.Builders;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Extensions;
using DtpCore.Strategy;
using System;
using DtpCore.Factories;
using DtpCore.Enumerations;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DtpCore.Services
{


    public class TrustSchemaService : ITrustSchemaService
    {
        private IDerivationStrategyFactory _derivationServiceFactory;
        private IMerkleStrategyFactory _merkleStrategyFactory;
        private IHashAlgorithmFactory _hashAlgorithmFactory;
        private ITrustBinary _trustBinary;


        public TrustSchemaService(IDerivationStrategyFactory derivationServiceFactory, IMerkleStrategyFactory merkleStrategyFactory, IHashAlgorithmFactory hashAlgorithmFactory, ITrustBinary trustBinary)
        {
            _derivationServiceFactory = derivationServiceFactory;
            _merkleStrategyFactory = merkleStrategyFactory;
            _hashAlgorithmFactory = hashAlgorithmFactory;
            _trustBinary = trustBinary;
        }

        /// <summary>
        /// Gets the trust type string in a sanitized form, always in lowercase.
        /// </summary>
        /// <param name="trust"></param>
        /// <returns></returns>
        public string GetTrustTypeString(Trust trust)
        {
            if (IsTrustTypeAnObject(trust))
            {
                var trustType = GetTrustTypeObject(trust);
                return trustType.ToString().ToLower();
            }

            switch (trust.Type.ToLower())
            {
                case TrustBuilder.BINARY_TRUST_DTP1_SHORTFORM : return TrustBuilder.BINARY_TRUST_DTP1;
                case TrustBuilder.CONFIRM_TRUST_DTP1_SHORTFORM: return TrustBuilder.CONFIRM_TRUST_DTP1;
                case TrustBuilder.RATING_TRUST_DTP1_SHORTFORM : return TrustBuilder.RATING_TRUST_DTP1;
            }
            
            return trust.Type.ToLower();
        }

        public TrustType GetTrustTypeObject(Trust trust)
        {
            TrustType result;
            if (!IsTrustTypeAnObject(trust))
            {
                result = new TrustType();
                var parts = trust.Type.Split(".");
                if (parts.Length > 0) result.Attribute = parts[0];
                if (parts.Length > 1) result.Group = parts[1];
                if (parts.Length > 2) result.Protocol = parts[2];
            }
            else
            {
                result = JsonConvert.DeserializeObject<TrustType>(trust.Type);
            }

            return result;
        }

        public bool IsTrustTypeAnObject(Trust trust)
        {
            if (trust.Type == null)
                return false;

            var data = trust.Type.Trim();
            if (data.StartsWith("{") && data.EndsWith("}"))
                return true;

            return false;
        }


        public SchemaValidationResult Validate(Trust trust, TrustSchemaValidationOptions options = TrustSchemaValidationOptions.Full)
        {
            var engine = new ValidationEngine(_derivationServiceFactory, _merkleStrategyFactory, _hashAlgorithmFactory, _trustBinary, options);
            return engine.Validate(trust);
        }

        public SchemaValidationResult Validate(Package package, TrustSchemaValidationOptions options = TrustSchemaValidationOptions.Full)
        {
            var engine = new ValidationEngine(_derivationServiceFactory, _merkleStrategyFactory, _hashAlgorithmFactory, _trustBinary, options);
            return engine.Validate(package);
        }


        private class ValidationEngine
        {
            public const int ALGORITHM_MAX_LENGTH = 50;
            public const int ID_MAX_LENGTH = 100;
            public const int ADDRESS_MAX_LENGTH = 100;
            public const int SIGNATURE_MAX_LENGTH = 100;
            public const int NOTE_MAX_LENGTH = 100;
            public const int CLAIM_MAX_LENGTH = 1024;
            public const int TYPE_MAX_LENGTH = 50;
            public const int SCOPE_MAX_LENGTH = 100;
            public const int TIMESTAMP_MAX_COUNT = 10;
            public const int COST_LIMIT = 100;
            public const int TEXT50_MAX_LENGTH = 50;
            public const int TEXT200_MAX_LENGTH = 200;
            public const int TIMESTAMP_RECEIPT_MAX_LENGTH = 1024; // Allows for 4 billion timestamps with 32 byte hash
            



            private SchemaValidationResult result = new SchemaValidationResult();
            private ITrustBinary _trustBinary;

            private IDerivationStrategyFactory _derivationStrategyFactory;
            private IMerkleStrategyFactory _merkleStrategyFactory;
            private IHashAlgorithmFactory _hashAlgorithmFactory;

            private TrustSchemaValidationOptions _options; 


            public ValidationEngine(IDerivationStrategyFactory derivationStrategyFactory, IMerkleStrategyFactory merkleStrategyFactory, IHashAlgorithmFactory hashAlgorithmFactory, ITrustBinary trustBinary, TrustSchemaValidationOptions options)
            {
                _derivationStrategyFactory = derivationStrategyFactory;
                _merkleStrategyFactory = merkleStrategyFactory;
                _hashAlgorithmFactory = hashAlgorithmFactory;
                _trustBinary = trustBinary;
                _options = options;
            }

            public SchemaValidationResult Validate(Trust trust)
            {
                try
                {
                    var testBuilder = new TrustBuilder(_derivationStrategyFactory, _merkleStrategyFactory, _hashAlgorithmFactory, _trustBinary);
                    var trustIndex = 0;
                    testBuilder.AddTrust(trust);
                    ValidateTrust(trustIndex++, trust, result);
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
                
                    var testBuilder = new TrustBuilder(_derivationStrategyFactory, _merkleStrategyFactory, _hashAlgorithmFactory, _trustBinary);
                    var trustIndex = 0;
                    foreach (var trust in package.Trusts)
                    {
                        testBuilder.AddTrust(trust);
                        ValidateTrust(trustIndex++, trust, result);
                    }


                    if (package.Id != null && package.Id.Length > 0)
                    {
                        var testPackageID = testBuilder.Build().Package.Id;
                        if (testPackageID.Compare(package.Id) != 0)
                            result.Errors.Add("Package Id is not same as merkle tree root of all trust ID");
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
                MaxRangeCheck("Package Server Address", package.Server.Address, "", ADDRESS_MAX_LENGTH);
                MaxRangeCheck("Package Server Signature", package.Server.Signature, "", SIGNATURE_MAX_LENGTH);
            }

            private void ValidateTrust(int trustIndex, Trust trust, SchemaValidationResult result)
            {
                var location = $"Trust Index: {trustIndex} - ";

                MaxRangeCheck("Algorithm", trust.Algorithm, location, ALGORITHM_MAX_LENGTH);
                MaxRangeCheck("Id", trust.Id, location, ID_MAX_LENGTH);
                MaxRangeCheck("Type", trust.Type, location, TYPE_MAX_LENGTH);
                MaxRangeCheck("Claim", trust.Claim, location, CLAIM_MAX_LENGTH);
                MaxRangeCheck("Note", trust.Note, location, NOTE_MAX_LENGTH);

                ValidateIssuer(trust, location, result);
                ValidateSubject(trust, location, result);
                ValidateScope(trust, location, result);
                ValidateTimestamps(trust.Timestamps, location, result);

                if (_options == TrustSchemaValidationOptions.Full)
                {
                    MissingCheck("Trust Id", trust.Id, location);

                    // Avoid an attack vector, calculating hash on very large invalid trust
                    if (result.ErrorsFound == 0) 
                    {
                        var hashService = _hashAlgorithmFactory.GetAlgorithm(trust.Algorithm);
                        var trustID = hashService.HashOf(_trustBinary.GetIssuerBinary(trust));
                        if (trustID.Compare(trust.Id) != 0)
                            result.Errors.Add(location + "Invalid trust id");

                        // Make sure that subject has been validated before checking for Cost.

                        if (trust.Cost < COST_LIMIT)
                        {
                            // When cost is 0, then its default 100
                            if (trust.Cost > 0)
                            {
                                // Cost is less than 100, check that the subject signature exist, and has previously been checked.
                                if(trust.Subject.Signature == null || trust.Subject.Signature.Length == 0)
                                    result.Errors.Add(string.Format("{0}Missing Subject Signature for Cost to be lower than {1}", location, COST_LIMIT));
                            }
                        }
                    }
                }
            }

            private void ValidateTimestamps(IList<Timestamp> timestamps, string location, SchemaValidationResult result)
            {
                if (timestamps == null || timestamps.Count == 0)
                    return; // Zero timestamps are allowed.

                if(timestamps.Count > TIMESTAMP_MAX_COUNT)
                {
                    result.Errors.Add(string.Format("{0}To many timestamps in trust, there may not be more than {1}", location, timestamps.Count));
                    return; // Return before checking timestamps. Avoid attack vector.
                }

                foreach (var timestamp in timestamps)
                {
                    MaxRangeCheck("Timestamp Algorithm", timestamp.Algorithm, location, ALGORITHM_MAX_LENGTH);
                    MaxRangeCheck("Timestamp Blockchain", timestamp.Blockchain, location, TEXT50_MAX_LENGTH);
                    MaxRangeCheck("Timestamp Receipt", timestamp.Receipt, location, TIMESTAMP_RECEIPT_MAX_LENGTH);
                    MaxRangeCheck("Timestamp Service", timestamp.Service, location, TEXT200_MAX_LENGTH);
                    MaxRangeCheck("Timestamp Source", timestamp.Source, location, ID_MAX_LENGTH);
                }
            }


            private void ValidateIssuer(Trust trust, string location, SchemaValidationResult result)
            {
                if (!MissingCheck("Trust Issuer", trust.Issuer, location))
                    return; 

                MissingCheck("Issuer Address", trust.Issuer.Address, location);

                MaxRangeCheck("trust.Issuer.Type", trust.Issuer.Type, location, TYPE_MAX_LENGTH);
                MaxRangeCheck("trust.Issuer.Address", trust.Issuer.Address, location, ADDRESS_MAX_LENGTH);
                MaxRangeCheck("trust.Issuer.Signature", trust.Issuer.Signature, location, SIGNATURE_MAX_LENGTH);

                if (_options == TrustSchemaValidationOptions.Full)
                {
                    var scriptService = _derivationStrategyFactory.GetService(trust.Issuer.Type);

                    if (!scriptService.VerifySignatureMessage(trust.Id, trust.Issuer.Signature, trust.Issuer.Address))
                    {
                        result.Errors.Add(location + "Invalid issuer signature");
                    }
                }
            }



            private void ValidateSubject(Trust trust, string location, SchemaValidationResult result)
            {
                if (!MissingCheck("Trust Subject", trust.Subject, location))
                    return;

                MissingCheck("trust.Subject.Address", trust.Subject.Address, location);

                MaxRangeCheck("trust.Subject.Type", trust.Subject.Type, location, TYPE_MAX_LENGTH);
                MaxRangeCheck("trust.Subject.Address", trust.Subject.Address, location, ADDRESS_MAX_LENGTH);
                MaxRangeCheck("trust.Subject.Signature", trust.Subject.Signature, location, SIGNATURE_MAX_LENGTH);


                if (_options == TrustSchemaValidationOptions.Full)
                {
                    if (trust.Subject.Signature != null)
                    {
                        var scriptService = _derivationStrategyFactory.GetService(trust.Subject.Type);

                        if (!scriptService.VerifySignatureMessage(trust.Id, trust.Subject.Signature, trust.Subject.Address))
                        {
                            result.Errors.Add(location + "Invalid subject signature");
                        }
                    }
                }
            }

            private void ValidateScope(Trust trust, string location, SchemaValidationResult result)
            {
                if (trust.Scope == null)
                    return;

                MaxRangeCheck("trust.Scope.Type", trust.Scope.Type, location, TYPE_MAX_LENGTH);
                MaxRangeCheck("trust.Scope.Value", trust.Scope.Value, location, SCOPE_MAX_LENGTH);
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
