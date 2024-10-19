using E2e.Automation.Web.Tests.Resources.TestData;

namespace E2e.Automation.Web.Tests.E2ETests
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// ***********************************************************
  [TestFixture]
  [Category("Demo")]
  public class DemoTestFixture : TestBase
  {
    /*-----------------------------------------------------------*/
    /// ***********************************************************
    public static IEnumerable<TestCaseData> DeviceNames()
    {
      var devices = MobileTestData.GetDeviceNamesAsync().Result;
      foreach (var deviceName in devices)
      {
        yield return new TestCaseData(deviceName).SetName($"{deviceName}");
      }
    }
    /*-----------------------------------------------------------*/

    /*-----------------------------------------------------------*/
    /// ***********************************************************
    [Test]
    [Category(TestCat.Stable)]
    [TestCaseSource(nameof(DeviceNames))]
    public async Task Launch_Mobile_Page_And_Take_Screenshot(string deviceName)
    {
      var mobilePage = await this.LaunchMobilePageInNewContextAsync(deviceName);
      await mobilePage.GotoAsync("");

      await mobilePage.TakeScreenshotAsync($"reddit_{deviceName}");
    }

    /// ***********************************************************

  }
}
