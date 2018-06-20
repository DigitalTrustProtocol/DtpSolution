using System.Threading.Tasks;

namespace DtpCore.Interfaces
{
    public interface IKeyValueService
    {
        Task<byte[]> GetAsync(string key);
        byte[] Get(string key);

        Task<int> SetAsync(string key, byte[] value);
        int Set(string key, byte[] value);

        Task<int> RemoveAsync(string key);
        int Remove(string key);

    }
}