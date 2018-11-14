using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DtpCore.Attributes;
using DtpCore.Interfaces;
using DtpCore.Strategy.Serialization;

namespace DtpCore.Model
{
    /// <summary>
    /// Signing of an address with data
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

        [UIHint("UnixTimeUInt")]
        [JsonProperty(PropertyName = "created")]
        public uint Created { get; set; }
        public bool ShouldSerializeCreated() => Created > 0;

        [JsonProperty(PropertyName = "trusts", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Trust> Trusts { get; set; }
        public bool ShouldSerializeTrusts() => Trusts != null && Trusts.Count > 0;

        [JsonProperty(PropertyName = "server", NullValueHandling = NullValueHandling.Ignore)]
        public ServerIdentity Server { get; set; }

        [JsonProperty(PropertyName = "timestamps", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Timestamp> Timestamps { get; set; }
        public bool ShouldSerializeTimestamps() => Timestamps != null && Timestamps.Count > 0;

        [JsonProperty(PropertyName = "trustPackage", NullValueHandling = NullValueHandling.Ignore)]
        public IList<TrustPackage> TrustPackages { get; set; }
        public bool ShouldSerializeTrustPackages() => TrustPackages != null && TrustPackages.Count > 0;

        public Package()
        {
            Timestamps = new List<Timestamp>();
            TrustPackages = new List<TrustPackage>();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }


    [Table("Trust")]
    [JsonObject(MemberSerialization.OptIn)]
    public class Trust : DatabaseEntity
    {
        [JsonProperty(PropertyName = "algorithm")]
        public string Algorithm { get; set; }
        public bool ShouldSerializeAlgorithm() { return !string.IsNullOrWhiteSpace(Algorithm); }

        [UIHint("ByteToHex")]
        [JsonProperty(PropertyName = "id")]
        public byte[] Id { get; set; }
        public bool ShouldSerializeId() { return Id != null; }

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
        [JsonProperty(PropertyName = "claim")]
        [JsonConverter(typeof(ObjectToStringConverter))]
        public virtual string Claim { get; set; }

        //[UIHint("Serialize")]
        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }
        public bool ShouldSerializeScope() { return Scope!= null; }

        // Only relevant for the Binary trust type.
        //[JsonProperty(PropertyName = "cost")]
        //public short Cost { get; set; }
        //public bool ShouldSerializeCost() { return Cost > 0 && Cost != 100; }

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
        [Description("Issuers comment on the trust.")]
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
        public IList<TrustPackage> TrustPackages { get; set; }
        public bool ShouldSerializeTrustPackages() => TrustPackages != null && TrustPackages.Count > 0;


        [JsonIgnore]
        [Description("Current Trust has been replaced by a new Trust.")]
        public bool Replaced { get; set; }

        public Trust()
        {
            Timestamps = new List<Timestamp>();
            TrustPackages = new List<TrustPackage>();
            Scope = string.Empty;
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
        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }
        public bool ShouldSerializeAddress() { return !string.IsNullOrWhiteSpace(Address); }

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
        [JsonProperty(PropertyName = "receipt", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Receipt { get; set; }
        public bool ShouldSerializeReceipt() { return Receipt != null && Receipt.Length > 0; }

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