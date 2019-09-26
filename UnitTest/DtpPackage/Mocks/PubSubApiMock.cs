using Ipfs;
using Ipfs.CoreApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest.DtpPackage.Mocks
{
    public class PubSubApiMock : IPubSubApi
    {
        public Task<IEnumerable<Peer>> PeersAsync(string topic = null, CancellationToken cancel = default(CancellationToken))
        {
            var peers = new List<Peer>();
            return Task.FromResult(peers as IEnumerable<Peer>); 
        }

        public Task PublishAsync(string topic, string message, CancellationToken cancel = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task PublishAsync(string topic, byte[] message, CancellationToken cancel = default)
        {
            throw new NotImplementedException();
        }

        public Task PublishAsync(string topic, Stream message, CancellationToken cancel = default)
        {
            throw new NotImplementedException();
        }

        public Task SubscribeAsync(string topic, Action<IPublishedMessage> handler, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> SubscribedTopicsAsync(CancellationToken cancel = default(CancellationToken))
        {
            var topics = new List<string>();
            topics.Add("twitter.com");
            topics.Add("test");
            topics.Add("");

            return Task.FromResult(topics as IEnumerable<string>);
        }
    }
}
