using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Repository;

namespace DtpCore.Services
{
    public class KeyValueService : IKeyValueService
    {
        public TrustDBContext DBContext { get; }

        public KeyValueService(TrustDBContext trustDBContext)
        {
            DBContext = trustDBContext;
        }
        public async Task<int> SetAsync(string key, byte[] value)
        {
            var result = DBContext.KeyValues.FirstOrDefault(p => p.Key.Equals(key));
            if (result != null)
            {
                result.Value = value;
            }
            else
            {
                result = new KeyValue
                {
                    Key = key,
                    Value = value
                };
                DBContext.KeyValues.Add(result);
            }

            return await DBContext.SaveChangesAsync();
        }

        public int Set(string key, byte[] value)
        {
            return SetAsync(key, value).Result;
        }

        public async Task<byte[]> GetAsync(string key)
        {
            var result = await DBContext.KeyValues.FirstOrDefaultAsync(p => p.Key.Equals(key));
            if (result != null)
                return result.Value;
            return null;
        }

        public byte[] Get(string key)
        {
            return GetAsync(key).Result;
        }


        public async Task<int> RemoveAsync(string key)
        {
            var entity = await DBContext.KeyValues.FirstOrDefaultAsync(p => p.Key.Equals(key));
            if (entity != null)
            {
                DBContext.KeyValues.Remove(entity);
                return await DBContext.SaveChangesAsync();
            }
            return 0;
        }

        public int Remove(string key)
        {
            return RemoveAsync(key).Result;
        }

    }
}
