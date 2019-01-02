using System;
using System.Collections.Generic;
using System.Text;

namespace DtpCore.Interfaces
{
    public interface ITimestamp
    {
        byte[] Source { get; set; }
        byte[] Value { get; set; }
        long Registered { get; set; }
    }
}
