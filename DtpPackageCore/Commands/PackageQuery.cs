using DtpCore.Commands;
using DtpCore.Interfaces;
using DtpCore.Model;
using MediatR;
using System.Collections.Generic;

namespace DtpPackageCore.Commands
{
    public class PackageQuery : QueryCommand
    {

        //public int? DatabaseID { get; }
        //public bool IncludeClaims { get; }

        public string Scope { get; }
        



    }
}
