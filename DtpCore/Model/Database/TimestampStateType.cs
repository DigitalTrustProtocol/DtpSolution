using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DtpCore.Model.Database
{
    /// <summary>
    /// The state of the Timestamp for internal use with Database.
    /// </summary>
    [Flags]
    public enum TimestampStateType : long
    {
        /// <summary>
        /// Normal Claims without a special function
        /// </summary>
        [Display(Name = "New")]
        New = 0,

        /// <summary>
        /// Timestamps that have been processed
        /// </summary>
        [Display(Name = "Processed")]
        Processed = 1
    }

}
