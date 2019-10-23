using IS4Amin.STS.Identity.Configuration.Intefaces;

namespace IS4Amin.STS.Identity.Configuration
{
    public class RegisterConfiguration : IRegisterConfiguration
    {
        public bool Enabled { get; set; } = true;
    }
}
