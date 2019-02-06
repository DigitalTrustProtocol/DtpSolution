using Ipfs;
using Ipfs.CoreApi;
using Ipfs.Http;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest.DtpPackage.Mocks
{
    public class FileSystemApiMock : IFileSystemApi
    {
        private string content = string.Empty;


        public Task<IFileSystemNode> AddAsync(Stream stream, string name = "", AddFileOptions options = null, CancellationToken cancel = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<IFileSystemNode> AddDirectoryAsync(string path, bool recursive = true, AddFileOptions options = null, CancellationToken cancel = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<IFileSystemNode> AddFileAsync(string path, AddFileOptions options = null, CancellationToken cancel = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<IFileSystemNode> AddTextAsync(string text, AddFileOptions options = null, CancellationToken cancel = default(CancellationToken))
        {
            content = text;
            var node = new FileSystemNode();
            //node.DataBytes = Encoding.UTF8.GetBytes(text);
            node.Name = "123";
            return Task.FromResult((IFileSystemNode)node);
        }

        public Task<Stream> GetAsync(string path, bool compress = false, CancellationToken cancel = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<IFileSystemNode> ListFileAsync(string path, CancellationToken cancel = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<string> ReadAllTextAsync(string path, CancellationToken cancel = default(CancellationToken))
        {
            return Task.FromResult(content);
        }

        public Task<Stream> ReadFileAsync(string path, CancellationToken cancel = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<Stream> ReadFileAsync(string path, long offset, long count = 0, CancellationToken cancel = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
