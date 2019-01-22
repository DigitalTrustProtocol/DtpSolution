using System;
using System.Collections.Generic;
using System.Text;

namespace DtpCore.Model.Database
{
    /// <summary>
    /// The state of the claim for internal use with Database.
    /// </summary>
    [Flags]
    public enum ClaimState : long
    {
        Replaced = 1,
        /// <summary>
        /// The claim is functional, this would be like Ban claims.
        /// </summary>
        Functional = 2,
        Ban = 4,
        //Thursday = 8,
        //Friday = 16,
        //Saturday = 32,
        //Sunday = 64,
    }

}
