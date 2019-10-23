using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DtpGraphCore.Model
{
    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class GraphIssuer
    {
        [JsonProperty(PropertyName = "id", Order = 10)]
        public string Id;

        [JsonProperty(PropertyName = "index", Order = 10)]
        public int Index;

        //[JsonProperty(PropertyName = "referenceId", Order = 20)]
        //public int DataBaseID;

        [JsonProperty(PropertyName = "subjects", NullValueHandling = NullValueHandling.Ignore, Order = 100)]
        public Dictionary<int, GraphSubject> Subjects = null; // new Dictionary<int, GraphSubject>();

        public override int GetHashCode()
        {
            return Index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetSubject(int targetIndex, out GraphSubject graphSubject)
        {
            if (Subjects == null)
            {
                graphSubject = default(GraphSubject);
                return false;
            }

            return Subjects.TryGetValue(targetIndex, out graphSubject);

        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public IEnumerator<GraphSubject> GetSubjectEnumerator()
        //{
        //    if (Subjects != null && Subjects.Count > 0)
        //        return Subjects.GetEnumerator();
        //    return null;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SubjectsCount()
        {
            return (Subjects != null) ? Subjects.Count : 0;

        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public GraphSubject AddGraphSubject(GraphIssuer subject)
        {
            if(Subjects == null)
                Subjects = new Dictionary<int, GraphSubject>();

            if (!Subjects.ContainsKey(subject.Index))
            {
                var graphSubject = new GraphSubject
                {
                    TargetIssuer = subject
                };
                Subjects.Add(subject.Index, graphSubject);
                return graphSubject;
            }
            else 
                return Subjects[subject.Index];
        }

        public void AddSubjectAndClaim(GraphIssuer subject, int claimIndex)
        {
            if (Subjects == null)
                Subjects = new Dictionary<int, GraphSubject>();

            if (!Subjects.ContainsKey(subject.Index))
            {
                var graphSubject = new GraphSubject
                {
                    TargetIssuer = subject
                };
                graphSubject.AddClaim(claimIndex);
                Subjects.Add(subject.Index, graphSubject);
            }
            else
                Subjects[subject.Index].AddClaim(claimIndex);
        }

    }
}
