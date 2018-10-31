using System.Threading.Tasks;

namespace DtpCore.Interfaces
{
    public interface IPublicFileRepository
    {
        bool Exist(string name);
        void WriteFile(string name, string contents);
        Task WriteFileAsync(string name, string contents);
    }
}