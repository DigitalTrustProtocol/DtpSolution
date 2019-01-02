using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DtpCore.Attributes;
using DtpCore.Interfaces;
using DtpCore.Strategy.Serialization;
using JsonSubTypes;

namespace DtpCore.Model
{
    /// <summary>
    /// Signing of an Id with data
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="data">The data that is signed</param>
    /// <returns>Signature</returns>
    public delegate byte[] SignDelegate(byte[] data);


    [Table("Package")]
    [JsonObject(MemberSerialization.OptIn)]
    public class Package : DatabaseEntity
    {
        [JsonProperty(PropertyName = "algorithm")]
        public string Algorithm { get; set; }
        public bool ShouldSerializeAlgorithm() => !string.IsNullOrWhiteSpace(Algorithm);

        [UIHint("ByteToHex")]
        [JsonProperty(PropertyName = "id")]
        public byte[] Id { get; set; }
        public bool ShouldSerializeId() => Id != null && Id.Length > 0;

        [UIHint("ByteToHex")]
        [JsonProperty(PropertyName = "root")]
        public byte[] Root { get; set; }
        public bool ShouldSerializeRoot() { return Root != null; }

        [UIHint("UnixTimeUInt")]
        [JsonProperty(PropertyName = "created")]
        public uint Created { get; set; }
        public bool ShouldSerializeCreated() => Created > 0;

        [JsonProperty(PropertyName = "claims", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Claim> Claims { get; set; }
        public bool ShouldSerializeTrusts() => Claims != null && Claims.Count > 0;

        /// <summary>
        /// Acts as a default template for trusts in the package. If a trust is missing vital information, the template is assumed.
        /// </summary>
        [JsonProperty(PropertyName = "templates", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Claim> Templates { get; set; }
        public bool ShouldSerializeTemplate() => Templates != null && Templates.Count > 0;

        /// <summary>
        /// Contains multiple packages. 
        /// </summary>
        [JsonProperty(PropertyName = "packages", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Package> Packages { get; set; }
        public bool ShouldSerializePackages() => Packages != null && Packages.Count > 0;


        [JsonProperty(PropertyName = "server", NullValueHandling = NullValueHandling.Ignore)]
        public ServerIdentity Server { get; set; }

        [JsonProperty(PropertyName = "timestamps", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Timestamp> Timestamps { get; set; }
        public bool ShouldSerializeTimestamps() => Timestamps != null && Timestamps.Count > 0;


        //[JsonProperty(PropertyName = "trustPackage", NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore]
        public IList<ClaimPackage> ClaimPackages { get; set; }
        public bool ShouldSerializeClaimPackages() => ClaimPackages != null && ClaimPackages.Count > 0;

        public Package()
        {
            Claims = new List<Claim>();
            Templates = new List<Claim>();
            Timestamps = new List<Timestamp>();
            ClaimPackages = new List<ClaimPackage>();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }


    [Table("Claim")]
    [JsonConverter(typeof(JsonSubtypes))]
    [JsonSubtypes.KnownSubType(typeof(BinaryClaim), "Cost")]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(BinaryClaim), "Cost")]
    [JsonObject(MemberSerialization.OptIn)]
    public class Claim : DatabaseEntity
    {
        [JsonProperty(PropertyName = "algorithm")]
        public string Algorithm { get; set; }
        public bool ShouldSerializeAlgorithm() { return !string.IsNullOrWhiteSpace(Algorithm); }

        [UIHint("ByteToHex")]
        [JsonProperty(PropertyName = "id")]
        public byte[] Id { get; set; }
        public bool ShouldSerializeId() { return Id != null; }

        [UIHint("ByteToHex")]
        [JsonProperty(PropertyName = "root")]
        public byte[] Root { get; set; }
        public bool ShouldSerializeRoot() { return Root != null; }

        /// <summary>
        /// Defines a partial id for the trust, enables for single signing of many trusts.
        /// </summary>
        [JsonProperty(PropertyName = "partialId", NullValueHandling = NullValueHandling.Ignore)]
        public PartId PartialId { get; set; }
        public bool ShouldSerializePartId() { return PartialId != null; }

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
        [JsonConverter(typeof(ObjectToStringConverter))]
        public string Type { get; set; }

        [UIHint("JSON")]
        [JsonProperty(PropertyName = "value")]
        [JsonConverter(typeof(ObjectToStringConverter))]
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
        [JsonProperty(PropertyName = "note")]
        [Description("Issuers comment on the claim.")]
        public string Note { get; set; }
        public bool ShouldSerializeNote() { return Note != null; }


        [UIHint("Serialize")]
        [JsonProperty(PropertyName = "timestamps", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Timestamp> Timestamps { get; set; }
        public bool ShouldSerializeTimestamps() { return Timestamps != null && Timestamps.Count > 0; }

        /// <summary>
        /// Used for direct reference to a package created by the local server. Enables to identify trusts not packaged by the local server yet.
        /// </summary>
        [JsonIgnore]
        public int? PackageDatabaseID { get; set; }

        /// <summary>
        /// A trust can belong to multiple packages created by other servers. 
        /// </summary>
        [DisplayName("Packages")]
        [JsonProperty(PropertyName = "trustPackage", NullValueHandling = NullValueHandling.Ignore)]
        public IList<ClaimPackage> TrustPackages { get; set; }
        public bool ShouldSerializeTrustPackages() => TrustPackages != null && TrustPackages.Count > 0;


        [JsonIgnore]
        [Description("Current Trust has been replaced by a new Trust.")]
        public bool Replaced { get; set; }

        public Claim()
        {
            Timestamps = new List<Timestamp>();
            TrustPackages = new List<ClaimPackage>();
            Scope = string.Empty;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class BinaryClaim : Claim
    {
        //Only relevant for the Binary trust type.
        [JsonProperty(PropertyName = "cost")]
        public short Cost { get; set; }
        public bool ShouldSerializeCost() { return Cost > 0 && Cost != 100; }
    }



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
        [JsonProperty(PropertyName = "signature")]
        public byte[] Signature { get; set; }
        public bool ShouldSerializeSignature()
        {
            return Signature != null && Signature.Length > 0;
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
    public class Timestamp : DatabaseEntity, ITimestamp
    {
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
        [JsonProperty(PropertyName = "value", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Value { get; set; }
        public bool ShouldSerializeValue() { return Value != null && Value.Length > 0; }

        [UIHint("UnixTimeLong")]
        [JsonProperty(PropertyName = "registered")]
        public long Registered { get; set; }
        public bool ShouldSerializeRegistered() { return Registered != 0; }

        //[JsonIgnore]
        //public int WorkflowID { get; set; }
        [JsonIgnore]
        public int BlockchainProof_db_ID { get; set; }

        [JsonIgnore]
        public int PackageDatabase_db_ID { get; set; }

        [JsonIgnore]
        public int TrustDatabase_db_ID { get; set; }

    }


}