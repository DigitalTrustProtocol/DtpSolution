using System;
using System.Collections.Generic;
using System.Text;

namespace DtpCore.ViewModel
{
    public class TrustCommandView
    {
        public byte[] ID { get; set; }
        public string Message { get; set; }

    }

    public class PackageCommandView
    {
        public List<TrustCommandView> Trusts { get; set; }

        public PackageCommandView()
        {
            Trusts = new List<TrustCommandView>();

        }
    }
}
