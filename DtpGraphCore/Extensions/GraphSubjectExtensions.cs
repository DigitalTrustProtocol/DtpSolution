using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using DtpCore.Collections.Generic;
using DtpGraphCore.Enumerations;
using DtpGraphCore.Model;

namespace DtpGraphCore.Extensions
{
    public static class GraphSubjectExtensions
    {
        public const int HybirdCollectionThreshold = 10;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddClaim(this GraphSubject subject, long claimKey, int claimIndex)
        {
            if (subject.ClaimsData == null)
            {
                subject.Flags |= SubjectFlags.ClaimsAreArray;
            }

            if((subject.Flags & SubjectFlags.ClaimsAreArray) == SubjectFlags.ClaimsAreArray)
            {

                var list = new List<GraphClaimEntry>();
                list.AddRange((GraphClaimEntry[])subject.ClaimsData);
                list.Add(new GraphClaimEntry { ID = claimKey, Index = claimIndex });

                if(list.Count < HybirdCollectionThreshold)
                {
                    subject.ClaimsData = list.ToArray();
                }
                else
                {
                    var dict = new FastLongDictionary<int>(list.Count);
                    foreach (var item in list)
                        dict.Add(item.ID, item.Index);

                    subject.ClaimsData = dict;
                    subject.Flags ^= SubjectFlags.ClaimsAreArray;
                }
            }
            else
            {
                ((FastLongDictionary<int>)subject.ClaimsData).Add(claimKey, claimIndex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetClaimIndex(this GraphSubject subject, long id, out int index)
        {
            index = -1;
            if (subject.ClaimsData == null)
                return false;

            if ((subject.Flags & SubjectFlags.ClaimsAreArray) == SubjectFlags.ClaimsAreArray)
                return TryGetClaimIndex((GraphClaimEntry[])subject.ClaimsData, id, out index);
            else
                // Use dictionary
                return ((FastLongDictionary<int>)subject.ClaimsData).FastTryGetValue(id, out index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetClaimIndex(this GraphClaimEntry[] entries, long id, out int index)
        {
            index = -1;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].ID != id)
                    continue;

                index = entries[i].Index;
                return true;
            }

            // No hit found
            return false;
        }
    }
}
