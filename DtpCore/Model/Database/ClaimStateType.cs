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
        /// <summary>
        /// Normal Claims without a special function
        /// </summary>
        [Display(Name = "None")]
        None = 0,

        /// <summary>
        /// Claims that have been replaced
        /// </summary>
        [Display(Name = "Replaced")]
        Replaced = 1,

        /// <summary>
        /// The claim is functional, this would be like Ban claims.
        /// </summary>
        [Display(Name = "Functional")]
        Functional = 2,

        [Display(Name = "Ban")]
        Ban = 4,
    }

}
