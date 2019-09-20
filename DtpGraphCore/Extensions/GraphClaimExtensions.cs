using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DtpCore.Extensions;
using DtpGraphCore.Model;

namespace DtpGraphCore.Extensions
{
    public static class GraphClaimExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Exist(this Dictionary<long, int> claims, int scope, int type)
        {
            var subjectClaimIndex = new SubjectClaimIndex (scope, type );
            return claims.ContainsKey(subjectClaimIndex.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetIndex(this Dictionary<long, int> claims, int scope, int type, out int index)
        {
            var subjectClaimIndex = new SubjectClaimIndex(scope, type);
            return claims.TryGetValue(subjectClaimIndex.Value, out index);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Ensure(this Dictionary<long, int> claims, int scope, int type, int claimIndex)
        {
            var subjectClaimIndex = new SubjectClaimIndex(scope, type);
            if (claims.ContainsKey(subjectClaimIndex.Value))
                return true;

            claims[subjectClaimIndex.Value] = claimIndex;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Remove(this Dictionary<long, int> claims, int scope, int type)
        {
            var subjectClaimIndex = new SubjectClaimIndex(scope, type);
            if (claims.Remove(subjectClaimIndex.Value, out int claimIndex))
                return claimIndex;
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Exist(this ConcurrentDictionary<long, int> claims, int scope, int type)
        {
            var subjectClaimIndex = new SubjectClaimIndex(scope, type);
            return claims.ContainsKey(subjectClaimIndex.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetIndex(this ConcurrentDictionary<long, int> claims, int scope, int type, out int index)
        {
            var subjectClaimIndex = new SubjectClaimIndex(scope, type);
            return claims.TryGetValue(subjectClaimIndex.Value, out index);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Ensure(this ConcurrentDictionary<long, int> claims, int scope, int type, int claimIndex)
        {
            var subjectClaimIndex = new SubjectClaimIndex(scope, type);
            claims.Add(subjectClaimIndex.Value, claimIndex);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Remove(this ConcurrentDictionary<long, int> claims, int scope, int type)
        {
            var subjectClaimIndex = new SubjectClaimIndex(scope, type);
            if (claims.Remove(subjectClaimIndex.Value, out int claimIndex))
                return claimIndex;
            return -1;
        }
    }
}
