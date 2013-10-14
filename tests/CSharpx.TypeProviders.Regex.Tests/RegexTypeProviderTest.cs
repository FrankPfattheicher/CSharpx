using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpx.TypeProviders.Regex.Tests
{
  [TestClass]
  public class RegexTypeProviderTest
  {
    [TestMethod]
    public void TestRegexTypeProvider()
    {
      var provider = new RegexTypeProvider(@"(?<AreaCode>^\d{3})-(?<PhoneNumber>\d{7}$)");

      var type = provider.Type;
      Assert.IsNotNull(type);

      const string expectedAreaCode = "425";
      const string expectedPhoneNumber = "1232345";
      var result = provider.IsMatch("425-1232345");
      Assert.IsTrue(result);

      var regexPhone = Convert.ChangeType(provider.CreateInstance(), type);
      Assert.IsNotNull(regexPhone);

      var areaCode = regexPhone.AreaCode;
      var phoneNumber = regexPhone.PhoneNumber;
      Assert.AreEqual(expectedAreaCode, areaCode);
      Assert.AreEqual(expectedPhoneNumber, phoneNumber);
    }
  }
}
