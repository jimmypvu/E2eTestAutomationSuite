using System.Runtime.CompilerServices;
using E2e.Automation.Framework.Extensions;
using E2e.Automation.Framework.Web.Extensions;

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

    public override Tuple<BrowserNewContextOptions, string>? AdditionalContextOptionsAndContextName =>
      Tuple.Create(new BrowserNewContextOptions()
      {
        Locale = "en-US"
      }, "EnglishLocale");

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
          await page.ToHaveExpectedFullScreenshotAsync($"{(await page.TitleAsync()).CleanStringForPath()}_Baseline.png", 0.5f, null, null, true);
        }
      }, "", 90);
    }
    /*-----------------------------------------------------------*/
  }
}
