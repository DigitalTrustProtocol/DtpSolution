using Newtonsoft.Json;

namespace DtpCore.Model
{
    public class DatabaseEntity
    {
        [JsonIgnore]
        public int DatabaseID { get; set; } // Database row key
    }
}
