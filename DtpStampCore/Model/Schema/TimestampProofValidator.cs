using DtpCore.Interfaces;
using DtpCore.Model;
using DtpStampCore.Interfaces;
using System.Collections.Generic;

namespace DtpStampCore.Model.Schema
{
    public class TimestampProofValidator : ITimestampProofValidator
    {

        private ITimestampService _timestampService;

        public TimestampProofValidator(ITimestampService timestampService)
        {
            this._timestampService = timestampService;
        }

        public bool Validate(Timestamp timestamp, out IList<string> errors)
        {
            errors = new List<string>();

            if (timestamp == null)
                return false;

            var proof = _timestampService.GetProof(timestamp);

            if(proof == null)
            {
                errors.Add("No proof for timestamp was found");
                return false;
            }

            if(string.IsNullOrEmpty(proof.Address))
            {
                errors.Add("No address was found");
                return false;
            }

            if (proof.Confirmations < 1)
            {
                errors.Add("No confirmations on blockchain.");
                return false;
            }

            return true;
        }
    }
}
