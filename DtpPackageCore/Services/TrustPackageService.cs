using DtpPackageCore.Interfaces;
using System;

namespace DtpPackageCore.Services
{
    public class TrustPackageService : ITrustPackageService
    {

        public string CreatePackageName()
        {
            DateTime now = DateTime.Now;
            return $"Package_trustdance_{now.ToString("yyyyMMdd_hhmmss")}.json";
        }

    }
}
