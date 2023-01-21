using MCTG_Brian.Authentication;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace MTCG_UnitTest
{
  
    public class UnitTest1
    {
        [Test]
        public void TestMethod1()
        {
            //Act
            string hansi = "admin";


            //Arrange
            bool result = Auth.isAdmin(hansi);

            //Assert

            Assert.That(result == true);
            
        }
    }
}