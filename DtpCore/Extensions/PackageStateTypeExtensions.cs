using DtpCore.Model.Database;
using System.Runtime.CompilerServices;

namespace DtpCore.Extensions
{
    public static class PackageStateTypeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Match(this PackageStateType source, PackageStateType target)
        {
            return (source & target) == target;
        }
    }
}
