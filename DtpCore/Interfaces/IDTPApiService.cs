using System;
using System.Threading.Tasks;

namespace DtpCore.Interfaces
{
    public interface IDTPApiService
    {
        Task<T> DownloadData<T>(Uri uri);
        Task<T> DownloadData<T>(string ipAddress, string path);

        Task<T> UploadData<T>(Uri uri, byte[] data);
    }
}