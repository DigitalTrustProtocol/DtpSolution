using DtpCore.Model;
using DtpPackageCore.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DtpPackageCore.Notifications
{
    public class PackagePublishedNotification : INotification
    {

        public Package Package { get; }
        public PackageMessage Message { get; }

        public PackagePublishedNotification(Package package, PackageMessage message)
        {
            Package = package;
            Message = message;
        }
    }

}
