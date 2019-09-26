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
        public const string BINARY_CLAIM_DTP1 = "binarytrust";
        public const string BINARY_CLAIM_DTP1_SHORTFORM = "bt";

        public const string CONFIRM_CLAIM_DTP1 = "confirm";
        public const string CONFIRM_CLAIM_DTP1_SHORTFORM = "ct";

        public const string RATING_CLAIM_DTP1 = "rating";
        public const string RATING_CLAIM_DTP1_SHORTFORM = "rt";

        public const string ID_IDENTITY_DTP1 = "id";
        public const string ID_IDENTITY_DTP1_SHORTFORM = "iid1";

        public const string ENTITY_IDENTITY_DTP1 = "entity";
        public const string ENTITY_IDENTITY_DTP1_SHORTFORM = "eid1";

        public const string THING_IDENTITY_DTP1 = "thing";
        public const string THING_IDENTITY_DTP1_SHORTFORM = "tid1";


        public const string ALIAS_IDENTITY_DTP1 = "alias";
        public const string ALIAS_IDENTITY_DTP1_SHORTFORM = "aid1";

        public const string REMOVE_CLAIMS_DTP1 = "remove";
        public const string REMOVE_CLAIMS_DTP1_SHORTFORM = "rc1";


        public Package Package { get; set; }
        //private IClaimBinary _claimBinary;
        private IPackageBinary _packageBinary;

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


        public PackageBuilder() : this(new MerkleStrategyFactory(new HashAlgorithmFactory()), new HashAlgorithmFactory(), new PackageBinary(new ClaimBinary()))
        {
            
        }

        public PackageBuilder(IMerkleStrategyFactory merkleStrategyFactory, IHashAlgorithmFactory hashAlgorithmFactory, IPackageBinary packageBinary)
        {
            Package = new Package
            {
                Type = Package.DEFAULT_TYPE,
                Created = (uint)DateTime.UtcNow.ToUnixTime(),
                State = PackageStateType.New
            };
            //_derivationServiceFactory = derivationServiceFactory;
            _merkleStrategyFactory = merkleStrategyFactory;
            _hashAlgorithmFactory = hashAlgorithmFactory;
            _packageBinary = packageBinary;
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

        public PackageBuilder SignIssuer(Claim claim = null, SignDelegate sign = null)
        {
            if (claim == null)
                claim = CurrentClaim;

            var source = _packageBinary.ClaimBinary.GetIdSource(claim);
            if (sign != null)
            {
                claim.Issuer.Proof = sign(source);
            }
            else
            {
                if (claim.IssuerSign != null)
                    claim.Issuer.Proof = claim.IssuerSign(source);
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

            claim.Id = GetClaimID(claim);

            return this;
        }

        public static byte[] GetClaimID(Claim claim)
        {
            var claimBinary = new ClaimBinary();
            return new Sha256().HashOf(claimBinary.GetIdSource(claim));
        }


        public PackageBuilder SignClaim(Claim claim = null)
        {
            if (claim == null)
                claim = _currentClaim;

            SignIssuer(claim);

            return this;
        }


        public PackageBuilder AddSubject(string id)
        {
            if (CurrentClaim.Subject == null)
                CurrentClaim.Subject = new SubjectIdentity();

            _currentClaim.Subject.Type = "thing";
            _currentClaim.Subject.Id = id;

            return this;
        }

        public PackageBuilder AddType(string type, string claim)
        {
            _currentClaim.Type = type;
            _currentClaim.Value = claim;

            return this;
        }


        /// <summary>
        /// Build the Package ID from the hash of the package properties and all the claim ids computed in a merkle structure.
        /// This enables claims to prove existence in original package when it have been sepereated into another package.
        /// </summary>
        /// <returns>PackageBuilder</returns>
        public PackageBuilder Build()
        {
            var merkleTree = _merkleStrategyFactory.GetStrategy(Package.Type);
            
            var packageHash =_packageBinary.GetIdSource(Package);
            merkleTree.Add(packageHash); // Start with adding the package hash

            foreach (var claim in Package.Claims)
            {
                var source = _packageBinary.ClaimBinary.GetIdSource(claim);
                merkleTree.Add(source); // Add claim Id source (unique binary id)
            }

            Package.Id = merkleTree.Build().Hash;   // Merkle of Package hash and all the claims.
            Package.State = PackageStateType.Build;

            return this;
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
            if (!BINARY_CLAIM_DTP1.EqualsIgnoreCase(type))
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
