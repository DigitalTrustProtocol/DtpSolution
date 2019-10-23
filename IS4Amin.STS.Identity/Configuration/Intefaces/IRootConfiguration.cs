namespace IS4Amin.STS.Identity.Configuration.Intefaces
{
    public interface IRootConfiguration
    {
        IAdminConfiguration AdminConfiguration { get; }

        IRegisterConfiguration RegisterConfiguration { get; }
    }
}