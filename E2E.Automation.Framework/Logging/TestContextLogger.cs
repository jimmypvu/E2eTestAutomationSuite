using E2e.Automation.Framework.Web.Extensions;

namespace E2e.Automation.Framework.Logging
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
      TestContext.Out.WriteLine($"{timestamp} - {ex.Message}");
      TestContext.Out.WriteLine($"{timestamp} - {ex.StackTrace}");
    }
  }
}
