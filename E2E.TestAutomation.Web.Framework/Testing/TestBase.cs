using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Playwright.NUnit;
using Jvu.TestAutomation.Web.Framework.Services;

namespace Jvu.TestAutomation.Web.Framework.Testing
{
  public class TestBase : PageTest, ILogger
  {
    /// *****************************************
    public void WriteLine(string message)
    {
      var timestamp = $"{DateTime.Now:hh:mm:ss tt}";
      TestContext.WriteLine(message);
    }

    /// *****************************************
    public void DebugLine(string message)
    {
      var timestamp = $"{DateTime.Now:hh:mm:ss tt}";
      if (TestContext.Parameters["enableDebug"].EqualsAnyCase(""))
        TestContext.WriteLine(message);
    }
  }
}
