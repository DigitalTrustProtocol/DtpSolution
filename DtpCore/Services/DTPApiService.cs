using DtpCore.Extensions;
using DtpCore.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DtpCore.Services
{
    public class DTPApiService : IDTPApiService
    {

        private readonly ILogger<DTPApiService> _logger;

        public DTPApiService(ILogger<DTPApiService> logger)
        {
            _logger = logger;
        }

        public async Task<T> DownloadData<T>(string ipAddress, string path)
        {
            var port = 80;

            var callUrl = new Uri($"http://{ipAddress}:{port}").Append(path);

            return await DownloadData<T>(callUrl);
        }

        public async Task<T> DownloadData<T>(Uri uri)
        {

            try
            {

                using (var client = new WebClient())
                {
                    var json = await client.DownloadStringTaskAsync(uri);

                    var data = JsonConvert.DeserializeObject<T>(json);

                    return data;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get data from server {uri.ToString()} - Error : {ex.Message}");
                return default(T);
            }
        }

        public T UploadData<T>(Uri uri, byte[] data)
        {
            try
            {

                using (var client = new WebClient())
                {
                    var result = client.UploadData(uri, "POST", data);
                    var json = Encoding.UTF8.GetString(result);
                    var obj = JsonConvert.DeserializeObject<T>(json);

                    return obj;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get data from server {uri.ToString()} - Error : {ex.Message}");
                return default(T);
            }
        }

        Task<T> IDTPApiService.UploadData<T>(Uri uri, byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
