using System;
using System.Collections.Generic;
using System.Text;

namespace DtpCore.ViewModel
{
    public class PackageInfoCollection
    {
        public List<PackageInfo> Packages { get; set; }

        public PackageInfoCollection()
        {
            Packages = new List<PackageInfo>();
        }
    }

    public class PackageInfo
    {
        public byte[] Id { get; set; }
        public string File { get; set; }
    }
}
