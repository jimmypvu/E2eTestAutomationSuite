using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jvu.TestAutomation.Web.Framework.Extensions;

namespace Jvu.TestAutomation.Web.Framework.Logging
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// <remarks>logger</remarks>
  /// ***********************************************************
  public class TestContextLogger : ILogger
  {
    /// ***********************************************************
    public void WriteLine(string message)
    {
      TestContext.Out.WriteLine($"{DateTime.Now:hh:mm:ss tt} - {message}");
    }

    /// ***********************************************************
    public void DetailLine(string message)
    {
      TestContext.Out.WriteLine($"{DateTime.Now:hh:mm:ss tt} --- {message}");
    }

    /// ***********************************************************
    public void DebugLine(string message)
    {
      if ((bool)this.GetTestSettingIfExists("enableDebug").AsBool())
        TestContext.Out.WriteLine($"{DateTime.Now:hh:mm:ss tt} - {message}");
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
