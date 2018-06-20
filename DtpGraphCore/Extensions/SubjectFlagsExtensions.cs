using System.Runtime.CompilerServices;
using DtpGraphCore.Enumerations;

namespace DtpGraphCore.Extensions
{
    public static class SubjectFlagsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagFast(this SubjectFlags flags, SubjectFlags checkflag)
        {
            return (flags & checkflag) == checkflag;
        }
    }
}
