using DtpCore.Model.Database;
using System.Runtime.CompilerServices;

namespace DtpCore.Extensions
{
    public static class ClaimStateTypeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Match(this ClaimStateType source, ClaimStateType target)
        {
            return (source & target) > 0;
        }
    }
}
