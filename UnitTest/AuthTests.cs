using MCTG_Brian.Authentication;
using MCTG_Brian.Database.Models;
using NUnit.Framework;

namespace UnitTest.AUTH
{
    [TestFixture]
    public class AuthTests
    {
        private User _testUser;
        private string _testToken;

        [SetUp]
        public void Setup()
        {
            _testUser = new User
            {
                Id = Guid.Parse("b1424918-9117-41dd-acef-002996792686"),
                Username = "testuser",
                Password = "testpassword",
                Coins = 100,
                Bio = "Test bio",
                Image = "testimage.jpg",
                Stats = new Stats(),
                Stack = new List<Card>(),
                Deck = new List<Card>()
            };
            _testToken = "testuser-mtcgToken";
            Auth.loginUser(_testUser);
        }

        //[Test]
        //public void TestGetUser()
        //{
        //    var result = Auth.getUser(_testToken);
        //    Assert.AreEqual(_testUser, result);
        //}

        [Test]
        public void TestGetAll()
        {
            var result = Auth.getAll();
            Assert.Contains(_testUser, result);
        }

        //[Test]
        //public void TestGetUserViaId()
        //{
        //    var result = Auth.getUserViaId(_testUser.Id);
        //    Assert.AreEqual(_testUser, result);
        //}

        [Test]
        public void TestUpdateUser()
        {
            _testUser.Coins = 200;
            Auth.updateUser(_testToken, _testUser);
            var result = Auth.getUser(_testToken);
            Assert.That(result.Coins, Is.EqualTo(200));
        }

        [Test]
        public void TestLoginUser()
        {
            var result = Auth.loginUser(_testUser);
            Assert.IsFalse(result);
        }

        [Test]
        public void TestIsUserLoggedIn()
        {
            var result = Auth.isUserLoggedIn(_testToken);
            Assert.IsTrue(result);
        }

        [Test]
        public void TestIsAdmin()
        {
            var adminToken = "admin-mtcgToken";
            var nonAdminToken = "testuser-mtcgToken";
            var adminResult = Auth.isAdmin(adminToken);
            var nonAdminResult = Auth.isAdmin(nonAdminToken);
            Assert.IsTrue(adminResult);
            Assert.IsFalse(nonAdminResult);
        }
    }
}