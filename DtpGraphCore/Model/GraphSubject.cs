using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DtpGraphCore.Enumerations;
using System.Collections.Concurrent;

namespace DtpGraphCore.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GraphSubject
    {
        //public int DatabaseID; // Ensures that we can get back to the Trust subject with claim in the database,
        public GraphIssuer TargetIssuer; // The type of the subject
        public SubjectFlags Flags; // Containes metadata about the GraphSubject object
        public int AliasIndex; // The name of the issuer for this subject
        //public ConcurrentDictionary<long, int> Claims;  // Int is scope index
        public GraphSubjectDictionary<long, int> Claims;  // Int is scope index

        [JsonIgnore]
        public object ClaimsData;
    }
}
