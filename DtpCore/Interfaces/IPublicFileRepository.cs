namespace DtpCore.Interfaces
{
    public interface IPublicFileRepository
    {
        bool Exist(string name);
        void WriteFile(string name, string contents);
    }
}