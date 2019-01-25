using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DtpCore.Model.Database
{
    /// <summary>
    /// The state of the claim for internal use with Database.
    /// </summary>
    [Flags]
    public enum ClaimStateType : long
    {
        [Display(Name = "None")]
        None = 0,

        [Display(Name = "Replaced")]
        Replaced = 1,
        /// <summary>
        /// The claim is functional, this would be like Ban claims.
        /// </summary>
        [Display(Name = "Functional")]
        Functional = 2,
        [Display(Name = "Ban")]
        Ban = 4,
        //Thursday = 8,
        //Friday = 16,
        //Saturday = 32,
        //Sunday = 64,
    }

}
