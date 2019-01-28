using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Extensions;
using DtpCore.Factories;
using System;
using DtpCore.Strategy;
using DtpCore.Collections.Generic;
using DtpCore.Model.Database;

namespace DtpCore.Builders
{
    public class PackageBuilder
    {
        public const string BINARY_TRUST_DTP1 = "binary.trust.dtp1";
        public const string BINARY_TRUST_DTP1_SHORTFORM = "bt1";

        public const string CONFIRM_TRUST_DTP1 = "confirm.trust.dtp1";
        public const string CONFIRM_TRUST_DTP1_SHORTFORM = "ct1";

        public const string RATING_TRUST_DTP1 = "rating.trust.dtp1";
        public const string RATING_TRUST_DTP1_SHORTFORM = "rt1";


        public const string ALIAS_IDENTITY_DTP1 = "alias.identity.dtp1";
        public const string ALIAS_IDENTITY_DTP1_SHORTFORM = "aid1";

        public const string REMOVE_CLAIMS_DTP1 = "remove.claims.dtp1";
        public const string REMOVE_CLAIMS_DTP1_SHORTFORM = "rc1";


        public Package Package { get; set; }
        private IClaimBinary _claimBinary;

        private Claim _currentClaim;
        public Claim CurrentClaim
        {
            get
            {
                return _currentClaim;
            }
        }


        //private IDerivationStrategyFactory _derivationServiceFactory;
        private IMerkleStrategyFactory _merkleStrategyFactory;
        private IHashAlgorithmFactory _hashAlgorithmFactory;


        public PackageBuilder() : this(new MerkleStrategyFactory(new HashAlgorithmFactory()), new HashAlgorithmFactory(), new ClaimBinary())
        {
            
        }

        public PackageBuilder(IMerkleStrategyFactory merkleStrategyFactory, IHashAlgorithmFactory hashAlgorithmFactory, IClaimBinary trustBinary)
        {
            Package = new Package
            {
                Created = (uint)DateTime.UtcNow.ToUnixTime(),
                Algorithm = MerkleStrategyFactory.DOUBLE256_MERKLE_DTP1,
                State = PackageStateType.New
            };
            //_derivationServiceFactory = derivationServiceFactory;
            _merkleStrategyFactory = merkleStrategyFactory;
            _hashAlgorithmFactory = hashAlgorithmFactory;
            _claimBinary = trustBinary;
        }

        public PackageBuilder Load(string content)
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

        public PackageBuilder Verify()
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


        public PackageBuilder AddClaim()
        {
            return AddClaim(new Claim { Created = (uint)DateTime.Now.ToUnixTime() });
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

        public PackageBuilder AddClaim(IEnumerable<Claim> claims)
        {
            foreach (var claim in claims)
            {
                _currentClaim = claim;
                Package.Claims.Add(claim);
            }

            return this;
        }

        public PackageBuilder AddClaim(Claim claim)
        {
            _currentClaim = claim;
            Package.Claims.Add(_currentClaim);
            return this;
        }

        public PackageBuilder OrderClaims()
        {
            Package.Claims = Package.Claims.OrderBy(p => p.Id, ByteComparer.Compare).ToList();
            return this;
        }

        public void OrderClaimDescending()
        {
            Package.Claims = Package.Claims.OrderByDescending(p => p.Id, ByteComparer.Compare).ToList();
        }


        public PackageBuilder SetIssuer(string id, string type = "", SignDelegate sign = null)
        {
            if (string.IsNullOrEmpty(type))
                type = DerivationSecp256k1PKH.NAME;

            if (CurrentClaim.Issuer == null)
                CurrentClaim.Issuer = new IssuerIdentity();

            CurrentClaim.Issuer.Type = type;
            CurrentClaim.Issuer.Id = id;
            CurrentClaim.IssuerSign = sign;

            return this;
        }

        public PackageBuilder SetServer(string id, string type = "", SignDelegate sign = null)
        {
            if (string.IsNullOrEmpty(type))
                type = DerivationSecp256k1PKH.NAME;

            if (Package.Server == null)
                Package.Server = new ServerIdentity();

            Package.Server.Id = id;
            Package.Server.Type = type;
            Package.Server.Sign = sign;

            return this;
        }

        public PackageBuilder SignIssuer(Claim trust = null, SignDelegate sign = null)
        {
            if (trust == null)
                trust = CurrentClaim;

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

        public PackageBuilder SignServer(Package package = null, SignDelegate sign = null)
        {
            if (package == null)
                package = Package;

            if (sign != null) 
                Package.SetSignature(sign(Package.Id));
            else
                if(Package.Server.Sign != null)
                    Package.SetSignature(Package.Server.Sign(Package.Id));
            return this;
        }


        public PackageBuilder BuildClaimID(Claim claim = null)
        {
            if (claim == null)
                claim = _currentClaim;

            var _hashAlgorithm = _hashAlgorithmFactory.GetAlgorithm(claim.Algorithm);

            if (String.IsNullOrEmpty(claim.Algorithm))
                claim.Algorithm = _hashAlgorithm.AlgorithmName;

            claim.Id = _hashAlgorithm.HashOf(_claimBinary.GetIssuerBinary(claim));

            return this;
        }

        public PackageBuilder SignClaim(Claim claim = null)
        {
            if (claim == null)
                claim = _currentClaim;

            BuildClaimID(claim);
            SignIssuer(claim);

            return this;
        }


        public PackageBuilder AddSubject(string id)
        {
            if (CurrentClaim.Subject == null)
                CurrentClaim.Subject = new SubjectIdentity();

            _currentClaim.Subject.Id = id;

            return this;
        }

        public PackageBuilder AddType(string type, string claim)
        {
            _currentClaim.Type = type;
            _currentClaim.Value = claim;

            return this;
        }


        public PackageBuilder Build()
        {
            IMerkleTree merkleTree = CreateMerkleTree();

            //var _hashAlgorithm = _hashAlgorithmFactory.GetAlgorithm();

            //if (string.IsNullOrEmpty(Package.Algorithm))
            //    Package.Algorithm = _hashAlgorithm.AlgorithmName;

            
            Package.Id = merkleTree.HashAlgorithm.HashOf(_claimBinary.GetPackageBinary(Package, merkleTree.Build().Hash));
            Package.State = PackageStateType.Build;

            return this;
        }

        private IMerkleTree CreateMerkleTree()
        {
            var merkleTree = _merkleStrategyFactory.GetStrategy(Package.Algorithm);

            foreach (var claim in Package.Claims)
            {
                if (claim.Id == null)
                    BuildClaimID(claim);

                merkleTree.Add(new Timestamp { Source = claim.Id });
            }

            return merkleTree;
        }

        public Package Sign()
        {
            foreach (var claim in Package.Claims)
            {
                SignClaim(claim);
            }

            SignServer(Package);
            Package.State = PackageStateType.Signed;

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

            if ("true".EqualsIgnoreCase(data) || "1".EqualsIgnoreCase(data))
                return true;

            return false;

            //var jData = JToken.Parse(data);
            //if (jData.Type == JTokenType.Boolean)
            //{
            //    return (bool)((JValue)jData).Value;
            //}
            //else {
            //    if(jData.Type == JTokenType.Object)
            //        if (jData["trust"] != null && jData["trust"].Value<bool>() == true)
            //                return true;
            //}
            //return false;
        }

        public static JToken CreateTrust(bool value = true)
        {
            //var obj = new JObject(
            //        new JProperty("trust", value)
            //        );
            //return obj;
            return new JValue(value);
        }

        public static JToken CreateRating(byte value)
        {
            //return new JObject(
            //        new JProperty("rating", value)
            //        );

            return new JValue(value);
        }

        public static JToken CreateConfirm(bool value = true)
        {
            //return new JObject(
            //        new JProperty("confirm", value)
            //        );
            return new JValue(value);

        }


    }
}
