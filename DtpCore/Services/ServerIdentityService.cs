using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model.Configuration;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace DtpCore.Services
{
    public class ServerIdentityService : IServerIdentityService
    {
        private readonly IDerivationStrategyFactory _derivationStrategyFactory;
        private readonly IConfiguration _configuration;


        public ServerSection serverSection { get; set; }
        public IDerivationStrategy Derivation { get; set; }
        public byte[] Key { get; set; }
        public string Id { get; set; }


        public ServerIdentityService(IDerivationStrategyFactory derivationStrategyFactory, IConfiguration configuration)
        {
            _derivationStrategyFactory = derivationStrategyFactory;
            _configuration = configuration;
            Init();
        }

        private void Init()
        {
            serverSection = _configuration.GetModel(new ServerSection());
            Derivation = _derivationStrategyFactory.GetService(serverSection.Type);
            var scriptService = _derivationStrategyFactory.GetService(serverSection.Type);
            var keyword = _configuration.ServerKeyword();
            Key = scriptService.GetKey(Encoding.UTF8.GetBytes(keyword));
            Id = scriptService.GetAddress(Key);
        }

        public byte[] Sign(string text)
        {
            return Sign(Encoding.UTF8.GetBytes(text));
        }

        public byte[] Sign(byte[] data)
        {
            return Derivation.SignMessage(Key, data);
        }

        public bool Verify(byte[] data, byte[] signature)
        {
            return Derivation.VerifySignatureMessage(data, signature, Id);
        }


    }
}
