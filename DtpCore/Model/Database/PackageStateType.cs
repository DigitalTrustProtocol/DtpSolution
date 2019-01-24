using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DtpCore.Model.Database
{
    /// <summary>
    /// The state of the package for internal use with Database.
    /// </summary>
    [Flags]
    public enum PackageStateType : long
    {
        /// <summary>
        /// When claims in a package have changed.
        /// </summary>
        [Display(Name = "Obsolete")]
        Obsolete = 1,
        /// <summary>
        /// When a package has been replaced by a new packaged.
        /// </summary>
        [Display(Name = "Replaced")]
        Replaced = 2
    }

}
