using System;
using System.Collections.Generic;
using System.Text;

namespace DtpCore.Model.Database
{
    /// <summary>
    /// The state of the claim for internal use.
    /// </summary>
    [Flags]
    public enum ClaimState : long
    {
        Replaced = 1,
        //Processed = 2,
        //Wednesday = 4,
        //Thursday = 8,
        //Friday = 16,
        //Saturday = 32,
        //Sunday = 64,
    }

}
