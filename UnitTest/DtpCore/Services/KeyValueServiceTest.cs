using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using DtpCore.Interfaces;

namespace UnitTest.DtpCore.Workflow
{
    [TestClass]
    public class KeyValueServiceTest : StartupMock
    {

        [TestMethod]
        public void SetAndGet()
        {
            var keyValueService = ServiceProvider.GetRequiredService<IKeyValueService>();
            Assert.IsNotNull(keyValueService);

            var input = Encoding.UTF8.GetBytes("Hello world");
            keyValueService.Set("test", input);

            var output = keyValueService.Get("test");
            Assert.IsNotNull(output);
            Assert.AreEqual(input, output);
        }

        [TestMethod]
        public void Update()
        {
            var keyValueService = ServiceProvider.GetRequiredService<IKeyValueService>();
            Assert.IsNotNull(keyValueService);

            // Ensure an entity first on the same key
            keyValueService.Set("test", Encoding.UTF8.GetBytes("Random text"));

            // Update the key with the correct value
            var input = Encoding.UTF8.GetBytes("Hello world");
            keyValueService.Set("test", input);

            var output = keyValueService.Get("test");
            Assert.IsNotNull(output);
            Assert.AreEqual(input, output);
        }

        [TestMethod]
        public void Remove()
        {
            var keyValueService = ServiceProvider.GetRequiredService<IKeyValueService>();
            Assert.IsNotNull(keyValueService);

            var input = Encoding.UTF8.GetBytes("Hello world");
            keyValueService.Set("test", input);

            // Make sure that the entity exist
            var output = keyValueService.Get("test");
            Assert.IsNotNull(output);
            Assert.AreEqual(input, output);

            var removeCount = keyValueService.Remove("test");
            Assert.AreEqual(1, removeCount); 

            var noExist = keyValueService.Get("test");
            Assert.IsNull(noExist);
        }
    }
}
