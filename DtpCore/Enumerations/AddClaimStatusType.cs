using System;
using System.Collections.Generic;
using System.Text;

namespace DtpCore.Enumerations
{
    public enum AddClaimStatusType : int
    {
        AlreadyExists,
        Added,
        Updated,
        Removed,
        Error
    }
}
