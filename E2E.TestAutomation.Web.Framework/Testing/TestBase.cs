using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Playwright.NUnit;
using Jvu.TestAutomation.Web.Framework.Extensions;
using Jvu.TestAutomation.Web.Framework.Models;
using Jvu.TestAutomation.Web.Framework.Logging;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using static iText.Kernel.Pdf.Colorspace.PdfSpecialCs;
using Microsoft.Playwright;

namespace Jvu.TestAutomation.Web.Framework.Testing
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// <remarks></remarks>
  /// ***********************************************************
  public class TestBase : PlaywrightTest, ILogger
  {
    private TestContextLogger _log = new();

    public IBrowser Browser
    {
      get; private set;
    }

    public Dictionary<IBrowserContext, string> OpenBrowserContexts { get; private set; } = new Dictionary<IBrowserContext, string>();

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
    public string BrowserName
    {
      get; set;
    }
    public string BaseUrl
    {
      get; private set;
    } = "https://www.reddit.com/";
    /// ***********************************************************
    [SetUp]
    public async Task SetupAsync()
    {
      // get browser from runsettings
      this.BrowserName = this.GetTestSettingIfExists("browser");

      // if launch options are provided use those, otherwise use default launch options
      this.BrowserLaunchOptions = this.BrowserLaunchOptions ?? this.GetDefaultBrowserLaunchOptions(this.BrowserName);

      // browser will launch with default launchoptions from .runsettings files unless otherwise specified
      switch (this.BrowserName)
      {
        case "chromium":
        case "chrome":
        case "msedge":
          this.Browser = await this.Playwright.Chromium.LaunchAsync(this.BrowserLaunchOptions);
          break;
        case "firefox":
          this.Browser = await this.Playwright.Firefox.LaunchAsync(this.BrowserLaunchOptions);
          break;
        case "webkit":
          this.Browser = await this.Playwright.Webkit.LaunchAsync(this.BrowserLaunchOptions);
          break;
        default: throw new NotImplementedException();
      }

      // override ContextOptions for each browser context as needed
      var context = await this.Browser.NewContextAsync(this.ContextOptions());
      this.OpenBrowserContexts.Add(context, $"DefaultBrowserContext");

      this.WriteLine($"browserName: {this.BrowserName}");
      this.WriteLine($"opening page in {this.OpenBrowserContexts[context]}");

      this._page = await context.NewPageAsync();
      this.WriteLine($"viewportSizePage1: {this._page.ViewportSize.Width} x {this._page.ViewportSize.Height}");
    }

    /// ***********************************************************
    [TearDown]
    public async Task TeardownAsync()
    {
      foreach (var context in this.OpenBrowserContexts.Keys)
      {
        await context.CloseAsync();
      }

      await this.Browser.CloseAsync();
    }

    /// ***********************************************************
    public async Task<IPage> GetMobileDeviceContextAndLaunchPageAsync(string deviceName)
    {
      var device = this.Playwright.Devices[deviceName];
      device.BaseURL = this.BaseUrl;
      device.ColorScheme = ColorScheme.Dark;
      device.IsMobile = true;
      device.DeviceScaleFactor = 3;
      device.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.79 Safari/537.36";
      var mobileContext = await this.Browser.NewContextAsync(device);
      this.OpenBrowserContexts.Add(mobileContext, deviceName);
      return await mobileContext.NewPageAsync();
    }

    /// ***********************************************************
    protected virtual BrowserNewContextOptions ContextOptions()
    {
      // add any additional contextOptions as needed per test, or override altogether if needed
      var options = this.AdditionalContextOptions ??
        new BrowserNewContextOptions();

      // add default contextOptions here
      options.BaseURL = options.BaseURL ?? this.BaseUrl;
      options.ViewportSize = options.ViewportSize ??
        new ViewportSize
        {
          Height = (int)this.GetTestSettingIfExists("viewportHeight").AsInt(),
          Width = (int)this.GetTestSettingIfExists("viewportWidth").AsInt()
        };

      return options;
    }

    /// ***********************************************************
    public BrowserTypeLaunchOptions GetDefaultBrowserLaunchOptions(string browserName)
    {
      string channel = "";
      switch (browserName)
      {
        case "chromium": channel = "chromium"; break;
        case "chrome": channel = "chrome"; break;
        case "msedge": channel = "msedge"; break;
        case "firefox": channel = "firefox"; break;
        case "webkit": channel = "webkit"; break;
        default: channel = "chromium"; break;
      }

      return new BrowserTypeLaunchOptions()
      {
        SlowMo = (int)this.GetTestSettingIfExists("slowMo").AsInt(),
        Headless = (bool)this.GetTestSettingIfExists("headless").AsBool(),
        DownloadsPath = $"{this.GetTestDownloadsFolderPath()}\\downloads",
        Channel = channel
      };
    }

    /// ***********************************************************
    public void WriteLine(string message)
    {
      this._log.WriteLine($"{message}");
    }

    /// ***********************************************************
    public void DetailLine(string message)
    {
      this._log.DetailLine($"{message}");
    }

    /// ***********************************************************
    public void DebugLine(string message)
    {
      if (this.GetTestSettingIfExists("enableDebug").AsBool())
        this._log.DebugLine($"{message}");
    }

    /// ***********************************************************
    public void WriteException(Exception ex)
    {
      var timestamp = $"{DateTime.Now:hh:mm:ss tt}";
      this._log.DebugLine($"{ex.Message}");
      this._log.DebugLine($"{ex.StackTrace}");
    }
  }
}