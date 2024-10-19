using E2e.Automation.Framework.Web.Testing;

namespace E2e.Automation.Framework.Web.Pages
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// <remarks>Base class for Page models for Web tests</remarks>
  /// ***********************************************************
  public abstract class PageBase
  {
    /*-----------------------------------------------------------*/
    private IPage? _page { get; set; }

    public IPage Page => _page;

    public TestBase? TestBase { get; set; }

    public ILogger Log { get; private set; }
    /*-----------------------------------------------------------*/

    /*-----------------------------------------------------------*/
    /// ***********************************************************
    public PageBase()
    {
      this.Log = new TestContextLogger();
    }

    /// ***********************************************************
    public async Task InitAsync(IPage page, TestBase? testBase = null)
    {
      this._page = page;
      if (testBase != null)
      {
        this.TestBase = testBase;
        this.Log = testBase;
      }
      await Task.CompletedTask;
    }
    /*-----------------------------------------------------------*/

    /*-----------------------------------------------------------*/
    /// ***********************************************************
    public ILocator Locate(string selector)
    {
      return this._page.Locator(selector);
    }

    /// ***********************************************************
    public async Task<string> TakeScreenshotAsync(string screenshotTitle, bool shouldWaitForNetworkIdle = false)
    {
      await this._page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

      if (!shouldWaitForNetworkIdle)
        await Task.Delay(42); // small artificial wait for browser to finish rendering dom content, loaded != rendered
      else
        await this._page.WaitForLoadStateAsync(LoadState.NetworkIdle);  // not recommended but if necessary can wait for all network activity to stop before continuing test

      var timestamp = $"{DateTime.Now:hh-mm-ss-tt_MM-dd-yy}";
      var fullClassName = TestContext.CurrentContext.Test.ClassName;
      var classNameParts = fullClassName.Split('.');
      var className = classNameParts[classNameParts.Length - 1];
      var screenshotPath = $"{Directory.GetCurrentDirectory()}\\screenshots\\{className}\\{TestContext.CurrentContext.Test.MethodName}\\{timestamp}_{screenshotTitle}_{this._page.ViewportSize.Width}x{this._page.ViewportSize.Height}.png";
      await this._page.ScreenshotAsync(new()
      {
        Path = screenshotPath
      });

      return screenshotPath;
    }

    /// ***********************************************************
    public async Task<T> NavigateAsAsync<T>(ILocator locator) where T : PageBase, new()
    {
      await locator.ClickAsync();
      await this._page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

      var page = new T();
      await page.InitAsync(this._page, this.TestBase);
      return page;
    }

    /// ***********************************************************
    public async Task<T> ReloadAsAsync<T>() where T : PageBase, new()
    {
      var response = await this._page.ReloadAsync();
      await response.FinishedAsync();
      await this._page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

      var page = new T();
      await page.InitAsync(this._page, this.TestBase);
      return page;
    }
    /*-----------------------------------------------------------*/
  }
}
