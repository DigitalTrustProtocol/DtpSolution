using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DtpCore.Interfaces;
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


    public class JSON_LD_Object
    {
        /// <summary>
        /// Type defines the claim and its format. 
        /// </summary>
        [JsonProperty(PropertyName = "@context", Order = -300)]
        public string Context { get; set; }
        public bool ShouldSerializeContext() => !string.IsNullOrWhiteSpace(Context);


        /// <summary>
        /// Type defines the claim and its format. 
        /// </summary>
        [JsonProperty(PropertyName = "type", Order = -200)]
        public string Type { get; set; }
        public bool ShouldSerializeType() => !string.IsNullOrWhiteSpace(Type);
    }


    [NotMapped]
    public class PackageReference : JSON_LD_Object
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
        public const string DEFAULT_TYPE = "package";

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
    public class Claim : JSON_LD_Object
    {

        [JsonIgnore]
        public int DatabaseID { get; set; } // Database row key

        /// <summary>
        /// Id is an internal property used for identifying the claim within the database, it is not a part of the claim.
        /// The Issuer signs the claim binary data not the ID.
        /// </summary>
        [UIHint("ByteToHex")]
        [JsonIgnore] // Id is not serialized as it is an internal property
        public byte[] Id { get; set; }

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
        /// Usually ids for text tokens. 
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

    [JsonObject(MemberSerialization.OptIn)]
    public class IssuerIdentity : Identity
    {
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class SubjectIdentity : Identity
    {
    }


    [JsonObject(MemberSerialization.OptIn)]
    public class IdentityMetadata 
    {
        /// <summary>
        /// The ID of the identity
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public bool ShouldSerializeId() { return !string.IsNullOrWhiteSpace(Id); }

        ///// <summary>
        ///// Type defines the content of data. 
        ///// "Text" or "reference"
        ///// </summary>
        //[JsonProperty(PropertyName = "type", Order = -200)]
        //public string Type { get; set; }
        //public bool ShouldSerializeType() => !string.IsNullOrWhiteSpace(Type);

        ///// <summary>
        ///// A label on the data is for display and is included in the calculation of the Subject ID.
        ///// The format is always text and is limited in length.
        ///// </summary>
        //[JsonProperty(PropertyName = "label")]
        //public string Label { get; set; }
        //public bool ShouldSerializeLabel() { return !string.IsNullOrEmpty(Label); }


        /// <summary>
        /// Represents the source data used for calculating the Subject ID. Usually by a hash of the data.
        /// The data cannot be longer than 4k bytes in text. For data sources like pictures and documents, 
        /// a url link is defined in the Data field and the Type field is then set to "ref".
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }
        public bool ShouldSerializeData() { return !string.IsNullOrEmpty(Data); }
    }


    [JsonObject(MemberSerialization.OptIn)]
    public class ServerIdentity : Identity
    {
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Identity : JSON_LD_Object
    {
        //[JsonProperty(PropertyName = "type")]
        //public string Type { get; set; }
        //public bool ShouldSerializeType() { return !string.IsNullOrWhiteSpace(Type); }

        [JsonProperty(PropertyName = "algorithm")]
        public string Algorithm { get; set; }
        public bool ShouldSerializeAlgorithm() { return !string.IsNullOrWhiteSpace(Algorithm); }


        //[UIHint("ByteToAddress")]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public bool ShouldSerializeId() { return !string.IsNullOrWhiteSpace(Id); }

        /// <summary>
        /// Optional merkle tree path
        /// </summary>
        [UIHint("ByteToHexLong")]
        [DisplayName("Merkle tree path")]
        [JsonProperty(PropertyName = "path", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Path { get; set; }
        public bool ShouldSerializePath() { return Path != null && Path.Length > 0; }


        //[UIHint("ByteToHex")]
        [JsonProperty(PropertyName = "proof")]
        public byte[] Proof { get; set; }
        public bool ShouldSerializeProof()
        {
            return Proof != null && Proof.Length > 0;
        }

        /// <summary>
        /// Represents the metadata used for alias of the issuer ID.
        /// </summary>
        [JsonProperty(PropertyName = "meta")]
        public IdentityMetadata Meta { get; set; }
        public bool ShouldSerializeMetadata() { return Meta != null; } // 

        /// <summary>
        /// Internal property for holding the private key to sign with
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public SignDelegate Sign { get; set; }
    }


    [Table("Timestamp")]
    [JsonObject(MemberSerialization.OptIn)]
    public class Timestamp : ITimestamp
    {
        public const string DEFAULT_TYPE = "timestamp.dtp1";

        [JsonIgnore]
        public int DatabaseID { get; set; } // Database row key

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        public bool ShouldSerializeType() { return !string.IsNullOrWhiteSpace(Type); }

        [JsonProperty(PropertyName = "algorithm")]
        public string Algorithm { get; set; }
        public bool ShouldSerializeAlgorithm() { return !string.IsNullOrWhiteSpace(Algorithm); }

        [JsonProperty(PropertyName = "blockchain")]
        public string Blockchain { get; set; }
        public bool ShouldSerializeBlockchain() { return !string.IsNullOrWhiteSpace(Blockchain); }

        [UIHint("ByteToHex")]
        [JsonProperty(PropertyName = "source", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Source { get; set; }
        public bool ShouldSerializeSource() { return Source != null && Source.Length > 0; }

        // Optional merkle tree path combined with source
        [UIHint("ByteToHexLong")]
        [DisplayName("Merkle tree path")]
        [JsonProperty(PropertyName = "path", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Path { get; set; }
        public bool ShouldSerializePath() { return Path != null && Path.Length > 0; }

        [JsonProperty(PropertyName = "service")]
        public string Service { get; set; }
        public bool ShouldSerializeService() { return !string.IsNullOrWhiteSpace(Service); }

        [UIHint("ByteToHexLong")]
        [DisplayName("Service receipt")]
        [JsonProperty(PropertyName = "receipt", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Receipt { get; set; }
        public bool ShouldSerializeReceipt() { return Receipt != null && Receipt.Length > 0; }


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


}