using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DtpGraphCore.Enumerations;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace DtpGraphCore.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GraphSubject
    {
        //public int DatabaseID; // Ensures that we can get back to the Trust subject with claim in the database,
        public GraphIssuer TargetIssuer; // The type of the subject
        public SubjectFlags Flags; // Containes metadata about the GraphSubject object
        //public int AliasIndex; // The name of the issuer for this subject
        //public ConcurrentDictionary<long, int> Claims;  // Int is scope index
        public List<int> Claims;  // Int is scope index

        //[JsonIgnore]
        //public object ClaimsData;

        /// <summary>
        /// Adds the claim, if exit then the claim is ignored.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="type"></param>
        /// <param name="claimIndex"></param>
        /// <returns>Return true is succesfull add or false if already exit</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddClaim(int claimIndex)
        {

            if (ClaimExist(claimIndex))
                return false;
            //var subjectClaimIndex = new SubjectClaimIndex(scope, type);
            //if (Claims.ContainsKey(subjectClaimIndex.Value))
            //    return false;

            if (Claims == null)
                Claims = new List<int>(1); // new GraphSubjectDictionary<long, int>(1); // Lazy create the Dictionary here.

            Claims.Add(claimIndex);
            Claims.TrimExcess();

            //Claims[subjectClaimIndex.Value] = claimIndex;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ClaimExist(int index)
        {
            if (Claims == null || Claims.Count == 0)
                return false;

            for (int i = 0; i < Claims.Count; i++)
            {
                if (Claims[i] == index) return true;
            }
            return false;
        }





        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveClaim(int claimIndex)
        {
            if (Claims == null)
                return false;

            //var subjectClaimIndex = new SubjectClaimIndex(scope, type);

            return Claims.Remove(claimIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<int> GetClaimIndexEnumerator()
        {
            if(Claims != null && Claims.Count > 0)
                return Claims.GetEnumerator();
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ClaimCount()
        {
            return (Claims != null) ? Claims.Count : 0;

        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public int GetClaimIndex(int scope, int type)
        //{


        //}


        //public const int HybirdCollectionThreshold = 10;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static void AddClaim(this GraphSubject subject, long claimKey, int claimIndex)
        //{
        //    if (ClaimsData == null)
        //    {
        //        subject.Flags |= SubjectFlags.ClaimsAreArray;
        //    }

        //    if ((subject.Flags & SubjectFlags.ClaimsAreArray) == SubjectFlags.ClaimsAreArray)
        //    {

        //        var list = new List<GraphClaimEntry>();
        //        list.AddRange((GraphClaimEntry[])subject.ClaimsData);
        //        list.Add(new GraphClaimEntry { ID = claimKey, Index = claimIndex });

        //        if (list.Count < HybirdCollectionThreshold)
        //        {
        //            subject.ClaimsData = list.ToArray();
        //        }
        //        else
        //        {
        //            var dict = new FastLongDictionary<int>(list.Count);
        //            foreach (var item in list)
        //                dict.Add(item.ID, item.Index);

        //            subject.ClaimsData = dict;
        //            subject.Flags ^= SubjectFlags.ClaimsAreArray;
        //        }
        //    }
        //    else
        //    {
        //        ((FastLongDictionary<int>)subject.ClaimsData).Add(claimKey, claimIndex);
        //    }
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static bool TryGetClaimIndex(this GraphSubject subject, long id, out int index)
        //{
        //    index = -1;
        //    if (subject.ClaimsData == null)
        //        return false;

        //    if ((subject.Flags & SubjectFlags.ClaimsAreArray) == SubjectFlags.ClaimsAreArray)
        //        return TryGetClaimIndex((GraphClaimEntry[])subject.ClaimsData, id, out index);
        //    else
        //        // Use dictionary
        //        return ((FastLongDictionary<int>)subject.ClaimsData).FastTryGetValue(id, out index);
        //}
    }
}
