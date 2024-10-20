using E2e.Automation.Framework.Web.Extensions;

namespace E2e.Automation.Framework.Web.Testing
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// <remarks>setup methods and base test class for e2e UI & functional suites</remarks>
  /// ***********************************************************
  public class TestBase : PlaywrightTest, ILogger
  {
    /*-----------------------------------------------------------*/
    private TestContextLogger _log = new();
    private List<string> _blockedDomains => new List<string>(File.ReadAllLines(this.HostFilePath));

    public IBrowser Browser { get; private set; }

    public Dictionary<IBrowserContext, string?> OpenBrowserContextsToContextNameMap { get; private set; } = new Dictionary<IBrowserContext, string?>();
    private IPage _page;
    public IPage Page { get => _page; private set => _page = value; }

    public virtual BrowserTypeLaunchOptions? BrowserLaunchOptions { get; set; }
    public virtual BrowserNewContextOptions? BrowserContextOptions { get; private set; }
    public virtual Tuple<BrowserNewContextOptions, string>? AdditionalContextOptionsAndContextName { get; set; }

    public string TestName { get; private set; }
    public string TestFixtureName { get; private set; }
    public new string BrowserName { get; private set; }

    public virtual string BaseUrl { get; set; }
    public virtual List<string> AllowedDomains { get; set; }
    // add known ad domains to the adservers hostfile and provide in child test class to block domains as needed
    public virtual string HostFilePath => $"{this.GetTestResourcesFolderPath()}\\adservers.txt";
    /*-----------------------------------------------------------*/

    /*-----------------------------------------------------------*/
    /// ***********************************************************
    [SetUp]
    public async Task SetupAsync()
    {
      this.TestName = TestContext.CurrentContext.Test.MethodName;
      this.TestFixtureName = this.GetTestFixtureName();

      this.WriteLine($"{this.TestFixtureName}");
      this.WriteLine($"<<*******************************<< {this.TestName} >>*******************************>>");
      this.WriteLine($"running SetupAsync...");

      // get browser from runsettings, default to chromium if not set
      this.BrowserName = this.GetTestSettingIfExists("browser") ?? "chromium";

      this.DebugLine($"browser: {BrowserName}");

      // if launch options are provided use those, otherwise use default launch options
      bool wereAdditionalContextOptionsProvided = this.AdditionalContextOptionsAndContextName != null;
      this.BrowserLaunchOptions = this.BrowserLaunchOptions ?? this.GetDefaultBrowserLaunchOptions(this.BrowserName);

      this.BrowserContextOptions = this.SetBrowserContextOptions();

      this.WriteLine($"baseUrl: {this.BaseUrl}");

      var baseDomain = new Uri(this.BaseUrl).GetDomainFromUrl();
      this.WriteLine($"baseDomain: {baseDomain}");
      this.AllowedDomains = new List<string>() { baseDomain };

      this.WriteLine("allowed domains:");
      foreach (var domain in this.AllowedDomains) { this.WriteLine($" - {domain}"); }

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
      var context = await this.Browser.NewContextAsync(this.BrowserContextOptions);

      // filter requests so only requests from the same domain as the baseurl succeed or from allowed domains (block ads)
      await context.RouteAsync("**/*", async route =>
      {
        var url = new Uri(route.Request.Url);
        this.DebugLine($" - requestDomain: {url.Host}");

        var isAllowedDomain = this.AllowedDomains.Any(domain =>
        {
          this.DebugLine($" -- allowedDomains: {domain}");
          return url.Host.ContainsAnyCase(domain);
        });

        // allow all requests from the same domain as the base domain and any explictly defined domains, block any requests from explicitly defined domains, & allow all other requests (for resources, analytics, etc)
        if (!isAllowedDomain)
        {
          var isBlockedDomain = this._blockedDomains.Any(domain =>
          {
            this.DebugLine($" -- blockedDomains: {domain}");
            return url.Host.ContainsAnyCase(domain);
          });

          if (isBlockedDomain)
          {
            this.DebugLine($" - blocked non-domain request from: {url}");
            await route.AbortAsync();
          }
          else
          {
            this.DebugLine($" - allowed non-domain request from {url}...");
            await route.ContinueAsync();
          }
        }
        else
        {
          this.DebugLine($" - allowed request from {url}...");
          await route.ContinueAsync();
        }
      });

      if (wereAdditionalContextOptionsProvided)
        this.OpenBrowserContextsToContextNameMap.Add(context, this.AdditionalContextOptionsAndContextName.Item2);
      else
        this.OpenBrowserContextsToContextNameMap.Add(context, $"StandardBrowserContext");

      this.WriteLine($"browserName: {this.BrowserName}");
      this.WriteLine($"viewportSize: {this.GetTestSettingIfExists("viewportWidth")} x {this.GetTestSettingIfExists("viewportHeight")}");
      this.WriteLine($"opening page in {this.OpenBrowserContextsToContextNameMap[context]}");

      this._page = await context.NewPageAsync();
      this.WriteLine($"base.SetupAsync complete!");
      this.WriteLine("starting test...");
      this.WriteLine($"<<*******************************<< {this.TestName} >>*******************************>>");
    }

    /// ***********************************************************
    [TearDown]
    public async Task TeardownAsync()
    {
      foreach (var context in this.OpenBrowserContextsToContextNameMap.Keys)
      {
        await context.CloseAsync();
      }

      await this.Browser.CloseAsync();
    }

    /// ***********************************************************
    public async Task<IBrowserContext> GetDeviceContextAsync(string deviceName)
    {
      this.DebugLine($"getting device context for '{deviceName}'...");

      var device = Playwright.Devices[deviceName];
      device.BaseURL = this.BaseUrl;
      device.ColorScheme = ColorScheme.Dark;
      device.IsMobile = true;
      device.DeviceScaleFactor = 2;

      var mobileContext = await Browser.NewContextAsync(device);
      this.OpenBrowserContextsToContextNameMap.Add(mobileContext, deviceName);

      return mobileContext;
    }

    /// ***********************************************************
    public async Task<IPage> LaunchMobilePageInNewContextAsync(string deviceName)
    {
      var mobileContext = await this.GetDeviceContextAsync(deviceName);

      this.DebugLine($"launching mobile page in '{deviceName}' context...");
      return await mobileContext.NewPageAsync();
    }

    /// ***********************************************************
    public BrowserNewContextOptions SetBrowserContextOptions()
    {
      this.DebugLine($"setting BrowserNewContextOptions for {this.TestName}...");

      // add any additional contextOptions as needed per test
      BrowserNewContextOptions options;
      options = this.AdditionalContextOptionsAndContextName == null ?
        new BrowserNewContextOptions() :
        this.AdditionalContextOptionsAndContextName.Item1;

      // then add default contextOptions (or override altogether if needed)
      options.BaseURL = options.BaseURL ?? this.BaseUrl;
      options.ViewportSize = options.ViewportSize ??
        new ViewportSize
        {
          Height = this.GetTestSettingIfExists("viewportHeight").AsInt(),
          Width = this.GetTestSettingIfExists("viewportWidth").AsInt()
        };

      return options;
    }

    /// ***********************************************************
    protected BrowserTypeLaunchOptions GetDefaultBrowserLaunchOptions(string browserName)
    {
      this.DebugLine($"BrowserLaunchOptions was not specified");
      this.DebugLine($"getting default BrowserTypeLaunchOptions for '{browserName}'...");

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
        SlowMo = this.GetTestSettingIfExists("slowMo").AsInt(),
        Headless = this.GetTestSettingIfExists("headless").AsBool(),
        DownloadsPath = $"{this.GetTestDownloadsFolderPath()}\\downloads\\{TestContext.CurrentContext.Test.MethodName}",
        Channel = channel
      };
    }

    /// ***********************************************************
    public void WriteLine(string message)
    {
      _log.WriteLine($"{message}");
    }

    /// ***********************************************************
    public void DetailLine(string message)
    {
      _log.DetailLine($"{message}");
    }

    /// ***********************************************************
    public void DebugLine(string message)
    {
      if (this.GetTestSettingIfExists("enableDebug").AsBool())
        _log.DebugLine($"{message}");
    }

    /// ***********************************************************
    public void WriteException(Exception ex)
    {
      var timestamp = $"{DateTime.Now:hh:mm:ss tt}";
      _log.DebugLine($"{ex.Message}");
      _log.DebugLine($"{ex.StackTrace}");
    }
  }
}