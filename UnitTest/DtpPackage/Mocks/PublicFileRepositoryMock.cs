using DtpCore.Interfaces;
using System.Threading.Tasks;

namespace UnitTest.DtpPackage.Mocks
{
    public class PublicFileRepositoryMock : IPublicFileRepository
    {
        public bool FileExist = true;
        public string FileName = null;
        public string FileContent = null;

        public bool Exist(string name)
        {
            return FileExist;
        }

        public void WriteFile(string name, string contents)
        {
            FileName = name;
            FileContent = contents;
        }

        public Task WriteFileAsync(string name, string contents)
        {
            return Task.Run( () => WriteFile(name, contents));
        }

    }
}
