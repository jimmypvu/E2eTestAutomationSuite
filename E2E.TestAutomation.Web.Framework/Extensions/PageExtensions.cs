using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Jvu.TestAutomation.Web.Framework.Testing;
using Jvu.TestAutomation.Web.Framework.Pages;

namespace Jvu.TestAutomation.Web.Framework.Extensions
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// ***********************************************************
  public static partial class PageExtensions
  {
    /// ***********************************************************
    public static async Task TakeScreenshotAsync(this IPage page, string screenshotTitle, bool shouldWaitForNetworkIdle = false)
    {
      await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

      if (!shouldWaitForNetworkIdle)
        await Task.Delay(33); // artificial wait for browser to finish rendering dom content, loaded != rendered
      else
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);  // not recommended but if necessary can wait for all network activity to stop before continuing

      var timestamp = $"{DateTime.Now:hhmmssttMMddyy}";
      var fullClassName = TestContext.CurrentContext.Test.ClassName;
      var classNameParts = fullClassName.Split('.');
      var className = classNameParts[classNameParts.Length - 1];
      await page.ScreenshotAsync(new()
      {
        Path = $"{Directory.GetCurrentDirectory()}\\screenshots\\{className}\\{TestContext.CurrentContext.Test.MethodName}\\{timestamp}_{screenshotTitle}_{page.ViewportSize.Width}x{page.ViewportSize.Height}.png"
      });
    }
  }
}
