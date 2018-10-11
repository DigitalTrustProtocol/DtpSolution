using DtpCore.Interfaces;

namespace UnitTest.DtpCore.Repository
{
    public class PublicFileRepositoryMock : IPublicFileRepository
    {
        public bool FileExist = false;
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
    }
}
