//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using TrustchainCore.Configuration;
//using TrustchainCore.Interfaces;
//using TrustchainCore.Model;

//namespace TrustchainCore.Business
//{
    //public class TrustManager
    //{
    //    //public int AddTrust(TrustModel trust, TrustchainDatabase db)
    //    //{
    //    //    var result = db.Trust.Add(trust);
    //    //    if (result < 1)
    //    //        return result;

    //    //    foreach (var subject in trust.Issuer.Subjects)
    //    //    {
    //    //        subject.IssuerId = trust.Issuer.Id;
    //    //        subject.TrustId = trust.TrustId;
    //    //        result = db.Subject.Add(subject);
    //    //        if (result < 1)
    //    //            break;
    //    //    }
    //    //    return result;
    //    //}

    //    public static TrustModel Deserialize(string content)
    //    {
    //        var trust = JsonConvert.DeserializeObject<TrustModel>(content);
    //        trust.TrustId = GetTrustId(trust);
    //        return trust;
    //    }

    //    public static void EnsureTrustId(TrustModel trust, ITrustBinary trustBinary)
    //    {
    //        if (trust.TrustId != null && trust.TrustId.Length > 0)
    //            return;

    //        trust.TrustId = GetTrustId(trust);
    //    }



    //    public static byte[] GetTrustId(TrustModel trust)
    //    {
    //        return TrustECDSASignature.GetHashOfBinary(GetTrustBinary(trust));
    //    }

    //    public static byte[] GetTrustBinary(TrustModel trust)
    //    {
    //        var trustBinary = new TrustBinary(trust);
    //        return trustBinary.GetIssuerBinary();
    //    }

    //    public void VerifyTrust(TrustModel trust)
    //    {
    //        var schema = new TrustSchema(trust);
    //        if (!schema.Validate())
    //        {
    //            var msg = string.Join(". ", schema.Errors.ToArray());
    //            throw new ApplicationException(msg);
    //        }

    //        var signature = new TrustECDSASignature(trust);
    //        var errors = signature.VerifyTrustSignatureMessage();
    //        if (errors.Count > 0)
    //            throw new ApplicationException(string.Join(". ", errors.ToArray()));

    //    }
   // }
//}
