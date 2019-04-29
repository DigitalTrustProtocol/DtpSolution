using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DtpCore.Interfaces;
using DtpCore.Strategy.Serialization;
using JsonSubTypes;
using DtpCore.Model.Database;

namespace DtpCore.Model
{
    /// <summary>
    /// Signing of an Id with data
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="data">The data that is signed</param>
    /// <returns>Signature</returns>
    public delegate byte[] SignDelegate(byte[] data);

    [NotMapped]
    public class PackageReference
    {
        /// <summary>
        /// The Id of the package. 
        /// If a root property exist, then this is used with the Id to calculate the final value based on the merkle tree defined in the algorithm property.
        /// </summary>
        [UIHint("ByteToHex")]
        [JsonProperty(PropertyName = "id", Order = -100)]
        public byte[] Id { get; set; }
        public bool ShouldSerializeId() => Id != null && Id.Length > 0;

        [JsonProperty(PropertyName = "file")]
        public string File { get; set; }
        public bool ShouldSerializeFile() => !string.IsNullOrWhiteSpace(File);
    }

    [NotMapped]
    public class PackageInformation : PackageReference
    {
        /// <summary>
        /// The algorithm used to calculate the Id.
        /// </summary>
        [JsonProperty(PropertyName = "algorithm", Order = -200)]
        public string Algorithm { get; set; }
        public bool ShouldSerializeAlgorithm() => !string.IsNullOrWhiteSpace(Algorithm);

        [UIHint("ByteToHex")]
        [JsonProperty(PropertyName = "root", Order = -99)]
        public byte[] Root { get; set; }
        public bool ShouldSerializeRoot() { return Root != null; }

        [UIHint("UnixTimeUInt")]
        [JsonProperty(PropertyName = "created", Order = -50)]
        public uint Created { get; set; }
        public bool ShouldSerializeCreated() => Created > 0;

        /// <summary>
        /// The types of claims that the packages contains.
        /// </summary>
        [JsonProperty(PropertyName = "types")]
        public IList<string> Types { get; set; }
        public bool ShouldSerializeTypes() => Types != null && Types.Count > 0;

        /// <summary>
        /// The scopes of claims that the package contains.
        /// </summary>
        [JsonProperty(PropertyName = "scopes")]
        public string Scopes { get; set; }
        public bool ShouldSerializeScopes() => !string.IsNullOrWhiteSpace(Scopes);

        [JsonProperty(PropertyName = "server", NullValueHandling = NullValueHandling.Ignore)]
        public ServerIdentity Server { get; set; }
    }

    [Table("Package")]
    [JsonObject(MemberSerialization.OptIn)]
    //[JsonConverter(typeof(PackageConverter))] // logic for handling the template features has not been implemented.
    public class Package : PackageInformation
    {
        [JsonIgnore]
        public int DatabaseID { get; set; } // Database row key

        [NotMapped]
        [JsonProperty(PropertyName = "claims", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Claim> Claims { get; set; }
        public bool ShouldSerializeTrusts() => Claims != null && Claims.Count > 0;

        /// <summary>
        /// Acts as a default template for trusts in the package. If a claim is missing vital information, the template is assumed.
        /// Currently not used and implemented!
        /// </summary>
        //[JsonProperty(PropertyName = "templates", NullValueHandling = NullValueHandling.Ignore)]
        //[NotMapped] // When 
        //public IList<Claim> Templates { get; set; }
        //public bool ShouldSerializeTemplates() => Templates != null && Templates.Count > 0;

        // Not used for the moment
        ///// <summary>
        ///// Contains multiple packages. 
        ///// </summary>
        //[JsonProperty(PropertyName = "packages", NullValueHandling = NullValueHandling.Ignore)]
        //public IList<Package> Packages { get; set; }
        //public bool ShouldSerializePackages() => Packages != null && Packages.Count > 0;

        /// <summary>
        /// List of packages that are replaced by this package. 
        /// </summary>
        [JsonProperty(PropertyName = "obsoletes", NullValueHandling = NullValueHandling.Ignore)]
        public IList<PackageReference> Obsoletes { get; set; }
        public bool ShouldSerializeReplacing() => Obsoletes != null && Obsoletes.Count > 0;

        [JsonProperty(PropertyName = "timestamps", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Timestamp> Timestamps { get; set; }
        public bool ShouldSerializeTimestamps() => Timestamps != null && Timestamps.Count > 0;

        [JsonIgnore]
        [Description("The system state of the package.")]
        public PackageStateType State { get; set; }


        /// <summary>
        /// Used for internal database relationship.
        /// </summary>
        [JsonIgnore]
        public IList<ClaimPackageRelationship> ClaimPackages { get; set; }
        public bool ShouldSerializeClaimPackages() => ClaimPackages != null && ClaimPackages.Count > 0;

        public Package()
        {
            Claims = new List<Claim>();
            //Templates = new List<Claim>();
            //Packages = new List<Package>();
            Timestamps = new List<Timestamp>();
            Obsoletes = new List<PackageReference>();
            ClaimPackages = new List<ClaimPackageRelationship>();
            Server = new ServerIdentity();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }


    [Table("Claim")]
    [JsonObject(MemberSerialization.OptIn)]
    public class Claim : DatabaseEntity
    {
        /// <summary>
        /// Id is an internal property used for identifying the claim within the database, it is not a part of the claim.
        /// The Issuer signs the claim binary data not the ID.
        /// </summary>
        [UIHint("ByteToHex")]
        [JsonIgnore] // Id is not serialized as it is an internal property
        public byte[] Id { get; set; }

        /// <summary>
        /// Defines a root id for the claim, enables for single signing of many claims.
        /// </summary>
        [UIHint("ByteToHex")]
        [JsonProperty(PropertyName = "root")]
        public byte[] Root { get; set; }
        public bool ShouldSerializeRoot() { return Root != null; }

        [UIHint("UnixTimeUInt")]
        [JsonProperty(PropertyName = "created")]
        public uint Created { get; set; }
        public bool ShouldSerializeCreated() { return Created > 0; }

        [JsonProperty(PropertyName = "issuer", NullValueHandling = NullValueHandling.Ignore)]
        public IssuerIdentity Issuer { get; set; }

        /// <summary>
        /// Internal property for holding the private key to sign with, need to be removed as its business logic.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public SignDelegate IssuerSign { get; set; }

        [JsonProperty(PropertyName = "subject", NullValueHandling = NullValueHandling.Ignore)]
        public SubjectIdentity Subject { get; set; }

        /// <summary>
        /// Internal property for holding the private key to sign with, need to be removed as its business logic.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public SignDelegate SubjectSign { get; set; }


        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        //[UIHint("JSON")]
        [JsonProperty(PropertyName = "value")]
        public virtual string Value { get; set; }

        //[UIHint("Serialize")]
        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }
        public bool ShouldSerializeScope() { return Scope!= null; }

        [UIHint("UnixTimeUint")]
        [JsonProperty(PropertyName = "activate")]
        public uint Activate { get; set; }
        public bool ShouldSerializeActivate() { return Activate > 0; }

        [UIHint("UnixTimeUInt")]
        [JsonProperty(PropertyName = "expire")]
        public uint Expire { get; set; }
        public bool ShouldSerializeExpire() { return Expire > 0; }

        /// <summary>
        /// A short comment on the reason for the trust. Very limit in size. Single word is optimal.
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        [Description("Issuers metadata about the claim.")]
        public string Metadata { get; set; }
        public bool ShouldSerializeMetadata() { return Metadata != null; }


        [UIHint("Serialize")]
        [JsonProperty(PropertyName = "timestamps", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Timestamp> Timestamps { get; set; }
        public bool ShouldSerializeTimestamps() { return Timestamps != null && Timestamps.Count > 0; }

        [JsonProperty(PropertyName = "templateId")]
        public uint TemplateId { get; set; }
        public bool ShouldSerializeTemplateId() { return TemplateId > 0; }

        /// <summary>
        /// A trust can belong to multiple packages created by other servers. 
        /// </summary>
        [DisplayName("Packages")]
        [JsonIgnore]
        public IList<ClaimPackageRelationship> ClaimPackages { get; set; }
        public bool ShouldSerializeClaimPackages() => ClaimPackages != null && ClaimPackages.Count > 0;

        [JsonIgnore]
        [UIHint("ClaimStateType")]
        [Description("The system state of the claim.")]
        public ClaimStateType State { get; set; }


        public Claim()
        {
            Timestamps = new List<Timestamp>();
            ClaimPackages = new List<ClaimPackageRelationship>();
            Scope = string.Empty;
        }
    }



    //[JsonObject(MemberSerialization.OptIn)]
    //public class BinaryClaim : Claim
    //{
    //    //Only relevant for the Binary trust type.
    //    [JsonProperty(PropertyName = "cost")]
    //    public short Cost { get; set; }
    //    public bool ShouldSerializeCost() { return Cost > 0 && Cost != 100; }
    //}



    [JsonObject(MemberSerialization.OptIn)]
    public class PartId
    {
        [JsonProperty(PropertyName = "algorithm")]
        public string Algorithm { get; set; }
        public bool ShouldSerializeAlgorithm() { return !string.IsNullOrWhiteSpace(Algorithm); }

        [UIHint("ByteToHexLong")]
        [JsonProperty(PropertyName = "receipt", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Receipt { get; set; }
        public bool ShouldSerializeReceipt() { return Receipt != null && Receipt.Length > 0; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class TrustType
    {
        [JsonProperty(PropertyName = "attribute")]
        public string Attribute { get; set; }
        public bool ShouldSerializeAttribute() { return !string.IsNullOrWhiteSpace(Attribute); }

        [JsonProperty(PropertyName = "group")]
        public string Group { get; set; }
        public bool ShouldSerializeGroup() { return !string.IsNullOrWhiteSpace(Group); }

        [JsonProperty(PropertyName = "protocol")]
        public string Protocol { get; set; }
        public bool ShouldSerializeProtocol() { return !string.IsNullOrWhiteSpace(Protocol); }

        public override string ToString()
        {
            return String.Join('.', Attribute, Group, Protocol);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class IssuerIdentity : Identity
    {
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class SubjectIdentity : Identity
    {
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ServerIdentity : Identity
    {
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Identity
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        public bool ShouldSerializeType() { return !string.IsNullOrWhiteSpace(Type); }

        //[UIHint("ByteToAddress")]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public bool ShouldSerializeId() { return !string.IsNullOrWhiteSpace(Id); }

        //[UIHint("ByteToHex")]
        [JsonProperty(PropertyName = "proof")]
        public byte[] Proof { get; set; }
        public bool ShouldSerializeProof()
        {
            return Proof != null && Proof.Length > 0;
        }

        /// <summary>
        /// Internal property for holding the private key to sign with
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public SignDelegate Sign { get; set; }
    }

    //[JsonObject(MemberSerialization.OptIn)]
    //public class Scope
    //{
    //    [JsonProperty(PropertyName = "type")]
    //    public string Type { get; set; }
    //    public bool ShouldSerializeType() { return !string.IsNullOrWhiteSpace(Type); }

    //    [JsonProperty(PropertyName = "value")]
    //    public string Value { get; set; }
    //    public bool ShouldSerializeValue() { return !string.IsNullOrWhiteSpace(Value); }
    //}


    [Table("Timestamp")]
    [JsonObject(MemberSerialization.OptIn)]
    public class Timestamp : ITimestamp
    {
        [JsonIgnore]
        public int DatabaseID { get; set; } // Database row key

        [JsonProperty(PropertyName = "blockchain")]
        public string Blockchain { get; set; }
        public bool ShouldSerializeBlockchain() { return !string.IsNullOrWhiteSpace(Blockchain); }

        [JsonProperty(PropertyName = "algorithm")]
        public string Algorithm { get; set; }
        public bool ShouldSerializeAlgorithm() { return !string.IsNullOrWhiteSpace(Algorithm); }

        [JsonProperty(PropertyName = "service")]
        public string Service { get; set; }
        public bool ShouldSerializeService() { return !string.IsNullOrWhiteSpace(Service); }

        [UIHint("ByteToHex")]
        [JsonProperty(PropertyName = "source", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Source { get; set; }
        public bool ShouldSerializeSource() { return Source != null && Source.Length > 0; }

        [UIHint("ByteToHexLong")]
        [DisplayName("Merkle path")]
        [JsonProperty(PropertyName = "path", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Path { get; set; }
        public bool ShouldSerializePath() { return Path != null && Path.Length > 0; }

        [UIHint("UnixTimeLong")]
        [JsonProperty(PropertyName = "registered")]
        public long Registered { get; set; }
        public bool ShouldSerializeRegistered() { return Registered != 0; }

        //[JsonIgnore]
        //[Description("The system state of the timestamp.")]
        //public TimestampStateType State { get; set; }

        // No read of proof into the system!! Only render! Make Converter.
        //[JsonProperty(PropertyName = "proof")]
        [JsonIgnore]
        [DisplayName("Proof")]
        public BlockchainProof Proof { get; set; }

        [JsonIgnore]
        public int? ProofDatabaseID { get; set; } // Database row key

        [JsonIgnore]
        public int? PackageDatabaseID { get; set; } // Database row key

        [JsonIgnore]
        public int? ClaimDatabaseID { get; set; } // Database row key

        public Timestamp()
        {
            //State = TimestampStateType.New;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class TimestampAlgorithm
    {
        public string Derivation { get; set; }
        public string Hash { get; set; }
        public string Merkle { get; set; }
    }


}