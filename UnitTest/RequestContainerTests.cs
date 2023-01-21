using MCTG_Brian.Server;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.PARSE
{
    [TestFixture]
    public class RequestContainerTests
    {
        private string _testRequestString;

        [SetUp]
        public void Setup()
        {
            _testRequestString = "POST /sessions HTTP/1.1\r\nContent-Type: application/json\r\nAuthorization: Basic admin-mtcgToken\r\n\r\n{\"Username\":\"admin\",    \"Password\":\"istrator\"}";
        }

        [Test]
        public void TestParseMethod()
        {
            var request = new RequestContainer(_testRequestString);
            Assert.AreEqual("POST", request.Method);
        }

        [Test]
        public void TestParsePath()
        {
            var request = new RequestContainer(_testRequestString);
            Assert.AreEqual("/sessions", request.Path);
        }

        [Test]
        public void TestParseProtocol()
        {
            var request = new RequestContainer(_testRequestString);
            Assert.AreEqual("HTTP/1.1", request.Protocol);
        }

        [Test]
        public void TestParseHeaders()
        {
            var request = new RequestContainer(_testRequestString);
            Assert.AreEqual("application/json", request.Headers["Content-Type"]);
            Assert.AreEqual("Basic admin-mtcgToken", request.Headers["Authorization"]);
        }

       
    }
}
