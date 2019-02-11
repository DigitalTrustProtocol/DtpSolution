using System.Collections.Generic;
using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface ITimestampProofValidator
    {
        bool Validate(Timestamp timestamp, out IList<string> errors);
    }
}