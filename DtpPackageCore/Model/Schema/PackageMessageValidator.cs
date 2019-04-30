using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpPackageCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DtpPackageCore.Model.Schema
{
    public class PackageMessageValidator : IPackageMessageValidator
    {

        private readonly IServerIdentityService serverIdentityService;

        public PackageMessageValidator(IServerIdentityService serverIdentityService)
        {
            this.serverIdentityService = serverIdentityService;
        }

        public void Validate(PackageMessage message)
        {
            Validate(message, out IList<string> errors);
            if (errors.Count > 0)
                throw new ApplicationException(string.Join(" - ", errors));
        }

        public bool Validate(PackageMessage message, out IList<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(message.File))
                errors.Add("Path is null or empty.");

            if (string.IsNullOrWhiteSpace(message.ServerId))
                errors.Add("ServerId is null or empty.");

            if (message.ServerSignature == null || message.ServerSignature.Length == 0)
                errors.Add("Server signature is null or empty.");

            if (!serverIdentityService.Derivation.VerifySignatureMessage(message.ToBinary().ConvertToBase64(), message.ServerSignature, message.ServerId))
                errors.Add("Server signature do not match address and/or binary of message.");

            return errors.Count == 0;
        }
    }
}
