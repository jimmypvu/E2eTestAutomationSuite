using E2e.Automation.Framework.Extensions;

namespace E2e.Automation.Web.Tests.VisualBaseline
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// ***********************************************************
  [TestFixture]
  [Category("Demo")]
  public class VisualBaselinesFixture : TestBase
  {
    /*-----------------------------------------------------------*/
    public override string BaseUrl => "https://toolsqa.com/";

    public override Tuple<BrowserNewContextOptions, string>? AdditionalContextOptionsAndContextName =>
      Tuple.Create(new BrowserNewContextOptions()
      {
        Locale = "en-US"
      }, "EnglishLocale");
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
          "googleadservices.com",
          "google.com",
          "googletagmanager.com",
          "demoqa.com",
          "linkedin.com",
          "paypal.com",
          "bing.com",
          "securepubads.g.doubleclick.net",
          "securepubads",
          "twitter.com",
          "facebook.com",
          "adtrafficquality.google",
          "adtrafficquality",
          "amazon-adsystem.com",
          "yahoo.com",
          "doubleclick.net",
          "adsrvr.org",
          "tapad.com",
          "adnxs.com",
          "ad.turn.com"
      };
    }
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
      }, "", 180);
    }
    /*-----------------------------------------------------------*/
  }
}
