﻿using DtpCore.Extensions;
using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class ClaimReplacedNotification : INotification
    {
        public Claim Claim { get; set; }

        public override string ToString()
        {
            return $"Trust {Claim?.Id.ToHex()} is replaced";
        }
    }
}
