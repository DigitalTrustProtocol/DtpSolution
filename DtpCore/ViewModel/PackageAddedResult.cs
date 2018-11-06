using System;
using System.Collections.Generic;
using System.Text;

namespace DtpCore.ViewModel
{
    public class TrustAddedResult
    {
        public byte[] ID { get; set; }
        public string Message { get; set; }

    }

    public class PackageAddedResult
    {
        public List<TrustAddedResult> Trusts { get; set; }

        public PackageAddedResult()
        {
            Trusts = new List<TrustAddedResult>();

        }
    }
}
