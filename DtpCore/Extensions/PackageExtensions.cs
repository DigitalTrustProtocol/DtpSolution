using DtpCore.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DtpCore.Extensions
{
    public static class PackageExtensions
    {
        public static Timestamp AddTimestamp(this Package package, Timestamp timestamp)
        {
            if (package == null)
                return null;

            if (package.Timestamps == null)
                package.Timestamps = new List<Timestamp>();

            package.Timestamps.Add(timestamp);
            return timestamp;
        }

    }
}
