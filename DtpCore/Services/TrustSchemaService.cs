using DtpCore.Builders;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Extensions;
using DtpCore.Strategy;
using System;
using DtpCore.Factories;
using DtpCore.Enumerations;

namespace DtpCore.Services
{


    public class TrustSchemaService : ITrustSchemaService
    {
        private IDerivationStrategyFactory _derivationServiceFactory;
        private IMerkleStrategyFactory _merkleStrategyFactory;
        private IHashAlgorithmFactory _hashAlgorithmFactory;

        private ITrustBinary _trustBinary;

        //public TrustSchemaService(IServiceProvider serviceProvider) : this(new DerivationStrategyFactory(serviceProvider), new MerkleStrategyFactory(new HashAlgorithmFactory()), new HashAlgorithmFactory(), new TrustBinary())
        //{

        //}

        public TrustSchemaService(IDerivationStrategyFactory derivationServiceFactory, IMerkleStrategyFactory merkleStrategyFactory, IHashAlgorithmFactory hashAlgorithmFactory, ITrustBinary trustBinary)
        {
            _derivationServiceFactory = derivationServiceFactory;
            _merkleStrategyFactory = merkleStrategyFactory;
            _hashAlgorithmFactory = hashAlgorithmFactory;
            _trustBinary = trustBinary;
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
                //if (_options == TrustSchemaValidationOptions.Full)
                //    if (package.Id == null)
                //        result.Errors.Add("Package.PackageID is missing");

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

                    //var testPackageID = testBuilder.BuildPackageID().Package.PackageId;
                    //if (testPackageID.Compare(package.PackageId) != 0)
                       // result.Errors.Add("Package.PackageID is not same as merkle tree root of all trust ID");

                }
                catch (Exception ex)
                {
                    result.Errors.Add(ex.Message);
                }

                return result;
            }

            //private void ValidateServer(Head head, SchemaValidationResult result)
            //{

            //    if (head == null)
            //        return;

            //    if (string.IsNullOrEmpty(head.Script))
            //        result.Errors.Add("Missing Head Script");

            //    if (string.IsNullOrEmpty(head.Version))
            //        result.Errors.Add("Missing Head Version");

            //}

            private void ValidateTrust(int trustIndex, Trust trust, SchemaValidationResult result)
            {
                var location = $"Trust Index: {trustIndex} - ";

                if (_options == TrustSchemaValidationOptions.Full)
                    if (trust.Id == null)
                        result.Errors.Add(location+"Missing trust id");

                ValidateIssuer(trust, location, result);
                ValidateSubject(trust, location, result);


                if (_options == TrustSchemaValidationOptions.Full)
                {
                    ValidateClaim(trust, location, result);

                    var hashService = _hashAlgorithmFactory.GetAlgorithm(trust.Algorithm);
                    var trustID = hashService.HashOf(_trustBinary.GetIssuerBinary(trust));
                    if (trustID.Compare(trust.Id) != 0)
                        result.Errors.Add(location + "Invalid trust id");

                    
                }
            }

            private void ValidateClaim(Trust trust, string location, SchemaValidationResult result)
            {
                if (trust.Claim == null) return;

                var maxClaimLength = 100;
                if(trust.Claim.Length > maxClaimLength)
                {
                    result.Errors.Add(location + $"Claim is {trust.Claim.Length} in serialized length, may not be more than {maxClaimLength}");
                }
            }

            private void ValidateIssuer(Trust trust, string location, SchemaValidationResult result)
            {
                if (trust.Issuer == null)
                    result.Errors.Add(location + "Missing issuer");

                if (trust.Issuer.Address == null || trust.Issuer.Address.Length == 0)
                    result.Errors.Add(location + "Missing issuer address");

                if (_options == TrustSchemaValidationOptions.Full)
                {
                    if (trust.Issuer.Signature == null || trust.Issuer.Signature.Length == 0)
                        result.Errors.Add(location + "Missing issuer signature");
                    else
                    {
                        var scriptService = _derivationStrategyFactory.GetService(trust.Issuer.Type);

                        if (!scriptService.VerifySignatureMessage(trust.Id, trust.Issuer.Signature, trust.Issuer.Address))
                        {
                            result.Errors.Add(location + "Invalid issuer signature");
                        }
                    }
                }
            }

            private void ValidateSubject(Trust trust, string location, SchemaValidationResult result)
            {
                //if (trust.SubjectAddress == null || trust.SubjectAddress.Length == 0)
                //    result.Errors.Add(location + "Missing subject address");

                if (_options == TrustSchemaValidationOptions.Full)
                {
                    if (trust.Subject != null && trust.Subject.Signature != null)
                    {
                        var scriptService = _derivationStrategyFactory.GetService(trust.Subject.Type);

                        if (!scriptService.VerifySignatureMessage(trust.Id, trust.Subject.Signature, trust.Subject.Address))
                        {
                            result.Errors.Add(location + "Invalid subject signature");
                        }
                    }
                }
            }
        }
    }
}
