using System;
using System.Collections.Generic;
using System.Text;

namespace DtpCore.ViewModel
{
    public class ClaimAddedResult
    {
        public byte[] ID { get; set; }
        public string Message { get; set; }

    }

    public class PackageAddedResult
    {
        public List<ClaimAddedResult> Claims { get; set; }

        public PackageAddedResult()
        {
            Claims = new List<ClaimAddedResult>();

        }
    }
}
