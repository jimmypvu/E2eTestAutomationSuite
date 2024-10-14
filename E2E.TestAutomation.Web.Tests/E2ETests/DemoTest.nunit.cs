using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jvu.TestAutomation.Web.Framework.Testing;
using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;

namespace Jvu.TestAutomation.Web.Tests.E2ETests
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// ***********************************************************
  public class DemoTest : TestBase
  {
    /// ***********************************************************
    [Test]
    public async Task Launch_Window()
    {
      await this.Page.GotoAsync("");
      this.WriteLine($"pageUrl: {this.Page.Url}");
    }



  }
}
