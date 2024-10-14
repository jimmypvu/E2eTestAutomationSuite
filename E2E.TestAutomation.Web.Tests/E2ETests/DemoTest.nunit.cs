using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jvu.TestAutomation.Web.Framework.Testing;
using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
using Jvu.TestAutomation.Web.Framework.Models;
using Jvu.TestAutomation.Web.Framework.Extensions;

namespace Jvu.TestAutomation.Web.Tests.E2ETests
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// ***********************************************************
  public class DemoTest : TestBase
  {
    /// ***********************************************************
    public static IEnumerable<TestCaseData> DeviceNames()
    {
      var devices = MobileTestData.GetDeviceNamesAsync().Result;
      foreach (var deviceName in devices)
      {
        yield return new TestCaseData(deviceName).SetName($"{deviceName}");
      }
    }

    /// ***********************************************************
    [Test]
    [TestCaseSource(nameof(DeviceNames))]
    public async Task Launch_Window(string deviceName)
    {
      //await this.Page.GotoAsync("");
      //await this.Page.ScreenshotAsync(new()
      //{
      //  Path = "C:\\Users\\Public\\ss1_Full_1440x900.png"
      //});

      //this.WriteLine($"pageUrl: {this.Page.Url}");

      //var highDpiMobileContextOptions = new BrowserNewContextOptions()
      //{
      //  ViewportSize = new() { Height = 2400, Width = 1080 },
      //  DeviceScaleFactor = 2,
      //  BaseURL = this.BaseUrl,
      //  UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.79 Safari/537.36"
      //};
      //var pixelSizedContext = await this.Browser.NewContextAsync(highDpiMobileContextOptions);
      //var pixelSizedPage = await pixelSizedContext.NewPageAsync();

      //await pixelSizedPage.GotoAsync("");
      //await pixelSizedPage.ScreenshotAsync(new()
      //{
      //  Path = "C:\\Users\\Public\\ss1_GooglePixelSizedViewport_1080x2400.png"
      //});

      //var iPadDevice = this.Playwright.Devices["iPad Pro 11 landscape"];
      //iPadDevice.BaseURL = this.BaseUrl;
      //iPadDevice.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.79 Safari/537.36";
      //var iPadContext = await this.Browser.NewContextAsync(iPadDevice);
      //var iPadPage = await iPadContext.NewPageAsync();

      //await iPadPage.GotoAsync("");
      //await iPadPage.ScreenshotAsync(new()
      //{
      //  Path = $"C:\\Users\\Public\\ss1_iPad11Landscape_{iPadPage.ViewportSize.Width}x{iPadPage.ViewportSize.Height}.png"
      //});

      var mobilePage = await this.GetMobileDeviceContextAndLaunchPageAsync(deviceName);
      await mobilePage.GotoAsync("");

      await mobilePage.TakeScreenshotAsync($"reddit_{deviceName}");
    }
  }
}
