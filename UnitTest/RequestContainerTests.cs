using MCTG_Brian.Server;


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
            Assert.That(request.Method, Is.EqualTo("POST"));
        }

        [Test]
        public void TestParsePath()
        {
            var request = new RequestContainer(_testRequestString);
            Assert.That(request.Path, Is.EqualTo("/sessions"));
        }

        [Test]
        public void TestParseProtocol()
        {
            var request = new RequestContainer(_testRequestString);
            Assert.That(request.Protocol, Is.EqualTo("HTTP/1.1"));
        }

        [Test]
        public void TestParseHeaders()
        {
            var request = new RequestContainer(_testRequestString);
            Assert.That(request.Headers["Content-Type"], Is.EqualTo("application/json"));
            Assert.That(request.Headers["Authorization"], Is.EqualTo("Basic admin-mtcgToken"));
        }

       
    }
}
