using E2e.Automation.Framework.Extensions;

namespace E2e.Automation.Web.Tests.VisualBaseline
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// ***********************************************************
  public class DemoVisualBaselinesFixture : TestBase
  {
    /*-----------------------------------------------------------*/
    public override string BaseUrl => "https://toolsqa.com/";

    public Tuple<BrowserNewContextOptions, string>? AdditionalContextOptionsAndContextName =>
      Tuple.Create(new BrowserNewContextOptions()
      {
        Locale = "es-ES"
      }, "SpanishLocale");
    /*-----------------------------------------------------------*/

    /*-----------------------------------------------------------*/
    /// ***********************************************************
    [SetUp]
    public async Task SetupAsync()
    {
      this.BlockedDomains = new List<string>()
        {
          "ad.plus",
          "googlesyndication.com",
          "googletagservices.com",
          "google.com",
          "googletagmanager.com",
          "demoqa.com",
          "linkedin.com",
          "paypal.com",
          "bing.com"
        };
    }
    /*-----------------------------------------------------------*/

    /*-----------------------------------------------------------*/
    /// ***********************************************************
    [Test]
    public async Task Crawl_Pages_And_Check_Baseline_Images()
    {
      await this.Page.GotoAsync("");

      await this.Page.CrawlWebAndRunCustomLogicOnPagesAsync(async (page) =>
      {
        if (this.AllowedDomains.Any(domain => page.Url.ContainsAnyCase(domain)))
        {
          await page.ToHaveExpectedFullScreenshotAsync($"{(await page.TitleAsync()).CleanStringForPath()}_Baseline.png");
        }
      }, "", 60);
    }
    /*-----------------------------------------------------------*/
  }
}
