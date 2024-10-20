namespace E2e.Automation.Web.Tests.E2ETests
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// ***********************************************************
  [TestFixture]
  [Category(TestCat.Demo)]
  [Category(TestCat.Web)]
  public class FunctionalWebUiFixture : TestBase
  {
    /*-----------------------------------------------------------*/
    public override string BaseUrl => "https://toolsqa.com/";
    public override string HostFilePath => $"{this.GetTestResourcesFolderPath()}\\testqaadservers.txt";
    /*-----------------------------------------------------------*/

    /*-----------------------------------------------------------*/
    /// ***********************************************************
    [Test]
    public async Task TBC_Functional_Test()
    {
      Assert.Pass();
    }
  }
}
