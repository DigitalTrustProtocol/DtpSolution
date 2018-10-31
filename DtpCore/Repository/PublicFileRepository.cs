using DtpCore.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace DtpCore.Repository
{
    public class PublicFileRepository : IPublicFileRepository
    {
        public const string PUBLIC = "public";
        public const string REQUESTPATH = @"/public";

        public static string PublicFullPath
        {
            get
            {
                return Path.Combine(Directory.GetCurrentDirectory(), PUBLIC);
            }
        }

        public bool Exist(string name)
        {
            var path = Path.Combine(PublicFullPath, name);
            return File.Exists(path);
        }

        public void WriteFile(string name, string contents)
        {
            var fullName = name;
            if (!name.Contains("\\") && !name.Contains("/"))
            {
                fullName = Path.Combine(PublicFullPath, name);
            }

            File.WriteAllText(fullName, contents);

        }

        public async Task WriteFileAsync(string name, string contents)
        {
            var fullName = name;
            if (!name.Contains("\\") && !name.Contains("/"))
            {
                fullName = Path.Combine(PublicFullPath, name);
            }

            await File.WriteAllTextAsync(fullName, contents);
        }

    }
}



