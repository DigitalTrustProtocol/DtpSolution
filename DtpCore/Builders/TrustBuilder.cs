using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Extensions;
using DtpCore.Factories;
using System;
using DtpCore.Strategy;

namespace DtpCore.Builders
{
    public class TrustBuilder
    {
        public const string BINARY_TRUST_DTP1 = "binary.trust.dtp1";
        public const string CONFIRM_TRUST_DTP1 = "confirm.trust.dtp1";
        public const string RATING_TRUST_DTP1 = "rating.trust.dtp1";

        public Package Package { get; set; }
        private ITrustBinary _trustBinary;

        private Trust _currentTrust;
        public Trust CurrentTrust
        {
            get
            {
                return _currentTrust;
            }
        }


        private IDerivationStrategyFactory _derivationServiceFactory;
        private IMerkleStrategyFactory _merkleStrategyFactory;
        private IHashAlgorithmFactory _hashAlgorithmFactory;


        public TrustBuilder(IServiceProvider serviceProvider) : this(new DerivationStrategyFactory(serviceProvider), new MerkleStrategyFactory(new HashAlgorithmFactory()), new HashAlgorithmFactory(), new TrustBinary())
        {
            
        }

        public TrustBuilder(IDerivationStrategyFactory derivationServiceFactory, IMerkleStrategyFactory merkleStrategyFactory, IHashAlgorithmFactory hashAlgorithmFactory, ITrustBinary trustBinary)
        {
            Package = new Package
            {
                Trusts = new List<Trust>()
            };
            _derivationServiceFactory = derivationServiceFactory;
            _merkleStrategyFactory = merkleStrategyFactory;
            _hashAlgorithmFactory = hashAlgorithmFactory;
            _trustBinary = trustBinary;
        }

        public TrustBuilder Load(string content)
        {
            Package = JsonConvert.DeserializeObject<Package>(content);
            return this;
        }

        //public static PackageBuilder Load(PackageModel package)
        //{
        //    var builder = new PackageBuilder();
        //    builder.Package = package;
        //    return builder;
        //}

        //public static PackageModel EnsureHead(PackageModel package, string version = "standard 0.1.0", string script = "btc-pkh")
        //{
        //    if(package.Head == null)
        //    {
        //        package.Head = new HeadModel
        //        {
        //            Version = version,
        //            Script = script
        //        };
        //    }
        //    return package;
        //}

        public string Serialize(Formatting format)
        {
            return JsonConvert.SerializeObject(Package, format);
        }

        //public TrustBuilder SetIssuerKey(byte[] key)
        //{
        //    _issuerKey = key;
        //    return this;
        //}

        //public TrustBuilder SetServerKey(byte[] key)
        //{
        //    _serverKey = key;
        //    return this;
        //}

        public override string ToString()
        {
            return Serialize(Formatting.Indented);
        }

        public TrustBuilder Verify()
        {
            //var schema = new PackageSchema(Package);
            //if (!schema.Validate())
            //{
            //    var msg = string.Join(". ", schema.Errors.ToArray());
            //    throw new ApplicationException(msg);
            //}

            //var signature = new TrustECDSASignature(trust);
            //var errors = signature.VerifyTrustSignatureMessage();
            //if (errors.Count > 0)
            //    throw new ApplicationException(string.Join(". ", errors.ToArray()));

            return this;
        }


        public TrustBuilder AddTrust()
        {
            return AddTrust(new Trust());
        }
        //public TrustBuilder AddTrust(string issuerName, string script = CryptoStrategyFactory.BTC_PKH)
        //{
        //    var _cryptoService = _cryptoServiceFactory.GetService(script);
        //    var issuerKey = _cryptoService.GetKey(Encoding.UTF8.GetBytes(issuerName));
        //    AddTrust(issuerKey, _cryptoService, issuerName);
        //    return this;
        //}

        //public TrustBuilder AddTrust(byte[] issuerKey, ICryptoStrategy cryptoStrategy = null, string issuerName = null)
        //{
        //    _currentTrust = new Trust();
        //    _currentTrust.Issuer = new Identity
        //    {
        //        Script = cryptoStrategy.ScriptName,
        //        Id = cryptoStrategy.GetAddress(issuerKey),
        //        PrivateKey = issuerKey
        //    };
        //    Package.Trusts.Add(_currentTrust);

        //    return this;
        //}

        public TrustBuilder AddTrust(Trust trust)
        {
            _currentTrust = trust;
            Package.Trusts.Add(_currentTrust);
            return this;
        }

        public TrustBuilder SetIssuer(string address, string type = "", SignDelegate sign = null)
        {
            if (string.IsNullOrEmpty(type))
                type = DerivationStrategyFactory.BTC_PKH;

            if (CurrentTrust.Issuer == null)
                CurrentTrust.Issuer = new IssuerIdentity();

            CurrentTrust.Issuer.Type = type;
            CurrentTrust.Issuer.Address = address;
            CurrentTrust.IssuerSign = sign;

            return this;
        }

        public TrustBuilder SetServer(string address, string type = "", SignDelegate sign = null)
        {
            if (string.IsNullOrEmpty(type))
                type = DerivationStrategyFactory.BTC_PKH;

            if (Package.Server == null)
                Package.Server = new ServerIdentity();

            Package.Server.Address = address;
            Package.Server.Type = type;
            Package.Server.Sign = sign;

            return this;
        }

        public TrustBuilder SignIssuer(Trust trust = null, SignDelegate sign = null)
        {
            if (trust == null)
                trust = CurrentTrust;

            if (sign != null)
            {
                trust.Issuer.Signature = sign(trust.Id);
            }
            else
            {
                if (trust.IssuerSign != null)
                    trust.Issuer.Signature = trust.IssuerSign(trust.Id);
            }
            return this;
        }

        public TrustBuilder SignServer(Package package = null, SignDelegate sign = null)
        {
            if (package == null)
                package = Package;

            if (sign != null) 
                Package.SetSignature(sign(Package.Id));
            else
                if(Package.Server.Sign != null)
                    Package.SetSignature(Package.Server.Sign(Package.Id));
                else
                    if (Package.ServerSign != null)
                        Package.SetSignature(Package.ServerSign(Package.Id)); 
            return this;
        }


        public TrustBuilder BuildTrustID(Trust trust = null)
        {
            if (trust == null)
                trust = _currentTrust;

            var _hashAlgorithm = _hashAlgorithmFactory.GetAlgorithm(trust.Algorithm);

            if (String.IsNullOrEmpty(trust.Algorithm))
                trust.Algorithm = _hashAlgorithm.AlgorithmName;

            trust.Id = _hashAlgorithm.HashOf(_trustBinary.GetIssuerBinary(trust));

            return this;
        }

        public TrustBuilder SignTrust(Trust trust = null)
        {
            if (trust == null)
                trust = _currentTrust;

            BuildTrustID(trust);
            SignIssuer(trust);

            return this;
        }


        public TrustBuilder AddSubject(string address)
        {
            if (CurrentTrust.Subject == null)
                CurrentTrust.Subject = new SubjectIdentity();

            _currentTrust.Subject.Address = address;

            return this;
        }

        public TrustBuilder AddType(string type, string claim)
        {
            _currentTrust.Type = type;
            _currentTrust.Claim = claim;

            return this;
        }


        public TrustBuilder Build()
        {
            var merkleTree = _merkleStrategyFactory.GetStrategy(Package.Algorithm);

            foreach (var trust in Package.Trusts)
            {
                if(trust.Id == null)
                    BuildTrustID(trust);

                merkleTree.Add(new Timestamp { Source = trust.Id });
            }
            Package.Id = merkleTree.Build().Hash;
                
            return this;
        }

        public Package Sign()
        {
            foreach (var trust in Package.Trusts)
            {
                SignTrust(trust);
            }

            SignServer(Package);

            return Package;
        }


        //public TrustBuilder SignPackageServerID(byte[] serverKey = null)
        //{
        //    if (serverKey == null)
        //        serverKey = _serverKey;

        //    BuildPackageID();

        //    Package.Server = new ServerModel
        //    {
        //        Id = _cryptoService.GetAddress(serverKey),
        //        Signature = _cryptoService.Sign(serverKey, Package.PackageId)
        //    };

        //    return this;
        //}




        //public TrustBuilder ServerID(byte[] serverKey)
        //{
        //    Package.Server.Id = _cryptoService.GetAddress(serverKey);
        //    //var serverKey = new Key(Hashes.SHA256(Encoding.UTF8.GetBytes("server")));
        //    //return serverKey.PubKey.GetAddress(App.BitcoinNetwork).Hash.ToBytes();
        //    return this;
        //}

        //public TrustBuilder AddClaim(Claim claim, Trust trust = null)
        //{
        //    if (trust == null)
        //        trust = CurrentTrust;

        //    var claimID = claim.GetHashCode();
        //    if (trust.Claims == null)
        //        trust.Claims = new List<Claim>();

        //    for(int i = 0; i < CurrentTrust.Claims.Count; i++)
        //    {
        //        var item = CurrentTrust.Claims[i];
        //        var currentId = item.Attributes.GetHashCode();
        //        if (currentId == claimID)
        //        {
        //            claim.Index = i;
        //            return this;
        //        }
        //    }

        //    claim.Index = CurrentTrust.Claims.Count;
        //    CurrentTrust.Claims.Add(claim);

        //    return this;
        //}

        public static string CreateBinaryTrustAttributes(bool trust = true)
        {
            return CreateTrust(trust).ToString(Formatting.None);
        }

        public static string CreateConfirmAttributes(bool confirm = true)
        {
            return CreateConfirm(confirm).ToString(Formatting.None);
        }

        public static string CreateRatingAttributes(byte rating)
        {
            return CreateRating(rating).ToString(Formatting.None);
        }

        //public static Claim CreateTrustClaim(string scope = "", bool trust = true)
        //{
        //    return CreateClaim(BINARYTRUST_TC1, scope, CreateTrust(trust).ToString(Formatting.None));
        //}

        //public static Claim CreateConfirmClaim(string scope = "")
        //{
        //    return CreateClaim(CONFIRMTRUST_TC1, scope, CreateConfirm().ToString(Formatting.None));
        //}

        //public static Claim CreateRatingClaim(byte rating = 100, string scope = "")
        //{
        //    return CreateClaim(RATING_TC1, scope, CreateRating(rating).ToString(Formatting.None));
        //}

        //public static Claim CreateClaim(string type, string scope, string attributes)
        //{
        //    var claim = new Claim
        //    {
        //        Type = type,
        //        Cost = 100,
        //        Attributes = attributes,
        //        Scope = scope // Global scope
        //    };

        //    return claim;
        //}

        public static bool IsTrustTrue(string type, string data)
        {
            if (!BINARY_TRUST_DTP1.EqualsIgnoreCase(type))
                return false;

            var jData = JObject.Parse(data);

            if (jData["trust"] != null && jData["trust"].Value<bool>() == true)
                return true;

            return false;
        }

        public static JObject CreateTrust(bool value = true)
        {
            var obj = new JObject(
                    new JProperty("trust", value)
                    );
            return obj;
        }

        public static JObject CreateRating(byte value)
        {
            return new JObject(
                    new JProperty("rating", value)
                    );
        }

        public static JObject CreateConfirm(bool value = true)
        {
            return new JObject(
                    new JProperty("confirm", value)
                    );
        }


    }
}
