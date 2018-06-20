using Newtonsoft.Json;
using System.Collections.Generic;

namespace DtpCore.Model
{
    public class SchemaValidationResult
    {
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }

        public int ErrorsFound
        {
            get
            {
                return Errors.Count;
            }
        }

        public SchemaValidationResult()
        {
            Warnings = new List<string>();
            Errors = new List<string>();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
