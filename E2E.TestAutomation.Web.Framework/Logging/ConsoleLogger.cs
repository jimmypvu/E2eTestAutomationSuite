using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jvu.TestAutomation.Web.Framework.Extensions;
using Jvu.TestAutomation.Web.Framework.Testing;

namespace Jvu.TestAutomation.Web.Framework.Logging
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// <remarks>console logger</remarks>
  /// ***********************************************************
  public class ConsoleLogger : ILogger
  {
    /// ***********************************************************
    public void WriteLine(string message)
    {
      Console.WriteLine($"{DateTime.Now:hh:mm:ss tt} - {message}");
    }

    /// ***********************************************************
    public void DetailLine(string message)
    {
      Console.WriteLine($"{DateTime.Now:hh:mm:ss tt} --- {message}");
    }

    /// ***********************************************************
    public void DebugLine(string message)
    {
      if (TestContext.Parameters["enableDebug"].EqualsAnyCase("true"))
        Console.WriteLine($"{DateTime.Now:hh:mm:ss tt} - {message}");
    }

    /// ***********************************************************
    public void WriteException(Exception ex)
    {
      var timestamp = $"{DateTime.Now:hh:mm:ss tt}";
      Console.WriteLine($"{timestamp} - {ex.Message}");
      Console.WriteLine($"{timestamp} - {ex.StackTrace}");
    }
  }
}
