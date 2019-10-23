using IS4Amin.STS.Identity.Configuration.Intefaces;

namespace IS4Amin.STS.Identity.Configuration
{
    public class AdminConfiguration : IAdminConfiguration
    {
        public string IdentityAdminBaseUrl { get; set; } = "http://localhost:9000";
    }
}