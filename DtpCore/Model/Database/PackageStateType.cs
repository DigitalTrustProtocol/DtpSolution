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
        /// The package is new without any ID and server signature.
        /// </summary>
        [Display(Name = "New")]
        New = 1,

        /// <summary>
        /// The package are currently under construction where claims are added on the fly.
        /// </summary>
        [Display(Name = "Building")]
        Building = 2,

        /// <summary>
        /// The package is now complete.
        /// </summary>
        [Display(Name = "Build")]
        Build = 4,

        /// <summary>
        /// When claims in a package have changed.
        /// </summary>
        [Display(Name = "Obsolete")]
        Obsolete = 8,

        /// <summary>
        /// When a package has been replaced by a new packaged.
        /// </summary>
        [Display(Name = "Replaced")]
        Replaced = 16,

        /// <summary>
        /// The package is build and signed.
        /// </summary>
        [Display(Name = "Signed")]
        Signed = 32 + 4,

    }

}
