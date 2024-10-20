using E2e.Automation.Framework.Extensions;

namespace E2e.Automation.Web.Tests.VisualBaseline
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// ***********************************************************
  [TestFixture]
  [Category(TestCat.Demo)]
  [Category(TestCat.Visual)]
  public class VisualBaselinesFixture : TestBase
  {
    /*-----------------------------------------------------------*/
    public override string BaseUrl => "https://toolsqa.com/";

    // testcasesource
    public static IEnumerable<TestCaseData> DeviceNames => MobileTestData.GetDeviceNamesAsync().Result.Select(deviceName =>
        new TestCaseData(deviceName).SetName($"{deviceName}"));

    public override string HostFilePath => $"{this.GetTestResourcesFolderPath()}\\testqaadservers.txt";
    /*-----------------------------------------------------------*/

    /*-----------------------------------------------------------*/
    /// ***********************************************************
    [Test]
    [Category(TestCat.Stable)]
    public async Task Crawl_Pages_And_Check_Baseline_Images()
    {
      await this.Page.GotoAsync("");

      await this.Page.CrawlWebAndRunCustomLogicOnPagesAsync(async (page) =>
      {
        if (this.AllowedDomains.Any(domain => page.Url.ContainsAnyCase(domain)))
        {
          await page.ToHaveExpectedFullScreenshotAsync($"{(await page.TitleAsync()).CleanStringForPath()}_Baseline.png");
        }
      }, "", 90);
    }

    /// ***********************************************************
    [Test]
    [TestCaseSource(nameof(DeviceNames))]
    [Category(TestCat.Stable)]
    [Category(TestCat.Mobile)]
    public async Task Launch_Mobile_Page_And_Check_Baseline_Image(string deviceName)
    {
      var mobilePage = await this.LaunchMobilePageInNewContextAsync(deviceName);
      await mobilePage.GotoAsync("");

      await mobilePage.ToHaveExpectedMobileScreenshotAsync($"{(await mobilePage.TitleAsync()).CleanStringForPath()}_Baseline.png", deviceName);
    }
    /*-----------------------------------------------------------*/
  }
}
