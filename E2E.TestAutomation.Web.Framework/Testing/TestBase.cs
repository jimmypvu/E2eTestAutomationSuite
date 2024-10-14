using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Playwright.NUnit;
using Jvu.TestAutomation.Web.Framework.Extensions;
using Jvu.TestAutomation.Web.Framework.Logging;
using System.Runtime.CompilerServices;

namespace Jvu.TestAutomation.Web.Framework.Testing
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// <remarks></remarks>
  /// ***********************************************************
  public class TestBase : PlaywrightTest, ILogger
  {
    public IBrowser Browser
    {
      get; private set;
    }

    public Dictionary<int, IBrowserContext> OpenBrowserContexts { get; private set; } = new Dictionary<int, IBrowserContext>();

    private IPage _page;

    public IPage Page
    {
      get
      {
        return this._page;
      }
      private set
      {
        this._page = value;
      }
    }

    public int ViewportWidth
    {
      get; private set;
    }

    public int ViewportHeight
    {
      get; private set;
    }

    public virtual BrowserTypeLaunchOptions? BrowserLaunchOptions
    {
      get; set;
    } = null;

    public BrowserNewContextOptions? AdditionalContextOptions
    {
      get; set;
    } = null;

    public string TestName => TestContext.CurrentContext.Test.MethodName;
    /// ***********************************************************
    [SetUp]
    public async Task SetupAsync()
    {
      // get browser from runsettings
      var browser = this.GetTestSettingIfExists("browser");

      this.WriteLine($"{this.GetTestSettingIfExists("slowMo")}");
      this.WriteLine($"{(bool)this.GetTestSettingIfExists("headless").AsBool()}");

      this.BrowserLaunchOptions = this.BrowserLaunchOptions ?? new BrowserTypeLaunchOptions()
      {
        SlowMo = (int)this.GetTestSettingIfExists("slowMo").AsInt(),
        Headless = (bool)this.GetTestSettingIfExists("headless").AsBool(),
        DownloadsPath = $"{this.GetTestDownloadsFolderPath()}\\downloads",
        Channel = this.GetTestSettingIfExists("channel"),
        //SlowMo = 33,
        //Headless = false,
      };

      // browser will launch with default launchoptions from .runsettings files unless otherwise specified
      switch (browser)
      {
        case "chromium":
        case "chrome":
        case "msedge":
          {
            this.Browser = this.BrowserLaunchOptions == null ?
              await this.Playwright.Chromium.LaunchAsync() :
              await this.Playwright.Chromium.LaunchAsync(this.BrowserLaunchOptions);
          }
          break;
        case "firefox":
          {
            this.Browser = this.BrowserLaunchOptions == null ?
              await this.Playwright.Firefox.LaunchAsync() :
              await this.Playwright.Firefox.LaunchAsync(this.BrowserLaunchOptions);
          }
          break;
        case "webkit":
          {
            this.Browser = this.BrowserLaunchOptions == null ?
              await this.Playwright.Webkit.LaunchAsync() :
              await this.Playwright.Webkit.LaunchAsync(this.BrowserLaunchOptions);
          }
          break;
        default: throw new NotImplementedException();
      }

      // override ContextOptions for each browser context as needed
      var context = await this.Browser.NewContextAsync(ContextOptions());
      this.OpenBrowserContexts.Add(1, context);

      this._page = await context.NewPageAsync();

      this.WriteLine($"browserName: {this.BrowserName}");
      this.WriteLine($"pageWindow: {this._page.ViewportSize.Width} x {this._page.ViewportSize.Height}");
    }

    /// ***********************************************************
    protected virtual BrowserNewContextOptions ContextOptions()
    {
      // add any additional contextOptions as needed per test, or override altogether if needed
      var options = this.AdditionalContextOptions != null ?
        this.AdditionalContextOptions :
        new BrowserNewContextOptions();

      // add default contextOptions here
      options.BaseURL = options.BaseURL.IsNotNullOrWhiteSpace() ?
        options.BaseURL :
        "https://www.reddit.com/";
      options.ViewportSize = options.ViewportSize != null ?
        options.ViewportSize :
        new ViewportSize
        {
          Height = (int)this.GetTestSettingIfExists("viewportHeight").AsInt(),
          Width = (int)this.GetTestSettingIfExists("viewportWidth").AsInt()
        };

      return options;
    }

    /// ***********************************************************
    public void WriteLine(string message)
    {
      TestContext.WriteLine($"{DateTime.Now:hh:mm:ss tt} - {message}");
    }

    /// ***********************************************************
    public void DetailLine(string message)
    {
      TestContext.WriteLine($"{DateTime.Now:hh:mm:ss tt} --- {message}");
    }

    /// ***********************************************************
    public void DebugLine(string message)
    {
      if ((bool)this.GetTestSettingIfExists("enableDebug").AsBool())
        TestContext.WriteLine($"{DateTime.Now:hh:mm:ss tt} - {message}");
    }

    /// ***********************************************************
    public void WriteException(Exception ex)
    {
      var timestamp = $"{DateTime.Now:hh:mm:ss tt}";
      TestContext.WriteLine($"{timestamp} - {ex.Message}");
      TestContext.WriteLine($"{timestamp} - {ex.StackTrace}");
    }
  }
}